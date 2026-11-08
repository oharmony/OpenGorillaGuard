using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using GorillaGuard.Security;
using Microsoft.Win32.SafeHandles;

namespace GorillaGuard.Launcher;

internal sealed class KernelHealthClient : IDisposable
{
	private readonly SafeFileHandle _handle;

	private readonly KernelIdentity _owner;

	private KernelHealthClient()
	{
		using Process process = Process.GetCurrentProcess();
		_owner = A(process);
		_handle = CreateFileW(_"\\\\.\\GorillaGuardKernelDev", 2147483648u, 3u, IntPtr.Zero, 3u, 0u, IntPtr.Zero);
		if (_handle.IsInvalid)
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			_handle.Dispose();
			throw new Win32Exception(lastWin32Error, _"Required kernel protection is unavailable.");
		}
	}

	internal static KernelHealthClient? A(string P_0)
	{
		byte[] array = A(Path.Combine(P_0, _"GorillaGuard.manifest"), 1048576);
		if (!ReleaseVerifier.A(array, A(Path.Combine(P_0, _"GorillaGuard.manifest.sig"), 4096)))
		{
			throw new InvalidDataException(_"Kernel requirement policy signature is invalid.");
		}
		Dictionary<string, string> dictionary = (from P_0 in Encoding.UTF8.GetString(array).Split('\n', StringSplitOptions.RemoveEmptyEntries)
			select P_0.Trim().Split('=', 2) into P_0
			where P_0.Length == 2
			select P_0).ToDictionary<string[], string, string>((string[] P_0) => P_0[0], (string[] P_0) => P_0[1], StringComparer.Ordinal);
		if (!dictionary.TryGetValue(_"kernelProtocolVersion", out var value))
		{
			return null;
		}
		if (value != 1u.ToString() || !dictionary.TryGetValue(_"kernelDriverSha256", out var value2) || value2.Length != 64 || !value2.All(Uri.IsHexDigit))
		{
			throw new InvalidDataException(_"Required kernel release policy is unsupported.");
		}
		string text = Path.Combine(P_0, _"kernel", _"GorillaGuard.Kernel.sys");
		VrPreflight.e(text);
		if (!ReleaseVerifier.a(text).Equals(value2, StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidDataException(_"Kernel driver file differs from signed release policy.");
		}
		return new KernelHealthClient();
	}

	private static byte[] A(string P_0, int P_1)
	{
		using FileStream fileStream = new FileStream(P_0, FileMode.Open, FileAccess.Read, FileShare.Read);
		if (fileStream.Length <= 0 || fileStream.Length > P_1)
		{
			throw new InvalidDataException(_"Kernel policy file exceeds its allowed size.");
		}
		byte[] array = new byte[(int)fileStream.Length];
		fileStream.ReadExactly(array);
		return array;
	}

	internal KernelHealthSnapshot Verify(Process game, VrProcessIdentity? server, VrProcessIdentity? compositor)
	{
		ulong num = BinaryPrimitives.ReadUInt64LittleEndian(RandomNumberGenerator.GetBytes(8));
		if (num == 0L)
		{
			num = 1uL;
		}
		byte[] array = KernelHealthProtocol.A(num);
		byte[] array2 = new byte[96];
		if (!DeviceIoControl(_handle, 2252800u, array, (uint)array.Length, array2, (uint)array2.Length, out var returned, IntPtr.Zero))
		{
			throw new Win32Exception(Marshal.GetLastWin32Error(), _"Required kernel status query failed.");
		}
		if (returned != array2.Length)
		{
			throw new InvalidDataException(_"Incomplete kernel status reply.");
		}
		return KernelHealthProtocol.A(array2, num, _owner, new KernelIdentity[3]
		{
			A(game),
			A(server),
			A(compositor)
		});
	}

	private static KernelIdentity A(Process P_0)
	{
		return new KernelIdentity((ulong)P_0.Id, (ulong)P_0.StartTime.ToUniversalTime().ToFileTimeUtc());
	}

	private static KernelIdentity? A(VrProcessIdentity? P_0)
	{
		if (!(P_0 == null))
		{
			return new KernelIdentity((ulong)P_0.Pid, (ulong)new DateTime(P_0.StartedUtcTicks, DateTimeKind.Utc).ToFileTimeUtc());
		}
		return null;
	}

	public void Dispose()
	{
		_handle.Dispose();
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern SafeFileHandle CreateFileW(string name, uint access, uint share, nint security, uint creation, uint flags, nint template);

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool DeviceIoControl(SafeFileHandle device, uint code, byte[] input, uint inputSize, byte[] output, uint outputSize, out uint returned, nint overlapped);
}
