using System;
using System.IO;
using System.Threading;

namespace BepInEx.Logging;

public class DiskLogListener : ILogListener, IDisposable
{

	public LogLevel DisplayedLogLevel { get; set; }

	public TextWriter LogWriter { get; protected set; }

	public Timer FlushTimer { get; protected set; }

	public bool WriteFromUnityLog { get; set; }

	public DiskLogListener(string localPath, LogLevel displayedLogLevel = LogLevel.Info, bool appendLog = false, bool includeUnityLog = false)
	{
		WriteFromUnityLog = includeUnityLog;
		DisplayedLogLevel = displayedLogLevel;
		int num = 1;
		FileStream fileStream;
		while (!Utility.TryOpenFileStream(Path.Combine(Paths.BepInExRootPath, localPath), appendLog ? FileMode.Append : FileMode.Create, out fileStream, FileAccess.Write))
		{
			if (num == 5)
			{
				Logger.LogError("Couldn't open a log file for writing. Skipping log file creation");
				return;
			}
			Logger.LogWarning("Couldn't open log file '" + localPath + "' for writing, trying another...");
			localPath = $"LogOutput.log.{num++}";
		}
		LogWriter = TextWriter.Synchronized(new StreamWriter(fileStream, Utility.UTF8NoBom));
		FlushTimer = new Timer(delegate
		{
			LogWriter?.Flush();
		}, null, 2000, 2000);
	}

	public void LogEvent(object sender, LogEventArgs eventArgs)
	{
		if ((WriteFromUnityLog || !(eventArgs.Source is UnityLogSource)) && (eventArgs.Level & DisplayedLogLevel) != 0)
		{
			LogWriter.WriteLine(eventArgs.ToString());
		}
	}

	public void Dispose()
	{
		FlushTimer?.Dispose();
		LogWriter?.Flush();
		LogWriter?.Dispose();
	}

	~DiskLogListener()
	{
		Dispose();
	}
}
