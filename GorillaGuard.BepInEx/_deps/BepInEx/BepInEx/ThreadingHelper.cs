using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using UnityEngine;

namespace BepInEx;

public sealed class ThreadingHelper : MonoBehaviour, ISynchronizeInvoke
{
	private sealed class InvokeResult : IAsyncResult
	{
		internal bool ExceptionThrown;

		public bool IsCompleted { get; private set; }

		public WaitHandle AsyncWaitHandle { get; }

		public object AsyncState { get; private set; }

		public bool CompletedSynchronously { get; private set; }

		public InvokeResult()
		{
			AsyncWaitHandle = new EventWaitHandle(initialState: false, EventResetMode.ManualReset);
		}

		public void Finish(object result, bool completedSynchronously)
		{
			AsyncState = result;
			CompletedSynchronously = completedSynchronously;
			IsCompleted = true;
			((EventWaitHandle)AsyncWaitHandle).Set();
		}
	}

	private readonly object _invokeLock = new object();

	private Action _invokeList;

	private Thread _mainThread;

	public static ThreadingHelper Instance { get; private set; }

	public static ISynchronizeInvoke SynchronizingObject => Instance;

	public bool InvokeRequired
	{
		get
		{
			if (_mainThread != null)
			{
				return _mainThread != Thread.CurrentThread;
			}
			return true;
		}
	}

	internal static void Initialize()
	{

		GameObject val = new GameObject("BepInEx_ThreadingHelper");
		if (Chainloader.ConfigHideBepInExGOs.Value)
		{
			((Object)val).hideFlags = (HideFlags)61;
		}
		Object.DontDestroyOnLoad((Object)(object)val);
		Instance = val.AddComponent<ThreadingHelper>();
	}

	public void StartSyncInvoke(Action action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		lock (_invokeLock)
		{
			_invokeList = (Action)Delegate.Combine(_invokeList, action);
		}
	}

	private void Update()
	{
		if (_mainThread == null)
		{
			_mainThread = Thread.CurrentThread;
		}
		if (_invokeList == null)
		{
			return;
		}
		Action invokeList;
		lock (_invokeLock)
		{
			invokeList = _invokeList;
			_invokeList = null;
		}
		foreach (Action item in invokeList.GetInvocationList().Cast<Action>())
		{
			try
			{
				item();
			}
			catch (Exception ex)
			{
				LogInvocationException(ex);
			}
		}
	}

	public void StartAsyncInvoke(Func<Action> action)
	{
		if (!ThreadPool.QueueUserWorkItem(DoWork))
		{
			throw new NotSupportedException("Failed to queue the action on ThreadPool");
		}
		void DoWork(object _)
		{
			try
			{
				Action action2 = action();
				if (action2 != null)
				{
					StartSyncInvoke(action2);
				}
			}
			catch (Exception ex)
			{
				LogInvocationException(ex);
			}
		}
	}

	private static void LogInvocationException(Exception ex)
	{
		Logger.Log(LogLevel.Error, ex);
		if (ex.InnerException != null)
		{
			Logger.Log(LogLevel.Error, "INNER: " + ex.InnerException);
		}
	}

	IAsyncResult ISynchronizeInvoke.BeginInvoke(Delegate method, object[] args)
	{
		InvokeResult result = new InvokeResult();
		if (!InvokeRequired)
		{
			result.Finish(Invoke(), completedSynchronously: true);
		}
		else
		{
			StartSyncInvoke(delegate
			{
				result.Finish(Invoke(), completedSynchronously: false);
			});
		}
		return result;
		object Invoke()
		{
			try
			{
				return method.DynamicInvoke(args);
			}
			catch (Exception result2)
			{
				result.ExceptionThrown = true;
				return result2;
			}
		}
	}

	object ISynchronizeInvoke.EndInvoke(IAsyncResult result)
	{
		InvokeResult invokeResult = (InvokeResult)result;
		invokeResult.AsyncWaitHandle.WaitOne();
		if (invokeResult.ExceptionThrown)
		{
			throw (Exception)invokeResult.AsyncState;
		}
		return invokeResult.AsyncState;
	}

	object ISynchronizeInvoke.Invoke(Delegate method, object[] args)
	{
		IAsyncResult result = ((ISynchronizeInvoke)this).BeginInvoke(method, args);
		return ((ISynchronizeInvoke)this).EndInvoke(result);
	}
}
