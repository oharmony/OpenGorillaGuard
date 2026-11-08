using System;
using System.Collections;
using System.Collections.Generic;

namespace BepInEx.Logging;

public static class Logger
{
	private sealed class LogSourceCollection : ThreadSafeCollection<ILogSource>, ICollection<ILogSource>, IEnumerable<ILogSource>, IEnumerable
	{
		public override void Add(ILogSource item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item", "Log sources cannot be null when added to the source list.");
			}
			lock (Lock)
			{
				item.LogEvent += InternalLogEvent;
				List<ILogSource> list = new List<ILogSource>(BaseList.Count + 1);
				list.AddRange(BaseList);
				list.Add(item);
				BaseList = list;
			}
		}

		public override void Clear()
		{
			if (base.Count == 0)
			{
				return;
			}
			lock (Lock)
			{
				for (int i = 0; i < BaseList.Count; i++)
				{
					BaseList[i].LogEvent -= InternalLogEvent;
				}
				BaseList = new List<ILogSource>(0);
			}
		}

		public override bool Remove(ILogSource item)
		{
			if (item == null)
			{
				return false;
			}
			lock (Lock)
			{
				bool num = base.Remove(item);
				if (num)
				{
					item.LogEvent -= InternalLogEvent;
				}
				return num;
			}
		}
	}

	private sealed class LogListenerCollection : ThreadSafeCollection<ILogListener>
	{
		public void SendLogEvent(object sender, LogEventArgs eventArgs)
		{
			List<ILogListener> baseList = BaseList;
			for (int i = 0; i < baseList.Count; i++)
			{
				baseList[i].LogEvent(sender, eventArgs);
			}
		}
	}

	private class ThreadSafeCollection<T> : ICollection<T>, IEnumerable<T>, IEnumerable where T : class
	{
		protected readonly object Lock = new object();

		protected List<T> BaseList = new List<T>(0);

		public int Count => BaseList.Count;

		public bool IsReadOnly => false;

		public IEnumerator<T> GetEnumerator()
		{
			return BaseList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return BaseList.GetEnumerator();
		}

		public virtual void Add(T item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item", "item can't be null");
			}
			lock (Lock)
			{
				List<T> list = new List<T>(BaseList.Count + 1);
				list.AddRange(BaseList);
				list.Add(item);
				BaseList = list;
			}
		}

		public virtual void Clear()
		{
			if (Count == 0)
			{
				return;
			}
			lock (Lock)
			{
				BaseList = new List<T>(0);
			}
		}

		public bool Contains(T item)
		{
			return BaseList.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			BaseList.CopyTo(array, arrayIndex);
		}

		public virtual bool Remove(T item)
		{
			if (item == null)
			{
				return false;
			}
			lock (Lock)
			{
				List<T> list = new List<T>(BaseList.Count);
				bool result = false;
				for (int i = 0; i < BaseList.Count; i++)
				{
					T val = BaseList[i];
					if (val.Equals(item))
					{
						result = true;
					}
					else
					{
						list.Add(val);
					}
				}
				BaseList = list;
				return result;
			}
		}
	}

	private static readonly LogListenerCollection _Listeners = new LogListenerCollection();

	private static readonly ManualLogSource InternalLogSource = CreateLogSource("BepInEx");

	private static bool internalLogsInitialized;

	public static ICollection<ILogListener> Listeners => _Listeners;

	public static ICollection<ILogSource> Sources { get; } = new LogSourceCollection();

	internal static void InitializeInternalLoggers()
	{
		if (!internalLogsInitialized)
		{
			Sources.Add(new HarmonyLogSource());
			internalLogsInitialized = true;
		}
	}

	internal static void InternalLogEvent(object sender, LogEventArgs eventArgs)
	{
		_Listeners.SendLogEvent(sender, eventArgs);
	}

	internal static void Log(LogLevel level, object data)
	{
		InternalLogSource.Log(level, data);
	}

	internal static void LogFatal(object data)
	{
		Log(LogLevel.Fatal, data);
	}

	internal static void LogError(object data)
	{
		Log(LogLevel.Error, data);
	}

	internal static void LogWarning(object data)
	{
		Log(LogLevel.Warning, data);
	}

	internal static void LogMessage(object data)
	{
		Log(LogLevel.Message, data);
	}

	internal static void LogInfo(object data)
	{
		Log(LogLevel.Info, data);
	}

	internal static void LogDebug(object data)
	{
		Log(LogLevel.Debug, data);
	}

	public static ManualLogSource CreateLogSource(string sourceName)
	{
		ManualLogSource manualLogSource = new ManualLogSource(sourceName);
		Sources.Add(manualLogSource);
		return manualLogSource;
	}
}
