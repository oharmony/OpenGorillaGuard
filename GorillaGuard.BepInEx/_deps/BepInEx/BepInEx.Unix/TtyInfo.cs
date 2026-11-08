using System;

namespace BepInEx.Unix;

internal class TtyInfo
{
	public string TerminalType { get; set; } = "default";

	public int MaxColors { get; set; }

	public string[] ForegroundColorStrings { get; set; }

	public static TtyInfo Default { get; } = new TtyInfo
	{
		MaxColors = 0
	};

	public string GetAnsiCode(ConsoleColor color)
	{
		if (MaxColors <= 0 || ForegroundColorStrings == null)
		{
			return string.Empty;
		}
		int num = (int)color % MaxColors;
		return ForegroundColorStrings[num];
	}
}
