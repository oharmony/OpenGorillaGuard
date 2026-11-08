using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Preloader.RuntimeFixes;
using Mono.Cecil;

namespace BepInEx.Preloader.Patching;

public static class AssemblyPatcher
{
	private const BindingFlags ALL = BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	private static readonly string DumpedAssembliesPath = Utility.CombinePaths(Paths.BepInExRootPath, "DumpedAssemblies", Paths.ProcessName);

	private static readonly Dictionary<string, string> DumpedAssemblyPaths = new Dictionary<string, string>();

	private static readonly ConfigEntry<bool> ConfigDumpAssemblies = ConfigFile.CoreConfig.Bind("Preloader", "DumpAssemblies", defaultValue: false, "If enabled, BepInEx will save patched assemblies into BepInEx/DumpedAssemblies.\nThis can be used by developers to inspect and debug preloader patchers.");

	private static readonly ConfigEntry<bool> ConfigLoadDumpedAssemblies = ConfigFile.CoreConfig.Bind("Preloader", "LoadDumpedAssemblies", defaultValue: false, "If enabled, BepInEx will load patched assemblies from BepInEx/DumpedAssemblies instead of memory.\nThis can be used to be able to load patched assemblies into debuggers like dnSpy.\nIf set to true, will override DumpAssemblies.");

	private static readonly ConfigEntry<bool> ConfigBreakBeforeLoadAssemblies = ConfigFile.CoreConfig.Bind("Preloader", "BreakBeforeLoadAssemblies", defaultValue: false, "If enabled, BepInEx will call Debugger.Break() once before loading patched assemblies.\nThis can be used with debuggers like dnSpy to install breakpoints into patched assemblies before they are loaded.");

	public static List<PatcherPlugin> PatcherPlugins { get; } = new List<PatcherPlugin>();

	private static IEnumerable<PatcherPlugin> PatcherPluginsSafe => PatcherPlugins.ToList();

	public static void AddPatcher(PatcherPlugin patcher)
	{
		PatcherPlugins.Add(patcher);
	}

	private static T CreateDelegate<T>(MethodInfo method) where T : class
	{
		if (method == null)
		{
			return null;
		}
		return Delegate.CreateDelegate(typeof(T), method) as T;
	}

	private static PatcherPlugin ToPatcherPlugin(TypeDefinition type)
	{
		if (type.IsInterface || (type.IsAbstract && !type.IsSealed))
		{
			return null;
		}
		MethodDefinition methodDefinition = type.Methods.FirstOrDefault((MethodDefinition m) => m.Name.Equals("get_TargetDLLs", StringComparison.InvariantCultureIgnoreCase) && m.IsPublic && m.IsStatic);
		if (methodDefinition == null || methodDefinition.ReturnType.FullName != "System.Collections.Generic.IEnumerable`1<System.String>")
		{
			return null;
		}
		if (type.Methods.FirstOrDefault((MethodDefinition m) => m.Name.Equals("Patch") && m.IsPublic && m.IsStatic && m.ReturnType.FullName == "System.Void" && m.Parameters.Count == 1 && (m.Parameters[0].ParameterType.FullName == "Mono.Cecil.AssemblyDefinition&" || m.Parameters[0].ParameterType.FullName == "Mono.Cecil.AssemblyDefinition")) == null)
		{
			return null;
		}
		return new PatcherPlugin
		{
			TypeName = type.FullName
		};
	}

	public static void AddPatchersFromDirectory(string directory)
	{
		if (!Directory.Exists(directory))
		{
			return;
		}
		SortedDictionary<string, PatcherPlugin> sortedDictionary = new SortedDictionary<string, PatcherPlugin>();
		foreach (KeyValuePair<string, List<PatcherPlugin>> item in TypeLoader.FindPluginTypes(directory, ToPatcherPlugin))
		{
			string key = item.Key;
			List<PatcherPlugin> value = item.Value;
			if (value.Count == 0)
			{
				continue;
			}
			Assembly assembly = Assembly.LoadFile(key);
			foreach (PatcherPlugin item2 in value)
			{
				try
				{
					Type type = assembly.GetType(item2.TypeName);
					MethodInfo[] methods = type.GetMethods(BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					item2.Initializer = CreateDelegate<Action>(methods.FirstOrDefault((MethodInfo m) => m.Name.Equals("Initialize", StringComparison.InvariantCultureIgnoreCase) && m.GetParameters().Length == 0 && m.ReturnType == typeof(void)));
					item2.Finalizer = CreateDelegate<Action>(methods.FirstOrDefault((MethodInfo m) => m.Name.Equals("Finish", StringComparison.InvariantCultureIgnoreCase) && m.GetParameters().Length == 0 && m.ReturnType == typeof(void)));
					item2.TargetDLLs = CreateDelegate<Func<IEnumerable<string>>>(type.GetProperty("TargetDLLs", BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetGetMethod());
					MethodInfo patcher = methods.FirstOrDefault((MethodInfo m) => m.Name.Equals("Patch", StringComparison.CurrentCultureIgnoreCase) && m.ReturnType == typeof(void) && m.GetParameters().Length == 1 && (m.GetParameters()[0].ParameterType == typeof(AssemblyDefinition) || m.GetParameters()[0].ParameterType == typeof(AssemblyDefinition).MakeByRefType()));
					item2.Patcher = delegate(ref AssemblyDefinition pAss)
					{
						object[] array = new object[1] { pAss };
						patcher.Invoke(null, array);
						pAss = (AssemblyDefinition)array[0];
					};
					sortedDictionary.Add(assembly.GetName().Name + "/" + type.FullName, item2);
				}
				catch (Exception ex)
				{
					Logger.LogError("Failed to load patcher [" + item2.TypeName + "]: " + ex.Message);
					if (ex is ReflectionTypeLoadException ex2)
					{
						Logger.LogDebug(TypeLoader.TypeLoadExceptionToString(ex2));
					}
					else
					{
						Logger.LogDebug(ex.ToString());
					}
				}
			}
			AssemblyName name = assembly.GetName();
			Logger.Log(value.Any() ? LogLevel.Info : LogLevel.Debug, string.Format("Loaded {0} patcher method{1} from [{2} {3}]", value.Count, (value.Count == 1) ? "" : "s", name.Name, name.Version));
		}
		foreach (KeyValuePair<string, PatcherPlugin> item3 in sortedDictionary)
		{
			AddPatcher(item3.Value);
		}
	}

	private static void InitializePatchers()
	{
		foreach (PatcherPlugin item in PatcherPluginsSafe)
		{
			try
			{
				item.Initializer?.Invoke();
			}
			catch (Exception arg)
			{
				Logger.LogError($"Failed to run Initializer of {item.TypeName}: {arg}");
			}
		}
	}

	private static void FinalizePatching()
	{
		foreach (PatcherPlugin item in PatcherPluginsSafe)
		{
			try
			{
				item.Finalizer?.Invoke();
			}
			catch (Exception arg)
			{
				Logger.LogError($"Failed to run Finalizer of {item.TypeName}: {arg}");
			}
		}
	}

	public static void DisposePatchers()
	{
		PatcherPlugins.Clear();
	}

	private static string GetAssemblyName(string fullName)
	{
		if (!Utility.TryParseAssemblyName(fullName, out var assemblyName))
		{
			return fullName;
		}
		return assemblyName.Name;
	}

	public static void PatchAndLoad(params string[] directories)
	{
		Dictionary<string, AssemblyDefinition> dictionary = new Dictionary<string, AssemblyDefinition>(StringComparer.InvariantCultureIgnoreCase);
		foreach (string uniqueFilesInDirectory in Utility.GetUniqueFilesInDirectories(directories, "*.dll"))
		{
			AssemblyDefinition assemblyDefinition;
			try
			{
				assemblyDefinition = AssemblyDefinition.ReadAssembly(uniqueFilesInDirectory);
			}
			catch (BadImageFormatException)
			{
				continue;
			}
			if (assemblyDefinition.Name.Name == "System" || assemblyDefinition.Name.Name == "mscorlib")
			{
				assemblyDefinition.Dispose();
				continue;
			}
			if (UnityPatches.AssemblyLocations.ContainsKey(assemblyDefinition.FullName))
			{
				Logger.LogWarning("Tried to load duplicate assembly " + Path.GetFileName(uniqueFilesInDirectory) + " from Managed folder! Skipping...");
				continue;
			}
			dictionary.Add(Path.GetFileName(uniqueFilesInDirectory), assemblyDefinition);
			UnityPatches.AssemblyLocations.Add(assemblyDefinition.FullName, Path.GetFullPath(uniqueFilesInDirectory));
		}
		InitializePatchers();
		HashSet<string> patchedAssemblies = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
		Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
		foreach (PatcherPlugin item in PatcherPluginsSafe)
		{
			foreach (string item2 in item.TargetDLLs())
			{
				if (!dictionary.TryGetValue(item2, out var value) || hashSet.Contains(item2))
				{
					continue;
				}
				Logger.LogInfo("Patching [" + value.Name.Name + "] with [" + item.TypeName + "]");
				try
				{
					item.Patcher?.Invoke(ref value);
				}
				catch (Exception arg)
				{
					Logger.LogError($"Failed to run [{item.TypeName}] when patching [{value.Name.Name}]. This assembly will not be patched. Error: {arg}");
					patchedAssemblies.Remove(item2);
					hashSet.Add(item2);
					continue;
				}
				dictionary[item2] = value;
				patchedAssemblies.Add(item2);
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				for (int i = 0; i < assemblies.Length; i++)
				{
					string assemblyName = GetAssemblyName(assemblies[i].FullName);
					if (!dictionary2.ContainsKey(assemblyName))
					{
						dictionary2[assemblyName] = item.TypeName;
					}
				}
			}
		}
		HashSet<string> patchedAssemblyNames = new HashSet<string>(from kv in dictionary
			where patchedAssemblies.Contains(kv.Key)
			select kv.Value.Name.Name, StringComparer.InvariantCultureIgnoreCase);
		List<KeyValuePair<string, string>> list = dictionary2.Where((KeyValuePair<string, string> kv) => patchedAssemblyNames.Contains(kv.Key)).ToList();
		if (list.Count != 0)
		{
			Logger.LogWarning(new StringBuilder().AppendLine("The following assemblies have been loaded too early and will not be patched by preloader:").AppendLine(string.Join(Environment.NewLine, list.Select((KeyValuePair<string, string> kv) => "* [" + kv.Key + "] (first loaded by [" + kv.Value + "])").ToArray())).AppendLine("Expect unexpected behavior and issues with plugins and patchers not being loaded.")
				.ToString());
		}
		DumpedAssemblyPaths.Clear();
		if (ConfigDumpAssemblies.Value || ConfigLoadDumpedAssemblies.Value)
		{
			if (!Directory.Exists(DumpedAssembliesPath))
			{
				Directory.CreateDirectory(DumpedAssembliesPath);
			}
			foreach (KeyValuePair<string, AssemblyDefinition> item3 in dictionary)
			{
				string key = item3.Key;
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(key);
				string extension = Path.GetExtension(key);
				AssemblyDefinition value2 = item3.Value;
				if (!patchedAssemblies.Contains(key))
				{
					continue;
				}
				int num = 0;
				string text2;
				FileStream fileStream;
				while (true)
				{
					string text = ((num > 0) ? $"_{num}" : "");
					text2 = Path.Combine(DumpedAssembliesPath, fileNameWithoutExtension + text + extension);
					if (Utility.TryOpenFileStream(text2, FileMode.Create, out fileStream))
					{
						break;
					}
					num++;
				}
				value2.Write(fileStream);
				fileStream.Dispose();
				DumpedAssemblyPaths[key] = text2;
			}
		}
		if (ConfigBreakBeforeLoadAssemblies.Value)
		{
			Logger.LogInfo("BepInEx is about load the following assemblies:\n" + string.Join("\n", patchedAssemblies.ToArray()));
			Logger.LogInfo("The assemblies were dumped into " + DumpedAssembliesPath);
			Logger.LogInfo("Load any assemblies into the debugger, set breakpoints and continue execution.");
			Debugger.Break();
		}
		foreach (KeyValuePair<string, AssemblyDefinition> item4 in dictionary)
		{
			string key2 = item4.Key;
			AssemblyDefinition value3 = item4.Value;
			if (patchedAssemblies.Contains(key2))
			{
				Load(value3, key2);
			}
			value3.Dispose();
		}
		FinalizePatching();
	}

	public static void Load(AssemblyDefinition assembly, string filename)
	{
		if (ConfigLoadDumpedAssemblies.Value && DumpedAssemblyPaths.TryGetValue(filename, out var value))
		{
			Assembly.LoadFile(value);
			return;
		}
		using MemoryStream memoryStream = new MemoryStream();
		assembly.Write(memoryStream);
		Assembly.Load(memoryStream.ToArray());
	}
}
