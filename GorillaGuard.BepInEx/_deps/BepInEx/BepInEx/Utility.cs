using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Mono.Cecil;

namespace BepInEx;

public static class Utility
{
	private static bool? sreEnabled;

	public static bool CLRSupportsDynamicAssemblies => CheckSRE();

	public static Encoding UTF8NoBom { get; } = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

	private static bool CheckSRE()
	{
		if (sreEnabled.HasValue)
		{
			return sreEnabled.Value;
		}
		try
		{
			new CustomAttributeBuilder(null, new object[0]);
		}
		catch (PlatformNotSupportedException)
		{
			sreEnabled = false;
			return sreEnabled.Value;
		}
		catch (ArgumentNullException)
		{
		}
		sreEnabled = true;
		return sreEnabled.Value;
	}

	public static bool TryDo(Action action, out Exception exception)
	{
		exception = null;
		try
		{
			action();
			return true;
		}
		catch (Exception ex)
		{
			exception = ex;
			return false;
		}
	}

	public static string CombinePaths(params string[] parts)
	{
		return parts.Aggregate(Path.Combine);
	}

	public static string ParentDirectory(string path, int levels = 1)
	{
		for (int i = 0; i < levels; i++)
		{
			path = Path.GetDirectoryName(path);
		}
		return path;
	}

	public static bool SafeParseBool(string input, bool defaultValue = false)
	{
		if (!bool.TryParse(input, out var result))
		{
			return defaultValue;
		}
		return result;
	}

	public static string ConvertToWWWFormat(string path)
	{
		return "file://" + path.Replace('\\', '/');
	}

	public static bool IsNullOrWhiteSpace(this string self)
	{
		return self?.All(char.IsWhiteSpace) ?? true;
	}

	public static IEnumerable<TNode> TopologicalSort<TNode>(IEnumerable<TNode> nodes, Func<TNode, IEnumerable<TNode>> dependencySelector)
	{
		List<TNode> sorted_list = new List<TNode>();
		HashSet<TNode> visited = new HashSet<TNode>();
		HashSet<TNode> sorted = new HashSet<TNode>();
		foreach (TNode node in nodes)
		{
			Stack<TNode> stack2 = new Stack<TNode>();
			if (!Visit(node, stack2))
			{
				throw new Exception("Cyclic Dependency:\r\n" + stack2.Select((TNode x) => $" - {x}").Aggregate((string a, string b) => a + "\r\n" + b));
			}
		}
		return sorted_list;
		bool Visit(TNode node, Stack<TNode> stack)
		{
			if (visited.Contains(node))
			{
				if (!sorted.Contains(node))
				{
					return false;
				}
			}
			else
			{
				visited.Add(node);
				stack.Push(node);
				foreach (TNode item in dependencySelector(node))
				{
					if (!Visit(item, stack))
					{
						return false;
					}
				}
				sorted.Add(node);
				sorted_list.Add(node);
				stack.Pop();
			}
			return true;
		}
	}

	private static bool TryResolveDllAssembly<T>(AssemblyName assemblyName, string directory, Func<string, T> loader, out T assembly) where T : class
	{
		assembly = null;
		List<string> list = new List<string>();
		list.Add(directory);
		list.AddRange(Directory.GetDirectories(directory, "*", SearchOption.AllDirectories));
		foreach (string item in list)
		{
			string text = Path.Combine(item, assemblyName.Name + ".dll");
			if (File.Exists(text))
			{
				try
				{
					assembly = loader(text);
				}
				catch (Exception)
				{
					continue;
				}
				return true;
			}
		}
		return false;
	}

	public static bool IsSubtypeOf(this TypeDefinition self, Type td)
	{
		if (self.FullName == td.FullName)
		{
			return true;
		}
		if (self.FullName != "System.Object")
		{
			return self.BaseType?.Resolve()?.IsSubtypeOf(td) == true;
		}
		return false;
	}

	public static bool TryResolveDllAssembly(AssemblyName assemblyName, string directory, out Assembly assembly)
	{
		return TryResolveDllAssembly(assemblyName, directory, Assembly.LoadFile, out assembly);
	}

	public static bool TryResolveDllAssembly(AssemblyName assemblyName, string directory, ReaderParameters readerParameters, out AssemblyDefinition assembly)
	{
		return TryResolveDllAssembly(assemblyName, directory, (string s) => AssemblyDefinition.ReadAssembly(s, readerParameters), out assembly);
	}

	public static bool TryOpenFileStream(string path, FileMode mode, out FileStream fileStream, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.Read)
	{
		try
		{
			fileStream = new FileStream(path, mode, access, share);
			return true;
		}
		catch (IOException)
		{
			fileStream = null;
			return false;
		}
	}

	public static bool TryParseAssemblyName(string fullName, out AssemblyName assemblyName)
	{
		try
		{
			assemblyName = new AssemblyName(fullName);
			return true;
		}
		catch (Exception)
		{
			assemblyName = null;
			return false;
		}
	}

	public static IEnumerable<string> GetUniqueFilesInDirectories(IEnumerable<string> directories, string pattern = "*")
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
		foreach (string directory in directories)
		{
			string[] files = Directory.GetFiles(directory, pattern);
			foreach (string text in files)
			{
				string fileName = Path.GetFileName(text);
				if (!dictionary.ContainsKey(fileName))
				{
					dictionary[fileName] = text;
				}
			}
		}
		return dictionary.Values;
	}
}
