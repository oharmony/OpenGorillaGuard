using System.Runtime.CompilerServices;

namespace UnityEngine;

internal sealed class UnityLogWriter
{
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern void WriteStringToUnityLogImpl(string s);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern void WriteStringToUnityLog(string s);
}
