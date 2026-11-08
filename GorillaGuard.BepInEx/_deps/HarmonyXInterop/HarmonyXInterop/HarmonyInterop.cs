using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using HarmonyLib.Public.Patching;
using HarmonyLib.Tools;
using Mono.Cecil;

namespace HarmonyXInterop;

public static class HarmonyInterop
{
	private const string BACKUP_PATH = "BepInEx_Shim_Backup";

	private static Version maxAvailableShimVersion;

	private static readonly SortedDictionary<Version, string> Assemblies = new SortedDictionary<Version, string>();

	private static readonly HashSet<string> InteropAssemblyNames = new HashSet<string>();

	private static readonly Func<MethodBase, PatchInfo, MethodInfo> UpdateWrapper = AccessTools.MethodDelegate<Func<MethodBase, PatchInfo, MethodInfo>>(AccessTools.Method(typeof(HarmonyManipulator).Assembly.GetType("HarmonyLib.PatchFunctions"), "UpdateWrapper"));

	private static readonly Action<Logger.LogChannel, Func<string>, bool> HarmonyLog = AccessTools.MethodDelegate<Action<Logger.LogChannel, Func<string>, bool>>(AccessTools.Method(typeof(Logger), "Log"));

	private static readonly Action<Logger.LogChannel, string, bool> HarmonyLogText = AccessTools.MethodDelegate<Action<Logger.LogChannel, string, bool>>(AccessTools.Method(typeof(Logger), "LogText"));

	private static readonly Dictionary<string, long> shimCache = new Dictionary<string, long>();

	private static BinaryWriter cacheWriter;

	public static void Log(int channel, Func<string> message)
	{
		HarmonyLog((Logger.LogChannel)channel, message, arg3: false);
	}

	public static void LogText(int channel, string message)
	{
		HarmonyLogText((Logger.LogChannel)channel, message, arg3: false);
	}

	public static void Initialize(string cachePath)
	{
		Directory.CreateDirectory(cachePath);
		string path = Path.Combine(cachePath, "harmony_interop_cache.dat");
		string[] files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "0Harmony*.dll", SearchOption.AllDirectories);
		for (int i = 0; i < files.Length; i++)
		{
			using AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(files[i]);
			if (assemblyDefinition.Name.Name != "0Harmony")
			{
				Assemblies.Add(assemblyDefinition.Name.Version, assemblyDefinition.Name.Name);
				InteropAssemblyNames.Add(assemblyDefinition.Name.Name);
			}
		}
		maxAvailableShimVersion = Assemblies.LastOrDefault().Key;
		if (File.Exists(path))
		{
			try
			{
				using BinaryReader binaryReader = new BinaryReader(File.OpenRead(path));
				while (true)
				{
					string key = binaryReader.ReadString();
					long value = binaryReader.ReadInt64();
					shimCache[key] = value;
				}
			}
			catch (Exception)
			{
			}
		}
		try
		{
			cacheWriter = new BinaryWriter(File.Create(path));
			foreach (KeyValuePair<string, long> item in shimCache)
			{
				cacheWriter.Write(item.Key);
				cacheWriter.Write(item.Value);
			}
			cacheWriter.Flush();
		}
		catch (IOException)
		{
		}
	}

	public static byte[] TryShim(string path, string gameRootDirectory, Action<string> logMessage = null, ReaderParameters readerParameters = null)
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
		List<string> deps;
		byte[] result = TryShimInternal(path, gameRootDirectory, logMessage, readerParameters, out deps);
		foreach (string item in deps)
		{
			hashSet.Add(item);
		}
		while (hashSet.Count != 0)
		{
			string text = hashSet.First();
			TryShimInternal(text, gameRootDirectory, logMessage, readerParameters, out deps);
			foreach (string item2 in deps)
			{
				hashSet.Add(item2);
			}
			hashSet.Remove(text);
		}
		return result;
	}

	private static bool NeedsShimming(string path, out long lastWriteTime, Action<string> logMessage = null)
	{
		lastWriteTime = 0L;
		try
		{
			if (!File.Exists(path))
			{
				return false;
			}
		}
		catch (Exception arg)
		{
			logMessage?.Invoke($"Failed to read path {path}: {arg}");
			return false;
		}
		lastWriteTime = File.GetLastWriteTimeUtc(path).Ticks;
		if (shimCache.TryGetValue(path, out var value))
		{
			return value != lastWriteTime;
		}
		return true;
	}

	private static byte[] TryShimInternal(string path, string gameRootDirectory, Action<string> logMessage, ReaderParameters readerParameters, out List<string> deps)
	{
		deps = new List<string>();
		if (!NeedsShimming(path, out var lastWriteTime, logMessage))
		{
			return null;
		}
		byte[] result = null;
		try
		{
			string dir = Path.GetDirectoryName(path);
			byte[] array;
			try
			{
				array = File.ReadAllBytes(path);
			}
			catch (Exception)
			{
				return null;
			}
			using MemoryStream stream = new MemoryStream(array);
			using AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(stream, readerParameters ?? new ReaderParameters());
			deps.AddRange(from a in assemblyDefinition.MainModule.AssemblyReferences
				select Path.Combine(dir, a.Name + ".dll") into p
				where NeedsShimming(p, out var _, logMessage)
				select p);
			AssemblyNameReference harmonyRef = assemblyDefinition.MainModule.AssemblyReferences.FirstOrDefault((AssemblyNameReference a) => a.Name.StartsWith("0Harmony") && !InteropAssemblyNames.Contains(a.Name));
			if (harmonyRef != null)
			{
				KeyValuePair<Version, string> keyValuePair = Assemblies.LastOrDefault((KeyValuePair<Version, string> kv) => VersionMatches(kv.Key, harmonyRef.Version));
				if (keyValuePair.Value != null)
				{
					logMessage?.Invoke("Shimming " + path + " to use older version of Harmony (" + keyValuePair.Value + "). Please update the plugin if possible.");
					harmonyRef.Name = keyValuePair.Value;
					using MemoryStream memoryStream = new MemoryStream();
					assemblyDefinition.Write(memoryStream);
					try
					{
						string path2 = Path.Combine(gameRootDirectory, "BepInEx_Shim_Backup");
						string directoryName = Path.GetDirectoryName(Path.GetFullPath(path).Substring(gameRootDirectory.Length + 1));
						string text = Path.Combine(path2, directoryName);
						Directory.CreateDirectory(text);
						File.WriteAllBytes(Path.Combine(text, Path.GetFileName(path)), array);
						File.WriteAllBytes(path, memoryStream.ToArray());
						lastWriteTime = File.GetLastWriteTimeUtc(path).Ticks;
					}
					catch (IOException)
					{
						lastWriteTime = 0L;
					}
					result = memoryStream.ToArray();
				}
			}
			shimCache[path] = lastWriteTime;
			if (cacheWriter != null)
			{
				cacheWriter.Write(path);
				cacheWriter.Write(lastWriteTime);
				cacheWriter.Flush();
			}
		}
		catch (Exception arg)
		{
			logMessage?.Invoke($"Failed to shim {path}: {arg}");
		}
		return result;
		static bool VersionMatches(Version cmpV, Version refV)
		{
			if (maxAvailableShimVersion != null && refV <= maxAvailableShimVersion && cmpV.Major == refV.Major && cmpV.Minor == refV.Minor)
			{
				return cmpV <= refV;
			}
			return false;
		}
	}

	public static void ApplyPatch(MethodBase target, PatchInfoWrapper add, PatchInfoWrapper remove)
	{
		PatchInfo patchInfo = target.ToPatchInfo();
		lock (patchInfo)
		{
			patchInfo.prefixes = Sync(add.prefixes, remove.prefixes, patchInfo.prefixes);
			patchInfo.postfixes = Sync(add.postfixes, remove.postfixes, patchInfo.postfixes);
			patchInfo.transpilers = Sync(WrapTranspilers(add.transpilers), WrapTranspilers(remove.transpilers), patchInfo.transpilers);
			patchInfo.finalizers = Sync(add.finalizers, remove.finalizers, patchInfo.finalizers);
		}
		UpdateWrapper(target, patchInfo);
		static PatchMethod[] WrapTranspilers(PatchMethod[] transpilers)
		{
			return transpilers.Select((PatchMethod p) => new PatchMethod
			{
				after = p.after,
				before = p.before,
				method = TranspilerInterop.WrapInterop(p.method),
				owner = p.owner,
				priority = p.priority
			}).ToArray();
		}
	}

	private static Patch[] Sync(PatchMethod[] add, PatchMethod[] remove, Patch[] current)
	{
		if (add.Length == 0 && remove.Length == 0)
		{
			return current;
		}
		current = current.Where((Patch p) => !remove.Any((PatchMethod r) => r.method == p.PatchMethod && r.owner == p.owner)).ToArray();
		int initialIndex = current.Length;
		return current.Concat(add.Where((PatchMethod method) => method != null).Select((PatchMethod method, int i) => new Patch(method.ToHarmonyMethod(), i + initialIndex, method.owner))).ToArray();
	}
}
