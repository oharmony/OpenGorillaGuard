using System;
using BepInEx.Configuration;

namespace BepInEx.Logging;

public class ConsoleLogListener : ILogListener, IDisposable
{
	private static readonly ConfigEntry<LogLevel> ConfigConsoleDisplayedLevel = ConfigFile.CoreConfig.Bind("Logging.Console", "LogLevels", LogLevel.Fatal | LogLevel.Error | LogLevel.Warning | LogLevel.Message | LogLevel.Info, "Which log levels to show in the console output.");

	internal bool WriteUnityLogs { get; set; } = true;

	public void LogEvent(object sender, LogEventArgs eventArgs)
	{
		if ((WriteUnityLogs || !(sender is UnityLogSource)) && (eventArgs.Level & ConfigConsoleDisplayedLevel.Value) != 0)
		{
			ConsoleManager.SetConsoleColor(eventArgs.Level.GetConsoleColor());
			ConsoleManager.ConsoleStream?.Write(eventArgs.ToStringLine());
			ConsoleManager.SetConsoleColor(ConsoleColor.Gray);
		}
	}

	public void Dispose()
	{
	}
}
