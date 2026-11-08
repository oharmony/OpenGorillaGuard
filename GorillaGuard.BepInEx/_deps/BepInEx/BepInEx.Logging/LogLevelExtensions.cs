using System;

namespace BepInEx.Logging;

public static class LogLevelExtensions
{

	public static LogLevel GetHighestLevel(this LogLevel levels)
	{
		Array values = Enum.GetValues(typeof(LogLevel));
		Array.Sort(values);
		foreach (LogLevel item in values)
		{
			if ((levels & item) != 0)
			{
				return item;
			}
		}
		return LogLevel.None;
	}

	public static ConsoleColor GetConsoleColor(this LogLevel level)
	{
		level = level.GetHighestLevel();
		return level switch
		{
			LogLevel.Fatal => ConsoleColor.Red,
			LogLevel.Error => ConsoleColor.DarkRed,
			LogLevel.Warning => ConsoleColor.Yellow,
			LogLevel.Message => ConsoleColor.White,
			LogLevel.Debug => ConsoleColor.DarkGray,
			_ => ConsoleColor.Gray,
		};
	}
}
