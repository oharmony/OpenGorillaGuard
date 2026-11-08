using System;
using System.Reflection;
using UnityEngine;

namespace BepInEx.Logging;

public class UnityLogSource : ILogSource, IDisposable
{
	private bool disposed;

	public string SourceName { get; } = "Unity Log";

	public event EventHandler<LogEventArgs> LogEvent;

	private static event EventHandler<LogEventArgs> InternalUnityLogMessage;

	public UnityLogSource()
	{
		InternalUnityLogMessage += UnityLogMessageHandler;
	}

	private void UnityLogMessageHandler(object sender, LogEventArgs eventArgs)
	{
		LogEventArgs e = new LogEventArgs(eventArgs.Data, eventArgs.Level, this);
		this.LogEvent?.Invoke(this, e);
	}

	public void Dispose()
	{
		if (!disposed)
		{
			InternalUnityLogMessage -= UnityLogMessageHandler;
			disposed = true;
		}
	}

	static UnityLogSource()
	{

		LogCallback val = new LogCallback(OnUnityLogMessageReceived);
		EventInfo @event = typeof(Application).GetEvent("logMessageReceived", BindingFlags.Static | BindingFlags.Public);
		if (@event != null)
		{
			@event.AddEventHandler(null, (Delegate)(object)val);
			return;
		}
		typeof(Application).GetMethod("RegisterLogCallback", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[1] { val });
	}

	private static void OnUnityLogMessageReceived(string message, string stackTrace, LogType type)
	{

		LogLevel level;
		switch ((int)type)
		{
		case 0:
		case 1:
		case 4:
			level = LogLevel.Error;
			break;
		case 2:
			level = LogLevel.Warning;
			break;
		default:
			level = LogLevel.Info;
			break;
		}
		if ((int)type == 4)
		{
			message = message + "\nStack trace:\n" + stackTrace;
		}
		UnityLogSource.InternalUnityLogMessage?.Invoke(null, new LogEventArgs(message, level, null));
	}
}
