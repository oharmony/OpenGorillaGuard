using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace BepInEx.Preloader;

public class PreloaderConsoleListener : ILogListener, IDisposable
{
	private static readonly ConfigEntry<LogLevel> ConfigConsoleDisplayedLevel = ConfigFile.CoreConfig.Bind("Logging.Console", "LogLevels", LogLevel.Fatal | LogLevel.Error | LogLevel.Warning | LogLevel.Message | LogLevel.Info, "Which log levels to show in the console output.");

	public static List<LogEventArgs> LogEvents { get; } = new List<LogEventArgs>();

	public void LogEvent(object sender, LogEventArgs eventArgs)
	{
		if ((eventArgs.Level & ConfigConsoleDisplayedLevel.Value) != 0)
		{
			LogEvents.Add(eventArgs);
		}
	}

	public void Dispose()
	{
	}
}
