using System;
using System.Collections.Generic;
using System.IO;

namespace GorillaGuard.Security;

internal static class PrelaunchGuard
{
	internal static void A(string P_0, string P_1)
	{
		string fullPath = Path.GetFullPath(P_0);
		string text = Path.Combine(fullPath, P_1);
		if (!File.Exists(text))
		{
			throw new InvalidDataException(_"Required signed code file is missing: " + text);
		}
		Stack<string> stack = new Stack<string>();
		stack.Push(fullPath);
		while (stack.Count > 0)
		{
			string text2 = stack.Pop();
			if ((File.GetAttributes(text2) & FileAttributes.ReparsePoint) != 0)
			{
				throw new InvalidDataException(_"Redirected pre-launch code directory: " + text2);
			}
			string[] directories = Directory.GetDirectories(text2);
			foreach (string item in directories)
			{
				stack.Push(item);
			}
			directories = Directory.GetFiles(text2);
			foreach (string text3 in directories)
			{
				string extension = Path.GetExtension(text3);
				if ((extension.Equals(_".dll", StringComparison.OrdinalIgnoreCase) || extension.Equals(_".exe", StringComparison.OrdinalIgnoreCase) || extension.Equals(_".winmd", StringComparison.OrdinalIgnoreCase) || extension.Equals(_".so", StringComparison.OrdinalIgnoreCase)) && ((File.GetAttributes(text3) & FileAttributes.ReparsePoint) != 0 || !string.Equals(Path.GetFullPath(text3), text, StringComparison.OrdinalIgnoreCase)))
				{
					throw new InvalidDataException(_"Unapproved code remained before launch: " + text3);
				}
			}
		}
	}
}
