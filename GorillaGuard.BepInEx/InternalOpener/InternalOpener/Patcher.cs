using System;
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
