
#pragma region Properties/AssemblyInfo.cs
﻿using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
#pragma endregion

#pragma region InternalOpener/HarmonyPatches.cs
﻿using System.Reflection;
using HarmonyLib;

namespace InternalOpener;

public class HarmonyPatches
{
	private static Harmony instance;

	public const string InstanceId = "com.jacksonjonjr.gorillatag.internalopener";

	public static bool IsPatched { get; private set; }

	internal static void ApplyHarmonyPatches()
	{
		if (!IsPatched)
		{
			if (instance == null)
			{
				instance = new Harmony("com.jacksonjonjr.gorillatag.internalopener");
			}
			instance.PatchAll(Assembly.GetExecutingAssembly());
			IsPatched = true;
		}
	}

	internal static void RemoveHarmonyPatches()
	{
		if (instance != null && IsPatched)
		{
			instance.UnpatchSelf();
			IsPatched = false;
		}
	}
}
#pragma endregion

#pragma region InternalOpener/Patcher.cs
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using Mono.Cecil;

namespace InternalOpener;

public static class Patcher
{
	public static List<string> assemblyNames = new List<string>();

	public static IEnumerable<string> TargetDLLs { get; } = new string[1] { "Assembly-CSharp.dll" };

	public static void FindAssemblyNames(string path)
	{
		if (!Directory.Exists(path))
		{
			throw new ArgumentException("Invalid directory: " + path);
		}
		string[] files = Directory.GetFiles(path, "*.dll");
		string[] array = files;
		foreach (string assemblyFile in array)
		{
			try
			{
				AssemblyName assemblyName = AssemblyName.GetAssemblyName(assemblyFile);
				assemblyNames.Add(assemblyName.Name);
			}
			catch (BadImageFormatException)
			{
			}
		}
		string[] directories = Directory.GetDirectories(path);
		string[] array2 = directories;
		foreach (string path2 in array2)
		{
			FindAssemblyNames(path2);
		}
	}

	public static void Patch(ref AssemblyDefinition assembly)
	{
		Console.WriteLine("TESTTIME");
		FindAssemblyNames(Paths.PluginPath);
		foreach (string assemblyName in assemblyNames)
		{
			Console.WriteLine(assemblyName);
			MethodReference constructor = assembly.MainModule.ImportReference(typeof(InternalsVisibleToAttribute).GetConstructor(new Type[1] { typeof(string) }));
			CustomAttribute customAttribute = new CustomAttribute(constructor);
			customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(assembly.MainModule.TypeSystem.String, assemblyName));
			assembly.CustomAttributes.Add(customAttribute);
		}
	}
}
#pragma endregion

#pragma region InternalOpener/Plugin.cs
﻿using BepInEx;

namespace InternalOpener;

[BepInPlugin("com.jacksonjonjr.gorillatag.internalopener", "InternalOpener", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
}
#pragma endregion

#pragma region InternalOpener/PluginInfo.cs
﻿namespace InternalOpener;

internal class PluginInfo
{
	public const string GUID = "com.jacksonjonjr.gorillatag.internalopener";

	public const string Name = "InternalOpener";

	public const string Version = "1.0.0";
}
#pragma endregion
