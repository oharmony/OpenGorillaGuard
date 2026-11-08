using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GorillaGuard.Launcher;

internal sealed class ProcessHandleMonitor
{
	internal const uint WriteMemory = 32u;

	internal const uint MemoryOperation = 8u;

	internal const uint CreateThread = 2u;

	internal static bool A(uint P_0)
	{
		if ((P_0 & 0x28) != 40)
		{
			return (P_0 & 0x22) == 34;
		}
		return true;
	}

	internal IReadOnlyList<string> A(int P_0)
	{
		if (!Environment.Is64BitProcess)
		{
			throw new NotSupportedException(_"Handle inspection requires x64.");
		}
		nint num = IntPtr.Zero;
		nint num2 = OpenProcess(4096u, inherit: false, (uint)Environment.ProcessId);
		if (num2 == IntPtr.Zero)
		{
			throw new IOException(_"Cannot establish process object type.");
		}
		nint num3 = OpenProcess(4096u, inherit: false, (uint)P_0);
		if (num3 == IntPtr.Zero)
		{
			CloseHandle(num2);
			throw new IOException(_"Cannot open observation target.");
		}
		try
		{
			int num4 = 1048576;
			int length;
			while (true)
			{
				num = Marshal.AllocHGlobal(num4);
				int num5 = NtQuerySystemInformation(64, num, num4, out length);
				if (num5 == 0)
				{
					break;
				}
				Marshal.FreeHGlobal(num);
				num = IntPtr.Zero;
				if (num5 != -1073741820 || num4 >= 67108864)
				{
					throw new IOException(_"Handle snapshot unavailable: NTSTATUS=" + num5.ToString(_"X8"));
				}
				num4 = Math.Min(67108864, Math.Max(num4 * 2, length));
			}
			long num6 = Math.Min(Marshal.ReadInt64(num), ((long)Math.Min(length, num4) - 16L) / 40);
			List<string> list = new List<string>();
			int num7 = -1;
			nint num8 = IntPtr.Zero;
			for (long num9 = 0L; num9 < num6; num9++)
			{
				nint ptr = IntPtr.Add(num, checked(16 + (int)(num9 * 40)));
				if (Marshal.ReadInt64(ptr, 8) == Environment.ProcessId && Marshal.ReadIntPtr(ptr, 16) == num2)
				{
					num7 = (ushort)Marshal.ReadInt16(ptr, 30);
				}
				if (Marshal.ReadInt64(ptr, 8) == Environment.ProcessId && Marshal.ReadIntPtr(ptr, 16) == num3)
				{
					num8 = Marshal.ReadIntPtr(ptr);
				}
			}
			if (num7 < 0)
			{
				throw new IOException(_"Process object type unavailable; observation incomplete.");
			}
			if (num8 == IntPtr.Zero)
			{
				throw new IOException(_"Windows restricted target-object visibility; observation unavailable.");
			}
			HashSet<uint> hashSet = new HashSet<uint>();
			int num10 = 0;
			for (long num11 = 0L; num11 < num6; num11++)
			{
				nint ptr2 = IntPtr.Add(num, checked(16 + (int)(num11 * 40)));
				if ((ushort)Marshal.ReadInt16(ptr2, 30) != num7 || Marshal.ReadIntPtr(ptr2) != num8)
				{
					continue;
				}
				long num12 = Marshal.ReadInt64(ptr2, 8);
				if (num12 <= 4 || num12 > int.MaxValue || num12 == Environment.ProcessId || num12 == P_0)
				{
					continue;
				}
				uint num13 = (uint)Marshal.ReadInt32(ptr2, 24);
				if (!A(num13))
				{
					continue;
				}
				if (++num10 > 2048)
				{
					throw new IOException(_"Handle inspection candidate limit exceeded; observation incomplete.");
				}
				uint num14 = (uint)num12;
				nint num15 = OpenProcess(64u, inherit: false, num14);
				if (num15 == IntPtr.Zero)
				{
					continue;
				}
				try
				{
					if (!DuplicateHandle(num15, Marshal.ReadIntPtr(ptr2, 16), GetCurrentProcess(), out var copy, 0u, inherit: false, 2u))
					{
						continue;
					}
					try
					{
						if (GetProcessId(copy) != P_0 || !hashSet.Add(num14))
						{
							continue;
						}
						string value;
						try
						{
							using Process process = Process.GetProcessById((int)num14);
							value = process.ProcessName;
						}
						catch
						{
							value = _"unknown";
						}
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(35, 4);
						defaultInterpolatedStringHandler.AppendLiteral(_"pid=");
						defaultInterpolatedStringHandler.AppendFormatted(num14);
						defaultInterpolatedStringHandler.AppendLiteral(_"; process=");
						defaultInterpolatedStringHandler.AppendFormatted(value);
						defaultInterpolatedStringHandler.AppendLiteral(_"; access=0x");
						defaultInterpolatedStringHandler.AppendFormatted(num13, _"X8");
						defaultInterpolatedStringHandler.AppendLiteral(_"; gamePid=");
						defaultInterpolatedStringHandler.AppendFormatted(P_0);
						list.Add(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					finally
					{
						CloseHandle(copy);
					}
				}
				finally
				{
					CloseHandle(num15);
				}
			}
			return list;
		}
		finally
		{
			if (num != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(num);
			}
			CloseHandle(num2);
			CloseHandle(num3);
		}
	}

	[DllImport("ntdll.dll")]
	private static extern int NtQuerySystemInformation(int kind, nint buffer, int size, out int length);

	[DllImport("kernel32.dll")]
	private static extern nint OpenProcess(uint access, bool inherit, uint pid);

	[DllImport("kernel32.dll")]
	private static extern nint GetCurrentProcess();

	[DllImport("kernel32.dll")]
	private static extern uint GetProcessId(nint handle);

	[DllImport("kernel32.dll")]
	private static extern bool DuplicateHandle(nint source, nint handle, nint target, out nint copy, uint access, bool inherit, uint options);

	[DllImport("kernel32.dll")]
	private static extern bool CloseHandle(nint handle);
}
