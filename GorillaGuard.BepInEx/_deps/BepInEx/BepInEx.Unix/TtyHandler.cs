using System;
using System.IO;

namespace BepInEx.Unix;

internal static class TtyHandler
{
	private static readonly string[] ncursesLocations = new string[4] { "/usr/share/terminfo", "/etc/terminfo", "/usr/lib/terminfo", "/lib/terminfo" };

	private static string TryTermInfoDir(string dir, string term)
	{
		string text = $"{dir}/{(int)term[0]:x}/{term}";
		if (File.Exists(text))
		{
			return text;
		}
		text = Utility.CombinePaths(dir, term.Substring(0, 1), term);
		if (File.Exists(text))
		{
			return text;
		}
		return null;
	}

	private static string FindTermInfoPath(string term)
	{
		if (string.IsNullOrEmpty(term))
		{
			return null;
		}
		string environmentVariable = Environment.GetEnvironmentVariable("TERMINFO");
		if (environmentVariable != null && Directory.Exists(environmentVariable))
		{
			string text = TryTermInfoDir(environmentVariable, term);
			if (text != null)
			{
				return text;
			}
		}
		string[] array = ncursesLocations;
		foreach (string text2 in array)
		{
			if (Directory.Exists(text2))
			{
				string text3 = TryTermInfoDir(text2, term);
				if (text3 != null)
				{
					return text3;
				}
			}
		}
		return null;
	}

	public static TtyInfo GetTtyInfo(string terminal = null)
	{
		terminal = terminal ?? Environment.GetEnvironmentVariable("TERM");
		string text = FindTermInfoPath(terminal);
		if (text == null)
		{
			return TtyInfo.Default;
		}
		TtyInfo ttyInfo = TtyInfoParser.Parse(File.ReadAllBytes(text));
		ttyInfo.TerminalType = terminal;
		return ttyInfo;
	}
}
