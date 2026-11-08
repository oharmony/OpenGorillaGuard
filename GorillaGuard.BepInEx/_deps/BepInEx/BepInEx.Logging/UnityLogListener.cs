using System;
using System.Reflection;
using System.Text;
using BepInEx.Configuration;
using UnityEngine;

namespace BepInEx.Logging;

public class UnityLogListener : ILogListener, IDisposable
{
	internal static readonly Action<string> WriteStringToUnityLog;

	private ConfigEntry<bool> LogConsoleToUnity = ConfigFile.CoreConfig.Bind("Logging", "LogConsoleToUnityLog", defaultValue: false, new StringBuilder().AppendLine("If enabled, writes Standard Output messages to Unity log").AppendLine("NOTE: By default, Unity does so automatically. Only use this option if no console messages are visible in Unity log").ToString());

	static UnityLogListener()
	{
		MethodInfo[] methods = typeof(UnityLogWriter).GetMethods(BindingFlags.Static | BindingFlags.Public);
		foreach (MethodInfo methodInfo in methods)
		{
			try
			{
				methodInfo.Invoke(null, new object[1] { "" });
			}
			catch
			{
				continue;
			}
			WriteStringToUnityLog = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), methodInfo);
			break;
		}
		if (WriteStringToUnityLog == null)
		{
			Logger.LogError("Unable to start Unity log writer");
		}
	}

	public void LogEvent(object sender, LogEventArgs eventArgs)
	{
		if (!(eventArgs.Source is UnityLogSource) && (LogConsoleToUnity.Value || eventArgs.Source.SourceName != "Console"))
		{
			WriteStringToUnityLog?.Invoke(eventArgs.ToStringLine());
		}
	}

	public void Dispose()
	{
	}
}
