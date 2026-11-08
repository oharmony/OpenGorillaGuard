using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace GorillaGuard;

internal sealed class NativeLoadNotifications : IDisposable
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate void Notification(uint reason, IntPtr data, IntPtr context);

	private readonly Notification callback;

	private IntPtr cookie;

	private int pending;

	internal NativeLoadNotifications()
	{
		callback = OnDllNotification;
		int num = LdrRegisterDllNotification(0u, callback, IntPtr.Zero, out cookie);
		if (num != 0)
		{
			throw new InvalidOperationException("DLL notifications unavailable: NTSTATUS=" + num.ToString("X8"));
		}
	}

	private void OnDllNotification(uint reason, IntPtr data, IntPtr context)
	{
		if (reason == 1 || reason == 2)
		{
			Interlocked.Exchange(ref pending, 1);
		}
	}

	internal bool ConsumePendingNotification()
	{
		return Interlocked.Exchange(ref pending, 0) != 0;
	}

	public void Dispose()
	{
		if (!(cookie == IntPtr.Zero))
		{
			if (LdrUnregisterDllNotification(cookie) != 0)
			{
				GCHandle.Alloc(callback);
			}
			cookie = IntPtr.Zero;
			GC.KeepAlive(callback);
		}
	}

	[DllImport("ntdll.dll")]
	private static extern int LdrRegisterDllNotification(uint flags, Notification callback, IntPtr context, out IntPtr cookie);

	[DllImport("ntdll.dll")]
	private static extern int LdrUnregisterDllNotification(IntPtr cookie);
}
