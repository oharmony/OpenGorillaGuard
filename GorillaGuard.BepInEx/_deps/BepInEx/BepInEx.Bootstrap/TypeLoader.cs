using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx.Configuration;
using BepInEx.Logging;
using Mono.Cecil;

namespace BepInEx.Bootstrap;

public static class TypeLoader
{

	public static readonly DefaultAssemblyResolver Resolver;

	public static readonly ReaderParameters ReaderParameters;

	private static readonly ConfigEntry<bool> EnableAssemblyCache;

	public static event AssemblyResolveEventHandler AssemblyResolve;

	static TypeLoader()
	{
		EnableAssemblyCache = ConfigFile.CoreConfig.Bind("Caching", "EnableAssemblyCache", defaultValue: true, "Enable/disable assembly metadata cache\nEnabling this will speed up discovery of plugins and patchers by caching the metadata of all types BepInEx discovers.");
		Resolver = new DefaultAssemblyResolver();
		ReaderParameters = new ReaderParameters
		{
			AssemblyResolver = Resolver
		};
		Resolver.ResolveFailure += delegate(object sender, AssemblyNameReference reference)
		{
			if (!Utility.TryParseAssemblyName(reference.FullName, out var assemblyName))
			{
				return TypeLoader.AssemblyResolve?.Invoke(sender, reference);
			}
			foreach (string item in new string[3]
			{
				Paths.BepInExAssemblyDirectory,
				Paths.PluginPath,
				Paths.PatcherPluginPath
			}.Concat(Paths.DllSearchPaths))
			{
				if (Utility.TryResolveDllAssembly(assemblyName, item, ReaderParameters, out var assembly))
				{
					return assembly;
				}
			}
			return TypeLoader.AssemblyResolve?.Invoke(sender, reference);
		};
	}

	public static Dictionary<string, List<T>> FindPluginTypes<T>(string directory, Func<TypeDefinition, T> typeSelector, Func<AssemblyDefinition, bool> assemblyFilter = null, string cacheName = null) where T : ICacheable, new()
	{
		Dictionary<string, List<T>> dictionary = new Dictionary<string, List<T>>();
		Dictionary<string, CachedAssembly<T>> dictionary2 = null;
		if (cacheName != null)
		{
			dictionary2 = LoadAssemblyCache<T>(cacheName);
		}
		string[] files = Directory.GetFiles(Path.GetFullPath(directory), "*.dll", SearchOption.AllDirectories);
		foreach (string text in files)
		{
			try
			{
				if (dictionary2 != null && dictionary2.TryGetValue(text, out var value) && File.GetLastWriteTimeUtc(text).Ticks == value.Timestamp)
				{
					dictionary[text] = value.CacheItems;
					continue;
				}
				AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(text, ReaderParameters);
				if (assemblyFilter != null && !assemblyFilter(assemblyDefinition))
				{
					dictionary[text] = new List<T>();
					assemblyDefinition.Dispose();
					continue;
				}
				List<T> value2 = (from t in assemblyDefinition.MainModule.Types.Select(typeSelector)
					where t != null
					select t).ToList();
				dictionary[text] = value2;
				assemblyDefinition.Dispose();
			}
			catch (BadImageFormatException ex)
			{
				Logger.LogDebug("Skipping loading " + text + " because it's not a valid .NET assembly. Full error: " + ex.Message);
			}
			catch (Exception ex2)
			{
				Logger.LogError(ex2.ToString());
			}
		}
		if (cacheName != null)
		{
			SaveAssemblyCache(cacheName, dictionary);
		}
		return dictionary;
	}

	public static Dictionary<string, CachedAssembly<T>> LoadAssemblyCache<T>(string cacheName) where T : ICacheable, new()
	{
		if (!EnableAssemblyCache.Value)
		{
			return null;
		}
		Dictionary<string, CachedAssembly<T>> dictionary = new Dictionary<string, CachedAssembly<T>>();
		try
		{
			string path = Path.Combine(Paths.CachePath, cacheName + "_typeloader.dat");
			if (!File.Exists(path))
			{
				return null;
			}
			using BinaryReader binaryReader = new BinaryReader(File.OpenRead(path));
			int num = binaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				string key = binaryReader.ReadString();
				long timestamp = binaryReader.ReadInt64();
				int num2 = binaryReader.ReadInt32();
				List<T> list = new List<T>();
				for (int j = 0; j < num2; j++)
				{
					T item = new T();
					item.Load(binaryReader);
					list.Add(item);
				}
				dictionary[key] = new CachedAssembly<T>
				{
					Timestamp = timestamp,
					CacheItems = list
				};
			}
		}
		catch (Exception ex)
		{
			Logger.LogWarning("Failed to load cache \"" + cacheName + "\"; skipping loading cache. Reason: " + ex.Message + ".");
		}
		return dictionary;
	}

	public static void SaveAssemblyCache<T>(string cacheName, Dictionary<string, List<T>> entries) where T : ICacheable
	{
		if (!EnableAssemblyCache.Value)
		{
			return;
		}
		try
		{
			if (!Directory.Exists(Paths.CachePath))
			{
				Directory.CreateDirectory(Paths.CachePath);
			}
			using BinaryWriter binaryWriter = new BinaryWriter(File.OpenWrite(Path.Combine(Paths.CachePath, cacheName + "_typeloader.dat")));
			binaryWriter.Write(entries.Count);
			foreach (KeyValuePair<string, List<T>> entry in entries)
			{
				binaryWriter.Write(entry.Key);
				binaryWriter.Write(File.GetLastWriteTimeUtc(entry.Key).Ticks);
				binaryWriter.Write(entry.Value.Count);
				foreach (T item in entry.Value)
				{
					item.Save(binaryWriter);
				}
			}
		}
		catch (Exception ex)
		{
			Logger.LogWarning("Failed to save cache \"" + cacheName + "\"; skipping saving cache. Reason: " + ex.Message + ".");
		}
	}

	public static string TypeLoadExceptionToString(ReflectionTypeLoadException ex)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Exception[] loaderExceptions = ex.LoaderExceptions;
		foreach (Exception ex2 in loaderExceptions)
		{
			stringBuilder.AppendLine(ex2.Message);
			if (ex2 is FileNotFoundException ex3)
			{
				if (!string.IsNullOrEmpty(ex3.FusionLog))
				{
					stringBuilder.AppendLine("Fusion Log:");
					stringBuilder.AppendLine(ex3.FusionLog);
				}
			}
			else if (ex2 is FileLoadException ex4 && !string.IsNullOrEmpty(ex4.FusionLog))
			{
				stringBuilder.AppendLine("Fusion Log:");
				stringBuilder.AppendLine(ex4.FusionLog);
			}
			stringBuilder.AppendLine();
		}
		return stringBuilder.ToString();
	}
}
