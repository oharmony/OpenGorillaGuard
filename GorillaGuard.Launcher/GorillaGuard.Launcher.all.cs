
#pragma region GorillaGuard.Launcher/Authenticode.cs
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace GorillaGuard.Launcher;

internal static class Authenticode
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct WinTrustFile
	{
		public uint Size;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string Path;

		public nint Handle;

		public nint Subject;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct WinTrustData
	{
		public uint Size;

		public nint PolicyCallback;

		public nint SipClient;

		public uint UiChoice;

		public uint RevocationChecks;

		public uint UnionChoice;

		public nint File;

		public uint StateAction;

		public nint StateData;

		public nint UrlReference;

		public uint ProviderFlags;

		public uint UiContext;

		public nint SignatureSettings;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct CatalogInfo
	{
		public uint Size;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string Path;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct CatalogSubject
	{
		public uint Size;

		public uint Version;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string CatalogPath;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string MemberTag;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string MemberPath;

		public nint MemberFile;

		public nint CalculatedHash;

		public uint HashSize;

		public nint CatalogContext;

		public nint Admin;
	}

	internal static string A(string P_0)
	{
		WinTrustFile winTrustFile = default(WinTrustFile);
		winTrustFile.Size = (uint)Marshal.SizeOf<WinTrustFile>();
		winTrustFile.Path = P_0;
		WinTrustFile structure = winTrustFile;
		nint num = Marshal.AllocHGlobal(Marshal.SizeOf<WinTrustFile>());
		bool flag = false;
		try
		{
			Marshal.StructureToPtr(structure, num, fDeleteOld: false);
			flag = true;
			WinTrustData winTrustData = default(WinTrustData);
			winTrustData.Size = (uint)Marshal.SizeOf<WinTrustData>();
			winTrustData.UiChoice = 2u;
			winTrustData.UnionChoice = 1u;
			winTrustData.File = num;
			winTrustData.StateAction = 1u;
			winTrustData.ProviderFlags = 4096u;
			WinTrustData data = winTrustData;
			Guid action = new Guid(_"00AAC56B-CD44-11d0-8CC2-00C04FC295EE");
			int num2;
			try
			{
				num2 = WinVerifyTrust(new IntPtr(-1), ref action, ref data);
			}
			finally
			{
				data.StateAction = 2u;
				WinVerifyTrust(new IntPtr(-1), ref action, ref data);
			}
			if (num2 != 0)
			{
				string[] array = new string[2]
				{
					_"SHA256",
					_"SHA1"
				};
				foreach (string text in array)
				{
					string text2 = A(P_0, text);
					if (text2 != null)
					{
						return text2;
					}
				}
				throw new InvalidDataException(_"VR binary has no trusted embedded or catalog signature (0x" + num2.ToString(_"X8") + _"): " + P_0);
			}
			using X509Certificate2 x509Certificate = new X509Certificate2(X509Certificate.CreateFromSignedFile(P_0));
			return x509Certificate.GetNameInfo(X509NameType.SimpleName, forIssuer: false);
		}
		finally
		{
			if (flag)
			{
				Marshal.DestroyStructure<WinTrustFile>(num);
			}
			Marshal.FreeHGlobal(num);
		}
	}

	private static string? A(string P_0, string P_1)
	{
		if (!CryptCATAdminAcquireContext2(out var admin, IntPtr.Zero, P_1, IntPtr.Zero, 0u))
		{
			return null;
		}
		nint num = IntPtr.Zero;
		try
		{
			using FileStream fileStream = new FileStream(P_0, FileMode.Open, FileAccess.Read, FileShare.Read);
			nint num2 = fileStream.SafeFileHandle.DangerousGetHandle();
			uint size = 0u;
			CryptCATAdminCalcHashFromFileHandle2(admin, num2, ref size, null, 0u);
			if (size == 0 || size > 128)
			{
				return null;
			}
			byte[] array = new byte[size];
			if (!CryptCATAdminCalcHashFromFileHandle2(admin, num2, ref size, array, 0u))
			{
				return null;
			}
			num = CryptCATAdminEnumCatalogFromHash(admin, array, size, 0u, IntPtr.Zero);
			if (num == IntPtr.Zero)
			{
				return null;
			}
			CatalogInfo catalogInfo = default(CatalogInfo);
			catalogInfo.Size = (uint)Marshal.SizeOf<CatalogInfo>();
			CatalogInfo info = catalogInfo;
			if (!CryptCATCatalogInfoFromContext(num, ref info, 0u))
			{
				return null;
			}
			CatalogSubject catalogSubject = default(CatalogSubject);
			catalogSubject.Size = (uint)Marshal.SizeOf<CatalogSubject>();
			catalogSubject.CatalogPath = info.Path;
			catalogSubject.MemberTag = Convert.ToHexString(array);
			catalogSubject.MemberPath = P_0;
			catalogSubject.MemberFile = num2;
			catalogSubject.Admin = admin;
			CatalogSubject structure = catalogSubject;
			nint num3 = Marshal.AllocHGlobal(Marshal.SizeOf<CatalogSubject>());
			try
			{
				Marshal.StructureToPtr(structure, num3, fDeleteOld: false);
				WinTrustData winTrustData = default(WinTrustData);
				winTrustData.Size = (uint)Marshal.SizeOf<WinTrustData>();
				winTrustData.UiChoice = 2u;
				winTrustData.UnionChoice = 2u;
				winTrustData.File = num3;
				winTrustData.StateAction = 1u;
				winTrustData.ProviderFlags = 4096u;
				WinTrustData data = winTrustData;
				Guid action = new Guid(_"00AAC56B-CD44-11d0-8CC2-00C04FC295EE");
				int num4;
				try
				{
					num4 = WinVerifyTrust(new IntPtr(-1), ref action, ref data);
				}
				finally
				{
					data.StateAction = 2u;
					WinVerifyTrust(new IntPtr(-1), ref action, ref data);
				}
				if (num4 != 0)
				{
					return null;
				}
				using X509Certificate2 x509Certificate = new X509Certificate2(X509Certificate.CreateFromSignedFile(info.Path));
				return x509Certificate.GetNameInfo(X509NameType.SimpleName, forIssuer: false);
			}
			finally
			{
				Marshal.DestroyStructure<CatalogSubject>(num3);
				Marshal.FreeHGlobal(num3);
			}
		}
		finally
		{
			if (num != IntPtr.Zero)
			{
				CryptCATAdminReleaseCatalogContext(admin, num, 0u);
			}
			CryptCATAdminReleaseContext(admin, 0u);
		}
	}

	[DllImport("wintrust.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool CryptCATAdminAcquireContext2(out nint admin, nint subsystem, string algorithm, nint policy, uint flags);

	[DllImport("wintrust.dll", ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool CryptCATAdminCalcHashFromFileHandle2(nint admin, nint file, ref uint size, [Out] byte[]? hash, uint flags);

	[DllImport("wintrust.dll", ExactSpelling = true)]
	private static extern nint CryptCATAdminEnumCatalogFromHash(nint admin, byte[] hash, uint size, uint flags, nint previous);

	[DllImport("wintrust.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool CryptCATCatalogInfoFromContext(nint catalog, ref CatalogInfo info, uint flags);

	[DllImport("wintrust.dll", ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool CryptCATAdminReleaseCatalogContext(nint admin, nint catalog, uint flags);

	[DllImport("wintrust.dll", ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool CryptCATAdminReleaseContext(nint admin, uint flags);

	[DllImport("wintrust.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
	private static extern int WinVerifyTrust(nint window, ref Guid action, ref WinTrustData data);
}
#pragma endregion

#pragma region GorillaGuard.Launcher/BootstrapReceipt.cs
using System.Collections.Generic;

namespace GorillaGuard.Launcher;

internal sealed class BootstrapReceipt
{
	public string Root { get; set; } = _"";

	public string BootstrapPath { get; set; } = _"";

	public string BootstrapHash { get; set; } = _"";

	public List<CleanupFile> Files { get; set; } = new List<CleanupFile>();
}
#pragma endregion

#pragma region GorillaGuard.Launcher/CleanupFile.cs

namespace GorillaGuard.Launcher;

internal sealed class CleanupFile
{
	public string Path { get; set; } = _"";

	public string Hash { get; set; } = _"";

	public string Kind { get; set; } = _"";
}
#pragma endregion

#pragma region GorillaGuard.Launcher/FailureDiagnostics.cs
using System;
using System.IO;
using System.Text;

namespace GorillaGuard.Launcher;

internal static class FailureDiagnostics
{
	internal static bool A(string P_0, out string P_1, out string P_2, string? directory = null)
	{
		P_1 = _"";
		P_2 = _"";
		try
		{
			if (directory == null)
			{
				directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _"GorillaGuard", _"Diagnostics");
			}
			Directory.CreateDirectory(directory);
			string text = Path.Combine(directory, _"failure-" + DateTime.UtcNow.ToString(_"yyyyMMdd-HHmmssfff") + _"-" + Guid.NewGuid().ToString(_"N") + _".txt");
			using StreamWriter streamWriter = new StreamWriter(new FileStream(text, FileMode.CreateNew, FileAccess.Write, FileShare.Read), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
			streamWriter.WriteLine(_"Gorilla Guard 1.12.31 — local failure diagnostics");
			streamWriter.WriteLine(_"UTC: " + DateTimeOffset.UtcNow.ToString(_"O"));
			streamWriter.WriteLine((P_0.Length > 32768) ? (P_0.Substring(0, 32768) + _"\n[truncated]") : P_0);
			streamWriter.Flush();
			P_1 = text;
			return true;
		}
		catch (Exception ex)
		{
			P_1 = _"";
			P_2 = ex.Message;
			return false;
		}
	}

	internal static string A(string P_0, string P_1, string P_2)
	{
		int num = P_0.IndexOf(_"Managed-file diagnosis:", StringComparison.Ordinal);
		string text = ((num > 0) ? (P_0.Substring(num) + _"\n\n" + P_0.Substring(0, num)) : P_0);
		if (text.Length > 2200)
		{
			text = text.Substring(0, 2200) + _"\n[See saved diagnostics for remaining details.]";
		}
		string text2 = ((P_1.Length > 0) ? (_"Saved outside session cleanup:\n" + P_1) : (_"Could not save persistent diagnostics: " + P_2));
		return _"Gorilla Guard did not launch the game.\n\n" + text + _"\n\n" + text2;
	}
}
#pragma endregion

#pragma region GorillaGuard.Launcher/InstalledFile.cs

namespace GorillaGuard.Launcher;

internal sealed class InstalledFile
{
	public string Relative { get; set; } = _"";

	public string? BackupName { get; set; }

	public string? OriginalHash { get; set; }

	public string? InstalledHash { get; set; }
}
#pragma endregion

#pragma region GorillaGuard.Launcher/KernelHealthClient.cs
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
#pragma endregion

#pragma region GorillaGuard.Launcher/KnownEvasionMonitor.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using GorillaGuard.Security;

namespace GorillaGuard.Launcher;

internal sealed class KnownEvasionMonitor
{
	private sealed class ProcessSnapshot(int Pid, int ParentPid, string Name, string Path, string CommandLine) : IEquatable<ProcessSnapshot>
	{
				private Type EqualityContract
		{
						get
			{
				return typeof(ProcessSnapshot);
			}
		}

		public int Pid { get; init; }

		public int ParentPid { get; init; }

		public string Name { get; init; }

		public string Path { get; init; }

		public string CommandLine { get; init; }

		public ProcessSnapshot(int Pid, int ParentPid, string Name, string Path, string CommandLine)
		{
			this.Pid = Pid;
			this.ParentPid = ParentPid;
			this.Name = Name;
			this.Path = Path;
			this.CommandLine = CommandLine;
			base._002Ector();
		}

				public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(_"ProcessSnapshot");
			stringBuilder.Append(_" { ");
			if (A(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

				private bool A(StringBuilder P_0)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			P_0.Append(_"Pid = ");
			P_0.Append(Pid.ToString());
			P_0.Append(_", ParentPid = ");
			P_0.Append(ParentPid.ToString());
			P_0.Append(_", Name = ");
			P_0.Append((object?)Name);
			P_0.Append(_", Path = ");
			P_0.Append((object?)Path);
			P_0.Append(_", CommandLine = ");
			P_0.Append((object?)CommandLine);
			return true;
		}

				public static bool operator !=(ProcessSnapshot? left, ProcessSnapshot? right)
		{
			return !(left == right);
		}

				public static bool operator ==(ProcessSnapshot? left, ProcessSnapshot? right)
		{
			if ((object)left != right)
			{
				return left?.Equals(right) ?? false;
			}
			return true;
		}

				public override int GetHashCode()
		{
			return ((((EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(Pid)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(ParentPid)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Path)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CommandLine);
		}

				public override bool Equals(object? obj)
		{
			return Equals(obj as ProcessSnapshot);
		}

				public bool Equals(ProcessSnapshot? other)
		{
			if ((object)this != other)
			{
				if ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<int>.Default.Equals(Pid, other.Pid) && EqualityComparer<int>.Default.Equals(ParentPid, other.ParentPid) && EqualityComparer<string>.Default.Equals(Name, other.Name) && EqualityComparer<string>.Default.Equals(Path, other.Path))
				{
					return EqualityComparer<string>.Default.Equals(CommandLine, other.CommandLine);
				}
				return false;
			}
			return true;
		}

				private ProcessSnapshot(ProcessSnapshot original)
		{
			Pid = original.Pid;
			ParentPid = original.ParentPid;
			Name = original.Name;
			Path = original.Path;
			CommandLine = original.CommandLine;
		}

				public void Deconstruct(out int Pid, out int ParentPid, out string Name, out string Path, out string CommandLine)
		{
			Pid = this.Pid;
			ParentPid = this.ParentPid;
			Name = this.Name;
			Path = this.Path;
			CommandLine = this.CommandLine;
		}
	}

	private sealed class HashCacheEntry(long Length, DateTime Written, long Checked, string Hash) : IEquatable<HashCacheEntry>
	{
				private Type EqualityContract
		{
						get
			{
				return typeof(HashCacheEntry);
			}
		}

		public long Length { get; init; }

		public DateTime Written { get; init; }

		public long Checked { get; init; }

		public string Hash { get; init; }

		public HashCacheEntry(long Length, DateTime Written, long Checked, string Hash)
		{
			this.Length = Length;
			this.Written = Written;
			this.Checked = Checked;
			this.Hash = Hash;
			base._002Ector();
		}

				public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(_"HashCacheEntry");
			stringBuilder.Append(_" { ");
			if (A(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

				private bool A(StringBuilder P_0)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			P_0.Append(_"Length = ");
			P_0.Append(Length.ToString());
			P_0.Append(_", Written = ");
			P_0.Append(Written.ToString());
			P_0.Append(_", Checked = ");
			P_0.Append(Checked.ToString());
			P_0.Append(_", Hash = ");
			P_0.Append((object?)Hash);
			return true;
		}

				public static bool operator !=(HashCacheEntry? left, HashCacheEntry? right)
		{
			return !(left == right);
		}

				public static bool operator ==(HashCacheEntry? left, HashCacheEntry? right)
		{
			if ((object)left != right)
			{
				return left?.Equals(right) ?? false;
			}
			return true;
		}

				public override int GetHashCode()
		{
			return (((EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<long>.Default.GetHashCode(Length)) * -1521134295 + EqualityComparer<DateTime>.Default.GetHashCode(Written)) * -1521134295 + EqualityComparer<long>.Default.GetHashCode(Checked)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Hash);
		}

				public override bool Equals(object? obj)
		{
			return Equals(obj as HashCacheEntry);
		}

				public bool Equals(HashCacheEntry? other)
		{
			if ((object)this != other)
			{
				if ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<long>.Default.Equals(Length, other.Length) && EqualityComparer<DateTime>.Default.Equals(Written, other.Written) && EqualityComparer<long>.Default.Equals(Checked, other.Checked))
				{
					return EqualityComparer<string>.Default.Equals(Hash, other.Hash);
				}
				return false;
			}
			return true;
		}

				private HashCacheEntry(HashCacheEntry original)
		{
			Length = original.Length;
			Written = original.Written;
			Checked = original.Checked;
			Hash = original.Hash;
		}

				public void Deconstruct(out long Length, out DateTime Written, out long Checked, out string Hash)
		{
			Length = this.Length;
			Written = this.Written;
			Checked = this.Checked;
			Hash = this.Hash;
		}
	}

	private const int MaxProbeEntries = 2048;

	private readonly Dictionary<string, HashCacheEntry> _hashCache = new Dictionary<string, HashCacheEntry>(StringComparer.OrdinalIgnoreCase);

	private readonly Action<string, string> _log;

	private bool _wmiWarningLogged;

	internal KnownEvasionMonitor(Action<string, string> log)
	{
		_log = log;
	}

	internal EvasionFinding? A(string P_0, bool P_1)
	{
		EvasionFinding evasionFinding = A();
		if (evasionFinding != null)
		{
			return evasionFinding;
		}
		evasionFinding = a();
		if (evasionFinding != null || !P_1)
		{
			return evasionFinding;
		}
		evasionFinding = A(P_0);
		return evasionFinding ?? B();
	}

	private EvasionFinding? A()
	{
		Process[] processes = Process.GetProcesses();
		try
		{
			Process[] array = processes;
			foreach (Process process in array)
			{
				try
				{
					if (process.Id == Environment.ProcessId)
					{
						continue;
					}
					string processName = process.ProcessName;
					string text = A(process) ?? _"";
					EvasionFinding evasionFinding = EvasionPolicy.A(processName, text, process.Id);
					if (evasionFinding != null)
					{
						return evasionFinding;
					}
					if (EvasionPolicy.A(processName, text) && A(text, out string evidence))
					{
						return new EvasionFinding(_"IMAGE_HASH", evidence, process.Id, text);
					}
					bool flag = processName.Equals(_"Gorilla Tag", StringComparison.OrdinalIgnoreCase);
					if (!flag && !EvasionPolicy.a(processName))
					{
						continue;
					}
					foreach (ProcessModule module in process.Modules)
					{
						string text2 = module.FileName ?? _"";
						evasionFinding = EvasionPolicy.B(module.ModuleName, text2, process.Id);
						if (evasionFinding != null && (flag || evasionFinding.Kind != _"PROXY_MODULE"))
						{
							return evasionFinding;
						}
						if (!string.IsNullOrWhiteSpace(text2) && A(text2, out evidence))
						{
							return new EvasionFinding(_"MODULE_HASH", evidence, process.Id, text2);
						}
					}
				}
				catch (Exception ex) when (((ex is InvalidOperationException || ex is Win32Exception || ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
				{
				}
			}
		}
		finally
		{
			Process[] array = processes;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Dispose();
			}
		}
		return null;
	}

	private EvasionFinding? a()
	{

		List<ProcessSnapshot> list = new List<ProcessSnapshot>();
		try
		{
			ManagementObjectSearcher val = new ManagementObjectSearcher(_"SELECT ProcessId, ParentProcessId, Name, ExecutablePath, CommandLine FROM Win32_Process");
			try
			{
				ManagementObjectCollection val2 = val.Get();
				try
				{
					ManagementObjectEnumerator enumerator = val2.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							ManagementBaseObject current = enumerator.Current;
							ManagementBaseObject val3 = current;
							try
							{
								int num = Convert.ToInt32(current[_"ProcessId"]);
								if (num != Environment.ProcessId)
								{
									list.Add(new ProcessSnapshot(num, Convert.ToInt32(current[_"ParentProcessId"]), current[_"Name"]?.ToString() ?? _"", current[_"ExecutablePath"]?.ToString() ?? _"", current[_"CommandLine"]?.ToString() ?? _""));
								}
							}
							finally
							{
								((IDisposable)val3)?.Dispose();
							}
						}
					}
					finally
					{
						((IDisposable)enumerator)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (Exception ex) when (((ex is ManagementException || ex is InvalidOperationException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
		{
			if (!_wmiWarningLogged)
			{
				_wmiWarningLogged = true;
				_log(_"WARNING", _"Process lineage/command-line inspection unavailable: " + ex.Message);
			}
			return null;
		}
		foreach (ProcessSnapshot item in list)
		{
			EvasionFinding evasionFinding = EvasionPolicy.a(item.Name, item.CommandLine, item.Pid);
			if (evasionFinding != null)
			{
				return evasionFinding;
			}
		}
		ILookup<int, ProcessSnapshot> lookup = list.ToLookup((ProcessSnapshot P_0) => P_0.ParentPid);
		foreach (ProcessSnapshot item2 in list)
		{
			if (EvasionPolicy.a(item2.Name, item2.Path))
			{
				ProcessSnapshot processSnapshot = lookup[item2.Pid].FirstOrDefault((ProcessSnapshot P_0) => EvasionPolicy.a(P_0.Name));
				if (processSnapshot != null)
				{
					string kind = _"PROCESS_LINEAGE";
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 4);
					defaultInterpolatedStringHandler.AppendLiteral(_"suspicious parent ");
					defaultInterpolatedStringHandler.AppendFormatted(item2.Name);
					defaultInterpolatedStringHandler.AppendLiteral(_" (");
					defaultInterpolatedStringHandler.AppendFormatted(item2.Pid);
					defaultInterpolatedStringHandler.AppendLiteral(_") started carrier ");
					defaultInterpolatedStringHandler.AppendFormatted(processSnapshot.Name);
					defaultInterpolatedStringHandler.AppendLiteral(_" (");
					defaultInterpolatedStringHandler.AppendFormatted(processSnapshot.Pid);
					defaultInterpolatedStringHandler.AppendLiteral(_")");
					return new EvasionFinding(kind, defaultInterpolatedStringHandler.ToStringAndClear(), processSnapshot.Pid, processSnapshot.Path);
				}
			}
		}
		return null;
	}

	private static EvasionFinding? A(string P_0)
	{
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		string folderPath2 = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		string folderPath3 = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		string folderPath4 = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
		string text = Path.Combine(folderPath, _"Downloads");
		string text2 = new string[3]
		{
			Path.Combine(folderPath2, _"BlurAutoClicker"),
			Path.Combine(folderPath4, _"WinNetFormDriverService"),
			Path.Combine(text, _"VR-Cheat")
		}.FirstOrDefault(Directory.Exists);
		if (text2 != null)
		{
			return new EvasionFinding(_"KNOWN_DIRECTORY", text2, 0, text2);
		}
		foreach (string item in new string[2]
		{
			text,
			Path.Combine(folderPath3, _"Microsoft", _"Windows", _"Themes", _"CachedFiles")
		}.Distinct<string>(StringComparer.OrdinalIgnoreCase))
		{
			EvasionFinding evasionFinding = A(item, 2);
			if (evasionFinding != null)
			{
				return evasionFinding;
			}
		}
		string[] array = new string[3]
		{
			_"version.dll",
			_"winmm.dll",
			_"dsound.dll"
		};
		foreach (string path in array)
		{
			string text3 = Path.Combine(P_0, path);
			if (File.Exists(text3))
			{
				return new EvasionFinding(_"GAME_PROXY_FILE", text3, 0, text3);
			}
		}
		return null;
	}

	private static EvasionFinding? A(string P_0, int P_1)
	{
		if (string.IsNullOrWhiteSpace(P_0) || !Directory.Exists(P_0))
		{
			return null;
		}
		Queue<(string, int)> queue = new Queue<(string, int)>();
		queue.Enqueue((P_0, 0));
		int num = 0;
		while (queue.Count != 0 && num < 2048)
		{
			var (path, num2) = queue.Dequeue();
			try
			{
				foreach (string item in Directory.EnumerateFiles(path))
				{
					if (++num <= 2048)
					{
						EvasionFinding evasionFinding = EvasionPolicy.B(item);
						if (evasionFinding != null)
						{
							return evasionFinding;
						}
						continue;
					}
					break;
				}
				if (num2 >= P_1)
				{
					continue;
				}
				foreach (string item2 in Directory.EnumerateDirectories(path))
				{
					if (++num <= 2048)
					{
						DirectoryInfo directoryInfo = new DirectoryInfo(item2);
						if ((directoryInfo.Attributes & FileAttributes.ReparsePoint) == 0 && !directoryInfo.Name.Equals(_"node_modules", StringComparison.OrdinalIgnoreCase) && !directoryInfo.Name.Equals(_".git", StringComparison.OrdinalIgnoreCase) && !directoryInfo.Name.Equals(_"bin", StringComparison.OrdinalIgnoreCase) && !directoryInfo.Name.Equals(_"obj", StringComparison.OrdinalIgnoreCase))
						{
							queue.Enqueue((item2, num2 + 1));
						}
						continue;
					}
					break;
				}
			}
			catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
			{
			}
		}
		return null;
	}

	private static EvasionFinding? B()
	{
		try
		{
			string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), _"schtasks.exe");
			using Process process = Process.Start(new ProcessStartInfo
			{
				FileName = fileName,
				Arguments = _"/Query /TN \"WinNetFormDriverService\" /FO LIST",
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			});
			if (process == null)
			{
				return null;
			}
			if (!process.WaitForExit(4000))
			{
				try
				{
					process.Kill(entireProcessTree: true);
				}
				catch
				{
				}
				return null;
			}
			return (process.ExitCode == 0) ? new EvasionFinding(_"SCHEDULED_TASK", _"WinNetFormDriverService", 0, _"") : null;
		}
		catch (Exception ex) when (((ex is InvalidOperationException || ex is Win32Exception) ? 1 : 0) != 0)
		{
			return null;
		}
	}

	private bool A(string P_0, out string P_1)
	{
		P_1 = _"";
		try
		{
			if (string.IsNullOrWhiteSpace(P_0) || !File.Exists(P_0))
			{
				return false;
			}
			FileInfo fileInfo = new FileInfo(P_0);
			if (fileInfo.Length <= 0 || fileInfo.Length > 8388608)
			{
				return false;
			}
			long timestamp = Stopwatch.GetTimestamp();
			if (_hashCache.TryGetValue(P_0, out HashCacheEntry value) && value.Length == fileInfo.Length && value.Written == fileInfo.LastWriteTimeUtc && Stopwatch.GetElapsedTime(value.Checked, timestamp).TotalSeconds < 10.0)
			{
				P_1 = value.Hash;
			}
			else
			{
				using FileStream source = new FileStream(P_0, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
				P_1 = Convert.ToHexString(SHA256.HashData(source));
				_hashCache[P_0] = new HashCacheEntry(fileInfo.Length, fileInfo.LastWriteTimeUtc, timestamp, P_1);
				if (_hashCache.Count > 512)
				{
					_hashCache.Clear();
				}
			}
			return EvasionPolicy.A(P_1);
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
		{
			return false;
		}
	}

	private static string? A(Process P_0)
	{
		try
		{
			return P_0.MainModule?.FileName;
		}
		catch (Exception ex) when (((ex is InvalidOperationException || ex is Win32Exception) ? 1 : 0) != 0)
		{
			return null;
		}
	}
}
#pragma endregion

#pragma region GorillaGuard.Launcher/KnownLoaderMonitor.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using GorillaGuard.Security;

namespace GorillaGuard.Launcher;

internal sealed class KnownLoaderMonitor
{
	private sealed class CacheEntry(long Length, DateTime Written, long Checked, LoaderFinding? Finding) : IEquatable<CacheEntry>
	{
				private Type EqualityContract
		{
						get
			{
				return typeof(CacheEntry);
			}
		}

		public long Length { get; init; }

		public DateTime Written { get; init; }

		public long Checked { get; init; }

		public LoaderFinding? Finding { get; init; }

		public CacheEntry(long Length, DateTime Written, long Checked, LoaderFinding? Finding)
		{
			this.Length = Length;
			this.Written = Written;
			this.Checked = Checked;
			this.Finding = Finding;
			base._002Ector();
		}

				public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(_"CacheEntry");
			stringBuilder.Append(_" { ");
			if (A(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

				private bool A(StringBuilder P_0)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			P_0.Append(_"Length = ");
			P_0.Append(Length.ToString());
			P_0.Append(_", Written = ");
			P_0.Append(Written.ToString());
			P_0.Append(_", Checked = ");
			P_0.Append(Checked.ToString());
			P_0.Append(_", Finding = ");
			P_0.Append(Finding);
			return true;
		}

				public static bool operator !=(CacheEntry? left, CacheEntry? right)
		{
			return !(left == right);
		}

				public static bool operator ==(CacheEntry? left, CacheEntry? right)
		{
			if ((object)left != right)
			{
				return left?.Equals(right) ?? false;
			}
			return true;
		}

				public override int GetHashCode()
		{
			return (((EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<long>.Default.GetHashCode(Length)) * -1521134295 + EqualityComparer<DateTime>.Default.GetHashCode(Written)) * -1521134295 + EqualityComparer<long>.Default.GetHashCode(Checked)) * -1521134295 + EqualityComparer<LoaderFinding>.Default.GetHashCode(Finding);
		}

				public override bool Equals(object? obj)
		{
			return Equals(obj as CacheEntry);
		}

				public bool Equals(CacheEntry? other)
		{
			if ((object)this != other)
			{
				if ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<long>.Default.Equals(Length, other.Length) && EqualityComparer<DateTime>.Default.Equals(Written, other.Written) && EqualityComparer<long>.Default.Equals(Checked, other.Checked))
				{
					return EqualityComparer<LoaderFinding>.Default.Equals(Finding, other.Finding);
				}
				return false;
			}
			return true;
		}

				private CacheEntry(CacheEntry original)
		{
			Length = original.Length;
			Written = original.Written;
			Checked = original.Checked;
			Finding = original.Finding;
		}

				public void Deconstruct(out long Length, out DateTime Written, out long Checked, out LoaderFinding? Finding)
		{
			Length = this.Length;
			Written = this.Written;
			Checked = this.Checked;
			Finding = this.Finding;
		}
	}

	private readonly Dictionary<string, CacheEntry> _cache = new Dictionary<string, CacheEntry>(StringComparer.OrdinalIgnoreCase);

	private readonly Action<string, string> _log;

	internal KnownLoaderMonitor(Action<string, string> log)
	{
		_log = log;
	}

	internal LoaderFinding? A()
	{
		Process[] processes = Process.GetProcesses();
		try
		{
			Process[] array = processes;
			foreach (Process process in array)
			{
				try
				{
					string text = A(process.Id);
					if (text != null)
					{
						LoaderFinding loaderFinding = A(text);
						if (loaderFinding != null)
						{
							LoaderFinding loaderFinding2 = loaderFinding._003CClone_003E_0024();
							loaderFinding2.ProcessId = process.Id;
							return loaderFinding2;
						}
					}
				}
				catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException || ex is Win32Exception || ex is InvalidOperationException) ? 1 : 0) != 0)
				{
				}
			}
		}
		finally
		{
			Process[] array = processes;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Dispose();
			}
		}
		if (_cache.Count > 4096)
		{
			_cache.Clear();
		}
		return null;
	}

	internal LoaderFinding? A(string P_0)
	{
		FileInfo fileInfo = new FileInfo(P_0);
		long timestamp = Stopwatch.GetTimestamp();
		if (_cache.TryGetValue(P_0, out CacheEntry value) && value.Length == fileInfo.Length && value.Written == fileInfo.LastWriteTimeUtc && Stopwatch.GetElapsedTime(value.Checked, timestamp).TotalSeconds < 10.0)
		{
			return value.Finding;
		}
		LoaderFinding loaderFinding = a(P_0);
		_cache[P_0] = new CacheEntry(fileInfo.Length, fileInfo.LastWriteTimeUtc, timestamp, loaderFinding);
		if (loaderFinding != null)
		{
			_log(_"ERROR", _"Known loader evidence: " + loaderFinding.Evidence + _"; path=" + P_0);
		}
		return loaderFinding;
	}

	internal static LoaderFinding? a(string P_0)
	{
		using FileStream fileStream = new FileStream(P_0, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
		if (fileStream.Length == 9175040)
		{
			string text = Convert.ToHexString(SHA256.HashData(fileStream));
			if (text.Equals(_"DA9816B556B81B2444C12D42E67D5C25FB51F06E6BF884E553733770438775B3", StringComparison.OrdinalIgnoreCase))
			{
				return new LoaderFinding(_"Absense specimen SHA-256=" + text, P_0);
			}
			fileStream.Position = 0L;
		}
		if (fileStream.Length < 128 || fileStream.Length > 134217728)
		{
			return null;
		}
		try
		{
			using PEReader pEReader = new PEReader(fileStream, PEStreamOptions.LeaveOpen);
			if (!pEReader.HasMetadata || pEReader.PEHeaders.CorHeader.MetadataDirectory.Size > 16777216)
			{
				return null;
			}
			MetadataReader metadataReader = pEReader.GetMetadataReader();
			if (metadataReader.TypeDefinitions.Count > 100000 || metadataReader.MethodDefinitions.Count > 200000)
			{
				return null;
			}
			List<string> list = new List<string>();
			foreach (TypeDefinitionHandle typeDefinition2 in metadataReader.TypeDefinitions)
			{
				TypeDefinition typeDefinition = metadataReader.GetTypeDefinition(typeDefinition2);
				string @string = metadataReader.GetString(typeDefinition.Namespace);
				if (@string == _"Absense")
				{
					list.Add(@string + _"." + metadataReader.GetString(typeDefinition.Name));
				}
			}
			if (list.Count < 3)
			{
				return null;
			}
			List<string> list2 = new List<string>();
			foreach (MethodDefinitionHandle methodDefinition2 in metadataReader.MethodDefinitions)
			{
				MethodDefinition methodDefinition = metadataReader.GetMethodDefinition(methodDefinition2);
				if ((methodDefinition.Attributes & MethodAttributes.PinvokeImpl) != 0)
				{
					MethodImport import = methodDefinition.GetImport();
					if (!import.Module.IsNil && !import.Name.IsNil && metadataReader.GetString(metadataReader.GetModuleReference(import.Module).Name).Equals(_"kernel32.dll", StringComparison.OrdinalIgnoreCase))
					{
						list2.Add(metadataReader.GetString(import.Name));
					}
				}
			}
			return InjectionPolicy.A(list, list2) ? new LoaderFinding(_"Absense injector type/import combination (static family match)", P_0) : null;
		}
		catch (Exception ex) when (((ex is BadImageFormatException || ex is InvalidOperationException || ex is ArgumentException || ex is OverflowException || ex is IndexOutOfRangeException) ? 1 : 0) != 0)
		{
			return null;
		}
	}

	private static string? A(int P_0)
	{
		nint num = OpenProcess(4096u, inherit: false, P_0);
		if (num == IntPtr.Zero)
		{
			return null;
		}
		try
		{
			StringBuilder stringBuilder = new StringBuilder(32768);
			int size = stringBuilder.Capacity;
			return QueryFullProcessImageNameW(num, 0u, stringBuilder, ref size) ? stringBuilder.ToString() : null;
		}
		finally
		{
			CloseHandle(num);
		}
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern nint OpenProcess(uint access, bool inherit, int pid);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool QueryFullProcessImageNameW(nint process, uint flags, StringBuilder name, ref int size);

	[DllImport("kernel32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool CloseHandle(nint handle);
}
#pragma endregion

#pragma region GorillaGuard.Launcher/LoaderFinding.cs
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace GorillaGuard.Launcher;

internal sealed class LoaderFinding(string Evidence, string Path, int ProcessId = 0) : IEquatable<LoaderFinding>
{
		private Type EqualityContract
	{
				get
		{
			return typeof(LoaderFinding);
		}
	}

	public string Evidence { get; init; }

	public string Path { get; init; }

	public int ProcessId { get; init; }

	public LoaderFinding(string Evidence, string Path, int ProcessId = 0)
	{
		this.Evidence = Evidence;
		this.Path = Path;
		this.ProcessId = ProcessId;
		base._002Ector();
	}

		public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(_"LoaderFinding");
		stringBuilder.Append(_" { ");
		if (A(stringBuilder))
		{
			stringBuilder.Append(' ');
		}
		stringBuilder.Append('}');
		return stringBuilder.ToString();
	}

		private bool A(StringBuilder P_0)
	{
		RuntimeHelpers.EnsureSufficientExecutionStack();
		P_0.Append(_"Evidence = ");
		P_0.Append((object?)Evidence);
		P_0.Append(_", Path = ");
		P_0.Append((object?)Path);
		P_0.Append(_", ProcessId = ");
		P_0.Append(ProcessId.ToString());
		return true;
	}

		public static bool operator !=(LoaderFinding? left, LoaderFinding? right)
	{
		return !(left == right);
	}

		public static bool operator ==(LoaderFinding? left, LoaderFinding? right)
	{
		if ((object)left != right)
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

		public override int GetHashCode()
	{
		return ((EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Evidence)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Path)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(ProcessId);
	}

		public override bool Equals(object? obj)
	{
		return Equals(obj as LoaderFinding);
	}

		public bool Equals(LoaderFinding? other)
	{
		if ((object)this != other)
		{
			if ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<string>.Default.Equals(Evidence, other.Evidence) && EqualityComparer<string>.Default.Equals(Path, other.Path))
			{
				return EqualityComparer<int>.Default.Equals(ProcessId, other.ProcessId);
			}
			return false;
		}
		return true;
	}

		private LoaderFinding(LoaderFinding original)
	{
		Evidence = original.Evidence;
		Path = original.Path;
		ProcessId = original.ProcessId;
	}

		public void Deconstruct(out string Evidence, out string Path, out int ProcessId)
	{
		Evidence = this.Evidence;
		Path = this.Path;
		ProcessId = this.ProcessId;
	}
}
#pragma endregion

#pragma region GorillaGuard.Launcher/MonitoringFailureBudget.cs
using System;

namespace GorillaGuard.Launcher;

internal sealed class MonitoringFailureBudget
{
	private int _failures;

	internal bool A(bool P_0)
	{
		_failures = ((!P_0) ? Math.Min(3, _failures + 1) : 0);
		return _failures >= 3;
	}
}
#pragma endregion

#pragma region GorillaGuard.Launcher/NativeModuleMonitor.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace GorillaGuard.Launcher;

internal sealed class NativeModuleMonitor
{
	private readonly HashSet<string> _reported = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	internal NativeModuleScan A(IEnumerable<string> P_0, Func<string, LoaderFinding?> inspect, Func<string, bool> P_2, Action<string> P_3)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		LoaderFinding loaderFinding = null;
		foreach (string item in P_0)
		{
			if (string.IsNullOrWhiteSpace(item) || !hashSet.Add(item))
			{
				continue;
			}
			try
			{
				if ((object)loaderFinding == null)
				{
					loaderFinding = inspect(item);
				}
				bool flag = P_2(item);
				if (_reported.Add(item))
				{
					P_3(item);
					if (flag)
					{
						list.Add(item);
					}
				}
			}
			catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException || ex is Win32Exception) ? 1 : 0) != 0)
			{
				list2.Add(item + _": " + ex.Message);
			}
		}
		_reported.IntersectWith(hashSet);
		return new NativeModuleScan(list, loaderFinding, list2);
	}
}
#pragma endregion

#pragma region GorillaGuard.Launcher/NativeModuleScan.cs
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace GorillaGuard.Launcher;

internal sealed class NativeModuleScan(IReadOnlyList<string> SuspiciousPaths, LoaderFinding? Loader, IReadOnlyList<string> Errors) : IEquatable<NativeModuleScan>
{
		private Type EqualityContract
	{
				get
		{
			return typeof(NativeModuleScan);
		}
	}

	public IReadOnlyList<string> SuspiciousPaths { get; init; }

	public LoaderFinding? Loader { get; init; }

	public IReadOnlyList<string> Errors { get; init; }

	public NativeModuleScan(IReadOnlyList<string> SuspiciousPaths, LoaderFinding? Loader, IReadOnlyList<string> Errors)
	{
		this.SuspiciousPaths = SuspiciousPaths;
		this.Loader = Loader;
		this.Errors = Errors;
		base._002Ector();
	}

		public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(_"NativeModuleScan");
		stringBuilder.Append(_" { ");
		if (A(stringBuilder))
		{
			stringBuilder.Append(' ');
		}
		stringBuilder.Append('}');
		return stringBuilder.ToString();
	}

		private bool A(StringBuilder P_0)
	{
		RuntimeHelpers.EnsureSufficientExecutionStack();
		P_0.Append(_"SuspiciousPaths = ");
		P_0.Append(SuspiciousPaths);
		P_0.Append(_", Loader = ");
		P_0.Append(Loader);
		P_0.Append(_", Errors = ");
		P_0.Append(Errors);
		return true;
	}

		public static bool operator !=(NativeModuleScan? left, NativeModuleScan? right)
	{
		return !(left == right);
	}

		public static bool operator ==(NativeModuleScan? left, NativeModuleScan? right)
	{
		if ((object)left != right)
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

		public override int GetHashCode()
	{
		return ((EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<IReadOnlyList<string>>.Default.GetHashCode(SuspiciousPaths)) * -1521134295 + EqualityComparer<LoaderFinding>.Default.GetHashCode(Loader)) * -1521134295 + EqualityComparer<IReadOnlyList<string>>.Default.GetHashCode(Errors);
	}

		public override bool Equals(object? obj)
	{
		return Equals(obj as NativeModuleScan);
	}

		public bool Equals(NativeModuleScan? other)
	{
		if ((object)this != other)
		{
			if ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<IReadOnlyList<string>>.Default.Equals(SuspiciousPaths, other.SuspiciousPaths) && EqualityComparer<LoaderFinding>.Default.Equals(Loader, other.Loader))
			{
				return EqualityComparer<IReadOnlyList<string>>.Default.Equals(Errors, other.Errors);
			}
			return false;
		}
		return true;
	}

		private NativeModuleScan(NativeModuleScan original)
	{
		SuspiciousPaths = original.SuspiciousPaths;
		Loader = original.Loader;
		Errors = original.Errors;
	}

		public void Deconstruct(out IReadOnlyList<string> SuspiciousPaths, out LoaderFinding? Loader, out IReadOnlyList<string> Errors)
	{
		SuspiciousPaths = this.SuspiciousPaths;
		Loader = this.Loader;
		Errors = this.Errors;
	}
}
#pragma endregion

#pragma region GorillaGuard.Launcher/ProcessHandleMonitor.cs
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
#pragma endregion

#pragma region GorillaGuard.Launcher/Program.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GorillaGuard.Security;
using Microsoft.Win32;

namespace GorillaGuard.Launcher;

internal static class Program
{
	private struct JobObjectBasicLimitInformation
	{
		public long PerProcessUserTimeLimit;

		public long PerJobUserTimeLimit;

		public uint LimitFlags;

		public nuint MinimumWorkingSetSize;

		public nuint MaximumWorkingSetSize;

		public uint ActiveProcessLimit;

		public nuint Affinity;

		public uint PriorityClass;

		public uint SchedulingClass;
	}

	private struct IoCounters
	{
		public ulong ReadOperationCount;

		public ulong WriteOperationCount;

		public ulong OtherOperationCount;

		public ulong ReadTransferCount;

		public ulong WriteTransferCount;

		public ulong OtherTransferCount;
	}

	private struct JobObjectExtendedLimitInformation
	{
		public JobObjectBasicLimitInformation BasicLimitInformation;

		public IoCounters IoInfo;

		public nuint ProcessMemoryLimit;

		public nuint JobMemoryLimit;

		public nuint PeakProcessMemoryUsed;

		public nuint PeakJobMemoryUsed;
	}

	private sealed class SteamLaunch(string? SteamExecutable, string AppId) : IEquatable<SteamLaunch>
	{
				private Type EqualityContract
		{
						get
			{
				return typeof(SteamLaunch);
			}
		}

		public string? SteamExecutable { get; init; }

		public string AppId { get; init; }

		public SteamLaunch(string? SteamExecutable, string AppId)
		{
			this.SteamExecutable = SteamExecutable;
			this.AppId = AppId;
			base._002Ector();
		}

				public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(_"SteamLaunch");
			stringBuilder.Append(_" { ");
			if (A(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

				private bool A(StringBuilder P_0)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			P_0.Append(_"SteamExecutable = ");
			P_0.Append((object?)SteamExecutable);
			P_0.Append(_", AppId = ");
			P_0.Append((object?)AppId);
			return true;
		}

				public static bool operator !=(SteamLaunch? left, SteamLaunch? right)
		{
			return !(left == right);
		}

				public static bool operator ==(SteamLaunch? left, SteamLaunch? right)
		{
			if ((object)left != right)
			{
				return left?.Equals(right) ?? false;
			}
			return true;
		}

				public override int GetHashCode()
		{
			return (EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SteamExecutable)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AppId);
		}

				public override bool Equals(object? obj)
		{
			return Equals(obj as SteamLaunch);
		}

				public bool Equals(SteamLaunch? other)
		{
			if ((object)this != other)
			{
				if ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<string>.Default.Equals(SteamExecutable, other.SteamExecutable))
				{
					return EqualityComparer<string>.Default.Equals(AppId, other.AppId);
				}
				return false;
			}
			return true;
		}

				private SteamLaunch(SteamLaunch original)
		{
			SteamExecutable = original.SteamExecutable;
			AppId = original.AppId;
		}

				public void Deconstruct(out string? SteamExecutable, out string AppId)
		{
			SteamExecutable = this.SteamExecutable;
			AppId = this.AppId;
		}
	}

	private sealed class GuardedLocation(string Root, string Scope, bool TopDirectoryOnly) : IEquatable<GuardedLocation>
	{
				private Type EqualityContract
		{
						get
			{
				return typeof(GuardedLocation);
			}
		}

		public string Root { get; init; }

		public string Scope { get; init; }

		public bool TopDirectoryOnly { get; init; }

		public GuardedLocation(string Root, string Scope, bool TopDirectoryOnly)
		{
			this.Root = Root;
			this.Scope = Scope;
			this.TopDirectoryOnly = TopDirectoryOnly;
			base._002Ector();
		}

				public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(_"GuardedLocation");
			stringBuilder.Append(_" { ");
			if (A(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

				private bool A(StringBuilder P_0)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			P_0.Append(_"Root = ");
			P_0.Append((object?)Root);
			P_0.Append(_", Scope = ");
			P_0.Append((object?)Scope);
			P_0.Append(_", TopDirectoryOnly = ");
			P_0.Append(TopDirectoryOnly.ToString());
			return true;
		}

				public static bool operator !=(GuardedLocation? left, GuardedLocation? right)
		{
			return !(left == right);
		}

				public static bool operator ==(GuardedLocation? left, GuardedLocation? right)
		{
			if ((object)left != right)
			{
				return left?.Equals(right) ?? false;
			}
			return true;
		}

				public override int GetHashCode()
		{
			return ((EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Root)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Scope)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(TopDirectoryOnly);
		}

				public override bool Equals(object? obj)
		{
			return Equals(obj as GuardedLocation);
		}

				public bool Equals(GuardedLocation? other)
		{
			if ((object)this != other)
			{
				if ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<string>.Default.Equals(Root, other.Root) && EqualityComparer<string>.Default.Equals(Scope, other.Scope))
				{
					return EqualityComparer<bool>.Default.Equals(TopDirectoryOnly, other.TopDirectoryOnly);
				}
				return false;
			}
			return true;
		}

				private GuardedLocation(GuardedLocation original)
		{
			Root = original.Root;
			Scope = original.Scope;
			TopDirectoryOnly = original.TopDirectoryOnly;
		}

				public void Deconstruct(out string Root, out string Scope, out bool TopDirectoryOnly)
		{
			Root = this.Root;
			Scope = this.Scope;
			TopDirectoryOnly = this.TopDirectoryOnly;
		}
	}

	private sealed class InstallFile(string Source, string Destination) : IEquatable<InstallFile>
	{
				private Type EqualityContract
		{
						get
			{
				return typeof(InstallFile);
			}
		}

		public string Source { get; init; }

		public string Destination { get; init; }

		public InstallFile(string Source, string Destination)
		{
			this.Source = Source;
			this.Destination = Destination;
			base._002Ector();
		}

				public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(_"InstallFile");
			stringBuilder.Append(_" { ");
			if (A(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

				private bool A(StringBuilder P_0)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			P_0.Append(_"Source = ");
			P_0.Append((object?)Source);
			P_0.Append(_", Destination = ");
			P_0.Append((object?)Destination);
			return true;
		}

				public static bool operator !=(InstallFile? left, InstallFile? right)
		{
			return !(left == right);
		}

				public static bool operator ==(InstallFile? left, InstallFile? right)
		{
			if ((object)left != right)
			{
				return left?.Equals(right) ?? false;
			}
			return true;
		}

				public override int GetHashCode()
		{
			return (EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Source)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Destination);
		}

				public override bool Equals(object? obj)
		{
			return Equals(obj as InstallFile);
		}

				public bool Equals(InstallFile? other)
		{
			if ((object)this != other)
			{
				if ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<string>.Default.Equals(Source, other.Source))
				{
					return EqualityComparer<string>.Default.Equals(Destination, other.Destination);
				}
				return false;
			}
			return true;
		}

				private InstallFile(InstallFile original)
		{
			Source = original.Source;
			Destination = original.Destination;
		}

				public void Deconstruct(out string Source, out string Destination)
		{
			Source = this.Source;
			Destination = this.Destination;
		}
	}

	private sealed class LauncherEvent(string Category, string Details) : IEquatable<LauncherEvent>
	{
				private Type EqualityContract
		{
						get
			{
				return typeof(LauncherEvent);
			}
		}

		public string Category { get; init; }

		public string Details { get; init; }

		public LauncherEvent(string Category, string Details)
		{
			this.Category = Category;
			this.Details = Details;
			base._002Ector();
		}

				public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(_"LauncherEvent");
			stringBuilder.Append(_" { ");
			if (A(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

				private bool A(StringBuilder P_0)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			P_0.Append(_"Category = ");
			P_0.Append((object?)Category);
			P_0.Append(_", Details = ");
			P_0.Append((object?)Details);
			return true;
		}

				public static bool operator !=(LauncherEvent? left, LauncherEvent? right)
		{
			return !(left == right);
		}

				public static bool operator ==(LauncherEvent? left, LauncherEvent? right)
		{
			if ((object)left != right)
			{
				return left?.Equals(right) ?? false;
			}
			return true;
		}

				public override int GetHashCode()
		{
			return (EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Category)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Details);
		}

				public override bool Equals(object? obj)
		{
			return Equals(obj as LauncherEvent);
		}

				public bool Equals(LauncherEvent? other)
		{
			if ((object)this != other)
			{
				if ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<string>.Default.Equals(Category, other.Category))
				{
					return EqualityComparer<string>.Default.Equals(Details, other.Details);
				}
				return false;
			}
			return true;
		}

				private LauncherEvent(LauncherEvent original)
		{
			Category = original.Category;
			Details = original.Details;
		}

				public void Deconstruct(out string Category, out string Details)
		{
			Category = this.Category;
			Details = this.Details;
		}
	}

		private sealed class _003C_003Ec__DisplayClass40_0
	{
		public List<(string Check, string Status, string Detail)> findings;

		public bool json;

		public DateTimeOffset started;

		public bool packageVerified;

		public string target;

		internal void A(string P_0, string P_1, string P_2)
		{
			findings.Add((P_0, P_1, P_2));
			if (!json)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 3);
				defaultInterpolatedStringHandler.AppendLiteral(_"SCAN ");
				defaultInterpolatedStringHandler.AppendFormatted(P_1);
				defaultInterpolatedStringHandler.AppendLiteral(_" [");
				defaultInterpolatedStringHandler.AppendFormatted(P_0);
				defaultInterpolatedStringHandler.AppendLiteral(_"]: ");
				defaultInterpolatedStringHandler.AppendFormatted(P_2);
				Console.WriteLine(defaultInterpolatedStringHandler.ToStringAndClear());
			}
		}

		internal void A(string P_0)
		{
			if (json)
			{
				Console.WriteLine(JsonSerializer.Serialize(new
				{
					schemaVersion = 1,
					release = _"1.12.31",
					startedUtc = started,
					verdict = P_0,
					findings = Enumerable.Select(findings, ((string Check, string Status, string Detail) f) => new
					{
						check = f.Check,
						status = f.Status,
						detail = f.Detail
					})
				}));
			}
			else
			{
				Console.WriteLine(_"READ-ONLY SCAN " + P_0);
			}
		}

		internal void A(string P_0, Func<(string Status, string Detail)> inspect)
		{
			_desktopStop.ThrowIfCancellationRequested();
			try
			{
				var (text, text2) = inspect();
				_desktopStop.ThrowIfCancellationRequested();
				A(P_0, text, text2);
			}
			catch (InvalidDataException ex)
			{
				A(P_0, _"FAILED", ex.Message);
			}
			catch (Exception ex2) when (!(ex2 is OperationCanceledException))
			{
				A(P_0, _"INCOMPLETE", ex2.Message);
			}
		}

		internal (string Status, string Detail) A()
		{
			ReleaseVerificationResult releaseVerificationResult = ReleaseVerifier.Verify(_bepInExRoot);
			packageVerified = releaseVerificationResult.Success;
			return (Status: releaseVerificationResult.Success ? _"PASS" : _"FAILED", Detail: releaseVerificationResult.Message);
		}

		internal (string Status, string Detail) a()
		{
			target = (File.Exists(Path.Combine(_gameRoot, _"Gorilla Tag.exe")) ? _gameRoot : e());
			return (Status: _"PASS", Detail: target);
		}

		internal (string Status, string Detail) B()
		{
			if (!packageVerified || target == null)
			{
				return (Status: _"INCOMPLETE", Detail: _"Requires a verified package and a game installation.");
			}
			ReleaseVerificationResult releaseVerificationResult = ReleaseVerifier.A(target, _bepInExRoot, Program.b(target, _gameRoot));
			if (!releaseVerificationResult.Success)
			{
				return (Status: _"FAILED", Detail: Program.B(releaseVerificationResult));
			}
			return (Status: _"PASS", Detail: releaseVerificationResult.Message);
		}

		internal (string Status, string Detail) b()
		{
			_003C_003Ec__DisplayClass40_1 CS_0024_003C_003E8__locals4 = new _003C_003Ec__DisplayClass40_1
			{
				warnings = new List<string>()
			};
			EvasionFinding evasionFinding = new KnownEvasionMonitor(delegate(string P_0, string P_1)
			{
				if (P_0 == _"WARNING")
				{
					CS_0024_003C_003E8__locals4.warnings.Add(P_1);
				}
			}).A(target ?? _gameRoot, target != null);
			if (evasionFinding != null)
			{
				return (Status: _"FAILED", Detail: Program.A(evasionFinding));
			}
			if (target == null)
			{
				CS_0024_003C_003E8__locals4.warnings.Add(_"File and scheduled-task checks skipped because game discovery did not complete.");
			}
			if (CS_0024_003C_003E8__locals4.warnings.Count != 0)
			{
				return (Status: _"INCOMPLETE", Detail: string.Join(_" ", CS_0024_003C_003E8__locals4.warnings));
			}
			return (Status: _"PASS", Detail: _"No known indicator found.");
		}

		internal (string Status, string Detail) C()
		{
			if (!packageVerified || target == null)
			{
				return (Status: _"INCOMPLETE", Detail: _"Requires a verified package and a game installation.");
			}
			using VrPreflight vrPreflight = new VrPreflight(target, G().Concat(f()), Program.C, _bepInExRoot);
			vrPreflight.A(false);
			return (Status: _"PASS", Detail: _"Registered runtimes, drivers, layers, and restricted settings audited.");
		}
	}

		private sealed class _003C_003Ec__DisplayClass40_1
	{
		public List<string> warnings;

		internal void A(string P_0, string P_1)
		{
			if (P_0 == _"WARNING")
			{
				warnings.Add(P_1);
			}
		}
	}

		private sealed class _003CEnumerateFilesWithoutReparsePoints_003Ed__94 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private string _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private string root;

		public string _003C_003E3__root;

		private bool recursive;

		public bool _003C_003E3__recursive;

		private Stack<string> _003Cpending_003E5__2;

		private string _003Cdirectory_003E5__3;

		private IEnumerator<string> _003C_003E7__wrap3;

		string IEnumerator<string>.Current
		{
						get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
						get
			{
				return _003C_003E2__current;
			}
		}

				public _003CEnumerateFilesWithoutReparsePoints_003Ed__94(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

				private void A()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					a();
				}
			}
			_003Cpending_003E5__2 = null;
			_003Cdirectory_003E5__3 = null;
			_003C_003E7__wrap3 = null;
			_003C_003E1__state = -2;
		}

		void IDisposable.Dispose()
		{

			this.A();
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				if (num != 0)
				{
					if (num != 1)
					{
						return false;
					}
					_003C_003E1__state = -3;
					goto IL_0129;
				}
				_003C_003E1__state = -1;
				if (!Directory.Exists(root))
				{
					return false;
				}
				_003Cpending_003E5__2 = new Stack<string>();
				_003Cpending_003E5__2.Push(Path.GetFullPath(root));
				goto IL_01fa;
				IL_0129:
				if (_003C_003E7__wrap3.MoveNext())
				{
					string current = _003C_003E7__wrap3.Current;
					_003C_003E2__current = current;
					_003C_003E1__state = 1;
					return true;
				}
				a();
				_003C_003E7__wrap3 = null;
				if (recursive)
				{
					foreach (string item in Directory.EnumerateDirectories(_003Cdirectory_003E5__3, _"*", SearchOption.TopDirectoryOnly))
					{
						if ((File.GetAttributes(item) & FileAttributes.ReparsePoint) != 0)
						{
							throw new InvalidDataException(_"Redirected/reparse-point code directory is not allowed: " + item);
						}
						_003Cpending_003E5__2.Push(item);
					}
					_003Cdirectory_003E5__3 = null;
				}
				goto IL_01fa;
				IL_01fa:
				if (_003Cpending_003E5__2.Count > 0)
				{
					_003Cdirectory_003E5__3 = _003Cpending_003E5__2.Pop();
					_003C_003E7__wrap3 = Directory.EnumerateFiles(_003Cdirectory_003E5__3, _"*", SearchOption.TopDirectoryOnly).GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_0129;
				}
				return false;
			}
			catch
			{

				A();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{

			return this.MoveNext();
		}

		private void a()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap3 != null)
			{
				_003C_003E7__wrap3.Dispose();
			}
		}

				private void B()
		{
			throw new NotSupportedException();
		}

		void IEnumerator.Reset()
		{

			this.B();
		}

				private IEnumerator<string> b()
		{
			_003CEnumerateFilesWithoutReparsePoints_003Ed__94 _003CEnumerateFilesWithoutReparsePoints_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEnumerateFilesWithoutReparsePoints_003Ed__ = this;
			}
			else
			{
				_003CEnumerateFilesWithoutReparsePoints_003Ed__ = new _003CEnumerateFilesWithoutReparsePoints_003Ed__94(0);
			}
			_003CEnumerateFilesWithoutReparsePoints_003Ed__.root = _003C_003E3__root;
			_003CEnumerateFilesWithoutReparsePoints_003Ed__.recursive = _003C_003E3__recursive;
			return _003CEnumerateFilesWithoutReparsePoints_003Ed__;
		}

		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{

			return this.b();
		}

				private IEnumerator C()
		{
			return b();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{

			return this.C();
		}
	}

	private const string GameExecutableName = "Gorilla Tag.exe";

	private const string GorillaTagSteamAppId = "1533390";

	private static readonly object LogLock = new object();

	private static readonly WarningGate Warnings = new WarningGate();

	private static string _gameRoot = _"";

	private static string _bepInExRoot = _"";

	private static string _logPath = _"";

	private static string _nonce = _"";

	private static string _pipeName = _"";

	private static long _lastHeartbeatTicks;

	private static volatile bool _preloaderAuthenticated;

	private static volatile bool _runtimeAuthenticated;

	private static volatile bool _outOfOrderRuntimeRejected;

	private static int _expectedGamePid;

	private static string? _runtimeViolation;

	private static string? _watcherFailure;

	private static nint _jobHandle;

	private static FileStream? _sessionFileLock;

	private static readonly List<FileStream> CodeLocks = new List<FileStream>();

	private static readonly HashSet<string> LockedCodePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<string> ContainedCodePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly List<FileSystemWatcher> Watchers = new List<FileSystemWatcher>();

	private static readonly WorkQueue<string> PendingCodeFiles = new WorkQueue<string>(2048);

	private static readonly WorkQueue<LauncherEvent> PendingRuntimeEvents = new WorkQueue<LauncherEvent>(256);

	private static RemoteSettings _remoteSettings = new RemoteSettings();

	private static RemoteClient? _remote;

	private static bool _suppressFailureDialog;

	private static bool _scanInProgress;

	private static bool _scanJson;

	private static VrPreflight? _vrPreflight;

	private static string? _startupVrFailure;

	private static readonly KnownLoaderMonitor LoaderMonitor = new KnownLoaderMonitor(C);

	private static readonly KnownEvasionMonitor EvasionMonitor = new KnownEvasionMonitor(C);

	private static bool _vrCompatibilityWarning;

	private static int _cleanupGamePid;

	private static long _cleanupGameStarted;

	private static bool _cleanupArmed;

	private static CancellationToken _desktopStop;

	private const uint JobObjectLimitKillOnJobClose = 8192u;

	[STAThread]
	public static int Main(string[] args)
	{
		return DesktopShell.A(args, A, _"Protected session", delegate(CancellationToken P_0)
		{
			_desktopStop = P_0;
		});
	}

	private static async Task<int> A(string[] P_0)
	{
		if (P_0.Length == 3 && P_0[0] == _"--finish-cleanup")
		{
			return SessionUninstall.B(P_0[1], P_0[2]);
		}
		_gameRoot = Path.GetFullPath(AppContext.BaseDirectory).TrimEnd(Path.DirectorySeparatorChar);
		_bepInExRoot = Path.Combine(_gameRoot, _"BepInEx");
		_logPath = Path.Combine(_bepInExRoot, _"GorillaGuard.Launcher.log");
		_suppressFailureDialog = P_0.Any((string P_0) => P_0.Equals(_"--verify-package", StringComparison.OrdinalIgnoreCase) || P_0.Equals(_"--self-test", StringComparison.OrdinalIgnoreCase) || P_0.Equals(_"--diagnose-steam", StringComparison.OrdinalIgnoreCase));
		_suppressFailureDialog |= P_0.Contains<string>(_"--diagnose-vr", StringComparer.OrdinalIgnoreCase);
		_suppressFailureDialog |= P_0.Contains<string>(_"--diagnose-game", StringComparer.OrdinalIgnoreCase);
		_suppressFailureDialog |= P_0.Contains<string>(_"--diagnose-evasion", StringComparer.OrdinalIgnoreCase);
		if (P_0.Contains<string>(_"--scan-only", StringComparer.OrdinalIgnoreCase))
		{
			return A(P_0.Contains<string>(_"--scan-json", StringComparer.OrdinalIgnoreCase));
		}
		if (!Directory.Exists(_bepInExRoot))
		{
			return F(_"Keep GorillaGuard.Launcher.exe beside the package's BepInEx folder, then run it again.");
		}
		if (P_0.Contains<string>(_"--rollback", StringComparer.OrdinalIgnoreCase))
		{
			try
			{
				A();
				string text = RecoveryPolicy.A(true);
				bool num = await SignedUpdateClient.A(new RemoteSettings
				{
					UpdateManifestUrl = text,
					UpdateSignatureUrl = text + _".sig",
					RequireUpdateCheck = true
				}, _gameRoot, C, false, true);
				if (num && Environment.UserInteractive)
				{
					MessageBoxW(IntPtr.Zero, _"The signed 1.12.8 installer is staged in install-only mode. This launcher will close so installation can continue. After installation finishes, run GorillaGuard.Launcher.exe --skip-update to use the rollback for that session. Normal launches check for updates again. Existing replaced files are backed up.", _"Gorilla Guard — Rollback", 64u);
				}
				return (!num) ? 1 : 0;
			}
			catch (Exception ex)
			{
				return F(_"Rollback could not start: " + ex.Message);
			}
		}
		ReleaseVerificationResult sourceVerification = ReleaseVerifier.Verify(_bepInExRoot);
		if (!sourceVerification.Success || P_0.Contains<string>(_"--repair", StringComparer.OrdinalIgnoreCase))
		{
			if (!_suppressFailureDialog && Environment.UserInteractive && MessageBoxW(IntPtr.Zero, _"File verification result:\n\n" + sourceVerification.Message + _"\n\nDownload a fresh signed package and repair? Existing replaced files will be backed up. The game must be closed.", _"Gorilla Guard — Repair", 52u) == 6)
			{
				try
				{
					A();
					if (await SignedUpdateClient.A(new RemoteSettings
					{
						UpdateManifestUrl = _"https://github.com/DevyDevelopment/GorillaGuard-Releases/releases/latest/download/GorillaGuard.update.json",
						UpdateSignatureUrl = _"https://github.com/DevyDevelopment/GorillaGuard-Releases/releases/latest/download/GorillaGuard.update.json.sig",
						RequireUpdateCheck = true
					}, _gameRoot, C, true))
					{
						return 0;
					}
				}
				catch (Exception ex2)
				{
					return F(_"Repair could not start: " + ex2.Message);
				}
			}
			if (sourceVerification.Success)
			{
				C(_"INFO", _"Repair cancelled; no files changed.");
				return 0;
			}
			return F(_"Signed package verification failed: " + sourceVerification.Message);
		}
		if (P_0.Contains<string>(_"--verify-package", StringComparer.OrdinalIgnoreCase))
		{
			C(_"INFO", sourceVerification.Message);
			return 0;
		}
		if (P_0.Contains<string>(_"--self-test", StringComparer.OrdinalIgnoreCase))
		{
			return await A(sourceVerification);
		}
		if (P_0.Contains<string>(_"--diagnose-steam", StringComparer.OrdinalIgnoreCase))
		{
			return a(sourceVerification);
		}
		if (P_0.Contains<string>(_"--diagnose-game", StringComparer.OrdinalIgnoreCase))
		{
			try
			{
				string text2 = (File.Exists(Path.Combine(_gameRoot, _"Gorilla Tag.exe")) ? _gameRoot : e());
				C(_"INFO", _"Read-only game diagnosis: " + text2);
				ReleaseVerificationResult releaseVerificationResult = ReleaseVerifier.A(text2, _bepInExRoot, false);
				if (!releaseVerificationResult.Success)
				{
					return F(releaseVerificationResult.Message);
				}
				C(_"INFO", _"GAME DIAGNOSIS PASS: game code matches a signed approved baseline.");
				return 0;
			}
			catch (Exception ex3)
			{
				return F(_"Game diagnosis failed: " + ex3.Message);
			}
		}
		if (P_0.Contains<string>(_"--diagnose-evasion", StringComparer.OrdinalIgnoreCase))
		{
			try
			{
				string text3 = (File.Exists(Path.Combine(_gameRoot, _"Gorilla Tag.exe")) ? _gameRoot : e());
				EvasionFinding evasionFinding = EvasionMonitor.A(text3, true);
				if (evasionFinding != null)
				{
					return F(A(evasionFinding));
				}
				C(_"INFO", _"EVASION DIAGNOSIS PASS: no known process, module, lineage, command, task, or file IOC was found.");
				return 0;
			}
			catch (Exception ex4)
			{
				return F(_"Evasion diagnosis failed: " + ex4.Message);
			}
		}
		if (P_0.Contains<string>(_"--diagnose-vr", StringComparer.OrdinalIgnoreCase))
		{
			try
			{
				string text4 = (File.Exists(Path.Combine(_gameRoot, _"Gorilla Tag.exe")) ? _gameRoot : e());
				using VrPreflight vrPreflight = new VrPreflight(text4, G().Concat(f()), C, _bepInExRoot);
				vrPreflight.A(false);
				ReleaseVerificationResult releaseVerificationResult2 = ReleaseVerifier.A(text4, _bepInExRoot, false);
				if (!releaseVerificationResult2.Success)
				{
					return F(_"VR checks passed, but game baseline verification failed: " + releaseVerificationResult2.Message);
				}
				C(_"INFO", _"VR DIAGNOSTIC PASS: VR environment and signed game baseline verified.");
				return 0;
			}
			catch (Exception ex5)
			{
				return F(_"VR diagnostic: " + ex5.Message);
			}
		}
		if (P_0.Contains<string>(_"--repair-game", StringComparer.OrdinalIgnoreCase))
		{
			try
			{
				string target = (File.Exists(Path.Combine(_gameRoot, _"Gorilla Tag.exe")) ? _gameRoot : e());
				if (a(target) != null)
				{
					return F(_"Close Gorilla Tag before repairing its files.");
				}
				ReleaseVerificationResult releaseVerificationResult3 = ReleaseVerifier.A(target, _bepInExRoot, false);
				if (!releaseVerificationResult3.Success)
				{
					releaseVerificationResult3 = await A(target, _bepInExRoot, releaseVerificationResult3, _desktopStop);
				}
				if (!releaseVerificationResult3.Success)
				{
					return F(_"Steam game-file repair did not complete: " + B(releaseVerificationResult3));
				}
				if (!b(target, _gameRoot))
				{
					A(_gameRoot, target);
				}
				C(_"INFO", _"REPAIR PASS: Steam-owned Gorilla Tag code and signed Gorilla Guard files verified.");
				return 0;
			}
			catch (Exception ex6)
			{
				return F(_"Game repair failed: " + ex6.Message);
			}
		}
		int? num2 = A(P_0, _"--update-parent");
		if (!File.Exists(Path.Combine(_gameRoot, _"Gorilla Tag.exe")))
		{
			return await A(P_0.Contains<string>(_"--install-elevated", StringComparer.OrdinalIgnoreCase), P_0.Contains<string>(_"--install-only", StringComparer.OrdinalIgnoreCase), num2);
		}
		bool createdNew;
		using (new Mutex(initiallyOwned: true, _"Local\\GorillaGuard.Launcher.1.12.31", out createdNew))
		{
			if (!createdNew)
			{
				return F(_"Another Gorilla Guard launcher is already running.");
			}
			using CancellationTokenSource cancel = CancellationTokenSource.CreateLinkedTokenSource(_desktopStop);
			int result;
			try
			{
				int num3;
				_ = num3 - 5;
				_ = 6;
				try
				{
					C(_"INFO", _"=== Gorilla Guard Launcher 1.12.31 starting ===");
					A();
					SessionUninstall.A(_gameRoot);
					a();
					b();
					LoaderFinding loaderFinding = LoaderMonitor.A();
					if (loaderFinding != null)
					{
						result = F(A(loaderFinding));
					}
					else
					{
						EvasionFinding evasionFinding2 = EvasionMonitor.A(_gameRoot, true);
						if (evasionFinding2 != null)
						{
							result = F(A(evasionFinding2));
						}
						else
						{
							_remoteSettings = RemoteSettings.A(_gameRoot);
							_remote = new RemoteClient(_remoteSettings, _gameRoot);
							bool flag = !P_0.Contains<string>(_"--skip-update", StringComparer.OrdinalIgnoreCase);
							if (flag)
							{
								flag = await SignedUpdateClient.A(_remoteSettings, _gameRoot, C);
							}
							if (flag)
							{
								result = 0;
							}
							else
							{
								C(_"INFO", sourceVerification.Message);
								_vrPreflight = new VrPreflight(_gameRoot, G().Concat(f()), C);
								VrPreflight.a();
								_vrPreflight.A(true);
								ReleaseVerificationResult releaseVerificationResult4 = ReleaseVerifier.A(_gameRoot, null, true);
								if (!releaseVerificationResult4.Success)
								{
									releaseVerificationResult4 = await A(_gameRoot, _bepInExRoot, releaseVerificationResult4, cancel.Token);
								}
								if (!releaseVerificationResult4.Success)
								{
									result = F(_"Signed host baseline verification failed after Steam repair: " + B(releaseVerificationResult4));
								}
								else
								{
									C(_"INFO", releaseVerificationResult4.Message);
									C();
									ReleaseVerificationResult releaseVerificationResult5 = ReleaseVerifier.Verify(_bepInExRoot);
									if (!releaseVerificationResult5.Success)
									{
										result = F(_"Signed release changed while locks were being acquired: " + releaseVerificationResult5.Message);
									}
									else
									{
										releaseVerificationResult4 = ReleaseVerifier.A(_gameRoot, null, true);
										if (!releaseVerificationResult4.Success)
										{
											result = F(_"Protected host code changed while locks were being acquired: " + releaseVerificationResult4.Message);
										}
										else
										{
											_nonce = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
											_pipeName = _"GorillaGuard-" + Guid.NewGuid().ToString(_"N");
											c();
											D();
											d();
											if (!a(true))
											{
												result = F(_"A code file appeared while protected watchers were starting.");
											}
											else
											{
												Task pipeTask = RunPipeServerAsync(cancel.Token);
												SteamLaunch steam = E();
												string text5 = _"Protected files verified and locked; exclusive BepInEx mode active; Gorilla Tag starts in 10 seconds.";
												bool flag2 = await _remote.A(text5);
												if (_remote.Enabled)
												{
													C(flag2 ? _"INFO" : _"WARNING", flag2 ? _"Remote event relay acknowledged READY." : _"Remote event relay did not acknowledge READY.");
												}
												if (_remoteSettings.RequireReadyAcknowledgement && !flag2)
												{
													result = F(_"The configured event relay did not acknowledge READY; launch was stopped by policy.");
												}
												else
												{
													int seconds = 10;
													while (true)
													{
														if (seconds > 0)
														{
															d();
															if (!a(true))
															{
																result = F(_"Pre-launch quarantine could not remove a newly added code file. Close the application using it and retry.");
																break;
															}
															string text6 = _"INFO";
															DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(50, 1);
															defaultInterpolatedStringHandler.AppendLiteral(_"Protected launch ready; starting Gorilla Tag in ");
															defaultInterpolatedStringHandler.AppendFormatted(seconds);
															defaultInterpolatedStringHandler.AppendLiteral(_"s.");
															C(text6, defaultInterpolatedStringHandler.ToStringAndClear());
															await Task.Delay(1000, cancel.Token);
															seconds--;
															continue;
														}
														C(_"INFO", _"Starting Gorilla Tag through Steam app " + steam.AppId + _".");
														loaderFinding = LoaderMonitor.A();
														if (loaderFinding != null)
														{
															result = F(A(loaderFinding));
															break;
														}
														evasionFinding2 = EvasionMonitor.A(_gameRoot, false);
														if (evasionFinding2 != null)
														{
															result = F(A(evasionFinding2));
															break;
														}
														a();
														b();
														d();
														if (!a(true))
														{
															result = F(_"Final pre-launch code quarantine failed.");
															break;
														}
														B();
														releaseVerificationResult5 = ReleaseVerifier.Verify(_bepInExRoot);
														releaseVerificationResult4 = ReleaseVerifier.A(_gameRoot, null, true);
														if (!releaseVerificationResult5.Success || !releaseVerificationResult4.Success)
														{
															result = F(_"Final pre-launch verification failed: " + ((!releaseVerificationResult5.Success) ? releaseVerificationResult5.Message : releaseVerificationResult4.Message));
															break;
														}
														VrPreflight.a();
														H();
														a(steam);
														Process process = await A(TimeSpan.FromSeconds(180.0), cancel.Token);
														_cleanupGameStarted = process.StartTime.ToUniversalTime().Ticks;
														_cleanupGamePid = process.Id;
														Volatile.Write(ref _expectedGamePid, process.Id);
														A(process);
														_cleanupArmed = SessionUninstall.A(_gameRoot, _cleanupGamePid, _cleanupGameStarted, C);
														string text7 = _"INFO";
														DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(47, 2);
														defaultInterpolatedStringHandler2.AppendLiteral(_"Protected game process found; pid=");
														defaultInterpolatedStringHandler2.AppendFormatted(process.Id);
														defaultInterpolatedStringHandler2.AppendLiteral(_"; codeLocks=");
														defaultInterpolatedStringHandler2.AppendFormatted(CodeLocks.Count);
														defaultInterpolatedStringHandler2.AppendLiteral(_".");
														C(text7, defaultInterpolatedStringHandler2.ToStringAndClear());
														int result2 = await A(process, cancel.Token);
														cancel.Cancel();
														await A(pipeTask);
														result = result2;
														break;
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
				catch (OperationCanceledException)
				{
					result = 1;
				}
				catch (Exception ex8)
				{
					result = F(ex8.Message);
				}
			}
			finally
			{
				cancel.Cancel();
				if (_jobHandle != IntPtr.Zero)
				{
					CloseHandle(_jobHandle);
					_jobHandle = IntPtr.Zero;
				}
				foreach (FileSystemWatcher watcher in Watchers)
				{
					watcher.Dispose();
				}
				foreach (FileStream codeLock in CodeLocks)
				{
					codeLock.Dispose();
				}
				_sessionFileLock?.Dispose();
				_sessionFileLock = null;
				h();
				_vrPreflight?.Dispose();
				if (_remote != null)
				{
					await _remote.A();
					_remote.Dispose();
					_remote = null;
				}
				C(_"INFO", _"Launcher stopped; file locks released.");
				if (_cleanupGamePid != 0 && !_cleanupArmed)
				{
					try
					{
						SessionUninstall.A(_gameRoot, _cleanupGamePid, _cleanupGameStarted, C);
					}
					catch (Exception ex9)
					{
						C(_"ERROR", _"Could not schedule self-uninstall; installed files preserved: " + ex9.Message);
					}
				}
			}
			return result;
		}
	}

	private static int A(bool P_0)
	{
		_003C_003Ec__DisplayClass40_0 CS_0024_003C_003E8__locals29 = new _003C_003Ec__DisplayClass40_0();
		CS_0024_003C_003E8__locals29.json = P_0;
		CS_0024_003C_003E8__locals29.started = DateTimeOffset.UtcNow;
		CS_0024_003C_003E8__locals29.findings = new List<(string, string, string)>();
		CS_0024_003C_003E8__locals29.target = null;
		CS_0024_003C_003E8__locals29.packageVerified = false;
		_scanInProgress = true;
		_scanJson = CS_0024_003C_003E8__locals29.json;
		try
		{
			CS_0024_003C_003E8__locals29.A(_"SIGNED PACKAGE", delegate
			{
				ReleaseVerificationResult releaseVerificationResult = ReleaseVerifier.Verify(_bepInExRoot);
				CS_0024_003C_003E8__locals29.packageVerified = releaseVerificationResult.Success;
				return (Status: releaseVerificationResult.Success ? _"PASS" : _"FAILED", Detail: releaseVerificationResult.Message);
			});
			CS_0024_003C_003E8__locals29.A(_"GAME DISCOVERY", delegate
			{
				CS_0024_003C_003E8__locals29.target = (File.Exists(Path.Combine(_gameRoot, _"Gorilla Tag.exe")) ? _gameRoot : e());
				return (Status: _"PASS", Detail: CS_0024_003C_003E8__locals29.target);
			});
			CS_0024_003C_003E8__locals29.A(_"GAME BASELINE", delegate
			{
				if (!CS_0024_003C_003E8__locals29.packageVerified || CS_0024_003C_003E8__locals29.target == null)
				{
					return (Status: _"INCOMPLETE", Detail: _"Requires a verified package and a game installation.");
				}
				ReleaseVerificationResult releaseVerificationResult2 = ReleaseVerifier.A(CS_0024_003C_003E8__locals29.target, _bepInExRoot, b(CS_0024_003C_003E8__locals29.target, _gameRoot));
				return (!releaseVerificationResult2.Success) ? (Status: _"FAILED", Detail: B(releaseVerificationResult2)) : (Status: _"PASS", Detail: releaseVerificationResult2.Message);
			});
			CS_0024_003C_003E8__locals29.A(_"LOADER", delegate
			{
				LoaderFinding loaderFinding = LoaderMonitor.A();
				return (!(loaderFinding == null)) ? (Status: _"FAILED", Detail: A(loaderFinding)) : (Status: _"PASS", Detail: _"No known loader found.");
			});
			CS_0024_003C_003E8__locals29.A(_"EVASION", delegate
			{
				List<string> warnings = new List<string>();
				EvasionFinding evasionFinding = new KnownEvasionMonitor(delegate(string P_0, string P_1)
				{
					if (P_0 == _"WARNING")
					{
						warnings.Add(P_1);
					}
				}).A(CS_0024_003C_003E8__locals29.target ?? _gameRoot, CS_0024_003C_003E8__locals29.target != null);
				if (evasionFinding != null)
				{
					return (Status: _"FAILED", Detail: A(evasionFinding));
				}
				if (CS_0024_003C_003E8__locals29.target == null)
				{
					warnings.Add(_"File and scheduled-task checks skipped because game discovery did not complete.");
				}
				return (warnings.Count != 0) ? (Status: _"INCOMPLETE", Detail: string.Join(_" ", warnings)) : (Status: _"PASS", Detail: _"No known indicator found.");
			});
			CS_0024_003C_003E8__locals29.A(_"VR", delegate
			{
				if (!CS_0024_003C_003E8__locals29.packageVerified || CS_0024_003C_003E8__locals29.target == null)
				{
					return (Status: _"INCOMPLETE", Detail: _"Requires a verified package and a game installation.");
				}
				using VrPreflight vrPreflight = new VrPreflight(CS_0024_003C_003E8__locals29.target, G().Concat(f()), C, _bepInExRoot);
				vrPreflight.A(false);
				return (Status: _"PASS", Detail: _"Registered runtimes, drivers, layers, and restricted settings audited.");
			});
			string text = (CS_0024_003C_003E8__locals29.findings.Any<(string, string, string)>(((string Check, string Status, string Detail) f) => f.Status == _"FAILED") ? _"FAILED" : (CS_0024_003C_003E8__locals29.findings.Any<(string, string, string)>(((string Check, string Status, string Detail) f) => f.Status == _"INCOMPLETE") ? _"INCOMPLETE" : _"PASS"));
			CS_0024_003C_003E8__locals29.A(text);
			return (!(text == _"PASS")) ? ((text == _"FAILED") ? 1 : 2) : 0;
		}
		catch (OperationCanceledException)
		{
			CS_0024_003C_003E8__locals29.A(_"CANCELLED");
			return 1;
		}
		finally
		{
			_scanInProgress = false;
			_scanJson = false;
		}
	}

	private static async Task<int> A(ReleaseVerificationResult P_0)
	{
		C(_"INFO", P_0.Message);
		string testFile = Path.Combine(_bepInExRoot, _".GorillaGuard.lock-test-" + Guid.NewGuid().ToString(_"N"));
		string stage = _"initialization";
		try
		{
			stage = _"root code policy";
			string[] array = new string[7]
			{
				_"Gorilla Tag.exe",
				_"GorillaGuard.Launcher.exe",
				_"UnityPlayer.dll",
				_"UnityCrashHandler64.exe",
				_"UnityCrashHandler32.exe",
				_"GameAssembly.dll",
				_"winhttp.dll"
			};
			foreach (string text in array)
			{
				if (!A(text))
				{
					throw new InvalidOperationException(_"Expected root code was not allowed: " + text);
				}
			}
			array = new string[4]
			{
				_"dxgi.dll",
				_"dinput8.dll",
				_"version.dll",
				_"unknown-loader.exe"
			};
			foreach (string text2 in array)
			{
				if (A(text2))
				{
					throw new InvalidOperationException(_"Unexpected root code was allowed: " + text2);
				}
			}
			C(_"INFO", _"SELF TEST PASS: root-level native sideload allowlist.");
			stage = _"file lock";
			File.WriteAllText(testFile, _"lock-test", new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
			using (new FileStream(testFile, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				bool flag = false;
				try
				{
					using (new FileStream(testFile, FileMode.Open, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete))
					{
					}
				}
				catch (IOException)
				{
					flag = true;
				}
				if (!flag)
				{
					throw new InvalidOperationException(_"Windows did not enforce the expected file sharing lock.");
				}
				bool flag2 = false;
				try
				{
					File.Delete(testFile);
				}
				catch (IOException)
				{
					flag2 = true;
				}
				catch (UnauthorizedAccessException)
				{
					flag2 = true;
				}
				if (!flag2)
				{
					throw new InvalidOperationException(_"Windows did not enforce the expected delete sharing lock.");
				}
			}
			File.Delete(testFile);
			C(_"INFO", _"SELF TEST PASS: write/delete file lock semantics.");
			stage = _"job object";
			using Process child = Process.Start(new ProcessStartInfo
			{
				FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), _"ping.exe"),
				Arguments = _"-n 30 127.0.0.1",
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			}) ?? throw new InvalidOperationException(_"Could not create the harmless job-object test process.");
			if (!CheckRemoteDebuggerPresent(child.Handle, out var debuggerPresent))
			{
				throw new InvalidOperationException(_"Windows debugger-state query failed. Win32=" + Marshal.GetLastWin32Error());
			}
			if (debuggerPresent)
			{
				throw new InvalidOperationException(_"The harmless self-test process unexpectedly reports an attached debugger.");
			}
			C(_"INFO", _"SELF TEST PASS: external debugger-state query.");
			A(child);
			CloseHandle(_jobHandle);
			_jobHandle = IntPtr.Zero;
			if (!child.WaitForExit(5000))
			{
				try
				{
					child.Kill(entireProcessTree: true);
				}
				catch
				{
				}
				throw new InvalidOperationException(_"Closing the job handle did not terminate its test process.");
			}
			C(_"INFO", _"SELF TEST PASS: kill-on-close Windows job object.");
			_nonce = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
			_pipeName = _"GorillaGuard-SelfTest-" + Guid.NewGuid().ToString(_"N");
			Volatile.Write(ref _expectedGamePid, Environment.ProcessId);
			stage = _"pipe server startup";
			using (CancellationTokenSource pipeCancel = new CancellationTokenSource())
			{
				Task task = RunPipeServerAsync(pipeCancel.Token);
				stage = _"out-of-order runtime rejection";
				try
				{
					using NamedPipeClientStream namedPipeClientStream = new NamedPipeClientStream(_".", _pipeName, PipeDirection.InOut, PipeOptions.None);
					namedPipeClientStream.Connect(5000);
					using StreamReader streamReader = new StreamReader(namedPipeClientStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, 1024, leaveOpen: true);
					using StreamWriter streamWriter = new StreamWriter(namedPipeClientStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 1024, leaveOpen: true)
					{
						AutoFlush = true
					};
					streamWriter.WriteLine(_"HELLO|" + _nonce + _"|1.12.31");
					streamReader.ReadLine();
				}
				catch (IOException)
				{
				}
				if (!_outOfOrderRuntimeRejected || _runtimeAuthenticated)
				{
					throw new InvalidOperationException(_"The launcher did not record the expected runtime-before-preloader rejection.");
				}
				C(_"INFO", _"SELF TEST PASS: runtime-before-preloader connection rejected.");
				stage = _"preloader pipe proof";
				using (NamedPipeClientStream namedPipeClientStream2 = new NamedPipeClientStream(_".", _pipeName, PipeDirection.InOut, PipeOptions.None))
				{
					namedPipeClientStream2.Connect(5000);
					using StreamReader streamReader2 = new StreamReader(namedPipeClientStream2, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, 1024, leaveOpen: true);
					using StreamWriter streamWriter2 = new StreamWriter(namedPipeClientStream2, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 1024, leaveOpen: true)
					{
						AutoFlush = true
					};
					streamWriter2.WriteLine(_"PRELOAD|" + _nonce + _"|1.12.31");
					if (!string.Equals(streamReader2.ReadLine(), _"PRELOAD_OK|" + _nonce, StringComparison.Ordinal))
					{
						throw new InvalidOperationException(_"The PID-bound preloader proof failed.");
					}
				}
				stage = _"runtime pipe proof";
				using (NamedPipeClientStream namedPipeClientStream3 = new NamedPipeClientStream(_".", _pipeName, PipeDirection.InOut, PipeOptions.None))
				{
					namedPipeClientStream3.Connect(5000);
					using StreamReader streamReader3 = new StreamReader(namedPipeClientStream3, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, 1024, leaveOpen: true);
					using StreamWriter streamWriter3 = new StreamWriter(namedPipeClientStream3, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 1024, leaveOpen: true)
					{
						AutoFlush = true
					};
					streamWriter3.WriteLine(_"HELLO|" + _nonce + _"|1.12.31");
					if (!string.Equals(streamReader3.ReadLine(), _"OK|" + _nonce, StringComparison.Ordinal))
					{
						throw new InvalidOperationException(_"The PID-bound named-pipe handshake failed.");
					}
					stage = _"nonfatal runtime block report";
					string text3 = Convert.ToBase64String(Encoding.UTF8.GetBytes(_"self-test contained block"));
					streamWriter3.WriteLine(_"BLOCK|" + _nonce + _"|SELF TEST|" + text3);
					if (!string.Equals(streamReader3.ReadLine(), _"ACK|" + _nonce, StringComparison.Ordinal) || !string.IsNullOrWhiteSpace(_runtimeViolation))
					{
						throw new InvalidOperationException(_"A contained runtime block was incorrectly treated as fatal.");
					}
					streamWriter3.WriteLine(_"PING|" + _nonce);
					if (!string.Equals(streamReader3.ReadLine(), _"PONG|" + _nonce, StringComparison.Ordinal))
					{
						throw new InvalidOperationException(_"The authenticated session did not remain alive after a contained block.");
					}
				}
				pipeCancel.Cancel();
				await A(task);
			}
			_preloaderAuthenticated = false;
			_runtimeAuthenticated = false;
			_outOfOrderRuntimeRejected = false;
			Volatile.Write(ref _expectedGamePid, 0);
			C(_"INFO", _"SELF TEST PASS: ordered preloader/runtime kernel PID-bound pipe proofs.");
			C(_"INFO", _"SELF TEST PASS: contained runtime block keeps the protected session alive.");
			await Task.CompletedTask;
			return 0;
		}
		catch (Exception ex5)
		{
			return F(_"SELF TEST FAILED during " + stage + _": " + ex5.Message);
		}
		finally
		{
			try
			{
				if (File.Exists(testFile))
				{
					File.Delete(testFile);
				}
			}
			catch
			{
			}
			if (_jobHandle != IntPtr.Zero)
			{
				CloseHandle(_jobHandle);
				_jobHandle = IntPtr.Zero;
			}
		}
	}

	private static int a(ReleaseVerificationResult P_0)
	{
		C(_"INFO", P_0.Message);
		try
		{
			List<string> list = G().ToList();
			List<string> list2 = f().ToList();
			foreach (string item in list)
			{
				C(_"INFO", _"Steam client candidate: " + item);
			}
			foreach (string item2 in list2)
			{
				C(_"INFO", _"Steam library candidate: " + item2);
			}
			string text = e();
			string text2 = g();
			C(_"INFO", _"STEAM DISCOVERY PASS: Gorilla Tag=" + text);
			C(_"INFO", _"STEAM DISCOVERY PASS: launcher=" + (text2 ?? _"steam:// protocol fallback"));
			return 0;
		}
		catch (Exception ex)
		{
			return F(_"Steam discovery diagnostic failed: " + ex.Message);
		}
	}

	private static async Task<int> A(bool P_0, bool P_1, int? P_2)
	{
		_ = 1;
		try
		{
			if (P_2.HasValue)
			{
				string text = _"INFO";
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(51, 1);
				defaultInterpolatedStringHandler.AppendLiteral(_"Verified updater waiting for launcher pid=");
				defaultInterpolatedStringHandler.AppendFormatted(P_2.Value);
				defaultInterpolatedStringHandler.AppendLiteral(_" to exit.");
				C(text, defaultInterpolatedStringHandler.ToStringAndClear());
				try
				{
					using Process process = Process.GetProcessById(P_2.Value);
					if (!process.WaitForExit(90000))
					{
						return F(_"The previous launcher did not exit in time for the signed update.");
					}
				}
				catch (ArgumentException)
				{
				}
			}
			string sourceRoot = _gameRoot;
			_desktopStop.ThrowIfCancellationRequested();
			string targetRoot = e();
			if (a(targetRoot) != null)
			{
				return F(_"Close Gorilla Tag before installing Gorilla Guard.");
			}
			ReleaseVerificationResult releaseVerificationResult = ReleaseVerifier.A(targetRoot, Path.Combine(sourceRoot, _"BepInEx"), false);
			if (!releaseVerificationResult.Success)
			{
				releaseVerificationResult = await A(targetRoot, Path.Combine(sourceRoot, _"BepInEx"), releaseVerificationResult, _desktopStop);
			}
			if (!releaseVerificationResult.Success)
			{
				return F(_"Steam game-file repair did not complete: " + B(releaseVerificationResult));
			}
			try
			{
				A(sourceRoot, targetRoot);
			}
			catch (Exception ex2) when (!P_0 && A(ex2))
			{
				C(_"INFO", _"Installation needs administrator permission; requesting Windows approval.");
				_desktopStop.ThrowIfCancellationRequested();
				Process.Start(new ProcessStartInfo
				{
					FileName = Path.Combine(sourceRoot, _"GorillaGuard.Launcher.exe"),
					Arguments = (P_1 ? _"--install-elevated --install-only" : _"--install-elevated") + (P_2.HasValue ? (_" --update-parent " + P_2.Value) : _""),
					WorkingDirectory = sourceRoot,
					Verb = _"runas",
					UseShellExecute = true
				});
				return 0;
			}
			string text2 = Path.Combine(targetRoot, _"GorillaGuard.Launcher.exe");
			_desktopStop.ThrowIfCancellationRequested();
			C(_"INFO", _"Installation verified at " + targetRoot);
			if (P_1)
			{
				if (Environment.UserInteractive)
				{
					MessageBoxW(IntPtr.Zero, _"Signed installation/repair verified at:\n" + targetRoot + _"\n\nReplaced files are backed up under BepInEx\\quarantine\\install-backups. See GorillaGuard.Launcher.log for the exact backup path.\n\nFor a rollback session, start GorillaGuard.Launcher.exe --skip-update. Normal launches check for updates again.", _"Gorilla Guard — Installation complete", 64u);
				}
				return 0;
			}
			if (I())
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = _"explorer.exe",
					Arguments = _"\"" + text2 + _"\"",
					UseShellExecute = true
				});
			}
			else
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = text2,
					WorkingDirectory = targetRoot,
					UseShellExecute = true
				});
			}
			await Task.CompletedTask;
			return 0;
		}
		catch (Exception ex3)
		{
			return F(_"Installation failed: " + ex3.Message);
		}
	}

	private static void A(string P_0, string P_1)
	{
		string text = Path.Combine(P_0, _"BepInEx");
		ReleaseVerificationResult releaseVerificationResult = ReleaseVerifier.Verify(text);
		if (!releaseVerificationResult.Success)
		{
			throw new InvalidDataException(_"Source package is not trusted: " + releaseVerificationResult.Message);
		}
		string text2 = Path.Combine(P_1, _"BepInEx");
		using (VrPreflight vrPreflight = new VrPreflight(P_1, Array.Empty<string>(), C, text))
		{
			vrPreflight.a(true);
		}
		ReleaseVerificationResult releaseVerificationResult2 = ReleaseVerifier.A(P_1, text, false);
		if (!releaseVerificationResult2.Success)
		{
			throw new InvalidDataException(_"The target game/BepInEx files do not match this signed package: " + releaseVerificationResult2.Message);
		}
		UninstallPlan uninstallPlan = SessionUninstall.A(P_0, P_1);
		string text3 = Path.Combine(text2, _"quarantine", _"install-backups", _"pre-1.12.31-" + DateTime.UtcNow.ToString(_"yyyyMMdd-HHmmssfff"));
		if (a(text3, P_1))
		{
			throw new InvalidDataException(_"Refusing to create an installation backup through a redirected/reparse-point directory.");
		}
		A(text, text2, text3, P_1);
		InstallFile[] array = new InstallFile[8]
		{
			new InstallFile(Path.Combine(P_0, _"GorillaGuard.Launcher.exe"), Path.Combine(P_1, _"GorillaGuard.Launcher.exe")),
			new InstallFile(Path.Combine(text, _"GorillaGuard.manifest"), Path.Combine(text2, _"GorillaGuard.manifest")),
			new InstallFile(Path.Combine(text, _"GorillaGuard.manifest.sig"), Path.Combine(text2, _"GorillaGuard.manifest.sig")),
			new InstallFile(Path.Combine(text, _"plugins", _"GorillaGuard.dll"), Path.Combine(text2, _"plugins", _"GorillaGuard.dll")),
			new InstallFile(Path.Combine(text, _"patchers", _"GorillaGuard.Preloader.dll"), Path.Combine(text2, _"patchers", _"GorillaGuard.Preloader.dll")),
			new InstallFile(Path.Combine(text, _"repair", _"openvr_api.dll"), Path.Combine(text2, _"repair", _"openvr_api.dll")),
			new InstallFile(Path.Combine(P_0, _"winhttp.dll"), Path.Combine(P_1, _"winhttp.dll")),
			new InstallFile(Path.Combine(P_0, _"doorstop_config.ini"), Path.Combine(P_1, _"doorstop_config.ini"))
		};
		foreach (InstallFile installFile in array)
		{
			if (!File.Exists(installFile.Source))
			{
				throw new FileNotFoundException(_"The signed package is incomplete.", installFile.Source);
			}
			if (a(installFile.Destination, P_1))
			{
				throw new InvalidDataException(_"Refusing to install through a redirected/reparse-point directory: " + installFile.Destination);
			}
			if (File.Exists(installFile.Destination))
			{
				Directory.CreateDirectory(text3);
				string text4 = Path.Combine(text3, Path.GetFileName(installFile.Destination));
				if (File.Exists(text4))
				{
					text4 = text4 + _"." + Guid.NewGuid().ToString(_"N");
				}
				File.Copy(installFile.Destination, text4, overwrite: false);
			}
			Directory.CreateDirectory(Path.GetDirectoryName(installFile.Destination));
			File.Copy(installFile.Source, installFile.Destination, overwrite: true);
		}
		string text5 = Path.Combine(P_0, _"GorillaGuard.remote.json");
		string text6 = Path.Combine(P_1, _"GorillaGuard.remote.json");
		if (File.Exists(text5) && !File.Exists(text6))
		{
			File.Copy(text5, text6, overwrite: false);
		}
		ReleaseVerificationResult releaseVerificationResult3 = ReleaseVerifier.Verify(text2);
		if (!releaseVerificationResult3.Success)
		{
			throw new InvalidDataException(_"Installed files did not pass verification: " + releaseVerificationResult3.Message);
		}
		ReleaseVerificationResult releaseVerificationResult4 = ReleaseVerifier.A(P_1, null, true);
		if (!releaseVerificationResult4.Success)
		{
			throw new InvalidDataException(_"Installed host baseline did not pass verification: " + releaseVerificationResult4.Message);
		}
		SessionUninstall.A(uninstallPlan, text3);
		C(_"INFO", _"Signed installation verified; replaced-file backups: " + text3);
	}

	private static void A(string P_0, string P_1, string P_2, string P_3)
	{
		string text = Path.Combine(P_0, _"core");
		string text2 = Path.Combine(P_1, _"core");
		if (!Directory.Exists(text))
		{
			throw new DirectoryNotFoundException(_"The signed package is missing its BepInEx core directory: " + text);
		}
		if (a(text2, P_3))
		{
			throw new InvalidDataException(_"Refusing to install BepInEx core through a redirected/reparse-point directory.");
		}
		Directory.CreateDirectory(text2);
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach (string item in B(text, true))
		{
			if (e(item))
			{
				string relativePath = Path.GetRelativePath(text, item);
				dictionary.Add(relativePath, item);
			}
		}
		if (dictionary.Count == 0)
		{
			throw new InvalidDataException(_"The signed package contains no BepInEx core code files.");
		}
		string[] array = B(text2, true).Where(e).ToArray();
		foreach (string text3 in array)
		{
			string relativePath2 = Path.GetRelativePath(text2, text3);
			if (!dictionary.ContainsKey(relativePath2))
			{
				string text4 = Path.Combine(P_2, _"BepInEx", _"core", relativePath2);
				Directory.CreateDirectory(Path.GetDirectoryName(text4));
				if (File.Exists(text4))
				{
					text4 = text4 + _"." + Guid.NewGuid().ToString(_"N");
				}
				File.Move(text3, text4);
				C(_"WARNING", _"BLOCKED [BEPINEX CORE] source=" + text3 + _"; action=installer backup/quarantine; destination=" + text4);
			}
		}
		foreach (KeyValuePair<string, string> item2 in dictionary)
		{
			string text5 = Path.Combine(text2, item2.Key);
			if (a(text5, P_3))
			{
				throw new InvalidDataException(_"Refusing to install through a redirected/reparse-point directory: " + text5);
			}
			if (File.Exists(text5))
			{
				string text6 = Path.Combine(P_2, _"BepInEx", _"core", item2.Key);
				Directory.CreateDirectory(Path.GetDirectoryName(text6));
				if (File.Exists(text6))
				{
					text6 = text6 + _"." + Guid.NewGuid().ToString(_"N");
				}
				File.Copy(text5, text6, overwrite: false);
			}
			Directory.CreateDirectory(Path.GetDirectoryName(text5));
			File.Copy(item2.Value, text5, overwrite: true);
		}
	}

	private static void A()
	{
		if (a(_gameRoot) != null)
		{
			throw new InvalidOperationException(_"Gorilla Tag is already running. Close it, then start it with GorillaGuard.Launcher.exe.");
		}
	}

	private static void a()
	{
		A(Path.Combine(_bepInExRoot, _"plugins"), _"GorillaGuard.dll", _"plugins");
		A(Path.Combine(_bepInExRoot, _"patchers"), _"GorillaGuard.Preloader.dll", _"patchers");
	}

	private static void B()
	{
		(string, string)[] array = new(string, string)[2]
		{
			(Path.Combine(_bepInExRoot, _"plugins"), _"GorillaGuard.dll"),
			(Path.Combine(_bepInExRoot, _"patchers"), _"GorillaGuard.Preloader.dll")
		};
		for (int i = 0; i < array.Length; i++)
		{
			(string, string) tuple = array[i];
			PrelaunchGuard.A(tuple.Item1, tuple.Item2);
		}
	}

	private static void b()
	{
		string[] array = B(_gameRoot, false).ToArray();
		foreach (string text in array)
		{
			if (e(text) && !A(Path.GetFileName(text)))
			{
				a(text, _gameRoot, _"game-root-sideload");
			}
		}
	}

	private static bool A(string P_0)
	{
		if (!P_0.Equals(_"Gorilla Tag.exe", StringComparison.OrdinalIgnoreCase) && !P_0.Equals(_"GorillaGuard.Launcher.exe", StringComparison.OrdinalIgnoreCase) && !P_0.Equals(_"UnityPlayer.dll", StringComparison.OrdinalIgnoreCase) && !P_0.Equals(_"UnityCrashHandler64.exe", StringComparison.OrdinalIgnoreCase) && !P_0.Equals(_"UnityCrashHandler32.exe", StringComparison.OrdinalIgnoreCase) && !P_0.Equals(_"GameAssembly.dll", StringComparison.OrdinalIgnoreCase))
		{
			return P_0.Equals(_"winhttp.dll", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static void A(string P_0, string P_1, string P_2)
	{
		if (!Directory.Exists(P_0))
		{
			return;
		}
		string fullPath = Path.GetFullPath(Path.Combine(P_0, P_1));
		string[] array = B(P_0, true).ToArray();
		foreach (string text in array)
		{
			if (e(text) && !b(text, fullPath))
			{
				a(text, P_0, P_2);
			}
		}
	}

	private static void a(string P_0, string P_1, string P_2)
	{
		string fullPath = Path.GetFullPath(P_0);
		string text = Path.GetFullPath(P_1).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
		if (!fullPath.StartsWith(text, StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidOperationException(_"Refusing to quarantine code outside its guarded directory.");
		}
		if (a(fullPath, P_1))
		{
			throw new InvalidDataException(_"Refusing to quarantine through a redirected/reparse-point directory: " + fullPath);
		}
		if (File.Exists(fullPath))
		{
			string text2 = fullPath.Substring(text.Length);
			string text3 = Path.Combine(_bepInExRoot, _"quarantine", _"launcher", DateTime.UtcNow.ToString(_"yyyyMMdd-HHmmssfff"), P_2, text2);
			Directory.CreateDirectory(Path.GetDirectoryName(text3));
			if (File.Exists(text3))
			{
				text3 = text3 + _"." + Guid.NewGuid().ToString(_"N") + _".blocked";
			}
			File.Move(fullPath, text3);
			SessionUninstall.a(_gameRoot, text3);
			C(_"WARNING", _"BLOCKED [CODE FILE] source=" + fullPath + _"; action=launcher quarantine; destination=" + text3);
			_remote?.A(_"WARNING", _"FILE_BLOCKED", _"Executable code was quarantined: file=" + Path.GetFileName(fullPath) + _"; scope=" + P_2);
			WorkQueue<LauncherEvent> pendingRuntimeEvents = PendingRuntimeEvents;
			string category = _"FILE";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(35, 3);
			defaultInterpolatedStringHandler.AppendFormatted(Path.GetFileName(fullPath));
			defaultInterpolatedStringHandler.AppendLiteral(_" quarantined; source=");
			defaultInterpolatedStringHandler.AppendFormatted(fullPath);
			defaultInterpolatedStringHandler.AppendLiteral(_"; destination=");
			defaultInterpolatedStringHandler.AppendFormatted(text3);
			pendingRuntimeEvents.A(new LauncherEvent(category, defaultInterpolatedStringHandler.ToStringAndClear()));
		}
	}

	private static void C()
	{
		HashSet<string> obj = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			Path.Combine(_gameRoot, _"GorillaGuard.Launcher.exe"),
			Path.Combine(_bepInExRoot, _"GorillaGuard.manifest"),
			Path.Combine(_bepInExRoot, _"GorillaGuard.manifest.sig"),
			Path.Combine(_bepInExRoot, _"plugins", _"GorillaGuard.dll"),
			Path.Combine(_bepInExRoot, _"patchers", _"GorillaGuard.Preloader.dll"),
			Path.Combine(_gameRoot, _"doorstop_config.ini")
		};
		A(obj, _gameRoot, SearchOption.TopDirectoryOnly);
		A(obj, Path.Combine(_gameRoot, _"Gorilla Tag_Data", _"Managed"), SearchOption.AllDirectories);
		A(obj, Path.Combine(_gameRoot, _"Gorilla Tag_Data", _"Plugins"), SearchOption.AllDirectories);
		A(obj, Path.Combine(_bepInExRoot, _"core"), SearchOption.AllDirectories);
		A(obj, Path.Combine(_bepInExRoot, _"repair"), SearchOption.AllDirectories);
		foreach (string item in obj.OrderBy<string, string>((string P_0) => P_0, StringComparer.OrdinalIgnoreCase))
		{
			if (!File.Exists(item))
			{
				throw new FileNotFoundException(_"A required protected code file is missing.", item);
			}
			try
			{
				CodeLocks.Add(new FileStream(item, FileMode.Open, FileAccess.Read, FileShare.Read));
				LockedCodePaths.Add(Path.GetFullPath(item));
			}
			catch (Exception ex)
			{
				throw new IOException(_"Could not lock protected code file " + item + _": " + ex.Message, ex);
			}
		}
		string text = _"INFO";
		DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(67, 1);
		defaultInterpolatedStringHandler.AppendLiteral(_"Locked ");
		defaultInterpolatedStringHandler.AppendFormatted(CodeLocks.Count);
		defaultInterpolatedStringHandler.AppendLiteral(_" executable/code files against replacement for this session.");
		C(text, defaultInterpolatedStringHandler.ToStringAndClear());
	}

	private static void A(HashSet<string> P_0, string P_1, SearchOption P_2)
	{
		if (!Directory.Exists(P_1))
		{
			return;
		}
		foreach (string item in B(P_1, P_2 == SearchOption.AllDirectories))
		{
			if (e(item))
			{
				P_0.Add(Path.GetFullPath(item));
			}
		}
	}

	private static void c()
	{
		string text = SessionLocation.A(_gameRoot);
		Directory.CreateDirectory(Path.GetDirectoryName(text));
		string text2 = text + _".tmp-" + Guid.NewGuid().ToString(_"N");
		DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(47, 10);
		defaultInterpolatedStringHandler.AppendLiteral(_"version=");
		defaultInterpolatedStringHandler.AppendFormatted(_"1.12.31");
		defaultInterpolatedStringHandler.AppendFormatted(Environment.NewLine);
		defaultInterpolatedStringHandler.AppendLiteral(_"pipe=");
		defaultInterpolatedStringHandler.AppendFormatted(_pipeName);
		defaultInterpolatedStringHandler.AppendFormatted(Environment.NewLine);
		defaultInterpolatedStringHandler.AppendLiteral(_"nonce=");
		defaultInterpolatedStringHandler.AppendFormatted(_nonce);
		defaultInterpolatedStringHandler.AppendFormatted(Environment.NewLine);
		defaultInterpolatedStringHandler.AppendLiteral(_"launcherPid=");
		defaultInterpolatedStringHandler.AppendFormatted(Environment.ProcessId);
		defaultInterpolatedStringHandler.AppendFormatted(Environment.NewLine);
		defaultInterpolatedStringHandler.AppendLiteral(_"createdUtcTicks=");
		defaultInterpolatedStringHandler.AppendFormatted(DateTime.UtcNow.Ticks);
		defaultInterpolatedStringHandler.AppendFormatted(Environment.NewLine);
		string contents = defaultInterpolatedStringHandler.ToStringAndClear();
		File.WriteAllText(text2, contents, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
		File.Move(text2, text, overwrite: true);
		_sessionFileLock = new FileStream(text, FileMode.Open, FileAccess.Read, FileShare.Read);
		C(_"INFO", _"Authenticated runtime session created and locked against modification.");
	}

	private static void D()
	{
		A(_gameRoot, false);
		A(Path.Combine(_gameRoot, _"Gorilla Tag_Data", _"Managed"), true);
		A(Path.Combine(_gameRoot, _"Gorilla Tag_Data", _"Plugins"), true);
		A(Path.Combine(_bepInExRoot, _"core"), true);
		A(Path.Combine(_bepInExRoot, _"plugins"), true);
		A(Path.Combine(_bepInExRoot, _"patchers"), true);
	}

	private static void A(string P_0, bool P_1)
	{
		Directory.CreateDirectory(P_0);
		FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(P_0)
		{
			IncludeSubdirectories = P_1,
			NotifyFilter = (NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.CreationTime),
			InternalBufferSize = 65536,
			EnableRaisingEvents = true
		};
		fileSystemWatcher.Created += delegate(object P_0, FileSystemEventArgs P_1)
		{
			PendingCodeFiles.A(P_1.FullPath);
		};
		fileSystemWatcher.Renamed += delegate(object P_0, RenamedEventArgs P_1)
		{
			PendingCodeFiles.A(P_1.FullPath);
		};
		fileSystemWatcher.Error += delegate(object P_0, ErrorEventArgs P_1)
		{
			_watcherFailure = P_1.GetException()?.Message ?? _"Unknown file watcher failure";
		};
		Watchers.Add(fileSystemWatcher);
	}

	private static void d()
	{
		a(_gameRoot, false);
		a(Path.Combine(_gameRoot, _"Gorilla Tag_Data", _"Managed"), true);
		a(Path.Combine(_gameRoot, _"Gorilla Tag_Data", _"Plugins"), true);
		a(Path.Combine(_bepInExRoot, _"core"), true);
		a(Path.Combine(_bepInExRoot, _"plugins"), true);
		a(Path.Combine(_bepInExRoot, _"patchers"), true);
	}

	private static void a(string P_0, bool P_1)
	{
		foreach (string item in B(P_0, P_1))
		{
			if (e(item) && !d(item))
			{
				PendingCodeFiles.A(item);
			}
		}
	}

	private static async Task RunPipeServerAsync(CancellationToken cancellationToken)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			try
			{
				using NamedPipeServerStream pipe = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
				await pipe.WaitForConnectionAsync(cancellationToken);
				int num = Volatile.Read(in _expectedGamePid);
				if (!GetNamedPipeClientProcessId(pipe.SafePipeHandle.DangerousGetHandle(), out var clientProcessId) || num <= 0 || clientProcessId != (uint)num)
				{
					string text = _"WARNING";
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(56, 2);
					defaultInterpolatedStringHandler.AppendLiteral(_"BLOCKED [PIPE CLIENT] unexpected client pid=");
					defaultInterpolatedStringHandler.AppendFormatted(clientProcessId);
					defaultInterpolatedStringHandler.AppendLiteral(_"; expected=");
					defaultInterpolatedStringHandler.AppendFormatted(num);
					defaultInterpolatedStringHandler.AppendLiteral(_".");
					C(text, defaultInterpolatedStringHandler.ToStringAndClear());
					continue;
				}
				using StreamReader reader = new StreamReader(pipe, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, 1024, leaveOpen: true);
				using StreamWriter writer = new StreamWriter(pipe, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 1024, leaveOpen: true)
				{
					AutoFlush = true
				};
				string text2 = await PipeProtocol.A(reader, 256, TimeSpan.FromSeconds(3.0), cancellationToken);
				if (string.Equals(text2, _"PRELOAD|" + _nonce + _"|1.12.31", StringComparison.Ordinal))
				{
					if (_preloaderAuthenticated || _runtimeAuthenticated)
					{
						C(_"WARNING", _"BLOCKED [PRELOADER PROOF] duplicate or out-of-order proof.");
						continue;
					}
					_preloaderAuthenticated = true;
					await writer.WriteLineAsync(_"PRELOAD_OK|" + _nonce);
					C(_"INFO", _"GorillaGuard.Preloader.dll authenticated before the runtime connection.");
					continue;
				}
				if (!string.Equals(text2, _"HELLO|" + _nonce + _"|1.12.31", StringComparison.Ordinal))
				{
					C(_"WARNING", _"BLOCKED [PIPE CLIENT] invalid launcher handshake.");
					continue;
				}
				if (!_preloaderAuthenticated)
				{
					_outOfOrderRuntimeRejected = true;
					C(_"ERROR", _"BLOCKED [RUNTIME ORDER] runtime attempted to authenticate before the preloader proof.");
					continue;
				}
				_runtimeAuthenticated = true;
				Interlocked.Exchange(ref _lastHeartbeatTicks, DateTime.UtcNow.Ticks);
				await writer.WriteLineAsync(_"OK|" + _nonce);
				C(_"INFO", _"GorillaGuard.dll authenticated to the launcher.");
				while (pipe.IsConnected && !cancellationToken.IsCancellationRequested)
				{
					string text3 = await PipeProtocol.A(reader, 8192, TimeSpan.FromSeconds(20.0), cancellationToken);
					if (text3 == null)
					{
						break;
					}
					if (string.Equals(text3, _"PING|" + _nonce, StringComparison.Ordinal))
					{
						Interlocked.Exchange(ref _lastHeartbeatTicks, DateTime.UtcNow.Ticks);
						if (PendingRuntimeEvents.A(out LauncherEvent launcherEvent))
						{
							string value = Convert.ToBase64String(Encoding.UTF8.GetBytes(launcherEvent.Details));
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(8, 3);
							defaultInterpolatedStringHandler2.AppendLiteral(_"EVENT|");
							defaultInterpolatedStringHandler2.AppendFormatted(_nonce);
							defaultInterpolatedStringHandler2.AppendLiteral(_"|");
							defaultInterpolatedStringHandler2.AppendFormatted(launcherEvent.Category);
							defaultInterpolatedStringHandler2.AppendLiteral(_"|");
							defaultInterpolatedStringHandler2.AppendFormatted(value);
							await writer.WriteLineAsync(defaultInterpolatedStringHandler2.ToStringAndClear());
						}
						else
						{
							await writer.WriteLineAsync(_"PONG|" + _nonce);
						}
						continue;
					}
					if (text3 != null && text3.StartsWith(_"ALERT|" + _nonce + _"|", StringComparison.Ordinal))
					{
						string[] array = text3.Split(new char[1] { '|' }, 4);
						string text4 = ((array.Length > 2) ? A(array[2], 40) : _"RUNTIME");
						string value2 = _"Runtime reported a protected-session violation.";
						if (array.Length > 3)
						{
							try
							{
								value2 = Encoding.UTF8.GetString(Convert.FromBase64String(array[3]));
							}
							catch
							{
								value2 = _"Runtime alert payload was malformed.";
							}
						}
						_runtimeViolation = text4 + _": " + A(value2, 1000);
						C(_"ERROR", _"BLOCKED [RUNTIME ALERT] " + _runtimeViolation);
						_remote?.A(_"ERROR", _"RUNTIME_BLOCK", _runtimeViolation);
						await writer.WriteLineAsync(_"ACK|" + _nonce);
						break;
					}
					if (text3 != null && text3.StartsWith(_"BLOCK|" + _nonce + _"|", StringComparison.Ordinal))
					{
						string[] array2 = text3.Split(new char[1] { '|' }, 4);
						string text5 = ((array2.Length > 2) ? A(array2[2], 40) : _"RUNTIME");
						string value3 = _"Runtime reported and contained an unapproved action.";
						if (array2.Length > 3)
						{
							try
							{
								value3 = Encoding.UTF8.GetString(Convert.FromBase64String(array2[3]));
							}
							catch
							{
								value3 = _"Runtime block payload was malformed.";
							}
						}
						string text6 = text5 + _": " + A(value3, 1000);
						C(_"WARNING", _"BLOCKED [RUNTIME REPORT] " + text6 + _" Action=keep protected session running.");
						_remote?.A(_"WARNING", _"RUNTIME_BLOCK", text6);
						await writer.WriteLineAsync(_"ACK|" + _nonce);
						continue;
					}
					if (text3 != null && text3.StartsWith(_"TELEMETRY|" + _nonce + _"|", StringComparison.Ordinal))
					{
						string[] array3 = text3.Split(new char[1] { '|' }, 4);
						string text7 = ((array3.Length > 2) ? A(array3[2], 48) : _"RUNTIME");
						string value4 = _"Runtime telemetry payload was missing.";
						if (array3.Length > 3)
						{
							try
							{
								value4 = Encoding.UTF8.GetString(Convert.FromBase64String(array3[3]));
							}
							catch
							{
								value4 = _"Runtime telemetry payload was malformed.";
							}
						}
						_remote?.A(_"INFO", text7, A(value4, 1800));
						await writer.WriteLineAsync(_"ACK|" + _nonce);
						continue;
					}
					C(_"WARNING", _"BLOCKED [PIPE CLIENT] invalid authenticated message.");
					break;
				}
			}
			catch (OperationCanceledException)
			{
				break;
			}
			catch (Exception ex2)
			{
				C(_"WARNING", _"Runtime pipe disconnected: " + ex2.Message);
			}
		}
	}

	private static SteamLaunch E()
	{
		string text = g();
		if (text == null)
		{
			C(_"WARNING", _"Steam.exe was not directly discoverable; using Windows' registered steam:// protocol.");
		}
		else
		{
			C(_"INFO", _"Steam client discovered automatically: " + text);
		}
		return new SteamLaunch(text, _"1533390");
	}

	private static async Task<ReleaseVerificationResult> A(string P_0, string P_1, ReleaseVerificationResult P_2, CancellationToken P_3)
	{
		if (P_2.FailureKind != ReleaseFailureKind.HostIntegrityMismatch)
		{
			return P_2;
		}
		IReadOnlyDictionary<string, string> readOnlyDictionary = ReleaseVerifier.A(P_1);
		HostRecoveryPlan hostRecoveryPlan = HostRecovery.A(P_0, readOnlyDictionary);
		if (!hostRecoveryPlan.BuildMatches)
		{
			string value = ((hostRecoveryPlan.InstalledBuildId.Length == 0) ? _"unknown" : hostRecoveryPlan.InstalledBuildId);
			string value2 = ((hostRecoveryPlan.ExpectedBuildId.Length == 0) ? _"missing from signed manifest" : hostRecoveryPlan.ExpectedBuildId);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(67, 2);
			defaultInterpolatedStringHandler.AppendLiteral(_"Installed Steam build ");
			defaultInterpolatedStringHandler.AppendFormatted(value);
			defaultInterpolatedStringHandler.AppendLiteral(_" does not match signed Gorilla Guard build ");
			defaultInterpolatedStringHandler.AppendFormatted(value2);
			defaultInterpolatedStringHandler.AppendLiteral(_". ");
			throw new InvalidDataException(defaultInterpolatedStringHandler.ToStringAndClear() + _"Update Gorilla Guard for this game build; automatic quarantine will not treat a different official build as unwanted code.");
		}
		if (!hostRecoveryPlan.HasCompleteInventory)
		{
			throw new InvalidDataException(_"This signed Gorilla Guard package has no complete managed/native per-file inventory. Install the latest signed Guard release before using automatic quarantine and repair.");
		}
		string text = Path.Combine(P_0, _"BepInEx", _"quarantine", _"steam-repair", DateTime.UtcNow.ToString(_"yyyyMMdd-HHmmssfff"));
		IReadOnlyList<HostQuarantineMove> readOnlyList = HostRecovery.A(hostRecoveryPlan, P_0, text, delegate(string P_0)
		{
			C(_"WARNING", P_0);
		});
		int value3 = hostRecoveryPlan.Items.Count((HostRecoveryItem P_0) => P_0.Kind == HostRecoveryKind.Missing);
		string text2 = _"WARNING";
		DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(48, 3);
		defaultInterpolatedStringHandler2.AppendLiteral(_"HOST REPAIR: quarantined=");
		defaultInterpolatedStringHandler2.AppendFormatted(readOnlyList.Count);
		defaultInterpolatedStringHandler2.AppendLiteral(_"; missing=");
		defaultInterpolatedStringHandler2.AppendFormatted(value3);
		defaultInterpolatedStringHandler2.AppendLiteral(_"; quarantine=");
		defaultInterpolatedStringHandler2.AppendFormatted(text);
		C(text2, defaultInterpolatedStringHandler2.ToStringAndClear());
		A(E());
		C(_"INFO", _"Steam file validation requested. Gorilla Guard will wait for official files to be restored and verify them again.");
		DateTime deadline = DateTime.UtcNow.AddMinutes(20.0);
		DateTime nextProgress = DateTime.MinValue;
		ReleaseVerificationResult current = P_2;
		while (DateTime.UtcNow < deadline)
		{
			P_3.ThrowIfCancellationRequested();
			current = ReleaseVerifier.A(P_0, P_1, false);
			if (current.Success)
			{
				C(_"INFO", _"Steam-owned Gorilla Tag code was restored and matches the signed baseline.");
				return current;
			}
			if (DateTime.UtcNow >= nextProgress)
			{
				C(_"INFO", _"Waiting for Steam to finish validating Gorilla Tag files…");
				nextProgress = DateTime.UtcNow.AddSeconds(15.0);
			}
			await Task.Delay(TimeSpan.FromSeconds(2.0), P_3);
		}
		return new ReleaseVerificationResult
		{
			Success = false,
			FailureKind = current.FailureKind,
			Message = current.Message + _" Steam validation did not restore the signed baseline within 20 minutes. Open Steam > Gorilla Tag > Properties > Installed Files > Verify integrity, then retry.",
			Version = current.Version
		};
	}

	private static void A(SteamLaunch P_0)
	{
		string text = _"steam://validate/" + P_0.AppId;
		if (!string.IsNullOrWhiteSpace(P_0.SteamExecutable))
		{
			Process.Start(new ProcessStartInfo
			{
				FileName = P_0.SteamExecutable,
				Arguments = _"-silent \"" + text + _"\"",
				WorkingDirectory = Path.GetDirectoryName(P_0.SteamExecutable),
				UseShellExecute = true
			});
		}
		else
		{
			Process.Start(new ProcessStartInfo
			{
				FileName = text,
				UseShellExecute = true
			});
		}
	}

	private static void a(SteamLaunch P_0)
	{
		if (!string.IsNullOrWhiteSpace(P_0.SteamExecutable))
		{
			Process.Start(new ProcessStartInfo
			{
				FileName = P_0.SteamExecutable,
				Arguments = _"-applaunch " + P_0.AppId,
				WorkingDirectory = Path.GetDirectoryName(P_0.SteamExecutable),
				UseShellExecute = true
			});
		}
		else
		{
			Process.Start(new ProcessStartInfo
			{
				FileName = _"steam://run/" + P_0.AppId,
				UseShellExecute = true
			});
		}
	}

	private static async Task<Process> A(TimeSpan P_0, CancellationToken P_1)
	{
		DateTime deadline = DateTime.UtcNow + P_0;
		long nextAudit = Stopwatch.GetTimestamp();
		_startupVrFailure = null;
		while (DateTime.UtcNow < deadline)
		{
			P_1.ThrowIfCancellationRequested();
			if (_startupVrFailure == null && Stopwatch.GetTimestamp() >= nextAudit)
			{
				try
				{
					if (_vrPreflight == null)
					{
						throw new InvalidDataException(_"VR supervision was not initialized.");
					}
					H();
					LoaderFinding loaderFinding = LoaderMonitor.A();
					if (loaderFinding != null)
					{
						throw new InvalidDataException(A(loaderFinding));
					}
				}
				catch (Exception ex)
				{
					_startupVrFailure = _"VR verification failed during game startup: " + ex.Message;
					C(_"ERROR", _startupVrFailure);
				}
				nextAudit = Stopwatch.GetTimestamp() + Stopwatch.Frequency * 2;
			}
			Process process = a(_gameRoot);
			if (process != null)
			{
				return process;
			}
			await Task.Delay(500, P_1);
		}
		throw new TimeoutException(_"Steam did not start Gorilla Tag within the allowed startup period.");
	}

	private static Process? a(string P_0)
	{
		string fullPath = Path.GetFullPath(Path.Combine(P_0, _"Gorilla Tag.exe"));
		Process[] processesByName = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(_"Gorilla Tag.exe"));
		foreach (Process process in processesByName)
		{
			try
			{
				if (b(process.MainModule?.FileName, fullPath))
				{
					return process;
				}
			}
			catch
			{
				process.Dispose();
			}
		}
		return null;
	}

	private static string e()
	{
		List<string> list = new List<string>();
		foreach (string item in F())
		{
			list.Add(item);
			if (B(item))
			{
				string fullPath = Path.GetFullPath(item);
				C(_"INFO", _"Gorilla Tag discovered from Windows/Steam registration: " + fullPath);
				return fullPath;
			}
		}
		List<string> list2 = f().ToList();
		string text = _"INFO";
		DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(40, 2);
		defaultInterpolatedStringHandler.AppendLiteral(_"Steam discovery found ");
		defaultInterpolatedStringHandler.AppendFormatted(list2.Count);
		defaultInterpolatedStringHandler.AppendLiteral(_" candidate librar");
		defaultInterpolatedStringHandler.AppendFormatted((list2.Count == 1) ? _"y" : _"ies");
		defaultInterpolatedStringHandler.AppendLiteral(_".");
		C(text, defaultInterpolatedStringHandler.ToStringAndClear());
		foreach (string item2 in list2)
		{
			string text2 = Path.Combine(item2, _"steamapps");
			list.Add(text2);
			if (!Directory.Exists(text2))
			{
				continue;
			}
			if (A(Path.Combine(text2, _"appmanifest_1533390.acf"), text2, out string candidate))
			{
				C(_"INFO", _"Gorilla Tag discovered from Steam app manifest: " + candidate);
				return candidate;
			}
			try
			{
				foreach (string item3 in Directory.EnumerateFiles(text2, _"appmanifest_*.acf", SearchOption.TopDirectoryOnly))
				{
					if (A(item3, text2, out string candidate2))
					{
						C(_"INFO", _"Gorilla Tag discovered from Steam library manifest: " + candidate2);
						return candidate2;
					}
				}
			}
			catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
			{
				C(_"WARNING", _"Could not inspect Steam manifests under " + text2 + _": " + ex.Message);
			}
			string text3 = Path.Combine(text2, _"common", _"Gorilla Tag");
			list.Add(text3);
			if (B(text3))
			{
				string fullPath2 = Path.GetFullPath(text3);
				C(_"INFO", _"Gorilla Tag discovered from the standard library folder: " + fullPath2);
				return fullPath2;
			}
		}
		string text4 = ((list.Count == 0) ? _"No readable Steam locations were discovered." : (_"Checked: " + string.Join(_"; ", list.Distinct<string>(StringComparer.OrdinalIgnoreCase).Take(12))));
		throw new DirectoryNotFoundException(_"Could not find Gorilla Tag automatically. " + text4);
	}

	private static bool A(string P_0, string P_1, out string? candidate)
	{
		candidate = null;
		if (!File.Exists(P_0))
		{
			return false;
		}
		try
		{
			string input = File.ReadAllText(P_0);
			Match match = Regex.Match(input, _"\\\"appid\\\"\\s+\\\"(\\d+)\\\"", RegexOptions.IgnoreCase);
			Match match2 = Regex.Match(input, _"\\\"installdir\\\"\\s+\\\"([^\\\"]+)\\\"", RegexOptions.IgnoreCase);
			if (!(match.Success ? match.Groups[1].Value.Equals(_"1533390", StringComparison.Ordinal) : Path.GetFileName(P_0).Equals(_"appmanifest_1533390.acf", StringComparison.OrdinalIgnoreCase)) || !match2.Success)
			{
				return false;
			}
			string path = b(match2.Groups[1].Value);
			string text = Path.GetFullPath(Path.Combine(P_1, _"common")).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
			string fullPath = Path.GetFullPath(Path.Combine(text, path));
			if (!fullPath.StartsWith(text, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			if (!B(fullPath))
			{
				return false;
			}
			candidate = fullPath;
			return true;
		}
		catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException)
		{
			C(_"WARNING", _"Could not read Steam manifest " + P_0 + _": " + ex.Message);
			return false;
		}
	}

	private static bool B(string P_0)
	{
		try
		{
			return Directory.Exists(P_0) && File.Exists(Path.Combine(P_0, _"Gorilla Tag.exe")) && Directory.Exists(Path.Combine(P_0, _"Gorilla Tag_Data", _"Managed"));
		}
		catch
		{
			return false;
		}
	}

	private static IEnumerable<string> F()
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		RegistryHive[] array = new RegistryHive[2]
		{
			RegistryHive.CurrentUser,
			RegistryHive.LocalMachine
		};
		foreach (RegistryHive registryHive in array)
		{
			RegistryView[] array2 = new RegistryView[2]
			{
				RegistryView.Registry32,
				RegistryView.Registry64
			};
			foreach (RegistryView registryView in array2)
			{
				string value = A(registryHive, registryView, _"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Steam App 1533390", _"InstallLocation");
				a(hashSet, value);
			}
		}
		return hashSet.OrderBy<string, string>((string P_0) => P_0, StringComparer.OrdinalIgnoreCase);
	}

	private static IEnumerable<string> f()
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (string item in G().ToList())
		{
			if (Directory.Exists(Path.Combine(item, _"steamapps")))
			{
				hashSet.Add(item);
			}
			A(hashSet, Path.Combine(item, _"steamapps", _"libraryfolders.vdf"));
		}
		DriveInfo[] drives = DriveInfo.GetDrives();
		foreach (DriveInfo driveInfo in drives)
		{
			string fullName;
			try
			{
				if (!driveInfo.IsReady || driveInfo.DriveType == DriveType.CDRom || driveInfo.DriveType == DriveType.Network)
				{
					continue;
				}
				fullName = driveInfo.RootDirectory.FullName;
				goto IL_0112;
			}
			catch
			{
			}
			continue;
			IL_0112:
			string[] array = new string[3]
			{
				_"SteamLibrary",
				_"Steam",
				Path.Combine(_"Games", _"Steam")
			};
			foreach (string path in array)
			{
				string text = Path.Combine(fullName, path);
				if (Directory.Exists(Path.Combine(text, _"steamapps")))
				{
					hashSet.Add(Path.GetFullPath(text));
				}
			}
		}
		return hashSet.OrderBy<string, string>((string P_0) => P_0, StringComparer.OrdinalIgnoreCase);
	}

	private static IEnumerable<string> G()
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		RegistryHive[] array = new RegistryHive[2]
		{
			RegistryHive.CurrentUser,
			RegistryHive.LocalMachine
		};
		foreach (RegistryHive registryHive in array)
		{
			RegistryView[] array2 = new RegistryView[2]
			{
				RegistryView.Registry32,
				RegistryView.Registry64
			};
			foreach (RegistryView registryView in array2)
			{
				string[] array3 = new string[2]
				{
					_"SteamPath",
					_"InstallPath"
				};
				foreach (string text in array3)
				{
					a(hashSet, A(registryHive, registryView, _"SOFTWARE\\Valve\\Steam", text));
				}
				B(hashSet, A(registryHive, registryView, _"SOFTWARE\\Valve\\Steam", _"SteamExe"));
			}
		}
		try
		{
			Process[] processesByName = Process.GetProcessesByName(_"steam");
			foreach (Process process in processesByName)
			{
				using (process)
				{
					try
					{
						B(hashSet, process.MainModule?.FileName);
					}
					catch
					{
					}
				}
			}
		}
		catch
		{
		}
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
		string folderPath2 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
		a(hashSet, Path.Combine(folderPath, _"Steam"));
		a(hashSet, Path.Combine(folderPath2, _"Steam"));
		DriveInfo[] drives = DriveInfo.GetDrives();
		foreach (DriveInfo driveInfo in drives)
		{
			string fullName;
			try
			{
				if (!driveInfo.IsReady || driveInfo.DriveType == DriveType.CDRom || driveInfo.DriveType == DriveType.Network)
				{
					continue;
				}
				fullName = driveInfo.RootDirectory.FullName;
				goto IL_02cc;
			}
			catch
			{
			}
			continue;
			IL_02cc:
			a(hashSet, Path.Combine(fullName, _"Steam"));
			a(hashSet, Path.Combine(fullName, _"Program Files (x86)", _"Steam"));
			a(hashSet, Path.Combine(fullName, _"Program Files", _"Steam"));
		}
		return hashSet.OrderBy<string, string>((string P_0) => P_0, StringComparer.OrdinalIgnoreCase);
	}

	private static void A(HashSet<string> P_0, string P_1)
	{
		if (!File.Exists(P_1))
		{
			return;
		}
		try
		{
			string input = File.ReadAllText(P_1);
			foreach (Match item in Regex.Matches(input, _"\\\"path\\\"\\s+\\\"([^\\\"]+)\\\"", RegexOptions.IgnoreCase))
			{
				a(P_0, b(item.Groups[1].Value));
			}
			foreach (Match item2 in Regex.Matches(input, _"^\\s*\\\"\\d+\\\"\\s+\\\"([^\\\"]+)\\\"", RegexOptions.Multiline))
			{
				a(P_0, b(item2.Groups[1].Value));
			}
		}
		catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
		{
			C(_"WARNING", _"Could not read Steam library registry " + P_1 + _": " + ex.Message);
		}
	}

	private static string b(string P_0)
	{
		return Environment.ExpandEnvironmentVariables(P_0.Trim().Trim('"').Replace(_"\\\\", _"\\")
			.Replace('/', '\\'));
	}

	private static void a(HashSet<string> P_0, string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return;
		}
		try
		{
			string path = b(value);
			if (Directory.Exists(path))
			{
				P_0.Add(Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar));
			}
		}
		catch
		{
		}
	}

	private static void B(HashSet<string> P_0, string? executable)
	{
		if (string.IsNullOrWhiteSpace(executable))
		{
			return;
		}
		try
		{
			string path = b(executable);
			if (File.Exists(path))
			{
				string directoryName = Path.GetDirectoryName(Path.GetFullPath(path));
				a(P_0, directoryName);
			}
		}
		catch
		{
		}
	}

	private static string? g()
	{
		RegistryHive[] array = new RegistryHive[2]
		{
			RegistryHive.CurrentUser,
			RegistryHive.LocalMachine
		};
		foreach (RegistryHive registryHive in array)
		{
			RegistryView[] array2 = new RegistryView[2]
			{
				RegistryView.Registry32,
				RegistryView.Registry64
			};
			foreach (RegistryView registryView in array2)
			{
				string text = C(A(registryHive, registryView, _"SOFTWARE\\Valve\\Steam", _"SteamExe"));
				if (text != null)
				{
					return text;
				}
			}
		}
		foreach (string item in G())
		{
			string path = Path.Combine(item, _"steam.exe");
			if (File.Exists(path))
			{
				return Path.GetFullPath(path);
			}
		}
		return null;
	}

	private static string? C(string? P_0)
	{
		if (string.IsNullOrWhiteSpace(P_0))
		{
			return null;
		}
		try
		{
			string path = b(P_0);
			return File.Exists(path) ? Path.GetFullPath(path) : null;
		}
		catch
		{
			return null;
		}
	}

	private static string? A(RegistryHive P_0, RegistryView P_1, string P_2, string P_3)
	{
		try
		{
			using RegistryKey registryKey = RegistryKey.OpenBaseKey(P_0, P_1);
			using RegistryKey registryKey2 = registryKey.OpenSubKey(P_2, writable: false);
			return registryKey2?.GetValue(P_3) as string;
		}
		catch
		{
			return null;
		}
	}

	private static async Task<int> A(Process P_0, CancellationToken P_1)
	{
		if (_startupVrFailure != null)
		{
			return A(P_0, _startupVrFailure);
		}
		KernelHealthClient kernelHealthClient;
		try
		{
			kernelHealthClient = KernelHealthClient.A(_bepInExRoot);
		}
		catch (Exception ex)
		{
			return A(P_0, _"Required kernel initialization failed: " + ex.Message);
		}
		using KernelHealthClient kernelSession = kernelHealthClient;
		DateTime preloaderDeadline = DateTime.UtcNow.AddSeconds(120.0);
		DateTime heartbeatDeadline = DateTime.UtcNow.AddSeconds(180.0);
		DateTime nextModuleScan = DateTime.MinValue;
		bool debuggerQueryWarningLogged = false;
		MonitoringFailureBudget moduleFailures = new MonitoringFailureBudget();
		MonitoringFailureBudget debuggerFailures = new MonitoringFailureBudget();
		NativeModuleMonitor moduleMonitor = new NativeModuleMonitor();
		ProcessHandleMonitor handleMonitor = new ProcessHandleMonitor();
		DateTime nextHandleScan = DateTime.MinValue;
		bool moduleAuditDegraded = false;
		while (!P_0.HasExited)
		{
			P_1.ThrowIfCancellationRequested();
			if (PendingRuntimeEvents.Dropped > 0)
			{
				return A(P_0, _"Runtime notification capacity exceeded; restart Gorilla Guard.");
			}
			if (!string.IsNullOrWhiteSpace(_runtimeViolation))
			{
				return A(P_0, _"Runtime alert: " + _runtimeViolation);
			}
			if (!string.IsNullOrWhiteSpace(_watcherFailure))
			{
				return A(P_0, _"A protected code-directory watcher failed: " + _watcherFailure);
			}
			if (!a(false))
			{
				return A(P_0, _"A protected code-directory boundary could not be validated.");
			}
			if (!_preloaderAuthenticated && DateTime.UtcNow >= preloaderDeadline)
			{
				return A(P_0, _"GorillaGuard.Preloader.dll never proved that it ran before the runtime.");
			}
			if (DateTime.UtcNow >= nextModuleScan)
			{
				nextModuleScan = DateTime.UtcNow.AddSeconds(2.0);
				if (DateTime.UtcNow >= nextHandleScan)
				{
					nextHandleScan = DateTime.UtcNow.AddSeconds(15.0);
					try
					{
						foreach (string item in handleMonitor.A(P_0.Id))
						{
							if (Warnings.A(_"HANDLE " + item))
							{
								C(_"WARNING", _"OBSERVED [PROCESS HANDLE] " + item + _"; observation only; legitimate tools may request these permissions");
							}
						}
					}
					catch (Exception ex2) when (((ex2 is IOException || ex2 is NotSupportedException || ex2 is Win32Exception) ? 1 : 0) != 0)
					{
						if (Warnings.A(_"HANDLE SCAN " + ex2.Message))
						{
							C(_"WARNING", _"Process handle observation incomplete: " + ex2.Message);
						}
					}
				}
				try
				{
					if (_vrPreflight == null)
					{
						throw new InvalidDataException(_"VR supervision was lost.");
					}
					H();
					kernelSession?.Verify(P_0, _vrPreflight.ServerIdentity, _vrPreflight.CompositorIdentity);
				}
				catch (Exception ex3) when (((ex3 is IOException || ex3 is UnauthorizedAccessException || ex3 is InvalidDataException || ex3 is Win32Exception) ? 1 : 0) != 0)
				{
					return A(P_0, _"Continuous SteamVR verification failed: " + ex3.Message);
				}
				LoaderFinding loaderFinding = LoaderMonitor.A();
				if (loaderFinding != null)
				{
					return A(P_0, A(loaderFinding));
				}
				EvasionFinding evasionFinding = EvasionMonitor.A(_gameRoot, false);
				if (evasionFinding != null)
				{
					return A(P_0, A(evasionFinding));
				}
				NativeModuleScan nativeModuleScan = A(P_0, moduleMonitor);
				if (nativeModuleScan.Loader != null)
				{
					return A(P_0, _"Known injector code loaded in game: " + A(nativeModuleScan.Loader));
				}
				foreach (string suspiciousPath in nativeModuleScan.SuspiciousPaths)
				{
					C(_"WARNING", _"OBSERVED [NATIVE MODULE] path=" + suspiciousPath + _"; removal unverified; restart required");
					PendingRuntimeEvents.A(new LauncherEvent(_"NATIVE MODULE", Path.GetFileName(suspiciousPath) + _" observed; native removal unverified; restart required; path=" + suspiciousPath));
				}
				if (nativeModuleScan.Errors.Count > 0 && !moduleAuditDegraded)
				{
					C(_"WARNING", _"Native module monitoring incomplete: " + string.Join(_" | ", nativeModuleScan.Errors.Take(8)));
					PendingRuntimeEvents.A(new LauncherEvent(_"MONITORING INCOMPLETE", _"Native module inspection failed; protection cannot be fully verified. Restart required."));
				}
				moduleAuditDegraded = nativeModuleScan.Errors.Count > 0;
				if (moduleFailures.A(nativeModuleScan.Errors.Count == 0))
				{
					return A(P_0, _"Game module inspection failed three consecutive audits; restart protection.");
				}
				if (CheckRemoteDebuggerPresent(P_0.Handle, out var debuggerPresent))
				{
					debuggerFailures.A(true);
					if (debuggerPresent)
					{
						return A(P_0, _"An external debugger attached to Gorilla Tag.");
					}
				}
				else
				{
					if (debuggerFailures.A(false))
					{
						return A(P_0, _"Debugger monitoring failed three consecutive audits; restart protection.");
					}
					if (!debuggerQueryWarningLogged)
					{
						debuggerQueryWarningLogged = true;
						C(_"WARNING", _"Could not query the game debugger state; Win32=" + Marshal.GetLastWin32Error());
					}
				}
			}
			if (!_runtimeAuthenticated && DateTime.UtcNow >= heartbeatDeadline)
			{
				return A(P_0, _"GorillaGuard.dll never authenticated to the launcher.");
			}
			long num = Interlocked.Read(in _lastHeartbeatTicks);
			if (_runtimeAuthenticated && (num == 0L || new TimeSpan(DateTime.UtcNow.Ticks - num).TotalSeconds > 15.0))
			{
				return A(P_0, _"GorillaGuard.dll heartbeat was lost.");
			}
			await Task.Delay(1000, P_1);
		}
		string text = _"INFO";
		DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(39, 1);
		defaultInterpolatedStringHandler.AppendLiteral(_"Gorilla Tag exited normally with code ");
		defaultInterpolatedStringHandler.AppendFormatted(P_0.ExitCode);
		defaultInterpolatedStringHandler.AppendLiteral(_".");
		C(text, defaultInterpolatedStringHandler.ToStringAndClear());
		return P_0.ExitCode;
	}

	private static bool a(bool P_0 = true)
	{
		if (PendingCodeFiles.Dropped > 0)
		{
			_watcherFailure = _"Code-file event capacity exceeded; restart Gorilla Guard to restore monitoring.";
			return false;
		}
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		string item;
		while (PendingCodeFiles.A(out item))
		{
			hashSet.Add(item);
		}
		foreach (string item2 in hashSet)
		{
			if (Directory.Exists(item2) && (File.GetAttributes(item2) & FileAttributes.ReparsePoint) != 0)
			{
				C(_"ERROR", _"BLOCK FAILED [REPARSE DIRECTORY] path=" + item2);
				return false;
			}
			if (!e(item2) || d(item2) || !File.Exists(item2) || ContainedCodePaths.Contains(item2))
			{
				continue;
			}
			GuardedLocation guardedLocation = E(item2);
			if (guardedLocation == null)
			{
				continue;
			}
			Exception ex = null;
			for (int i = 0; i < 10 && File.Exists(item2); i++)
			{
				try
				{
					a(item2, guardedLocation.Root, guardedLocation.Scope);
					ex = null;
				}
				catch (Exception ex2)
				{
					ex = ex2;
					Thread.Sleep(100);
					continue;
				}
				break;
			}
			if (ex != null && File.Exists(item2))
			{
				C(_"ERROR", _"BLOCK FAILED [CODE FILE] path=" + item2 + _"; error=" + ex.Message);
				if (P_0)
				{
					return false;
				}
				string text;
				try
				{
					CodeLocks.Add(new FileStream(item2, FileMode.Open, FileAccess.Read, FileShare.Read));
					string fullPath = Path.GetFullPath(item2);
					LockedCodePaths.Add(fullPath);
					ContainedCodePaths.Add(fullPath);
					text = _"file locked against replacement; already-loaded code will be disabled or monitored";
				}
				catch (Exception ex3)
				{
					ContainedCodePaths.Add(Path.GetFullPath(item2));
					text = _"quarantine and file lock unavailable; runtime component/value enforcement remains active; lockError=" + ex3.Message;
				}
				C(_"WARNING", _"BLOCKED [CODE CONTAINMENT] path=" + item2 + _"; action=keep protected session running; containment=" + text);
				_remote?.A(_"WARNING", _"FILE_CONTAINMENT", _"Could not move " + Path.GetFileName(item2) + _" because it was in use; " + text);
				WorkQueue<LauncherEvent> pendingRuntimeEvents = PendingRuntimeEvents;
				string category = _"FILE CONTAINMENT";
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 3);
				defaultInterpolatedStringHandler.AppendFormatted(Path.GetFileName(item2));
				defaultInterpolatedStringHandler.AppendLiteral(_" was already in use; ");
				defaultInterpolatedStringHandler.AppendFormatted(text);
				defaultInterpolatedStringHandler.AppendLiteral(_"; path=");
				defaultInterpolatedStringHandler.AppendFormatted(item2);
				pendingRuntimeEvents.A(new LauncherEvent(category, defaultInterpolatedStringHandler.ToStringAndClear()));
			}
		}
		return true;
	}

	private static string A(LoaderFinding P_0)
	{
		DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(79, 3);
		defaultInterpolatedStringHandler.AppendLiteral(_"Known injector detected: ");
		defaultInterpolatedStringHandler.AppendFormatted(P_0.Evidence);
		defaultInterpolatedStringHandler.AppendLiteral(_"; pid=");
		defaultInterpolatedStringHandler.AppendFormatted(P_0.ProcessId);
		defaultInterpolatedStringHandler.AppendLiteral(_"; path=");
		defaultInterpolatedStringHandler.AppendFormatted(P_0.Path);
		defaultInterpolatedStringHandler.AppendLiteral(_". Close the loader before protected play.");
		return defaultInterpolatedStringHandler.ToStringAndClear();
	}

	private static void H()
	{
		try
		{
			if (_vrPreflight == null)
			{
				throw new InvalidDataException(_"VR monitoring is unavailable.");
			}
			_vrPreflight.A();
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException || ex is InvalidDataException || ex is Win32Exception) ? 1 : 0) != 0)
		{
			if (!_vrCompatibilityWarning)
			{
				_vrCompatibilityWarning = true;
				C(_"WARNING", _"VR COMPATIBILITY MODE: continuing gameplay with incomplete SteamVR verification: " + ex.Message);
				PendingRuntimeEvents.A(new LauncherEvent(_"MONITORING INCOMPLETE", _"Compatibility mode: SteamVR verification is incomplete. Gameplay continues with reduced VR protection."));
			}
		}
	}

	private static string A(EvasionFinding P_0)
	{
		string text;
		if (P_0.ProcessId <= 0)
		{
			text = _"";
		}
		else
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(6, 1);
			defaultInterpolatedStringHandler.AppendLiteral(_"; pid=");
			defaultInterpolatedStringHandler.AppendFormatted(P_0.ProcessId);
			text = defaultInterpolatedStringHandler.ToStringAndClear();
		}
		string value = text;
		string value2 = ((!string.IsNullOrWhiteSpace(P_0.Path) && !P_0.Evidence.Equals(P_0.Path, StringComparison.OrdinalIgnoreCase)) ? (_"; path=" + P_0.Path) : _"");
		DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(52, 4);
		defaultInterpolatedStringHandler2.AppendLiteral(_"Known cheat/evasion IOC detected: kind=");
		defaultInterpolatedStringHandler2.AppendFormatted(P_0.Kind);
		defaultInterpolatedStringHandler2.AppendFormatted(value);
		defaultInterpolatedStringHandler2.AppendLiteral(_"; evidence=");
		defaultInterpolatedStringHandler2.AppendFormatted(P_0.Evidence);
		defaultInterpolatedStringHandler2.AppendFormatted(value2);
		defaultInterpolatedStringHandler2.AppendLiteral(_". ");
		return defaultInterpolatedStringHandler2.ToStringAndClear() + _"Protected play was stopped; Gorilla Guard did not kill the external process or delete its files.";
	}

	private static string B(ReleaseVerificationResult P_0)
	{
		if (P_0.FailureKind == ReleaseFailureKind.HostIntegrityMismatch && P_0.Message.Contains(_"managed game code", StringComparison.OrdinalIgnoreCase))
		{
			return P_0.Message + _" Assembly-CSharp.dll is owned by Gorilla Tag; use Steam > Properties > Installed Files > Verify integrity of game files. Gorilla Guard only repairs its own files from the signed GitHub release channel.";
		}
		return P_0.Message;
	}

	private static NativeModuleScan A(Process P_0, NativeModuleMonitor P_1)
	{
		try
		{
			P_0.Refresh();
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			foreach (ProcessModule module in P_0.Modules)
			{
				try
				{
					list.Add(module.FileName ?? _"");
				}
				catch (Exception ex) when (((ex is Win32Exception || ex is InvalidOperationException) ? 1 : 0) != 0)
				{
					list2.Add(_"Module path unavailable: " + ex.Message);
				}
			}
			NativeModuleScan nativeModuleScan = P_1.A(list, LoaderMonitor.A, c, delegate(string P_0)
			{
				C(_"INFO", _"MODULE LOAD path=" + P_0);
			});
			list2.AddRange(nativeModuleScan.Errors);
			if (list.Count == 0)
			{
				list2.Add(_"No readable game modules were returned.");
			}
			NativeModuleScan nativeModuleScan2 = nativeModuleScan._003CClone_003E_0024();
			nativeModuleScan2.Errors = list2;
			return nativeModuleScan2;
		}
		catch (Exception ex2) when (((ex2 is Win32Exception || ex2 is InvalidOperationException || ex2 is NotSupportedException) ? 1 : 0) != 0)
		{
			return new NativeModuleScan(Array.Empty<string>(), null, new string[1] { _"Module enumeration failed: " + ex2.Message });
		}
	}

	private static bool c(string P_0)
	{
		if (B(P_0, Path.Combine(_bepInExRoot, _"plugins")) || B(P_0, Path.Combine(_bepInExRoot, _"patchers")))
		{
			return !D(P_0);
		}
		if (B(P_0, _gameRoot))
		{
			return false;
		}
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		string folderPath2 = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
		string text = Path.Combine(folderPath, _"Downloads");
		string tempPath = Path.GetTempPath();
		if (!B(P_0, folderPath2) && !B(P_0, text))
		{
			return B(P_0, tempPath);
		}
		return true;
	}

	private static int A(Process P_0, string P_1)
	{
		C(_"ERROR", _"BLOCKED [PROTECTED SESSION] " + P_1 + _" Action=terminate Gorilla Tag only.");
		_remote?.A(_"ERROR", _"SESSION_TERMINATED", P_1);
		try
		{
			P_0.CloseMainWindow();
			if (!P_0.WaitForExit(3000))
			{
				P_0.Kill(entireProcessTree: true);
			}
		}
		catch (Exception ex)
		{
			C(_"ERROR", _"Could not stop the unprotected game process: " + ex.Message);
		}
		return 3;
	}

	private static void A(Process P_0)
	{
		nint num = CreateJobObject(IntPtr.Zero, _"Local\\GorillaGuard.Game." + Guid.NewGuid().ToString(_"N"));
		if (num == IntPtr.Zero)
		{
			throw new InvalidOperationException(_"Could not create the protected Windows job object. Win32=" + Marshal.GetLastWin32Error());
		}
		JobObjectExtendedLimitInformation information = default(JobObjectExtendedLimitInformation);
		information.BasicLimitInformation.LimitFlags = 8192u;
		int informationLength = Marshal.SizeOf<JobObjectExtendedLimitInformation>();
		if (!SetInformationJobObject(num, 9, ref information, (uint)informationLength))
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			CloseHandle(num);
			throw new InvalidOperationException(_"Could not configure the protected Windows job object. Win32=" + lastWin32Error);
		}
		if (!AssignProcessToJobObject(num, P_0.Handle))
		{
			int lastWin32Error2 = Marshal.GetLastWin32Error();
			CloseHandle(num);
			throw new InvalidOperationException(_"Could not attach Gorilla Tag to the protected Windows job object. Win32=" + lastWin32Error2);
		}
		_jobHandle = num;
		C(_"INFO", _"OS process lifetime lock active: closing the launcher will terminate the protected process.");
	}

	private static bool D(string? P_0)
	{
		if (string.IsNullOrWhiteSpace(P_0))
		{
			return false;
		}
		if (!b(P_0, Path.Combine(_bepInExRoot, _"plugins", _"GorillaGuard.dll")))
		{
			return b(P_0, Path.Combine(_bepInExRoot, _"patchers", _"GorillaGuard.Preloader.dll"));
		}
		return true;
	}

	private static bool d(string P_0)
	{
		try
		{
			return LockedCodePaths.Contains(Path.GetFullPath(P_0));
		}
		catch
		{
			return false;
		}
	}

	private static GuardedLocation? E(string P_0)
	{
		GuardedLocation[] array = new GuardedLocation[6]
		{
			new GuardedLocation(Path.Combine(_bepInExRoot, _"plugins"), _"plugins", TopDirectoryOnly: false),
			new GuardedLocation(Path.Combine(_bepInExRoot, _"patchers"), _"patchers", TopDirectoryOnly: false),
			new GuardedLocation(Path.Combine(_gameRoot, _"Gorilla Tag_Data", _"Managed"), _"game-managed", TopDirectoryOnly: false),
			new GuardedLocation(Path.Combine(_gameRoot, _"Gorilla Tag_Data", _"Plugins"), _"game-native", TopDirectoryOnly: false),
			new GuardedLocation(Path.Combine(_bepInExRoot, _"core"), _"bepinex-core", TopDirectoryOnly: false),
			new GuardedLocation(_gameRoot, _"game-root", TopDirectoryOnly: true)
		};
		foreach (GuardedLocation guardedLocation in array)
		{
			if (!guardedLocation.TopDirectoryOnly && B(P_0, guardedLocation.Root))
			{
				return guardedLocation;
			}
			if (guardedLocation.TopDirectoryOnly && b(Path.GetDirectoryName(Path.GetFullPath(P_0)), guardedLocation.Root))
			{
				return guardedLocation;
			}
		}
		return null;
	}

	private static bool e(string P_0)
	{
		string extension = Path.GetExtension(P_0);
		if (!extension.Equals(_".dll", StringComparison.OrdinalIgnoreCase) && !extension.Equals(_".exe", StringComparison.OrdinalIgnoreCase) && !extension.Equals(_".winmd", StringComparison.OrdinalIgnoreCase))
		{
			return extension.Equals(_".so", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	[IteratorStateMachine(typeof(_003CEnumerateFilesWithoutReparsePoints_003Ed__94))]
	private static IEnumerable<string> B(string P_0, bool P_1)
	{

		return new _003CEnumerateFilesWithoutReparsePoints_003Ed__94(-2)
		{
			_003C_003E3__root = P_0,
			_003C_003E3__recursive = P_1
		};
	}

	private static bool a(string P_0, string P_1)
	{
		string text = Path.GetFullPath(P_1).TrimEnd(Path.DirectorySeparatorChar);
		string directoryName = Path.GetDirectoryName(Path.GetFullPath(P_0));
		while (!string.IsNullOrWhiteSpace(directoryName) && !b(directoryName, text))
		{
			if (!B(directoryName, text))
			{
				return true;
			}
			if (Directory.Exists(directoryName) && (File.GetAttributes(directoryName) & FileAttributes.ReparsePoint) != 0)
			{
				return true;
			}
			directoryName = Path.GetDirectoryName(directoryName);
		}
		return string.IsNullOrWhiteSpace(directoryName);
	}

	private static bool B(string? path, string P_1)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return false;
		}
		try
		{
			string fullPath = Path.GetFullPath(path);
			string value = Path.GetFullPath(P_1).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
			return fullPath.StartsWith(value, StringComparison.OrdinalIgnoreCase);
		}
		catch
		{
			return false;
		}
	}

	private static bool b(string? P_0, string? P_1)
	{
		if (string.IsNullOrWhiteSpace(P_0) || string.IsNullOrWhiteSpace(P_1))
		{
			return false;
		}
		try
		{
			return Path.GetFullPath(P_0).Equals(Path.GetFullPath(P_1), StringComparison.OrdinalIgnoreCase);
		}
		catch
		{
			return false;
		}
	}

	private static void h()
	{
		try
		{
			string path = SessionLocation.A(_gameRoot);
			if (File.Exists(path) && File.ReadAllText(path).Contains(_"nonce=" + _nonce, StringComparison.Ordinal))
			{
				File.Delete(path);
			}
		}
		catch (Exception ex)
		{
			C(_"WARNING", _"Could not remove launcher session file: " + ex.Message);
		}
	}

	private static async Task A(Task P_0)
	{
		try
		{
			await P_0;
		}
		catch (OperationCanceledException)
		{
		}
	}

	private static bool A(Exception P_0)
	{
		if (P_0 is UnauthorizedAccessException)
		{
			return true;
		}
		if (P_0.HResult != -2147024891)
		{
			if (P_0.InnerException != null)
			{
				return A(P_0.InnerException);
			}
			return false;
		}
		return true;
	}

	private static bool I()
	{
		try
		{
			using WindowsIdentity ntIdentity = WindowsIdentity.GetCurrent();
			return new WindowsPrincipal(ntIdentity).IsInRole(WindowsBuiltInRole.Administrator);
		}
		catch
		{
			return false;
		}
	}

	private static int F(string P_0)
	{
		FailureDiagnostics.A(P_0, out string text, out string text2);
		_remote?.A(_"ERROR", _"LAUNCH_ABORT", P_0);
		try
		{
			C(_"ERROR", P_0);
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine(_"Session log unavailable: " + ex.Message);
		}
		Console.Error.WriteLine(_"Gorilla Guard did not launch the game. " + P_0);
		Console.Error.WriteLine((text.Length > 0) ? (_"Persistent diagnostics: " + text) : (_"Could not save persistent diagnostics: " + text2));
		if (!_suppressFailureDialog && Environment.UserInteractive)
		{
			try
			{
				MessageBoxW(IntPtr.Zero, FailureDiagnostics.A(P_0, text, text2), _"Gorilla Guard", 16u);
			}
			catch
			{
			}
		}
		return 1;
	}

	private static int? A(string[] P_0, string P_1)
	{
		for (int i = 0; i + 1 < P_0.Length; i++)
		{
			if (P_0[i].Equals(P_1, StringComparison.OrdinalIgnoreCase) && int.TryParse(P_0[i + 1], out var result) && result > 0)
			{
				return result;
			}
		}
		return null;
	}

	private static void C(string P_0, string P_1)
	{
		if (P_0 == _"WARNING" && !Warnings.A(P_1))
		{
			return;
		}
		DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(6, 3);
		defaultInterpolatedStringHandler.AppendLiteral(_"[");
		defaultInterpolatedStringHandler.AppendFormatted(DateTimeOffset.Now, _"O");
		defaultInterpolatedStringHandler.AppendLiteral(_"] [");
		defaultInterpolatedStringHandler.AppendFormatted(P_0);
		defaultInterpolatedStringHandler.AppendLiteral(_"] ");
		defaultInterpolatedStringHandler.AppendFormatted(A(P_1, 4096));
		string text = defaultInterpolatedStringHandler.ToStringAndClear();
		lock (LogLock)
		{
			if (!_scanJson)
			{
				Console.WriteLine(text);
			}
			if (_scanInProgress)
			{
				return;
			}
			try
			{
				Directory.CreateDirectory(_bepInExRoot);
				File.AppendAllText(_logPath, text + Environment.NewLine);
			}
			catch
			{
				try
				{
					string text2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _"GorillaGuard");
					Directory.CreateDirectory(text2);
					File.AppendAllText(Path.Combine(text2, _"Launcher.log"), text + Environment.NewLine);
				}
				catch
				{
				}
			}
		}
	}

	private static string A(string? value, int P_1)
	{
		string text = (string.IsNullOrWhiteSpace(value) ? _"<empty>" : value.Replace('\r', ' ').Replace('\n', ' ').Trim());
		if (text.Length > P_1)
		{
			return text.Substring(0, P_1 - 3) + _"...";
		}
		return text;
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetNamedPipeClientProcessId(nint pipe, out uint clientProcessId);

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool CheckRemoteDebuggerPresent(nint process, [MarshalAs(UnmanagedType.Bool)] out bool debuggerPresent);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern nint CreateJobObject(nint jobAttributes, string? name);

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SetInformationJobObject(nint job, int informationClass, ref JobObjectExtendedLimitInformation information, uint informationLength);

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool AssignProcessToJobObject(nint job, nint process);

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool CloseHandle(nint handle);

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern int MessageBoxW(nint hWnd, string text, string caption, uint type);
}
#pragma endregion

#pragma region GorillaGuard.Launcher/RemoteClient.cs
using System;
using System.Threading.Tasks;

namespace GorillaGuard.Launcher;

internal sealed class RemoteClient : IDisposable
{
	internal bool Enabled => false;

	internal RemoteClient(RemoteSettings settings, string gameRoot)
	{
	}

	internal Task<bool> A(string P_0)
	{
		return Task.FromResult(result: false);
	}

	internal void A(string P_0, string P_1, string P_2)
	{
	}

	internal Task A()
	{
		return Task.CompletedTask;
	}

	public void Dispose()
	{
	}
}
#pragma endregion

#pragma region GorillaGuard.Launcher/RemoteSettings.cs
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace GorillaGuard.Launcher;

internal sealed class RemoteSettings
{
	internal const string FileName = "GorillaGuard.remote.json";

	public string UpdateManifestUrl { get; set; } = _"";

	public string UpdateSignatureUrl { get; set; } = _"";

	public string EventEndpoint { get; set; } = _"";

	public string RelayToken { get; set; } = _"";

	public bool RequireUpdateCheck { get; set; }

	public bool RequireReadyAcknowledgement { get; set; }

	internal bool HasUpdateChannel => !string.IsNullOrWhiteSpace(UpdateManifestUrl);

	internal bool HasEventEndpoint => false;

	internal static RemoteSettings A(string P_0)
	{
		string path = Path.Combine(P_0, _"GorillaGuard.remote.json");
		if (!File.Exists(path))
		{
			return new RemoteSettings();
		}
		RemoteSettings remoteSettings = JsonSerializer.Deserialize<RemoteSettings>(File.ReadAllText(path, Encoding.UTF8), A()) ?? throw new InvalidDataException(_"Remote settings JSON was empty.");
		remoteSettings.EventEndpoint = _"";
		remoteSettings.RelayToken = _"";
		remoteSettings.RequireReadyAcknowledgement = false;
		if (remoteSettings.HasUpdateChannel)
		{
			A(remoteSettings.UpdateManifestUrl, _"UpdateManifestUrl", false);
			A(remoteSettings.UpdateSignatureUrl, _"UpdateSignatureUrl", false);
		}
		else if (!string.IsNullOrWhiteSpace(remoteSettings.UpdateSignatureUrl))
		{
			throw new InvalidDataException(_"UpdateSignatureUrl requires UpdateManifestUrl.");
		}
		if (remoteSettings.HasEventEndpoint)
		{
			A(remoteSettings.EventEndpoint, _"EventEndpoint", false);
		}
		string relayToken = remoteSettings.RelayToken;
		if (relayToken != null && relayToken.Length > 512)
		{
			throw new InvalidDataException(_"RelayToken is too long.");
		}
		return remoteSettings;
	}

	internal static JsonSerializerOptions A()
	{
		return new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			WriteIndented = true
		};
	}

	private static void A(string P_0, string P_1, bool P_2)
	{
		if (!Uri.TryCreate(P_0, UriKind.Absolute, out Uri result) || !result.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(result.Host) || !string.IsNullOrEmpty(result.UserInfo))
		{
			throw new InvalidDataException(P_1 + _" must be an absolute HTTPS URL without embedded credentials.");
		}
		if (!P_2 && (result.Host.Equals(_"discord.com", StringComparison.OrdinalIgnoreCase) || result.Host.EndsWith(_".discord.com", StringComparison.OrdinalIgnoreCase) || result.Host.Equals(_"discordapp.com", StringComparison.OrdinalIgnoreCase) || result.Host.EndsWith(_".discordapp.com", StringComparison.OrdinalIgnoreCase)))
		{
			throw new InvalidDataException(P_1 + _" must point to your relay, not directly to a Discord webhook.");
		}
	}
}
#pragma endregion

#pragma region GorillaGuard.Launcher/SessionUninstall.cs
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using GorillaGuard.Security;

namespace GorillaGuard.Launcher;

internal static class SessionUninstall
{
	internal const string JournalRelative = "BepInEx/GorillaGuard.install.json";

	private static readonly string[] GuardFiles = new string[9]
	{
		_"GorillaGuard.Launcher.exe",
		_"GorillaGuard.remote.json",
		_"BepInEx/GorillaGuard.manifest",
		_"BepInEx/GorillaGuard.manifest.sig",
		_"BepInEx/plugins/GorillaGuard.dll",
		_"BepInEx/patchers/GorillaGuard.Preloader.dll",
		_"BepInEx/repair/openvr_api.dll",
		_"BepInEx/GorillaGuard.Launcher.log",
		_"BepInEx/plugins/GorillaGuard.log"
	};

	private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
	{
		WriteIndented = true
	};

	internal static UninstallPlan A(string P_0, string P_1)
	{
		string text = Path.Combine(P_1, _"BepInEx/GorillaGuard.install.json");
		UninstallPlan uninstallPlan;
		if (File.Exists(text))
		{
			uninstallPlan = C(text);
		}
		else
		{
			string text2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _"GorillaGuard", _"uninstall", Guid.NewGuid().ToString(_"N"));
			Directory.CreateDirectory(text2);
			uninstallPlan = new UninstallPlan
			{
				GameRoot = Path.GetFullPath(P_1),
				BackupRoot = text2
			};
		}
		a(uninstallPlan);
		uninstallPlan.ApprovalManifest = Convert.ToBase64String(File.ReadAllBytes(Path.Combine(P_0, _"BepInEx", _"GorillaGuard.manifest")));
		uninstallPlan.ApprovalSignature = Convert.ToBase64String(File.ReadAllBytes(Path.Combine(P_0, _"BepInEx", _"GorillaGuard.manifest.sig")));
		string[] guardFiles = GuardFiles;
		foreach (string text3 in guardFiles)
		{
			A(uninstallPlan, text3, false);
		}
		guardFiles = new string[2]
		{
			_"winhttp.dll",
			_"doorstop_config.ini"
		};
		foreach (string text4 in guardFiles)
		{
			A(uninstallPlan, text4, true);
		}
		string text5 = Path.Combine(P_0, _"BepInEx", _"core");
		foreach (string item in VrPreflight.F(text5).Where(a))
		{
			A(uninstallPlan, _"BepInEx/core/" + Path.GetRelativePath(text5, item).Replace('\\', '/'), true);
		}
		string text6 = Path.Combine(P_1, _"BepInEx", _"core");
		if (Directory.Exists(text6))
		{
			foreach (string item2 in VrPreflight.F(text6).Where(a))
			{
				A(uninstallPlan, c(P_1, item2), true);
			}
		}
		a(uninstallPlan, P_0);
		B(uninstallPlan);
		return uninstallPlan;
	}

	internal static void A(UninstallPlan P_0, string P_1)
	{
		foreach (InstalledFile file in P_0.Files)
		{
			string text = B(P_0, file.Relative);
			file.InstalledHash = (File.Exists(text) ? ReleaseVerifier.a(text) : null);
		}
		if (Directory.Exists(P_1))
		{
			foreach (string item in VrPreflight.F(P_1))
			{
				P_0.Extras.Add(new CleanupFile
				{
					Path = item,
					Hash = ReleaseVerifier.a(item),
					Kind = _"install-backup"
				});
			}
		}
		B(P_0);
	}

	internal static void A(string P_0)
	{
		string text = Path.Combine(P_0, _"BepInEx/GorillaGuard.install.json");
		if (!File.Exists(text))
		{
			return;
		}
		UninstallPlan uninstallPlan = C(text);
		foreach (CleanupFile item in uninstallPlan.Extras.Where((CleanupFile P_0) => P_0.Kind == _"cache" && Path.GetFileName(P_0.Path).Equals(_"GorillaGuard.Launcher.log", StringComparison.OrdinalIgnoreCase)))
		{
			if (File.Exists(item.Path))
			{
				item.Hash = ReleaseVerifier.a(item.Path);
			}
		}
		string[] array = new string[2]
		{
			_"BepInEx/plugins",
			_"BepInEx/patchers"
		};
		foreach (string path in array)
		{
			string text2 = Path.Combine(P_0, path);
			if (!Directory.Exists(text2))
			{
				continue;
			}
			foreach (string item2 in VrPreflight.F(text2).Where(a))
			{
				string text3 = c(P_0, item2);
				if (!GuardFiles.Contains<string>(text3, StringComparer.OrdinalIgnoreCase))
				{
					A(uninstallPlan, text3, true);
				}
			}
		}
		array = new string[2]
		{
			_"BepInEx/LogOutput.log",
			_"BepInEx/config/BepInEx.cfg"
		};
		foreach (string text4 in array)
		{
			if (!File.Exists(Path.Combine(P_0, text4)))
			{
				A(uninstallPlan, text4, false);
			}
		}
		B(uninstallPlan);
	}

	internal static void a(string P_0, string P_1)
	{
		string text = Path.Combine(P_0, _"BepInEx/GorillaGuard.install.json");
		if (File.Exists(text))
		{
			UninstallPlan uninstallPlan = C(text);
			if (!c(P_0, P_1).StartsWith(_"BepInEx/quarantine/", StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException(_"Invalid quarantine ownership path.");
			}
			uninstallPlan.Extras.Add(new CleanupFile
			{
				Path = P_1,
				Hash = ReleaseVerifier.a(P_1),
				Kind = _"install-backup"
			});
			B(uninstallPlan);
		}
	}

	internal static bool A(string P_0, int P_1, long P_2, Action<string, string> P_3, string? workRoot = null)
	{
		if (P_1 > 0 && P_2 <= 0)
		{
			throw new InvalidDataException(_"Cleanup needs the exact game process creation time.");
		}
		string text = Path.Combine(P_0, _"BepInEx/GorillaGuard.install.json");
		if (!File.Exists(text))
		{
			P_3(_"WARNING", _"Self-uninstall needs an installation ownership journal; reinstall with the current bootstrap.");
			return false;
		}
		UninstallPlan uninstallPlan = C(text);
		uninstallPlan.ParentPid = Environment.ProcessId;
		uninstallPlan.ParentStartedUtcTicks = Process.GetCurrentProcess().StartTime.ToUniversalTime().Ticks;
		uninstallPlan.GamePid = P_1;
		uninstallPlan.GameStartedUtcTicks = P_2;
		foreach (InstalledFile item2 in uninstallPlan.Files.Where((InstalledFile P_0) => B(P_0.Relative)))
		{
			string text2 = B(uninstallPlan, item2.Relative);
			item2.InstalledHash = (File.Exists(text2) ? ReleaseVerifier.a(text2) : null);
		}
		string text3 = SessionLocation.A(P_0);
		uninstallPlan.Extras.Add(new CleanupFile
		{
			Path = text3,
			Hash = (File.Exists(text3) ? ReleaseVerifier.a(text3) : new string('0', 64)),
			Kind = _"session"
		});
		string text4 = Path.Combine(workRoot ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _"GorillaGuard", _"cleanup"), Guid.NewGuid().ToString(_"N"));
		Directory.CreateDirectory(text4);
		string text5 = Path.Combine(text4, _"GorillaGuard.Cleanup.exe");
		File.Copy(Path.Combine(P_0, _"GorillaGuard.Launcher.exe"), text5);
		string text6 = Path.Combine(text4, _"plan.json");
		File.WriteAllText(text6, JsonSerializer.Serialize(uninstallPlan, JsonOptions));
		string text7 = ReleaseVerifier.a(text6);
		string text8 = _"$ErrorActionPreference='Stop'; $cleanupWorker=Start-Process -FilePath " + c(text5) + _" -ArgumentList " + c(_"--finish-cleanup \"" + text6 + _"\" " + text7) + _" -Wait -PassThru -WindowStyle Hidden; if($cleanupWorker.ExitCode -eq 0){Remove-Item -LiteralPath " + c(text5) + _"; Remove-Item -LiteralPath " + c(text6) + _"; Remove-Item -LiteralPath " + c(text4) + _"}";
		ProcessStartInfo processStartInfo = new ProcessStartInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), _"WindowsPowerShell", _"v1.0", _"powershell.exe"))
		{
			UseShellExecute = false,
			CreateNoWindow = true,
			WindowStyle = ProcessWindowStyle.Hidden
		};
		string[] array = new string[4]
		{
			_"-NoProfile",
			_"-NonInteractive",
			_"-Command",
			text8
		};
		foreach (string item in array)
		{
			processStartInfo.ArgumentList.Add(item);
		}
		if (Process.Start(processStartInfo) == null)
		{
			throw new IOException(_"Could not schedule post-game uninstall.");
		}
		P_3(_"INFO", _"One-session cleanup scheduled after game and launcher exit; shared originals will be restored.");
		return true;
	}

	internal static int B(string P_0, string P_1)
	{
		try
		{
			byte[] array = File.ReadAllBytes(P_0);
			if (!Convert.ToHexString(SHA256.HashData(array)).Equals(P_1, StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException(_"Cleanup plan changed.");
			}
			UninstallPlan plan = JsonSerializer.Deserialize<UninstallPlan>(array) ?? throw new InvalidDataException(_"Empty cleanup plan.");
			a(plan);
			A(plan.ParentPid, plan.ParentStartedUtcTicks);
			A(plan.GamePid, plan.GameStartedUtcTicks);
			string text = Path.Combine(plan.GameRoot, _"BepInEx/GorillaGuard.install.json");
			if (File.Exists(text))
			{
				UninstallPlan uninstallPlan = C(text);
				if (!uninstallPlan.BackupRoot.Equals(plan.BackupRoot, StringComparison.OrdinalIgnoreCase) || uninstallPlan.Files.Count != plan.Files.Count || uninstallPlan.Files.Any((InstalledFile P_0) => !plan.Files.Any((InstalledFile P_0) => P_0.Relative == P_0.Relative && P_0.BackupName == P_0.BackupName && P_0.OriginalHash == P_0.OriginalHash && (B(P_0.Relative) || P_0.InstalledHash == P_0.InstalledHash))))
				{
					throw new InvalidDataException(_"Installation ownership changed while cleanup was waiting.");
				}
				foreach (CleanupFile extra in uninstallPlan.Extras.Where((CleanupFile P_0) => P_0.Kind == _"install-backup"))
				{
					if (!plan.Extras.Any((CleanupFile P_0) => P_0.Path.Equals(extra.Path, StringComparison.OrdinalIgnoreCase)))
					{
						plan.Extras.Add(extra);
					}
				}
			}
			foreach (CleanupFile item in plan.Extras.Where((CleanupFile P_0) => P_0.Kind == _"cache" && Path.GetFileName(P_0.Path).Equals(_"GorillaGuard.Launcher.log", StringComparison.OrdinalIgnoreCase)))
			{
				b(item.Path);
				if (File.Exists(item.Path))
				{
					item.Hash = ReleaseVerifier.a(item.Path);
				}
			}
			int num = 0;
			while (true)
			{
				try
				{
					A(plan);
				}
				catch (IOException) when (num < 4)
				{
					Thread.Sleep(500 * (num + 1));
					goto IL_033f;
				}
				break;
				IL_033f:
				num++;
			}
			return 0;
		}
		catch (Exception ex2)
		{
			try
			{
				File.WriteAllText(Path.Combine(Path.GetDirectoryName(P_0), _"cleanup-error.txt"), ex2.ToString());
			}
			catch
			{
			}
			return 1;
		}
	}

	internal static void A(UninstallPlan P_0)
	{
		a(P_0);
		foreach (InstalledFile file in P_0.Files)
		{
			string text = B(P_0, file.Relative);
			if (File.Exists(text))
			{
				string text2 = ReleaseVerifier.a(text);
				if (!B(file.Relative) && text2 != file.InstalledHash && text2 != file.OriginalHash)
				{
					throw new IOException(_"Preserving a file changed since installation/session: " + text);
				}
			}
			if (file.BackupName != null)
			{
				string text3 = b(P_0, file.BackupName);
				if (ReleaseVerifier.a(text3) != file.OriginalHash)
				{
					throw new InvalidDataException(_"Restoration backup changed: " + text3);
				}
			}
		}
		foreach (CleanupFile extra in P_0.Extras)
		{
			A(P_0, extra);
		}
		foreach (InstalledFile item in P_0.Files.OrderBy((InstalledFile P_0) => P_0.Relative.Equals(_"GorillaGuard.Launcher.exe", StringComparison.OrdinalIgnoreCase) ? 1 : 0))
		{
			string text4 = B(P_0, item.Relative);
			if (item.BackupName != null)
			{
				Directory.CreateDirectory(Path.GetDirectoryName(text4));
				File.Copy(b(P_0, item.BackupName), text4, overwrite: true);
				if (ReleaseVerifier.a(text4) != item.OriginalHash)
				{
					throw new IOException(_"Restored file verification failed: " + text4);
				}
			}
			else if (File.Exists(text4))
			{
				File.Delete(text4);
			}
		}
		foreach (CleanupFile extra2 in P_0.Extras)
		{
			if (File.Exists(extra2.Path) && ReleaseVerifier.a(extra2.Path) == extra2.Hash)
			{
				File.Delete(extra2.Path);
			}
		}
		foreach (InstalledFile item2 in P_0.Files.Where((InstalledFile P_0) => P_0.BackupName != null))
		{
			File.Delete(b(P_0, item2.BackupName));
		}
		string path = Path.Combine(P_0.GameRoot, _"BepInEx/GorillaGuard.install.json");
		if (File.Exists(path))
		{
			File.Delete(path);
		}
		foreach (string item3 in from P_0 in P_0.Files.Select((InstalledFile P_0) => Path.GetDirectoryName(B(P_0, P_0.Relative))).Concat(P_0.Extras.Select((CleanupFile P_0) => Path.GetDirectoryName(P_0.Path))).Append<string>(P_0.BackupRoot)
				.Distinct<string>(StringComparer.OrdinalIgnoreCase)
			orderby P_0.Length descending
			select P_0)
		{
			D(item3, P_0.GameRoot);
		}
	}

	private static void A(UninstallPlan P_0, string P_1, bool P_2)
	{
		if (!P_0.Files.Any((InstalledFile P_0) => P_0.Relative.Equals(P_1, StringComparison.OrdinalIgnoreCase)))
		{
			string text = B(P_0, P_1);
			InstalledFile installedFile = new InstalledFile
			{
				Relative = P_1
			};
			if (P_2 && File.Exists(text))
			{
				installedFile.OriginalHash = ReleaseVerifier.a(text);
				installedFile.BackupName = Guid.NewGuid().ToString(_"N") + _".data";
				File.Copy(text, b(P_0, installedFile.BackupName));
			}
			P_0.Files.Add(installedFile);
		}
	}

	private static void a(UninstallPlan P_0, string P_1)
	{
		string text = Path.Combine(P_1, _"GorillaGuard.bootstrap.json");
		if (!File.Exists(text))
		{
			return;
		}
		BootstrapReceipt bootstrapReceipt = JsonSerializer.Deserialize<BootstrapReceipt>(File.ReadAllText(text)) ?? throw new InvalidDataException(_"Invalid bootstrap receipt.");
		string text2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _"GorillaGuard", _"bootstrap");
		string text3 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _"GorillaGuard", _"updates");
		if ((!C(bootstrapReceipt.Root, text2) && !C(bootstrapReceipt.Root, text3)) || !C(P_1, bootstrapReceipt.Root))
		{
			throw new InvalidDataException(_"Bootstrap cache receipt is outside its owned directory.");
		}
		if ((from P_0 in File.ReadAllLines(Path.Combine(P_1, _"BepInEx", _"GorillaGuard.manifest"))
			select P_0.Split('=', 2) into P_0
			where P_0.Length == 2
			select P_0).ToDictionary((string[] P_0) => P_0[0], (string[] P_0) => P_0[1]).TryGetValue(_"installerBootstrapSha256", out var value) && value.Split(';').Contains<string>(bootstrapReceipt.BootstrapHash, StringComparer.OrdinalIgnoreCase) && File.Exists(bootstrapReceipt.BootstrapPath) && ReleaseVerifier.a(bootstrapReceipt.BootstrapPath) == bootstrapReceipt.BootstrapHash)
		{
			P_0.Extras.Add(new CleanupFile
			{
				Path = bootstrapReceipt.BootstrapPath,
				Hash = bootstrapReceipt.BootstrapHash,
				Kind = _"bootstrap"
			});
		}
		foreach (CleanupFile file in bootstrapReceipt.Files)
		{
			string text4 = b(bootstrapReceipt.Root, file.Path);
			if (File.Exists(text4) && (ReleaseVerifier.a(text4) == file.Hash || Path.GetFileName(text4) == _"GorillaGuard.Launcher.log"))
			{
				P_0.Extras.Add(new CleanupFile
				{
					Path = text4,
					Hash = ReleaseVerifier.a(text4),
					Kind = _"cache"
				});
			}
		}
		P_0.Extras.Add(new CleanupFile
		{
			Path = text,
			Hash = ReleaseVerifier.a(text),
			Kind = _"cache"
		});
	}

	private static void A(UninstallPlan P_0, CleanupFile P_1)
	{
		string fullPath = Path.GetFullPath(P_1.Path);
		if (P_1.Hash.Length != 64 || !P_1.Hash.All(Uri.IsHexDigit))
		{
			throw new InvalidDataException(_"Invalid cleanup file digest.");
		}
		if (P_1.Kind == _"install-backup")
		{
			if (!C(fullPath, Path.Combine(P_0.GameRoot, _"BepInEx", _"quarantine")))
			{
				throw new InvalidDataException(_"Backup cleanup escaped quarantine.");
			}
		}
		else if (P_1.Kind == _"cache")
		{
			string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _"GorillaGuard", _"bootstrap");
			string text2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _"GorillaGuard", _"updates");
			if (!C(fullPath, text) && !C(fullPath, text2))
			{
				throw new InvalidDataException(_"Cache cleanup escaped application cache.");
			}
		}
		else if (P_1.Kind == _"session")
		{
			if (fullPath != SessionLocation.A(P_0.GameRoot))
			{
				throw new InvalidDataException(_"Wrong session cleanup path.");
			}
		}
		else
		{
			if (!(P_1.Kind == _"bootstrap"))
			{
				throw new InvalidDataException(_"Unknown cleanup ownership role.");
			}
			byte[] array = Convert.FromBase64String(P_0.ApprovalManifest);
			if (!ReleaseVerifier.A(array, Convert.FromBase64String(P_0.ApprovalSignature)))
			{
				throw new InvalidDataException(_"Bootstrap deletion has no signed release approval.");
			}
			string text3 = Encoding.UTF8.GetString(array).Split('\n').FirstOrDefault((string P_0) => P_0.StartsWith(_"installerBootstrapSha256=", StringComparison.Ordinal));
			if (text3 == null || !text3.Trim().Split('=', 2)[1].Split(';').Contains<string>(P_1.Hash, StringComparer.OrdinalIgnoreCase))
			{
				throw new InvalidDataException(_"Refusing to delete a file that is not an approved Guard bootstrap.");
			}
		}
		b(fullPath);
		if (File.Exists(fullPath) && ReleaseVerifier.a(fullPath) != P_1.Hash)
		{
			throw new IOException(_"Preserving changed cleanup file: " + fullPath);
		}
	}

	internal static void a(UninstallPlan P_0)
	{
		if (!Path.IsPathFullyQualified(P_0.GameRoot) || !Path.IsPathFullyQualified(P_0.BackupRoot) || P_0.Files.Count > 10000 || P_0.Extras.Count > 10000)
		{
			throw new InvalidDataException(_"Invalid uninstall ownership journal.");
		}
		string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _"GorillaGuard", _"uninstall");
		if (!C(P_0.BackupRoot, text) || !Guid.TryParseExact(Path.GetFileName(P_0.BackupRoot), _"N", out var _))
		{
			throw new InvalidDataException(_"Restoration storage escaped the owned uninstall cache.");
		}
		if (Path.GetFullPath(P_0.GameRoot).TrimEnd('\\', '/') == Path.GetPathRoot(P_0.GameRoot).TrimEnd('\\', '/'))
		{
			throw new InvalidDataException(_"A drive root cannot be an uninstall target.");
		}
		if (!File.Exists(Path.Combine(P_0.GameRoot, _"Gorilla Tag.exe")))
		{
			throw new InvalidDataException(_"Cleanup target is not a game installation.");
		}
		b(P_0.GameRoot);
		b(P_0.BackupRoot);
		foreach (InstalledFile file in P_0.Files)
		{
			B(P_0, file.Relative);
			if (file.BackupName != null)
			{
				b(P_0, file.BackupName);
			}
		}
	}

	private static string B(UninstallPlan P_0, string P_1)
	{
		string name = P_1.Replace('\\', '/');
		bool flag = GuardFiles.Contains<string>(name, StringComparer.OrdinalIgnoreCase);
		if (!flag)
		{
			string text = name;
			bool flag2 = ((text == _"winhttp.dll" || text == _"doorstop_config.ini" || text == _"BepInEx/LogOutput.log" || text == _"BepInEx/config/BepInEx.cfg") ? true : false);
			flag = flag2;
		}
		if (!flag && (!new string[3]
		{
			_"BepInEx/core/",
			_"BepInEx/plugins/",
			_"BepInEx/patchers/"
		}.Any((string P_0) => name.StartsWith(P_0, StringComparison.OrdinalIgnoreCase)) || !a(name)))
		{
			throw new InvalidDataException(_"Not a Guard-owned installation target: " + P_1);
		}
		return b(P_0.GameRoot, P_1);
	}

	private static string b(UninstallPlan P_0, string P_1)
	{
		if (Path.GetFileName(P_1) != P_1 || !P_1.EndsWith(_".data", StringComparison.Ordinal))
		{
			throw new InvalidDataException(_"Invalid restoration filename.");
		}
		return b(P_0.BackupRoot, P_1);
	}

	private static string b(string P_0, string P_1)
	{
		if (Path.IsPathFullyQualified(P_1) || P_1.Contains(':') || P_1.Split('/', '\\').Any((string P_0) => (P_0 == _".." || P_0 == _".") ? true : false))
		{
			throw new InvalidDataException(_"Unsafe relative cleanup path.");
		}
		string fullPath = Path.GetFullPath(Path.Combine(P_0, P_1));
		if (!C(fullPath, P_0))
		{
			throw new InvalidDataException(_"Cleanup path escaped its owned root.");
		}
		b(fullPath);
		return fullPath;
	}

	private static bool C(string P_0, string P_1)
	{
		return Path.GetFullPath(P_0).StartsWith(Path.GetFullPath(P_1).TrimEnd('\\', '/') + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
	}

	private static string c(string P_0, string P_1)
	{
		return Path.GetRelativePath(P_0, P_1).Replace('\\', '/');
	}

	private static bool a(string P_0)
	{
		if (!P_0.EndsWith(_".dll", StringComparison.OrdinalIgnoreCase))
		{
			return P_0.EndsWith(_".exe", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static bool B(string P_0)
	{
		if (!P_0.EndsWith(_".log", StringComparison.OrdinalIgnoreCase))
		{
			return P_0 == _"BepInEx/config/BepInEx.cfg";
		}
		return true;
	}

	private static void b(string P_0)
	{
		for (string text = Path.GetFullPath(P_0); text != null; text = Path.GetDirectoryName(text))
		{
			if ((File.Exists(text) || Directory.Exists(text)) && (File.GetAttributes(text) & FileAttributes.ReparsePoint) != 0)
			{
				throw new InvalidDataException(_"Redirected cleanup paths are forbidden: " + text);
			}
		}
	}

	private static void D(string P_0, string P_1)
	{
		string text = P_0;
		string text2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _"GorillaGuard");
		while (text != null && (C(text, P_1) || C(text, text2)))
		{
			b(text);
			if (Directory.Exists(text) && !Directory.EnumerateFileSystemEntries(text).Any())
			{
				try
				{
					Directory.Delete(text);
				}
				catch (IOException)
				{
					break;
				}
				catch (UnauthorizedAccessException)
				{
					break;
				}
				text = Path.GetDirectoryName(text);
				continue;
			}
			break;
		}
	}

	private static void A(int P_0, long P_1)
	{
		if (P_0 <= 0)
		{
			return;
		}
		try
		{
			using Process process = Process.GetProcessById(P_0);
			if (process.StartTime.ToUniversalTime().Ticks == P_1)
			{
				process.WaitForExit();
			}
		}
		catch (ArgumentException)
		{
		}
	}

	private static UninstallPlan C(string P_0)
	{
		UninstallPlan uninstallPlan = JsonSerializer.Deserialize<UninstallPlan>(File.ReadAllText(P_0)) ?? throw new InvalidDataException(_"Empty uninstall journal.");
		a(uninstallPlan);
		if (!Path.GetFullPath(P_0).Equals(Path.GetFullPath(Path.Combine(uninstallPlan.GameRoot, _"BepInEx/GorillaGuard.install.json")), StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidDataException(_"Ownership journal belongs to another installation.");
		}
		return uninstallPlan;
	}

	private static void B(UninstallPlan P_0)
	{
		a(P_0);
		string text = Path.Combine(P_0.GameRoot, _"BepInEx/GorillaGuard.install.json");
		Directory.CreateDirectory(Path.GetDirectoryName(text));
		string text2 = text + _".tmp";
		b(text);
		b(text2);
		File.WriteAllText(text2, JsonSerializer.Serialize(P_0, JsonOptions));
		File.Move(text2, text, overwrite: true);
	}

		internal static string c(string P_0)
	{
		return _"'" + P_0.Replace(_"'", _"''") + _"'";
	}
}
#pragma endregion

#pragma region GorillaGuard.Launcher/SignedUpdateClient.cs
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GorillaGuard.Security;

namespace GorillaGuard.Launcher;

internal static class SignedUpdateClient
{
	private sealed class UpdateDescriptor
	{
		public string Version { get; set; } = _"";

		public string PackageUrl { get; set; } = _"";

		public string PackageSha256 { get; set; } = _"";
	}

	private const long MaxMetadataBytes = 1048576L;

	private const long MaxPackageBytes = 104857600L;

	private const long MaxExtractedBytes = 314572800L;

	private const int MaxArchiveEntries = 4096;

	internal static async Task<bool> A(RemoteSettings P_0, string P_1, Action<string, string> P_2, bool P_3 = false, bool P_4 = false)
	{
		if (!P_0.HasUpdateChannel)
		{
			return false;
		}
		try
		{
			using HttpClient http = SecureDownloads.A();
			http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(_"GorillaGuard", _"1.12.31"));
			byte[] metadataBytes = await A(http, P_0.UpdateManifestUrl, 1048576L);
			if (!ReleaseVerifier.A(metadataBytes, await A(http, P_0.UpdateSignatureUrl, 1048576L)))
			{
				throw new InvalidDataException(_"The remote update descriptor signature is invalid.");
			}
			UpdateDescriptor descriptor = JsonSerializer.Deserialize<UpdateDescriptor>(metadataBytes, RemoteSettings.A()) ?? throw new InvalidDataException(_"The signed update descriptor was empty.");
			if (!Version.TryParse(descriptor.Version, out Version remoteVersion) || !Version.TryParse(_"1.12.31", out Version result))
			{
				throw new InvalidDataException(_"The signed update descriptor contains an invalid version.");
			}
			if (P_3 && !P_4 && remoteVersion < result)
			{
				throw new InvalidDataException(_"Repair cannot silently downgrade the release. Use the explicit rollback installer instead.");
			}
			if (!RecoveryPolicy.A(remoteVersion, result, P_3, P_4))
			{
				if (P_4)
				{
					throw new InvalidDataException(_"The rollback descriptor does not match the pinned previous release.");
				}
				return false;
			}
			if (!Uri.TryCreate(descriptor.PackageUrl, UriKind.Absolute, out Uri result2) || !result2.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) || !string.IsNullOrEmpty(result2.UserInfo))
			{
				throw new InvalidDataException(_"The signed update package URL is not valid HTTPS.");
			}
			if (!A(descriptor.PackageSha256))
			{
				throw new InvalidDataException(_"The signed update descriptor contains an invalid package SHA-256.");
			}
			string arg = _"INFO";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(72, 1);
			defaultInterpolatedStringHandler.AppendLiteral(_"Signed Gorilla Guard update ");
			defaultInterpolatedStringHandler.AppendFormatted(remoteVersion);
			defaultInterpolatedStringHandler.AppendLiteral(_" is available; downloading verified package.");
			P_2(arg, defaultInterpolatedStringHandler.ToStringAndClear());
			byte[] array = await A(http, descriptor.PackageUrl, 104857600L);
			string text = Convert.ToHexString(SHA256.HashData(array));
			if (!text.Equals(descriptor.PackageSha256, StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException(_"Downloaded update hash mismatch: expected=" + descriptor.PackageSha256 + _"; actual=" + text);
			}
			string updateRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _"GorillaGuard", _"updates", remoteVersion?.ToString() + _"-" + Guid.NewGuid().ToString(_"N"));
			Directory.CreateDirectory(updateRoot);
			string zipPath = Path.Combine(updateRoot, _"package.zip");
			await File.WriteAllBytesAsync(zipPath, array);
			string text2 = Path.Combine(updateRoot, _"package");
			Directory.CreateDirectory(text2);
			ExtractSafely(zipPath, text2);
			string[] files = Directory.GetFiles(text2, _"GorillaGuard.Launcher.exe", SearchOption.AllDirectories);
			if (files.Length != 1)
			{
				throw new InvalidDataException(_"The signed update package must contain exactly one launcher.");
			}
			string directoryName = Path.GetDirectoryName(files[0]);
			if (!Directory.Exists(Path.Combine(directoryName, _"BepInEx")))
			{
				throw new InvalidDataException(_"The signed update package has no BepInEx payload beside its launcher.");
			}
			string text3 = Path.Combine(P_1, _"GorillaGuard.remote.json");
			if (!P_3 && !P_4 && File.Exists(text3))
			{
				File.Copy(text3, Path.Combine(directoryName, _"GorillaGuard.remote.json"), overwrite: true);
			}
			BootstrapReceipt value = new BootstrapReceipt
			{
				Root = updateRoot,
				Files = (from P_0 in Directory.EnumerateFiles(updateRoot, _"*", SearchOption.AllDirectories)
					select new CleanupFile
					{
						Path = Path.GetRelativePath(updateRoot, P_0),
						Hash = ReleaseVerifier.a(P_0),
						Kind = _"cache"
					}).ToList()
			};
			File.WriteAllText(Path.Combine(directoryName, _"GorillaGuard.bootstrap.json"), JsonSerializer.Serialize(value));
			if (Process.Start(new ProcessStartInfo
			{
				FileName = files[0],
				Arguments = _"--update-parent " + Environment.ProcessId + ((P_3 || P_4) ? _" --install-only" : _""),
				WorkingDirectory = directoryName,
				UseShellExecute = true
			}) == null)
			{
				throw new InvalidOperationException(_"Windows did not start the verified update installer.");
			}
			string arg2 = _"INFO";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(76, 1);
			defaultInterpolatedStringHandler2.AppendLiteral(_"Verified update ");
			defaultInterpolatedStringHandler2.AppendFormatted(remoteVersion);
			defaultInterpolatedStringHandler2.AppendLiteral(_" staged; handing off installation after this launcher exits.");
			P_2(arg2, defaultInterpolatedStringHandler2.ToStringAndClear());
			return true;
		}
		catch (Exception ex)
		{
			P_2(P_0.RequireUpdateCheck ? _"ERROR" : _"WARNING", _"Remote update check failed: " + ex.Message);
			if (P_0.RequireUpdateCheck)
			{
				throw;
			}
			return false;
		}
	}

	private static async Task<byte[]> A(HttpClient P_0, string P_1, long P_2)
	{
		using CancellationTokenSource deadline = new CancellationTokenSource(TimeSpan.FromMinutes(3.0));
		using HttpResponseMessage response = await SecureDownloads.A(P_0, P_1, deadline.Token);
		Uri uri = response.RequestMessage?.RequestUri;
		if ((object)uri == null || !uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidDataException(_"Remote content resolved to a non-HTTPS address.");
		}
		response.EnsureSuccessStatusCode();
		long? contentLength = response.Content.Headers.ContentLength;
		if (contentLength.HasValue)
		{
			long valueOrDefault = contentLength.GetValueOrDefault();
			if (valueOrDefault > P_2)
			{
				throw new InvalidDataException(_"Remote content exceeds the allowed size.");
			}
		}
		byte[] result;
		await using (Stream input = await response.Content.ReadAsStreamAsync())
		{
			using MemoryStream output = new MemoryStream();
			byte[] buffer = new byte[81920];
			while (true)
			{
				int num = await input.ReadAsync(buffer, deadline.Token);
				if (num == 0)
				{
					break;
				}
				if (output.Length + num > P_2)
				{
					throw new InvalidDataException(_"Remote content exceeds the allowed size.");
				}
				output.Write(buffer, 0, num);
			}
			result = output.ToArray();
		}
		return result;
	}

	private static void ExtractSafely(string zipPath, string destinationRoot)
	{
		string value = Path.GetFullPath(destinationRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
		long num = 0L;
		using ZipArchive zipArchive = ZipFile.OpenRead(zipPath);
		if (zipArchive.Entries.Count > 4096)
		{
			throw new InvalidDataException(_"The update archive contains too many entries.");
		}
		foreach (ZipArchiveEntry entry in zipArchive.Entries)
		{
			string fullPath = Path.GetFullPath(Path.Combine(destinationRoot, entry.FullName));
			if (!fullPath.StartsWith(value, StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException(_"The update archive contains an unsafe path.");
			}
			if (string.IsNullOrEmpty(entry.Name))
			{
				Directory.CreateDirectory(fullPath);
				continue;
			}
			num += entry.Length;
			if (entry.Length > 104857600 || num > 314572800)
			{
				throw new InvalidDataException(_"The update archive expands beyond the allowed size.");
			}
			Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
			using Stream stream = entry.Open();
			using FileStream destination = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
			stream.CopyTo(destination);
		}
	}

	private static bool A(string? P_0)
	{
		if (P_0 != null && P_0.Length == 64)
		{
			return P_0.All(Uri.IsHexDigit);
		}
		return false;
	}
}
#pragma endregion

#pragma region GorillaGuard.Launcher/UninstallPlan.cs
using System.Collections.Generic;

namespace GorillaGuard.Launcher;

internal sealed class UninstallPlan
{
	public string ApprovalManifest { get; set; } = _"";

	public string ApprovalSignature { get; set; } = _"";

	public string GameRoot { get; set; } = _"";

	public string BackupRoot { get; set; } = _"";

	public List<InstalledFile> Files { get; set; } = new List<InstalledFile>();

	public List<CleanupFile> Extras { get; set; } = new List<CleanupFile>();

	public int ParentPid { get; set; }

	public long ParentStartedUtcTicks { get; set; }

	public int GamePid { get; set; }

	public long GameStartedUtcTicks { get; set; }
}
#pragma endregion

#pragma region GorillaGuard.Launcher/VrCodeWatcher.cs
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace GorillaGuard.Launcher;

internal sealed class VrCodeWatcher : IDisposable
{
	private readonly FileSystemWatcher _watcher;

	private string? _failure;

	internal string? Failure => Volatile.Read(in _failure);

	internal VrCodeWatcher(string root)
	{
		_watcher = new FileSystemWatcher(root)
		{
			IncludeSubdirectories = true,
			NotifyFilter = (NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size | NotifyFilters.LastWrite)
		};
		_watcher.Created += A;
		_watcher.Changed += A;
		_watcher.Deleted += A;
		_watcher.Renamed += delegate(object P_0, RenamedEventArgs P_1)
		{
			A(P_1.OldFullPath);
			A(P_1.FullPath);
		};
		_watcher.Error += delegate
		{
			Interlocked.CompareExchange(ref _failure, _"VR code watcher lost events; restart protection.", null);
		};
		_watcher.EnableRaisingEvents = true;
	}

	private void A(object P_0, FileSystemEventArgs P_1)
	{
		A(P_1.FullPath);
	}

	private void A(string P_0)
	{
		string text = Path.GetExtension(P_0).ToLowerInvariant();
		if ((text == _".dll" || text == _".exe" || text == _".vrdrivermanifest") ? true : false)
		{
			Interlocked.CompareExchange(ref _failure, _"VR code or manifest changed during the protected session: " + P_0, null);
		}
	}

	public void Dispose()
	{
		_watcher.Dispose();
	}

		private void A(object P_0, RenamedEventArgs P_1)
	{
		A(P_1.OldFullPath);
		A(P_1.FullPath);
	}

		private void A(object P_0, ErrorEventArgs P_1)
	{
		Interlocked.CompareExchange(ref _failure, _"VR code watcher lost events; restart protection.", null);
	}
}
#pragma endregion

#pragma region GorillaGuard.Launcher/VrPreflight.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using GorillaGuard.Security;
using Microsoft.Win32;

namespace GorillaGuard.Launcher;

internal sealed class VrPreflight : IDisposable
{
	private sealed class ModuleApprovalRoot
	{
		internal string Path { get; }

		internal HashSet<string> Publishers { get; }

		internal ModuleApprovalRoot(string path, HashSet<string> publishers)
		{
			Path = path;
			Publishers = publishers;
		}
	}

		private sealed class _003CReadPaths_003Ed__35 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private string _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private JsonElement root;

		public JsonElement _003C_003E3__root;

		private string key;

		public string _003C_003E3__key;

		private JsonElement.ArrayEnumerator _003C_003E7__wrap1;

		string IEnumerator<string>.Current
		{
						get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
						get
			{
				return _003C_003E2__current;
			}
		}

				public _003CReadPaths_003Ed__35(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

				private void A()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					a();
				}
			}
			_003C_003E7__wrap1 = default(JsonElement.ArrayEnumerator);
			_003C_003E1__state = -2;
		}

		void IDisposable.Dispose()
		{

			this.A();
		}

		private bool MoveNext()
		{
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
				{
					_003C_003E1__state = -1;
					if (root.ValueKind != JsonValueKind.Object)
					{
						throw new InvalidDataException(_"OpenVR paths JSON must be an object.");
					}
					if (!root.TryGetProperty(key, out var value) || value.ValueKind == JsonValueKind.Null)
					{
						return false;
					}
					if (value.ValueKind != JsonValueKind.Array)
					{
						throw new InvalidDataException(_"OpenVR " + key + _" must be an array.");
					}
					if (value.GetArrayLength() > 128)
					{
						throw new InvalidDataException(_"Too many OpenVR paths.");
					}
					_003C_003E7__wrap1 = value.EnumerateArray().GetEnumerator();
					_003C_003E1__state = -3;
					break;
				}
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				if (_003C_003E7__wrap1.MoveNext())
				{
					JsonElement current = _003C_003E7__wrap1.Current;
					string text = ((current.ValueKind == JsonValueKind.String) ? current.GetString() : null);
					if (string.IsNullOrWhiteSpace(text) || !Path.IsPathFullyQualified(text))
					{
						throw new InvalidDataException(_"OpenVR " + key + _" contains an invalid absolute path.");
					}
					_003C_003E2__current = Path.GetFullPath(text);
					_003C_003E1__state = 1;
					return true;
				}
				a();
				_003C_003E7__wrap1 = default(JsonElement.ArrayEnumerator);
				return false;
			}
			catch
			{

				A();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{

			return this.MoveNext();
		}

		private void a()
		{
			_003C_003E1__state = -1;
			((IDisposable)_003C_003E7__wrap1).Dispose();
		}

				private void B()
		{
			throw new NotSupportedException();
		}

		void IEnumerator.Reset()
		{

			this.B();
		}

				private IEnumerator<string> b()
		{
			_003CReadPaths_003Ed__35 _003CReadPaths_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CReadPaths_003Ed__ = this;
			}
			else
			{
				_003CReadPaths_003Ed__ = new _003CReadPaths_003Ed__35(0);
			}
			_003CReadPaths_003Ed__.root = _003C_003E3__root;
			_003CReadPaths_003Ed__.key = _003C_003E3__key;
			return _003CReadPaths_003Ed__;
		}

		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{

			return this.b();
		}

				private IEnumerator C()
		{
			return b();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{

			return this.C();
		}
	}

		private sealed class _003CWalkFiles_003Ed__66 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private string _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private string root;

		public string _003C_003E3__root;

		private Stack<string> _003Cpending_003E5__2;

		private int _003Centries_003E5__3;

		private IEnumerator<string> _003C_003E7__wrap3;

		string IEnumerator<string>.Current
		{
						get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
						get
			{
				return _003C_003E2__current;
			}
		}

				public _003CWalkFiles_003Ed__66(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

				private void A()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					a();
				}
			}
			_003Cpending_003E5__2 = null;
			_003C_003E7__wrap3 = null;
			_003C_003E1__state = -2;
		}

		void IDisposable.Dispose()
		{

			this.A();
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				if (num != 0)
				{
					if (num != 1)
					{
						return false;
					}
					_003C_003E1__state = -3;
					goto IL_0195;
				}
				_003C_003E1__state = -1;
				e(root);
				_003Cpending_003E5__2 = new Stack<string>();
				_003Cpending_003E5__2.Push(root);
				_003Centries_003E5__3 = 0;
				goto IL_01bb;
				IL_0195:
				while (_003C_003E7__wrap3.MoveNext())
				{
					string current = _003C_003E7__wrap3.Current;
					if (++_003Centries_003E5__3 > 20000)
					{
						throw new InvalidDataException(_"VR directory exceeds the audit limit: " + root);
					}
					FileAttributes attributes = File.GetAttributes(current);
					if ((attributes & FileAttributes.ReparsePoint) != 0)
					{
						throw new InvalidDataException(_"Redirected VR entry: " + current);
					}
					if ((attributes & FileAttributes.Directory) != 0)
					{
						_003Cpending_003E5__2.Push(current);
						continue;
					}
					_003C_003E2__current = current;
					_003C_003E1__state = 1;
					return true;
				}
				a();
				_003C_003E7__wrap3 = null;
				goto IL_01bb;
				IL_01bb:
				if (_003Cpending_003E5__2.Count != 0)
				{
					_003C_003E7__wrap3 = Directory.EnumerateFileSystemEntries(_003Cpending_003E5__2.Pop()).GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_0195;
				}
				return false;
			}
			catch
			{

				A();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{

			return this.MoveNext();
		}

		private void a()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap3 != null)
			{
				_003C_003E7__wrap3.Dispose();
			}
		}

				private void B()
		{
			throw new NotSupportedException();
		}

		void IEnumerator.Reset()
		{

			this.B();
		}

				private IEnumerator<string> b()
		{
			_003CWalkFiles_003Ed__66 _003CWalkFiles_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CWalkFiles_003Ed__ = this;
			}
			else
			{
				_003CWalkFiles_003Ed__ = new _003CWalkFiles_003Ed__66(0);
			}
			_003CWalkFiles_003Ed__.root = _003C_003E3__root;
			return _003CWalkFiles_003Ed__;
		}

		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{

			return this.b();
		}

				private IEnumerator C()
		{
			return b();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{

			return this.C();
		}
	}

	private readonly string _gameRoot;

	private readonly string[] _steamRoots;

	private readonly Action<string, string> _log;

	private readonly List<FileStream> _locks = new List<FileStream>();

	private readonly HashSet<string> _lockedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly List<(string Path, string Backup, string SanitizedHash)> _restores = new List<(string, string, string)>();

	private readonly HashSet<string> _approvedHashes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly HashSet<string> _revokedHashes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly HashSet<string> _approvedSpaceCalibratorDrivers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, string> _registrationHashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private readonly HashSet<string> _absentRegistrationPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, Dictionary<string, string>> _securityTrees = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

	private readonly HashSet<string> _configurationPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly HashSet<string> _initialConfigurationPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private string? _xrRegistrationBaseline;

	private readonly Dictionary<string, string> _layerManifestHashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private readonly HashSet<string> _vrServerPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly HashSet<string> _vrCompositorPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly List<ModuleApprovalRoot> _moduleRoots = new List<ModuleApprovalRoot>();

	private readonly HashSet<string> _approvedLoadedModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly VrProcessContinuity _serverContinuity = new VrProcessContinuity();

	private readonly VrProcessContinuity _compositorContinuity = new VrProcessContinuity();

	private readonly List<VrCodeWatcher> _codeWatchers = new List<VrCodeWatcher>();

	private readonly Dictionary<string, string> _manifest;

	private readonly string _manifestRoot;

	private readonly string _backupRoot;

	private static readonly HashSet<string> ValvePublishers = new HashSet<string>(StringComparer.Ordinal)
	{
		_"Valve",
		_"Valve Corp.",
		_"Valve Corporation"
	};

	private static readonly HashSet<string> HeadsetPublishers = new HashSet<string>(StringComparer.Ordinal)
	{
		_"Valve",
		_"Valve Corp.",
		_"Valve Corporation",
		_"Oculus VR, LLC",
		_"Oculus VR, Inc.",
		_"Meta Platforms Technologies, LLC",
		_"HTC Corporation",
		_"Microsoft Corporation"
	};

	private static readonly JsonDocumentOptions JsonOptions = new JsonDocumentOptions
	{
		AllowTrailingCommas = true,
		CommentHandling = JsonCommentHandling.Skip,
		MaxDepth = 64
	};

	internal VrProcessIdentity? ServerIdentity => _serverContinuity.Bound;

	internal VrProcessIdentity? CompositorIdentity => _compositorContinuity.Bound;

	internal VrPreflight(string gameRoot, IEnumerable<string> steamRoots, Action<string, string> log, string? trustedManifestRoot = null, string? backupRoot = null)
	{
		_gameRoot = gameRoot;
		_steamRoots = steamRoots.ToArray();
		_log = log;
		_backupRoot = backupRoot ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _"GorillaGuard", _"backups");
		string text = (_manifestRoot = trustedManifestRoot ?? Path.Combine(gameRoot, _"BepInEx"));
		ReleaseVerificationResult releaseVerificationResult = ReleaseVerifier.Verify(text);
		if (!releaseVerificationResult.Success)
		{
			throw new InvalidDataException(releaseVerificationResult.Message);
		}
		byte[] array = File.ReadAllBytes(Path.Combine(text, _"GorillaGuard.manifest"));
		if (!ReleaseVerifier.A(array, File.ReadAllBytes(Path.Combine(text, _"GorillaGuard.manifest.sig"))))
		{
			throw new InvalidDataException(_"VR policy manifest signature failed.");
		}
		_manifest = (from P_0 in Encoding.UTF8.GetString(array).Split('\n', StringSplitOptions.RemoveEmptyEntries)
			select P_0.Trim().Split('=', 2) into P_0
			where P_0.Length == 2
			select P_0).ToDictionary<string[], string, string>((string[] P_0) => P_0[0], (string[] P_0) => P_0[1], StringComparer.Ordinal);
		string[] array2;
		if (_manifest.TryGetValue(_"vrRevokedCodeSha256", out string value))
		{
			array2 = value.Split(';', StringSplitOptions.RemoveEmptyEntries);
			foreach (string text2 in array2)
			{
				if (text2.Length != 64 || !text2.All(Uri.IsHexDigit))
				{
					throw new InvalidDataException(_"Invalid signed VR revocation.");
				}
				_revokedHashes.Add(text2);
			}
		}
		if (_manifest.TryGetValue(_"vrApprovedCodeSha256", out string value2))
		{
			array2 = value2.Split(';', StringSplitOptions.RemoveEmptyEntries);
			foreach (string text3 in array2)
			{
				if (text3.Length != 64 || !text3.All(Uri.IsHexDigit))
				{
					throw new InvalidDataException(_"Invalid signed VR code approval.");
				}
				_approvedHashes.Add(text3);
			}
		}
		if (!_manifest.TryGetValue(_"spaceCalibratorDriverSha256", out string value3))
		{
			return;
		}
		array2 = value3.Split(';', StringSplitOptions.RemoveEmptyEntries);
		foreach (string text4 in array2)
		{
			if (text4.Length != 64 || !text4.All(Uri.IsHexDigit))
			{
				throw new InvalidDataException(_"Invalid signed SpaceCalibrator driver approval.");
			}
			_approvedSpaceCalibratorDrivers.Add(text4);
		}
	}

	internal void A(bool P_0 = true)
	{
		b();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		HashSet<string> hashSet2 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _"openvr");
		string[] array = new string[2]
		{
			_"openvrpaths.vrpath",
			_"openvrpaths.vrpaths"
		};
		foreach (string path2 in array)
		{
			string text = Path.Combine(path, path2);
			if (!File.Exists(text))
			{
				_absentRegistrationPaths.Add(Path.GetFullPath(text));
				continue;
			}
			a(text);
			using JsonDocument jsonDocument = d(text);
			foreach (string item2 in A(jsonDocument.RootElement, _"runtime"))
			{
				hashSet.Add(item2);
			}
			foreach (string item3 in A(jsonDocument.RootElement, _"config"))
			{
				hashSet2.Add(Path.Combine(item3, _"steamvr.vrsettings"));
			}
			string[] array2 = new string[2]
			{
				_"external_drivers",
				_"externaldrivers"
			};
			foreach (string text2 in array2)
			{
				foreach (string item4 in A(jsonDocument.RootElement, text2))
				{
					A(item4, HeadsetPublishers, true);
				}
			}
		}
		array = _steamRoots;
		foreach (string text3 in array)
		{
			A(text3, ValvePublishers);
			hashSet2.Add(Path.Combine(text3, _"config", _"steamvr.vrsettings"));
			string text4 = Path.Combine(text3, _"steamapps", _"common", _"SteamVR");
			if (Directory.Exists(text4))
			{
				hashSet.Add(text4);
			}
		}
		foreach (string item5 in hashSet)
		{
			string text5 = Path.Combine(item5, _"bin", _"win64", _"vrserver.exe");
			a(text5, ValvePublishers);
			_vrServerPaths.Add(Path.GetFullPath(text5));
			string text6 = Path.Combine(item5, _"bin", _"win64", _"vrcompositor.exe");
			a(text6, ValvePublishers);
			_vrCompositorPaths.Add(Path.GetFullPath(text6));
			string text7 = Path.Combine(item5, _"bin");
			A(text7, F(text7).ToArray());
			A(item5, ValvePublishers);
			A(Path.Combine(item5, _"drivers"), ValvePublishers, true);
			string item = Path.Combine(item5, _"resources", _"settings", _"default.vrsettings");
			hashSet2.Add(item);
		}
		_xrRegistrationBaseline = B();
		c();
		if (_xrRegistrationBaseline != B())
		{
			throw new InvalidDataException(_"OpenXR registration changed during preflight; retry protected startup.");
		}
		a(P_0);
		foreach (string item6 in hashSet2)
		{
			_configurationPaths.Add(Path.GetFullPath(item6));
			if (File.Exists(item6))
			{
				a(item6, P_0);
				_initialConfigurationPaths.Add(Path.GetFullPath(item6));
			}
		}
		_log(_"INFO", _"VR PREFLIGHT PASS: registered runtimes, drivers, OpenXR layers, prediction settings, and continuous VR session baselines audited.");
	}

	[IteratorStateMachine(typeof(_003CReadPaths_003Ed__35))]
	internal static IEnumerable<string> A(JsonElement P_0, string P_1)
	{

		return new _003CReadPaths_003Ed__35(-2)
		{
			_003C_003E3__root = P_0,
			_003C_003E3__key = P_1
		};
	}

	private void A(string P_0, HashSet<string> P_1, bool P_2)
	{
		int num = 0;
		string[] array = F(P_0).ToArray();
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (Path.GetFileName(text).StartsWith(_"driver", StringComparison.OrdinalIgnoreCase) && text.EndsWith(_".dll", StringComparison.OrdinalIgnoreCase))
			{
				a(text, P_1, true);
				num++;
			}
		}
		if (P_2 && num == 0)
		{
			throw new InvalidDataException(_"VR driver directory has no verifiable driver binaries: " + P_0);
		}
		A(P_0, P_1);
		A(P_0, array);
	}

	internal void A(string P_0)
	{
		a(P_0, ValvePublishers, true);
	}

	internal void a(string P_0, HashSet<string> P_1, bool P_2 = false)
	{
		e(P_0);
		FileStream fileStream = new FileStream(P_0, FileMode.Open, FileAccess.Read, FileShare.Read);
		try
		{
			string text = Convert.ToHexString(SHA256.HashData(fileStream));
			A(text, _revokedHashes, P_0);
			bool flag = P_2 && Path.GetFileName(P_0).Equals(_"driver_01spacecalibrator.dll", StringComparison.OrdinalIgnoreCase) && _approvedSpaceCalibratorDrivers.Contains(text);
			bool flag2 = _approvedHashes.Contains(text) || flag;
			if (!flag2)
			{
				string text2 = Authenticode.A(P_0);
				if (!P_1.Contains(text2))
				{
					throw new InvalidDataException(_"VR binary publisher is not approved: " + text2 + _"; path=" + P_0 + _"; sha256=" + text);
				}
				flag2 = true;
			}
			A(P_0, fileStream);
			if (flag)
			{
				_log(_"INFO", _"SpaceCalibrator approved by signed upstream-release hash; driver and calibration settings left unchanged: " + P_0);
			}
			if (flag2)
			{
				_log(_"INFO", _"VR binary verified: " + P_0 + _"; sha256=" + text);
			}
		}
		catch
		{
			fileStream.Dispose();
			throw;
		}
	}

	internal void A()
	{
		b();
		foreach (VrCodeWatcher codeWatcher in _codeWatchers)
		{
			string failure = codeWatcher.Failure;
			if (failure != null)
			{
				throw new InvalidDataException(failure);
			}
		}
		if (_xrRegistrationBaseline != null && _xrRegistrationBaseline != B())
		{
			throw new InvalidDataException(_"OpenXR runtime or layer registration changed; restart the protected session.");
		}
		foreach (KeyValuePair<string, string> layerManifestHash in _layerManifestHashes)
		{
			if (!C(layerManifestHash.Key).Equals(layerManifestHash.Value, StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException(_"OpenXR layer manifest changed: " + layerManifestHash.Key);
			}
		}
		foreach (string absentRegistrationPath in _absentRegistrationPaths)
		{
			if (File.Exists(absentRegistrationPath))
			{
				throw new InvalidDataException(_"OpenVR registration appeared during protected play: " + absentRegistrationPath);
			}
		}
		foreach (KeyValuePair<string, string> registrationHash in _registrationHashes)
		{
			if (!File.Exists(registrationHash.Key))
			{
				throw new InvalidDataException(_"OpenVR registration disappeared during protected play: " + registrationHash.Key);
			}
			if (!C(registrationHash.Key).Equals(registrationHash.Value, StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException(_"OpenVR registration changed during protected play: " + registrationHash.Key);
			}
		}
		foreach (KeyValuePair<string, Dictionary<string, string>> securityTree in _securityTrees)
		{
			string text = a(securityTree.Key, securityTree.Value.Keys);
			if (text != null)
			{
				throw new InvalidDataException(text);
			}
		}
		foreach (string configurationPath in _configurationPaths)
		{
			A(configurationPath, _initialConfigurationPaths.Contains(configurationPath));
		}
		C();
		A(_"vrcompositor", _vrCompositorPaths, _compositorContinuity);
	}

	internal static void A(string P_0, IEnumerable<string> P_1, string P_2)
	{
		if (P_1.Contains<string>(P_0, StringComparer.OrdinalIgnoreCase))
		{
			throw new InvalidDataException(_"VR code was revoked by the signed release policy: " + P_2 + _"; sha256=" + P_0);
		}
	}

	internal static void A(string P_0, bool P_1)
	{
		if (!File.Exists(P_0))
		{
			if (P_1)
			{
				throw new InvalidDataException(_"SteamVR configuration disappeared during protected play: " + P_0);
			}
			return;
		}
		ReadOnlySpan<byte> utf8Json = E(P_0);
		JsonDocumentOptions jsonOptions = JsonOptions;
		if (!A((JsonNode.Parse(utf8Json, null, jsonOptions) as JsonObject) ?? throw new InvalidDataException(_"SteamVR settings must be a JSON object: " + P_0)))
		{
			return;
		}
		throw new InvalidDataException(_"Restricted SteamVR prediction override appeared during protected play: " + P_0);
	}

	internal static void a()
	{
		string[] array = new string[2]
		{
			_"vrserver",
			_"vrcompositor"
		};
		for (int i = 0; i < array.Length; i++)
		{
			Process[] processesByName = Process.GetProcessesByName(array[i]);
			try
			{
				if (processesByName.Length != 0)
				{
					throw new InvalidDataException(_"Close SteamVR before starting Gorilla Guard. Protected play requires a fresh verified VR session.");
				}
			}
			finally
			{
				Process[] array2 = processesByName;
				for (int j = 0; j < array2.Length; j++)
				{
					array2[j].Dispose();
				}
			}
		}
	}

	private static string B()
	{
		List<string> list = new List<string>();
		RegistryHive[] array = new RegistryHive[2]
		{
			RegistryHive.LocalMachine,
			RegistryHive.CurrentUser
		};
		for (int i = 0; i < array.Length; i++)
		{
			RegistryHive registryHive = array[i];
			RegistryView[] array2 = new RegistryView[2]
			{
				RegistryView.Registry64,
				RegistryView.Registry32
			};
			for (int j = 0; j < array2.Length; j++)
			{
				RegistryView view = array2[j];
				string[] array3 = new string[3]
				{
					_"SOFTWARE\\Khronos\\OpenXR\\1",
					_"SOFTWARE\\Khronos\\OpenXR\\1\\ApiLayers\\Implicit",
					_"SOFTWARE\\Khronos\\OpenXR\\1\\ApiLayers\\Explicit"
				};
				foreach (string text in array3)
				{
					using RegistryKey registryKey = RegistryKey.OpenBaseKey(registryHive, view);
					using RegistryKey registryKey2 = registryKey.OpenSubKey(text);
					list.Add(registryHive.ToString() + _"|" + view.ToString() + _"|" + text + _"|present=" + (registryKey2 != null));
					if (registryKey2 == null)
					{
						continue;
					}
					foreach (string item in registryKey2.GetValueNames().OrderBy<string, string>((string P_0) => P_0, StringComparer.Ordinal))
					{
						if (!text.EndsWith(_"OpenXR\\1", StringComparison.Ordinal) || !(item != _"ActiveRuntime"))
						{
							object value = registryKey2.GetValue(item, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
							list.Add(JsonSerializer.Serialize(new
							{
								hive = registryHive,
								view = view,
								subkey = text,
								name = item,
								kind = registryKey2.GetValueKind(item),
								value = value
							}));
						}
					}
				}
			}
		}
		return string.Join(_"\n", list);
	}

	private static void b()
	{
		string[] array = new string[5]
		{
			_"VR_PATHREG_OVERRIDE",
			_"VR_OVERRIDE",
			_"VR_DRIVER_PATH",
			_"XR_API_LAYER_PATH",
			_"XR_ENABLE_API_LAYERS"
		};
		foreach (string text in array)
		{
			if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(text)))
			{
				throw new InvalidDataException(_"VR preflight: remove the runtime/layer override " + text + _" before protected play.");
			}
		}
	}

	private void a(string P_0)
	{
		string fullPath = Path.GetFullPath(P_0);
		e(fullPath);
		FileStream fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
		try
		{
			_registrationHashes[fullPath] = Convert.ToHexString(SHA256.HashData(fileStream));
			A(fullPath, fileStream);
		}
		catch
		{
			fileStream.Dispose();
			throw;
		}
	}

	private void A(string P_0, IEnumerable<string> P_1)
	{
		string fullPath = Path.GetFullPath(P_0);
		_codeWatchers.Add(new VrCodeWatcher(fullPath));
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach (string item in P_1.Where(b))
		{
			string fullPath2 = Path.GetFullPath(item);
			FileStream fileStream = new FileStream(fullPath2, FileMode.Open, FileAccess.Read, FileShare.Read);
			try
			{
				dictionary[fullPath2] = Convert.ToHexString(SHA256.HashData(fileStream));
				A(fullPath2, fileStream);
			}
			catch
			{
				fileStream.Dispose();
				throw;
			}
		}
		_securityTrees[fullPath] = dictionary;
	}

	private static HashSet<string> B(string P_0)
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (string item in F(P_0).Where(b))
		{
			hashSet.Add(Path.GetFullPath(item));
		}
		return hashSet;
	}

	internal static string? a(string P_0, IEnumerable<string> P_1)
	{
		HashSet<string> current = B(P_0);
		HashSet<string> expected = new HashSet<string>(P_1.Select(Path.GetFullPath), StringComparer.OrdinalIgnoreCase);
		string text = expected.FirstOrDefault((string P_0) => !current.Contains(P_0));
		if (text != null)
		{
			return _"Protected VR code or manifest disappeared during play: " + text;
		}
		string text2 = current.FirstOrDefault((string P_0) => !expected.Contains(P_0));
		if (text2 != null)
		{
			return _"New VR code or manifest appeared during protected play: " + text2;
		}
		return null;
	}

	private static bool b(string P_0)
	{
		string extension = Path.GetExtension(P_0);
		if (!extension.Equals(_".dll", StringComparison.OrdinalIgnoreCase) && !extension.Equals(_".exe", StringComparison.OrdinalIgnoreCase))
		{
			return extension.Equals(_".vrdrivermanifest", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static string C(string P_0)
	{
		e(P_0);
		using FileStream source = new FileStream(P_0, FileMode.Open, FileAccess.Read, FileShare.Read);
		return Convert.ToHexString(SHA256.HashData(source));
	}

	private void A(string P_0, FileStream P_1)
	{
		string fullPath = Path.GetFullPath(P_0);
		if (!_lockedPaths.Add(fullPath))
		{
			P_1.Dispose();
		}
		else
		{
			_locks.Add(P_1);
		}
	}

	private void A(string P_0, HashSet<string> P_1)
	{
		if (Directory.Exists(P_0))
		{
			string full = Path.GetFullPath(P_0).TrimEnd(Path.DirectorySeparatorChar);
			ModuleApprovalRoot moduleApprovalRoot = _moduleRoots.FirstOrDefault((ModuleApprovalRoot P_0) => P_0.Path.Equals(full, StringComparison.OrdinalIgnoreCase));
			if (moduleApprovalRoot != null)
			{
				moduleApprovalRoot.Publishers.UnionWith(P_1);
			}
			else
			{
				_moduleRoots.Add(new ModuleApprovalRoot(full, new HashSet<string>(P_1, StringComparer.Ordinal)));
			}
		}
	}

	private void C()
	{
		A(_"vrserver", _vrServerPaths, _serverContinuity);
	}

	private void A(string P_0, HashSet<string> P_1, VrProcessContinuity P_2)
	{
		Process[] processesByName = Process.GetProcessesByName(P_0);
		try
		{
			List<VrProcessIdentity> list = new List<VrProcessIdentity>();
			Process[] array = processesByName;
			foreach (Process process in array)
			{
				process.Refresh();
				string fullPath = Path.GetFullPath(process.MainModule?.FileName ?? throw new InvalidDataException(_"SteamVR vrserver executable path is unavailable."));
				if (!P_1.Contains(fullPath))
				{
					throw new InvalidDataException(_"Unapproved SteamVR vrserver process: pid=" + process.Id + _"; path=" + fullPath);
				}
				list.Add(new VrProcessIdentity(process.Id, process.StartTime.ToUniversalTime().Ticks, fullPath));
				HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				foreach (ProcessModule module in process.Modules)
				{
					string fileName = module.FileName;
					if (string.IsNullOrWhiteSpace(fileName))
					{
						throw new InvalidDataException(_"SteamVR vrserver returned a module without a readable path; pid=" + process.Id);
					}
					hashSet.Add(Path.GetFullPath(fileName));
				}
				if (hashSet.Count == 0)
				{
					throw new InvalidDataException(_"SteamVR vrserver returned no readable modules; pid=" + process.Id);
				}
				foreach (string item in hashSet)
				{
					c(item);
				}
			}
			string text = P_2.A(list);
			if (text != null)
			{
				throw new InvalidDataException(P_0 + _": " + text);
			}
		}
		catch (Win32Exception ex)
		{
			throw new InvalidDataException(_"SteamVR vrserver module inspection failed: " + ex.Message, ex);
		}
		finally
		{
			Process[] array = processesByName;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Dispose();
			}
		}
	}

	private void c(string P_0)
	{
		e(P_0);
		if (_vrServerPaths.Contains(P_0) || _vrCompositorPaths.Contains(P_0))
		{
			return;
		}
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
		if (_approvedLoadedModules.Contains(P_0))
		{
			return;
		}
		HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
		if (A(P_0, folderPath))
		{
			hashSet.UnionWith(new string[9]
			{
				_"Microsoft Windows",
				_"Microsoft Corporation",
				_"Microsoft Windows Publisher",
				_"Microsoft Windows Software Compatibility Publisher",
				_"Microsoft Windows Hardware Compatibility Publisher",
				_"NVIDIA Corporation",
				_"Advanced Micro Devices",
				_"Advanced Micro Devices, Inc.",
				_"Intel Corporation"
			});
		}
		foreach (ModuleApprovalRoot moduleRoot in _moduleRoots)
		{
			if (A(P_0, moduleRoot.Path))
			{
				hashSet.UnionWith(moduleRoot.Publishers);
			}
		}
		if (hashSet.Count == 0)
		{
			throw new InvalidDataException(_"SteamVR vrserver loaded a module outside approved Windows/SteamVR/driver roots: " + P_0);
		}
		e(P_0);
		FileStream fileStream = new FileStream(P_0, FileMode.Open, FileAccess.Read, FileShare.Read);
		try
		{
			string text = Convert.ToHexString(SHA256.HashData(fileStream));
			A(text, _revokedHashes, P_0);
			if (!_approvedHashes.Contains(text) && (!Path.GetFileName(P_0).Equals(_"driver_01spacecalibrator.dll", StringComparison.OrdinalIgnoreCase) || !_approvedSpaceCalibratorDrivers.Contains(text)))
			{
				string text2 = Authenticode.A(P_0);
				if (!hashSet.Contains(text2))
				{
					throw new InvalidDataException(_"SteamVR vrserver module publisher is not approved: " + text2 + _"; path=" + P_0 + _"; sha256=" + text);
				}
			}
			A(P_0, fileStream);
			_approvedLoadedModules.Add(P_0);
			_log(_"INFO", _"SteamVR vrserver module approved and locked: " + P_0 + _"; sha256=" + text);
		}
		catch
		{
			fileStream.Dispose();
			throw;
		}
	}

	private static bool A(string P_0, string P_1)
	{
		string fullPath = Path.GetFullPath(P_0);
		string value = Path.GetFullPath(P_1).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
		return fullPath.StartsWith(value, StringComparison.OrdinalIgnoreCase);
	}

	private void c()
	{
		RegistryHive[] array = new RegistryHive[2]
		{
			RegistryHive.LocalMachine,
			RegistryHive.CurrentUser
		};
		foreach (RegistryHive hKey in array)
		{
			RegistryView[] array2 = new RegistryView[2]
			{
				RegistryView.Registry64,
				RegistryView.Registry32
			};
			foreach (RegistryView view in array2)
			{
				using RegistryKey registryKey = RegistryKey.OpenBaseKey(hKey, view);
				using RegistryKey registryKey2 = registryKey.OpenSubKey(_"SOFTWARE\\Khronos\\OpenXR\\1\\ApiLayers\\Implicit");
				if (registryKey2 == null)
				{
					continue;
				}
				string[] valueNames = registryKey2.GetValueNames();
				int num = 0;
				while (num < valueNames.Length)
				{
					string text = valueNames[num];
					if (registryKey2.GetValueKind(text) == RegistryValueKind.DWord)
					{
						object value = registryKey2.GetValue(text);
						if (value is int)
						{
							if ((int)value == 0)
							{
								if (!Path.IsPathFullyQualified(text))
								{
									throw new InvalidDataException(_"OpenXR layer manifest must have an absolute path: " + text);
								}
								using JsonDocument jsonDocument = d(text);
								string path = jsonDocument.RootElement.GetProperty(_"api_layer").GetProperty(_"library_path").GetString() ?? throw new InvalidDataException(_"OpenXR layer library is missing: " + text);
								string fullPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(text), path));
								if (!_approvedHashes.Contains(ReleaseVerifier.a(fullPath)))
								{
									throw new InvalidDataException(_"Unapproved active OpenXR implicit layer: " + text + _". Disable it in its owning application before protected play.");
								}
								a(fullPath, new HashSet<string>(StringComparer.Ordinal));
								a(text);
								_layerManifestHashes[Path.GetFullPath(text)] = C(text);
								_log(_"INFO", _"Approved OpenXR implicit layer: " + text);
							}
							num++;
							continue;
						}
					}
					throw new InvalidDataException(_"Malformed OpenXR implicit layer registration: " + text);
				}
			}
		}
	}

	internal void a(bool P_0 = true)
	{
		string text = Path.Combine(_gameRoot, _"Gorilla Tag_Data", _"Plugins", _"x86_64", _"openvr_api.dll");
		if (!_manifest.TryGetValue(_"openvrApiSha256", out string value))
		{
			_log(_"INFO", _"OpenVR API integrity is covered by the signed native-game tree; this release has no separate repair payload.");
			return;
		}
		if (value.Length != 64 || !value.All(Uri.IsHexDigit))
		{
			throw new InvalidDataException(_"Invalid signed OpenVR API hash.");
		}
		if (File.Exists(text) && ReleaseVerifier.a(text).Equals(value, StringComparison.OrdinalIgnoreCase))
		{
			return;
		}
		if (!P_0)
		{
			throw new InvalidDataException(_"OpenVR API does not match the signed release; a normal protected launch will attempt authenticated repair.");
		}
		string text2 = Path.Combine(_manifestRoot, _"repair", _"openvr_api.dll");
		e(text2);
		e(Path.GetDirectoryName(text));
		if (!ReleaseVerifier.a(text2).Equals(value, StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidDataException(_"OpenVR API repair payload does not match the signed release. Reinstall the current signed package.");
		}
		using FileStream fileStream = new FileStream(text2, FileMode.Open, FileAccess.Read, FileShare.Read);
		if (!Convert.ToHexString(SHA256.HashData(fileStream)).Equals(value, StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidDataException(_"OpenVR repair payload changed during verification.");
		}
		fileStream.Position = 0L;
		if (File.Exists(text))
		{
			e(text);
			File.Copy(text, D(_"openvr_api.dll"), overwrite: false);
		}
		string text3 = text + _".guard-repair-" + Guid.NewGuid().ToString(_"N");
		try
		{
			using (FileStream destination = new FileStream(text3, FileMode.CreateNew, FileAccess.Write, FileShare.None))
			{
				fileStream.CopyTo(destination);
			}
			File.Move(text3, text, overwrite: true);
		}
		finally
		{
			if (File.Exists(text3))
			{
				File.Delete(text3);
			}
		}
		_log(_"WARNING", _"Repaired openvr_api.dll from the authenticated release payload; previous file backed up in LocalAppData/GorillaGuard/backups.");
	}

	internal static bool A(JsonObject P_0)
	{
		bool result = false;
		KeyValuePair<string, JsonNode>[] array = P_0.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<string, JsonNode> keyValuePair = array[i];
			if (keyValuePair.Key != _"steamvr" && keyValuePair.Key != _"steam.app.1533390")
			{
				continue;
			}
			if (!(keyValuePair.Value is JsonObject jsonObject))
			{
				throw new InvalidDataException(_"SteamVR settings section must be an object: " + keyValuePair.Key);
			}
			string[] array2 = new string[4]
			{
				_"poseTimeOffset",
				_"predictionTime",
				_"predictionTimeOffset",
				_"secondsFromVsyncToPhotons"
			};
			foreach (string propertyName in array2)
			{
				if (jsonObject.TryGetPropertyValue(propertyName, out JsonNode jsonNode) && (!(jsonNode is JsonValue jsonValue) || !jsonValue.TryGetValue<double>(out var value) || !double.IsFinite(value) || value != 0.0))
				{
					jsonObject.Remove(propertyName);
					result = true;
				}
			}
		}
		return result;
	}

	internal void a(string P_0, bool P_1)
	{
		e(P_0);
		byte[] array = E(P_0);
		ReadOnlySpan<byte> utf8Json = array;
		JsonDocumentOptions jsonOptions = JsonOptions;
		JsonObject jsonObject = (JsonNode.Parse(utf8Json, null, jsonOptions) as JsonObject) ?? throw new InvalidDataException(_"SteamVR settings must be a JSON object: " + P_0);
		if (!A(jsonObject))
		{
			return;
		}
		if (!P_1)
		{
			throw new InvalidDataException(_"Prediction overrides require cleanup: " + P_0);
		}
		string[] array2 = new string[2]
		{
			_"vrserver",
			_"vrcompositor"
		};
		for (int i = 0; i < array2.Length; i++)
		{
			Process[] processesByName = Process.GetProcessesByName(array2[i]);
			bool flag = processesByName.Length != 0;
			Process[] array3 = processesByName;
			for (int j = 0; j < array3.Length; j++)
			{
				array3[j].Dispose();
			}
			if (flag)
			{
				throw new InvalidDataException(_"Close SteamVR before cleaning prediction overrides in " + P_0 + _". Then run Gorilla Guard again.");
			}
		}
		string text = D(Path.GetFileName(P_0));
		byte[] bytes = Encoding.UTF8.GetBytes(jsonObject.ToJsonString(new JsonSerializerOptions
		{
			WriteIndented = true
		}));
		File.WriteAllBytes(text, array);
		using (FileStream fileStream = new FileStream(P_0, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
		{
			if (!SHA256.HashData(fileStream).SequenceEqual(SHA256.HashData(array)))
			{
				throw new IOException(_"SteamVR configuration changed during audit; retry launch: " + P_0);
			}
			try
			{
				fileStream.Position = 0L;
				fileStream.Write(bytes);
				fileStream.SetLength(bytes.Length);
				fileStream.Flush(flushToDisk: true);
			}
			catch
			{
				try
				{
					fileStream.Position = 0L;
					fileStream.Write(array);
					fileStream.SetLength(array.Length);
					fileStream.Flush(flushToDisk: true);
				}
				catch (Exception ex)
				{
					_log(_"ERROR", _"Configuration write and rollback failed: " + ex.Message + _"; original backup=" + text);
				}
				throw;
			}
		}
		_restores.Add((P_0, text, Convert.ToHexString(SHA256.HashData(bytes))));
		_log(_"WARNING", _"Temporarily removed prediction overrides: " + P_0 + _"; backup=" + text);
	}

	private string D(string P_0)
	{
		string text = Path.Combine(_backupRoot, Guid.NewGuid().ToString(_"N"));
		Directory.CreateDirectory(text);
		return Path.Combine(text, P_0);
	}

	internal static JsonDocument d(string P_0)
	{
		return JsonDocument.Parse(E(P_0), JsonOptions);
	}

	private static byte[] E(string P_0)
	{
		e(P_0);
		using FileStream fileStream = new FileStream(P_0, FileMode.Open, FileAccess.Read, FileShare.Read);
		if (fileStream.Length > 2097152)
		{
			throw new InvalidDataException(_"VR JSON exceeds the 2 MB limit: " + P_0);
		}
		using MemoryStream memoryStream = new MemoryStream();
		fileStream.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}

	internal static void e(string P_0)
	{
		for (string text = Path.GetFullPath(P_0); text != null; text = Path.GetDirectoryName(text))
		{
			if ((File.GetAttributes(text) & FileAttributes.ReparsePoint) != 0)
			{
				throw new InvalidDataException(_"Redirected VR paths are not allowed: " + text);
			}
		}
	}

	[IteratorStateMachine(typeof(_003CWalkFiles_003Ed__66))]
	internal static IEnumerable<string> F(string P_0)
	{

		return new _003CWalkFiles_003Ed__66(-2)
		{
			_003C_003E3__root = P_0
		};
	}

	public void Dispose()
	{
		foreach (VrCodeWatcher codeWatcher in _codeWatchers)
		{
			codeWatcher.Dispose();
		}
		_codeWatchers.Clear();
		foreach (FileStream @lock in _locks)
		{
			@lock.Dispose();
		}
		_locks.Clear();
		_lockedPaths.Clear();
		_approvedLoadedModules.Clear();
		foreach (var restore in _restores)
		{
			try
			{
				e(restore.Path);
				using FileStream fileStream = new FileStream(restore.Path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
				if (!Convert.ToHexString(SHA256.HashData(fileStream)).Equals(restore.SanitizedHash, StringComparison.Ordinal))
				{
					_log(_"WARNING", _"SteamVR settings were edited during play; preserving edits. Original backup: " + restore.Backup);
					continue;
				}
				byte[] array = File.ReadAllBytes(restore.Backup);
				fileStream.Position = 0L;
				fileStream.Write(array);
				fileStream.SetLength(array.Length);
				fileStream.Flush(flushToDisk: true);
				_log(_"INFO", _"Restored SteamVR configuration: " + restore.Path);
			}
			catch (Exception ex)
			{
				_log(_"WARNING", _"Could not restore SteamVR settings: " + ex.Message + _"; backup=" + restore.Backup);
			}
		}
		_restores.Clear();
	}
}
#pragma endregion

#pragma region GorillaGuard.Launcher/VrProcessContinuity.cs
using System;
using System.Collections.Generic;
using System.IO;

namespace GorillaGuard.Launcher;

internal sealed class VrProcessContinuity
{
	private VrProcessIdentity? _bound;

	private string? _failure;

	internal VrProcessIdentity? Bound => _bound;

	internal string? A(IReadOnlyList<VrProcessIdentity> P_0)
	{
		if (_failure != null)
		{
			return _failure;
		}
		if (P_0.Count > 1)
		{
			return _failure = _"Multiple SteamVR server processes; restart protected play.";
		}
		if (P_0.Count == 0)
		{
			if (!(_bound == null))
			{
				return _failure = _"The protected SteamVR server exited; restart protected play.";
			}
			return null;
		}
		VrProcessIdentity vrProcessIdentity = P_0[0];
		if (vrProcessIdentity.Pid <= 0 || vrProcessIdentity.StartedUtcTicks <= 0 || !Path.IsPathFullyQualified(vrProcessIdentity.Executable))
		{
			return _failure = _"SteamVR process identity could not be established.";
		}
		if (_bound == null)
		{
			_bound = vrProcessIdentity;
			return null;
		}
		if (_bound.Pid != vrProcessIdentity.Pid || _bound.StartedUtcTicks != vrProcessIdentity.StartedUtcTicks || !string.Equals(_bound.Executable, vrProcessIdentity.Executable, StringComparison.OrdinalIgnoreCase))
		{
			return _failure = _"The protected SteamVR server was replaced; restart protected play.";
		}
		return null;
	}
}
#pragma endregion

#pragma region GorillaGuard.Launcher/VrProcessIdentity.cs
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace GorillaGuard.Launcher;

internal sealed class VrProcessIdentity(int Pid, long StartedUtcTicks, string Executable) : IEquatable<VrProcessIdentity>
{
		private Type EqualityContract
	{
				get
		{
			return typeof(VrProcessIdentity);
		}
	}

	public int Pid { get; init; }

	public long StartedUtcTicks { get; init; }

	public string Executable { get; init; }

	public VrProcessIdentity(int Pid, long StartedUtcTicks, string Executable)
	{
		this.Pid = Pid;
		this.StartedUtcTicks = StartedUtcTicks;
		this.Executable = Executable;
		base._002Ector();
	}

		public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(_"VrProcessIdentity");
		stringBuilder.Append(_" { ");
		if (A(stringBuilder))
		{
			stringBuilder.Append(' ');
		}
		stringBuilder.Append('}');
		return stringBuilder.ToString();
	}

		private bool A(StringBuilder P_0)
	{
		RuntimeHelpers.EnsureSufficientExecutionStack();
		P_0.Append(_"Pid = ");
		P_0.Append(Pid.ToString());
		P_0.Append(_", StartedUtcTicks = ");
		P_0.Append(StartedUtcTicks.ToString());
		P_0.Append(_", Executable = ");
		P_0.Append((object?)Executable);
		return true;
	}

		public static bool operator !=(VrProcessIdentity? left, VrProcessIdentity? right)
	{
		return !(left == right);
	}

		public static bool operator ==(VrProcessIdentity? left, VrProcessIdentity? right)
	{
		if ((object)left != right)
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

		public override int GetHashCode()
	{
		return ((EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(Pid)) * -1521134295 + EqualityComparer<long>.Default.GetHashCode(StartedUtcTicks)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Executable);
	}

		public override bool Equals(object? obj)
	{
		return Equals(obj as VrProcessIdentity);
	}

		public bool Equals(VrProcessIdentity? other)
	{
		if ((object)this != other)
		{
			if ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<int>.Default.Equals(Pid, other.Pid) && EqualityComparer<long>.Default.Equals(StartedUtcTicks, other.StartedUtcTicks))
			{
				return EqualityComparer<string>.Default.Equals(Executable, other.Executable);
			}
			return false;
		}
		return true;
	}

		private VrProcessIdentity(VrProcessIdentity original)
	{
		Pid = original.Pid;
		StartedUtcTicks = original.StartedUtcTicks;
		Executable = original.Executable;
	}

		public void Deconstruct(out int Pid, out long StartedUtcTicks, out string Executable)
	{
		Pid = this.Pid;
		StartedUtcTicks = this.StartedUtcTicks;
		Executable = this.Executable;
	}
}
#pragma endregion

#pragma region GorillaGuard.Security/DesktopShell.cs
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GorillaGuard.Security;

internal static class DesktopShell
{
	private sealed class GuardWindow : Form
	{
		private readonly Label status = new Label
		{
			AutoSize = false,
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft
		};

		private readonly TextBox activity = new TextBox
		{
			Multiline = true,
			ReadOnly = true,
			ScrollBars = ScrollBars.Vertical,
			Dock = DockStyle.Fill,
			BorderStyle = BorderStyle.None
		};

		private readonly Button action = new Button
		{
			Text = _"Start protected session",
			AutoSize = true,
			Padding = new Padding(18, 10, 18, 10),
			FlatStyle = FlatStyle.Flat
		};

		private readonly Button scan = new Button
		{
			Text = _"Scan only",
			AutoSize = true,
			Padding = new Padding(18, 10, 18, 10),
			FlatStyle = FlatStyle.Flat
		};

		private readonly ProgressBar progress = new ProgressBar
		{
			Dock = DockStyle.Fill,
			Style = ProgressBarStyle.Blocks
		};

		private readonly CancellationTokenSource stop = new CancellationTokenSource();

		private readonly ConcurrentQueue<string> messages = new ConcurrentQueue<string>();

		private readonly System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer
		{
			Interval = 150
		};

		private readonly Func<string[], Task<int>> run;

		private readonly string[] args;

		private readonly string purpose;

		private readonly TextWriter previousOut = Console.Out;

		private readonly TextWriter previousError = Console.Error;

		private bool running;

		private bool finished;

		private bool closing;

		private bool activeScan;

		private int pending;

		internal int Result { get; private set; }

		internal GuardWindow(string[] arguments, Func<string[], Task<int>> backend, string purpose, Action<CancellationToken> setToken)
		{
			args = arguments;
			run = backend;
			this.purpose = purpose;
			setToken(stop.Token);
			Text = _"Gorilla Guard";
			base.ClientSize = new Size(820, 620);
			MinimumSize = new Size(700, 540);
			base.StartPosition = FormStartPosition.CenterScreen;
			base.AutoScaleMode = AutoScaleMode.Dpi;
			BackColor = Color.FromArgb(12, 20, 32);
			ForeColor = Color.FromArgb(235, 242, 248);
			Font = new Font(_"Segoe UI", 10f);
			base.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath);
			TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(28),
				ColumnCount = 1,
				RowCount = 7
			};
			int[] array = new int[5] { 110, 40, 52, 10, 18 };
			foreach (int num in array)
			{
				tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, num));
			}
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 64f));
			TableLayoutPanel tableLayoutPanel2 = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 2
			};
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108f));
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			PictureBox logo = new PictureBox
			{
				Dock = DockStyle.Fill,
				SizeMode = PictureBoxSizeMode.Zoom
			};
			using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(_"GorillaGuard.Logo.png"))
			{
				if (stream != null)
				{
					using Image original = Image.FromStream(stream);
					logo.Image = new Bitmap(original);
				}
			}
			Label control = new Label
			{
				Text = _"GORILLA GUARD\nLocal protection. One session at a time.",
				Dock = DockStyle.Fill,
				TextAlign = ContentAlignment.MiddleLeft,
				Font = new Font(_"Segoe UI Semibold", 18f)
			};
			tableLayoutPanel2.Controls.Add(logo, 0, 0);
			tableLayoutPanel2.Controls.Add(control, 1, 0);
			tableLayoutPanel.Controls.Add(tableLayoutPanel2, 0, 0);
			tableLayoutPanel.Controls.Add(new Label
			{
				Text = purpose + _"  •  v1.12.31",
				Dock = DockStyle.Fill,
				TextAlign = ContentAlignment.MiddleLeft,
				ForeColor = Color.FromArgb(101, 215, 211)
			}, 0, 1);
			status.Text = ((purpose == _"Secure installer") ? _"Ready to download and verify Gorilla Guard." : _"Ready to check your installation and launch Gorilla Tag.");
			tableLayoutPanel.Controls.Add(status, 0, 2);
			tableLayoutPanel.Controls.Add(progress, 0, 3);
			tableLayoutPanel.Controls.Add(new Label
			{
				Text = _"ACTIVITY",
				Dock = DockStyle.Fill,
				Font = new Font(_"Segoe UI", 8f)
			}, 0, 4);
			activity.BackColor = Color.FromArgb(19, 30, 45);
			activity.ForeColor = ForeColor;
			tableLayoutPanel.Controls.Add(activity, 0, 5);
			FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				FlowDirection = FlowDirection.RightToLeft,
				Padding = new Padding(0, 10, 0, 0)
			};
			action.BackColor = Color.FromArgb(65, 218, 205);
			action.ForeColor = Color.FromArgb(8, 22, 32);
			scan.BackColor = Color.FromArgb(28, 45, 63);
			scan.ForeColor = ForeColor;
			if (purpose == _"Secure installer")
			{
				action.Text = _"Install Gorilla Guard";
			}
			flowLayoutPanel.Controls.Add(action);
			if (purpose != _"Secure installer")
			{
				flowLayoutPanel.Controls.Add(scan);
			}
			tableLayoutPanel.Controls.Add(flowLayoutPanel, 0, 6);
			base.Controls.Add(tableLayoutPanel);
			UiWriter uiWriter = new UiWriter(A);
			Console.SetOut(uiWriter);
			Console.SetError(uiWriter);
			timer.Tick += delegate
			{
				A();
			};
			timer.Start();
			action.Click += async delegate
			{
				if (finished)
				{
					Close();
				}
				else if (running)
				{
					a();
				}
				else
				{
					await A(false);
				}
			};
			scan.Click += async delegate
			{
				if (!finished && !running)
				{
					await A(true);
				}
			};
			base.FormClosing += delegate(object? _, FormClosingEventArgs P_1)
			{
				if (running)
				{
					P_1.Cancel = true;
					closing = true;
					a();
				}
			};
			base.FormClosed += delegate
			{
				timer.Stop();
				Console.SetOut(previousOut);
				Console.SetError(previousError);
				logo.Image?.Dispose();
				stop.Dispose();
				timer.Dispose();
			};
			if (!args.Contains(_"--ui-preview"))
			{
				return;
			}
			base.Shown += delegate
			{
				using Bitmap bitmap = new Bitmap(base.Width, base.Height);
				DrawToBitmap(bitmap, new Rectangle(0, 0, base.Width, base.Height));
				bitmap.Save(Path.Combine(AppContext.BaseDirectory, _"GorillaGuard.UI.png"));
				Close();
			};
		}

		private void A(string P_0)
		{
			if (Interlocked.Increment(ref pending) > 256)
			{
				Interlocked.Decrement(ref pending);
			}
			else
			{
				messages.Enqueue((P_0.Length > 1200) ? P_0.Substring(0, 1200) : P_0);
			}
		}

		private void A()
		{
			for (int i = 0; i < 40; i++)
			{
				if (!messages.TryDequeue(out string result))
				{
					break;
				}
				Interlocked.Decrement(ref pending);
				if (activity.TextLength > 24000)
				{
					TextBox textBox = activity;
					string text = activity.Text;
					textBox.Text = text.Substring(text.Length - 12000);
				}
				activity.AppendText(result + Environment.NewLine);
				if (running && !stop.IsCancellationRequested)
				{
					if (result.Contains(_"[ERROR]") || result.Contains(_"STOPPED:"))
					{
						status.Text = _"An issue needs attention. Review the details below.";
					}
					else if (result.Contains(_"Protected game process found"))
					{
						status.Text = _"Game started. Waiting for the guard to verify the protected session.";
					}
					else if (result.Contains(_"GorillaGuard.dll authenticated to the launcher."))
					{
						status.Text = _"Protected session connected. Keep this window open while playing.";
					}
					else if (result.Contains(_"Downloading"))
					{
						status.Text = _"Downloading and verifying the signed release…";
					}
				}
			}
		}

		private void a()
		{
			stop.Cancel();
			action.Enabled = false;
			scan.Enabled = false;
			status.Text = (activeScan ? _"Stopping the scan safely…" : _"Stopping safely… The protected game will close and cleanup will follow.");
		}

		private async Task A(bool P_0)
		{
			activeScan = P_0;
			running = true;
			scan.Enabled = false;
			action.Text = (P_0 ? _"Cancel scan" : ((purpose == _"Secure installer") ? _"Cancel installation" : _"Stop session"));
			progress.Style = ProgressBarStyle.Marquee;
			status.Text = (P_0 ? _"Scanning only… No quarantine, repair, settings change, or game launch will occur." : _"Working… Checking files, updates, and your game environment.");
			string[] runArgs = (P_0 ? args.Append(_"--scan-only").ToArray() : args);
			try
			{
				Result = await Task.Run(() => run(runArgs));
			}
			catch (OperationCanceledException)
			{
				Result = 1;
			}
			catch (Exception ex2)
			{
				Result = 1;
				A(_"Could not complete this session: " + ex2.Message);
			}
			finally
			{
				running = false;
				finished = true;
				A();
				progress.Style = ProgressBarStyle.Blocks;
				status.Text = ((stop.IsCancellationRequested && P_0) ? _"Read-only scan stopped. Close this window when ready." : (stop.IsCancellationRequested ? _"Session stopped. Any scheduled cleanup runs after this window closes." : ((P_0 && Result == 0) ? _"Read-only scan passed. Review the activity below, then close this window." : ((P_0 && Result == 2) ? _"Read-only scan was incomplete. Review the activity below for details." : (P_0 ? _"Read-only scan found an issue. Review the activity below for details." : ((Result == 0) ? _"Finished. Close this window to allow any scheduled cleanup to complete." : _"Session could not complete. Review the activity below for details."))))));
				action.Enabled = true;
				action.Text = _"Close";
				if (closing || (!P_0 && Result == 0) || stop.IsCancellationRequested)
				{
					Close();
				}
			}
		}

				private void A(object? _, EventArgs P_1)
		{
			A();
		}

				private async void a(object? _, EventArgs P_1)
		{
			if (finished)
			{
				Close();
			}
			else if (running)
			{
				a();
			}
			else
			{
				await A(false);
			}
		}

				private async void B(object? _, EventArgs P_1)
		{
			if (!finished && !running)
			{
				await A(true);
			}
		}

				private void A(object? _, FormClosingEventArgs P_1)
		{
			if (running)
			{
				P_1.Cancel = true;
				closing = true;
				a();
			}
		}

				private void b(object? _, EventArgs P_1)
		{
			using Bitmap bitmap = new Bitmap(base.Width, base.Height);
			DrawToBitmap(bitmap, new Rectangle(0, 0, base.Width, base.Height));
			bitmap.Save(Path.Combine(AppContext.BaseDirectory, _"GorillaGuard.UI.png"));
			Close();
		}
	}

	private sealed class UiWriter(Action<string> write) : TextWriter()
	{
		public override Encoding Encoding => System.Text.Encoding.UTF8;

		public override void WriteLine(string? value)
		{
			write(value ?? _"");
		}

		public override void Write(char value)
		{
			if (value != '\r' && value != '\n')
			{
				write(value.ToString());
			}
		}
	}

	internal static int A(string[] P_0, Func<string[], Task<int>> P_1, string P_2, Action<CancellationToken> P_3)
	{
		if (P_0.Length != 0 && !P_0.Contains(_"--ui-preview"))
		{
			return P_1(P_0).GetAwaiter().GetResult();
		}
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		using GuardWindow guardWindow = new GuardWindow(P_0, P_1, P_2, P_3);
		Application.Run(guardWindow);
		return guardWindow.Result;
	}
}
#pragma endregion

#pragma region GorillaGuard.Security/EvasionFinding.cs
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace GorillaGuard.Security;

internal sealed class EvasionFinding(string Kind, string Evidence, int ProcessId = 0, string Path = "") : IEquatable<EvasionFinding>
{
		private Type EqualityContract
	{
				get
		{
			return typeof(EvasionFinding);
		}
	}

	public string Kind { get; init; }

	public string Evidence { get; init; }

	public int ProcessId { get; init; }

	public string Path { get; init; }

	public EvasionFinding(string Kind, string Evidence, int ProcessId = 0, string Path = "")
	{
		this.Kind = Kind;
		this.Evidence = Evidence;
		this.ProcessId = ProcessId;
		this.Path = Path;
		base._002Ector();
	}

		public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(_"EvasionFinding");
		stringBuilder.Append(_" { ");
		if (A(stringBuilder))
		{
			stringBuilder.Append(' ');
		}
		stringBuilder.Append('}');
		return stringBuilder.ToString();
	}

		private bool A(StringBuilder P_0)
	{
		RuntimeHelpers.EnsureSufficientExecutionStack();
		P_0.Append(_"Kind = ");
		P_0.Append((object?)Kind);
		P_0.Append(_", Evidence = ");
		P_0.Append((object?)Evidence);
		P_0.Append(_", ProcessId = ");
		P_0.Append(ProcessId.ToString());
		P_0.Append(_", Path = ");
		P_0.Append((object?)Path);
		return true;
	}

		public static bool operator !=(EvasionFinding? left, EvasionFinding? right)
	{
		return !(left == right);
	}

		public static bool operator ==(EvasionFinding? left, EvasionFinding? right)
	{
		if ((object)left != right)
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

		public override int GetHashCode()
	{
		return (((EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Kind)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Evidence)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(ProcessId)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Path);
	}

		public override bool Equals(object? obj)
	{
		return Equals(obj as EvasionFinding);
	}

		public bool Equals(EvasionFinding? other)
	{
		if ((object)this != other)
		{
			if ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<string>.Default.Equals(Kind, other.Kind) && EqualityComparer<string>.Default.Equals(Evidence, other.Evidence) && EqualityComparer<int>.Default.Equals(ProcessId, other.ProcessId))
			{
				return EqualityComparer<string>.Default.Equals(Path, other.Path);
			}
			return false;
		}
		return true;
	}

		private EvasionFinding(EvasionFinding original)
	{
		Kind = original.Kind;
		Evidence = original.Evidence;
		ProcessId = original.ProcessId;
		Path = original.Path;
	}

		public void Deconstruct(out string Kind, out string Evidence, out int ProcessId, out string Path)
	{
		Kind = this.Kind;
		Evidence = this.Evidence;
		ProcessId = this.ProcessId;
		Path = this.Path;
	}
}
#pragma endregion

#pragma region GorillaGuard.Security/EvasionPolicy.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GorillaGuard.Security;

internal static class EvasionPolicy
{
	private static readonly HashSet<string> KnownHashes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		_"0397458E8A32ECA4491C374354621531689F1BEF1E8729D1BB8DE173E830EB95",
		_"50FB76CA9CDABB4ADCDD182164071764BE99B8FD765F2C56B64198E3F51FA1A2",
		_"2A9F08864409279D8492495FD384279AB4C888C3D4453E0F79AF781B7908A67C",
		_"331B76F9C4680A044DA775F44293A19F62F4332235E6E6D62A965BB778E0867F",
		_"1A21CC03424FC82C3DD1346905D16494536B9595AE4162228D99FB7C285C1031"
	};

	private static readonly HashSet<string> KnownModuleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		_"GT_Cheat.dll",
		_"Dll2.dll",
		_"ovr2.dll",
		_"payload.bin",
		_"windows.net.form.driver.dll"
	};

	private static readonly HashSet<string> StrongProbeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		_"GT_Cheat.dll",
		_"Dll2.dll",
		_"ovr2.dll",
		_"windows.net.form.driver.exe",
		_"windows.net.form.driver.dll",
		_"OpenVRMon.exe",
		_"VrPoseHost.exe",
		_"deep-stealth.json",
		_"posehook.cfg",
		_"vrhook.ini",
		_"netwatch.vbs"
	};

	private static readonly string[] CommandMarkers = new string[13]
	{
		_"Pass-GorillaGuardChecks",
		_"ScreenShare-Guard",
		_"Apply-DeepStealth",
		_"Start-CheckerShield",
		_"_Run-Cheat",
		_"_Update-Cheat",
		_"COPY-HOOK",
		_"Update-VrHook-Deep",
		_"HollowConfig.ps1",
		_"VR-CheatCommon.ps1",
		_"netwatch.vbs",
		_"WinNetFormDriverService",
		_"windows.net.form.driver"
	};

	private static readonly string[] CheatPathMarkers = new string[11]
	{
		_"\\Themes\\CachedFiles\\plugin\\",
		_"\\BlurAutoClicker\\",
		_"\\VR-Cheat\\",
		_"\\hook-source\\",
		_"\\WinNetFormDriverService\\",
		_"windows.net.form.driver",
		_"\\GT_Cheat",
		_"deep-stealth.json",
		_"posehook.cfg",
		_"\\Dll2.dll",
		_"\\ovr2.dll"
	};

	private static readonly HashSet<string> HollowCarriers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		_"sihost",
		_"dllhost",
		_"conhost",
		_"RuntimeBroker",
		_"svchost",
		_"taskhostw",
		_"SearchProtocolHost",
		_"backgroundTaskHost",
		_"WmiPrvSE",
		_"fontdrvhost"
	};

	internal static EvasionFinding? A(string? P_0, string? P_1, int P_2 = 0)
	{
		string text = C(P_0);
		string text2 = P_1 ?? _"";
		if (text.Equals(_"windows.net.form.driver", StringComparison.OrdinalIgnoreCase) || text.Equals(_"OpenVRMon", StringComparison.OrdinalIgnoreCase) || text.Equals(_"VrPoseHost", StringComparison.OrdinalIgnoreCase) || text.Equals(_"SharpMonoInjector", StringComparison.OrdinalIgnoreCase) || text.Equals(_"ExtremeInjector", StringComparison.OrdinalIgnoreCase) || text.StartsWith(_"Xenos", StringComparison.OrdinalIgnoreCase))
		{
			return A(_"PROCESS_NAME", text, P_2, text2);
		}
		if (!string.IsNullOrWhiteSpace(text2) && text.Equals(_"ThemeSyncHost", StringComparison.OrdinalIgnoreCase) && !b(text2, Environment.GetFolderPath(Environment.SpecialFolder.System)))
		{
			return A(_"RENAMED_PROCESS", text, P_2, text2);
		}
		if (!string.IsNullOrWhiteSpace(text2) && text.StartsWith(_"EpicWebHelper", StringComparison.OrdinalIgnoreCase) && !B(text2, _"\\Epic Games\\"))
		{
			return A(_"DECOY_PROCESS", text, P_2, text2);
		}
		if (!string.IsNullOrWhiteSpace(text2) && (text.Equals(_"SteamVRCompositor", StringComparison.OrdinalIgnoreCase) || text.Equals(_"vrserver", StringComparison.OrdinalIgnoreCase)) && !B(text2, _"\\SteamVR\\"))
		{
			return A(_"FAKE_VR_PROCESS", text, P_2, text2);
		}
		if (!b(text2))
		{
			return null;
		}
		return A(_"PROCESS_PATH", text2, P_2, text2);
	}

	internal static EvasionFinding? a(string? P_0, string? P_1, int P_2 = 0)
	{
		if (C(P_0).StartsWith(_"GorillaGuard", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(P_1))
		{
			return null;
		}
		string text = CommandMarkers.FirstOrDefault((string P_0) => P_1.Contains(P_0, StringComparison.OrdinalIgnoreCase));
		if (text != null)
		{
			return A(_"COMMAND_LINE", text, P_2, P_1);
		}
		return null;
	}

	internal static EvasionFinding? B(string? P_0, string? P_1, int P_2 = 0)
	{
		string fileName = Path.GetFileName(P_0 ?? _"");
		string text = P_1 ?? _"";
		if (KnownModuleNames.Contains(fileName) || b(text))
		{
			return A(_"LOADED_MODULE", string.IsNullOrWhiteSpace(text) ? fileName : text, P_2, text);
		}
		if ((fileName.Equals(_"version.dll", StringComparison.OrdinalIgnoreCase) || fileName.Equals(_"winmm.dll", StringComparison.OrdinalIgnoreCase) || fileName.Equals(_"dsound.dll", StringComparison.OrdinalIgnoreCase)) && !b(text, Environment.GetFolderPath(Environment.SpecialFolder.Windows)))
		{
			return A(_"PROXY_MODULE", text, P_2, text);
		}
		return null;
	}

	internal static bool A(string? P_0)
	{
		if (P_0 != null)
		{
			return KnownHashes.Contains(P_0);
		}
		return false;
	}

	internal static bool A(string? P_0, string? P_1)
	{
		string text = C(P_0);
		string text2 = P_1 ?? _"";
		if (!text.StartsWith(_"EpicWebHelper", StringComparison.OrdinalIgnoreCase) && !text.StartsWith(_"Xenos", StringComparison.OrdinalIgnoreCase) && !text.Equals(_"OpenVRMon", StringComparison.OrdinalIgnoreCase) && !text.Equals(_"windows.net.form.driver", StringComparison.OrdinalIgnoreCase) && !B(text2, _"\\Downloads\\") && !B(text2, _"\\Desktop\\"))
		{
			return B(text2, _"\\Temp\\");
		}
		return true;
	}

	internal static bool a(string? P_0)
	{
		return HollowCarriers.Contains(C(P_0));
	}

	internal static bool a(string? P_0, string? P_1)
	{
		if (!(A(P_0, P_1, 0) != null))
		{
			return b(P_1);
		}
		return true;
	}

	internal static EvasionFinding? B(string P_0)
	{
		string fileName = Path.GetFileName(P_0);
		if (StrongProbeNames.Contains(fileName) || ((fileName.Equals(_"hollow.txt", StringComparison.OrdinalIgnoreCase) || fileName.Equals(_"service.ps1", StringComparison.OrdinalIgnoreCase) || fileName.Equals(_"Run Cheat.bat", StringComparison.OrdinalIgnoreCase) || fileName.Equals(_"payload.bin", StringComparison.OrdinalIgnoreCase)) && b(P_0)))
		{
			return A(_"KNOWN_FILE", P_0, 0, P_0);
		}
		return null;
	}

	internal static bool b(string? P_0)
	{
		if (!string.IsNullOrWhiteSpace(P_0))
		{
			return CheatPathMarkers.Any((string P_0) => P_0.Contains(P_0, StringComparison.OrdinalIgnoreCase));
		}
		return false;
	}

	private static EvasionFinding A(string P_0, string P_1, int P_2 = 0, string P_3 = "")
	{
		return new EvasionFinding(P_0, P_1, P_2, P_3);
	}

	private static string C(string? value)
	{
		string fileName = Path.GetFileName(value ?? _"");
		if (!fileName.EndsWith(_".exe", StringComparison.OrdinalIgnoreCase))
		{
			return fileName;
		}
		string text = fileName;
		return text.Substring(0, text.Length - 4);
	}

	private static bool B(string P_0, string P_1)
	{
		if (!string.IsNullOrWhiteSpace(P_0))
		{
			return P_0.Contains(P_1, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	private static bool b(string P_0, string P_1)
	{
		if (string.IsNullOrWhiteSpace(P_0) || string.IsNullOrWhiteSpace(P_1))
		{
			return false;
		}
		try
		{
			string value = Path.GetFullPath(P_1).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
			return Path.GetFullPath(P_0).StartsWith(value, StringComparison.OrdinalIgnoreCase);
		}
		catch
		{
			return false;
		}
	}
}
#pragma endregion

#pragma region GorillaGuard.Security/HostQuarantineMove.cs
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace GorillaGuard.Security;

internal sealed class HostQuarantineMove(string Source, string Destination, HostRecoveryKind Kind) : IEquatable<HostQuarantineMove>
{
		private Type EqualityContract
	{
				get
		{
			return typeof(HostQuarantineMove);
		}
	}

	public string Source { get; init; }

	public string Destination { get; init; }

	public HostRecoveryKind Kind { get; init; }

	public HostQuarantineMove(string Source, string Destination, HostRecoveryKind Kind)
	{
		this.Source = Source;
		this.Destination = Destination;
		this.Kind = Kind;
		base._002Ector();
	}

		public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(_"HostQuarantineMove");
		stringBuilder.Append(_" { ");
		if (A(stringBuilder))
		{
			stringBuilder.Append(' ');
		}
		stringBuilder.Append('}');
		return stringBuilder.ToString();
	}

		private bool A(StringBuilder P_0)
	{
		RuntimeHelpers.EnsureSufficientExecutionStack();
		P_0.Append(_"Source = ");
		P_0.Append((object?)Source);
		P_0.Append(_", Destination = ");
		P_0.Append((object?)Destination);
		P_0.Append(_", Kind = ");
		P_0.Append(Kind.ToString());
		return true;
	}

		public static bool operator !=(HostQuarantineMove? left, HostQuarantineMove? right)
	{
		return !(left == right);
	}

		public static bool operator ==(HostQuarantineMove? left, HostQuarantineMove? right)
	{
		if ((object)left != right)
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

		public override int GetHashCode()
	{
		return ((EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Source)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Destination)) * -1521134295 + EqualityComparer<HostRecoveryKind>.Default.GetHashCode(Kind);
	}

		public override bool Equals(object? obj)
	{
		return Equals(obj as HostQuarantineMove);
	}

		public bool Equals(HostQuarantineMove? other)
	{
		if ((object)this != other)
		{
			if ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<string>.Default.Equals(Source, other.Source) && EqualityComparer<string>.Default.Equals(Destination, other.Destination))
			{
				return EqualityComparer<HostRecoveryKind>.Default.Equals(Kind, other.Kind);
			}
			return false;
		}
		return true;
	}

		private HostQuarantineMove(HostQuarantineMove original)
	{
		Source = original.Source;
		Destination = original.Destination;
		Kind = original.Kind;
	}

		public void Deconstruct(out string Source, out string Destination, out HostRecoveryKind Kind)
	{
		Source = this.Source;
		Destination = this.Destination;
		Kind = this.Kind;
	}
}
#pragma endregion

#pragma region GorillaGuard.Security/HostRecovery.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace GorillaGuard.Security;

internal static class HostRecovery
{
	private const string SteamAppId = "1533390";

	internal static HostRecoveryPlan A(string P_0, IReadOnlyDictionary<string, string> P_1)
	{
		string text = Path.GetFullPath(P_0).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		string value;
		HostRecoveryPlan obj = new HostRecoveryPlan
		{
			ExpectedBuildId = (P_1.TryGetValue(_"steamBuildId", out value) ? value.Trim() : _""),
			InstalledBuildId = A(text),
			ManagedInventoryComplete = A(P_1, _"managedFileSha256."),
			NativeInventoryComplete = A(P_1, _"nativeFileSha256.")
		};
		A(obj, P_1, text, _"gameExecutableSha256", _"Gorilla Tag.exe");
		A(obj, P_1, text, _"unityPlayerSha256", _"UnityPlayer.dll");
		A(obj, P_1, text, _"managedFileSha256.", null, Path.Combine(_"Gorilla Tag_Data", _"Managed"));
		A(obj, P_1, text, _"nativeFileSha256.", _"nativeOptionalFileSha256.", Path.Combine(_"Gorilla Tag_Data", _"Plugins"));
		return obj;
	}

	internal static IReadOnlyList<HostQuarantineMove> A(HostRecoveryPlan P_0, string P_1, string P_2, Action<string>? log = null)
	{
		if (!P_0.BuildMatches)
		{
			throw new InvalidOperationException(_"Refusing automatic quarantine because the installed Steam build does not match the signed baseline.");
		}
		if (!P_0.HasCompleteInventory)
		{
			throw new InvalidOperationException(_"Refusing automatic quarantine because the signed release has no complete managed/native file inventory.");
		}
		string text = Path.GetFullPath(P_1).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		string value = text + Path.DirectorySeparatorChar;
		string text2 = Path.GetFullPath(P_2).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		string value2 = Path.GetFullPath(Path.Combine(text, _"BepInEx", _"quarantine")).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
		if (!text2.StartsWith(value2, StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidDataException(_"Host recovery quarantine must remain under the game BepInEx quarantine directory.");
		}
		List<HostQuarantineMove> list = new List<HostQuarantineMove>();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (HostRecoveryItem item in P_0.Items.Where(delegate(HostRecoveryItem P_0)
		{
			HostRecoveryKind kind = P_0.Kind;
			return (uint)(kind - 1) <= 1u;
		}))
		{
			string fullPath = Path.GetFullPath(item.Path);
			if (!fullPath.StartsWith(value, StringComparison.OrdinalIgnoreCase) || !hashSet.Add(fullPath))
			{
				throw new InvalidDataException(_"Host recovery escaped or duplicated the game directory: " + fullPath);
			}
			if (File.Exists(fullPath))
			{
				a(fullPath, text);
				string text3 = Path.GetFullPath(Path.Combine(text2, item.RelativePath));
				string value3 = text2 + Path.DirectorySeparatorChar;
				if (!text3.StartsWith(value3, StringComparison.OrdinalIgnoreCase))
				{
					throw new InvalidDataException(_"Host recovery quarantine path escaped its destination root.");
				}
				Directory.CreateDirectory(Path.GetDirectoryName(text3));
				if (File.Exists(text3))
				{
					text3 = text3 + _"." + Guid.NewGuid().ToString(_"N") + _".blocked";
				}
				File.Move(fullPath, text3);
				list.Add(new HostQuarantineMove(fullPath, text3, item.Kind));
				if (log != null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(84, 3);
					defaultInterpolatedStringHandler.AppendLiteral(_"BLOCKED [HOST CODE] kind=");
					defaultInterpolatedStringHandler.AppendFormatted(item.Kind);
					defaultInterpolatedStringHandler.AppendLiteral(_"; source=");
					defaultInterpolatedStringHandler.AppendFormatted(fullPath);
					defaultInterpolatedStringHandler.AppendLiteral(_"; action=quarantine for Steam repair; destination=");
					defaultInterpolatedStringHandler.AppendFormatted(text3);
					log(defaultInterpolatedStringHandler.ToStringAndClear());
				}
			}
		}
		return list;
	}

	internal static string A(string P_0)
	{
		try
		{
			string fullPath = Path.GetFullPath(Path.Combine(Path.GetFullPath(P_0), _"..", _"..", _"appmanifest_1533390.acf"));
			if (!File.Exists(fullPath))
			{
				return _"";
			}
			Match match = Regex.Match(File.ReadAllText(fullPath), _"\"buildid\"\\s*\"([0-9]+)\"", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
			return match.Success ? match.Groups[1].Value : _"";
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException || ex is NotSupportedException) ? 1 : 0) != 0)
		{
			return _"";
		}
	}

	private static bool A(IReadOnlyDictionary<string, string> P_0, string P_1)
	{
		return P_0.Any<KeyValuePair<string, string>>((KeyValuePair<string, string> P_0) => P_0.Key.StartsWith(P_1, StringComparison.OrdinalIgnoreCase));
	}

	private static void A(HostRecoveryPlan P_0, IReadOnlyDictionary<string, string> P_1, string P_2, string P_3, string P_4)
	{
		if (!P_1.TryGetValue(P_3, out string value) || !B(value))
		{
			throw new InvalidDataException(_"Signed host recovery inventory is missing " + P_3 + _".");
		}
		A(P_0, P_2, P_4, value);
	}

	private static void A(HostRecoveryPlan P_0, IReadOnlyDictionary<string, string> P_1, string P_2, string P_3, string? optionalPrefix, string P_5)
	{
		string text = A(P_2, P_5);
		string value = text.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (KeyValuePair<string, string> item in P_1.Where<KeyValuePair<string, string>>((KeyValuePair<string, string> P_0) => P_0.Key.StartsWith(P_3, StringComparison.OrdinalIgnoreCase)).OrderBy<KeyValuePair<string, string>, string>((KeyValuePair<string, string> P_0) => P_0.Key, StringComparer.OrdinalIgnoreCase))
		{
			string text2 = item.Key.Substring(P_3.Length).Replace('/', Path.DirectorySeparatorChar);
			if (!B(item.Value))
			{
				throw new InvalidDataException(_"Invalid signed host recovery hash: " + item.Key);
			}
			string text3 = A(text, text2);
			if (!text3.StartsWith(value, StringComparison.OrdinalIgnoreCase) || !b(text3) || !hashSet.Add(text3))
			{
				throw new InvalidDataException(_"Invalid signed host recovery inventory entry: " + item.Key);
			}
			A(P_0, P_2, Path.GetRelativePath(P_2, text3), item.Value);
		}
		if (optionalPrefix != null)
		{
			foreach (KeyValuePair<string, string> item2 in P_1.Where<KeyValuePair<string, string>>((KeyValuePair<string, string> P_0) => P_0.Key.StartsWith(optionalPrefix, StringComparison.OrdinalIgnoreCase)).OrderBy<KeyValuePair<string, string>, string>((KeyValuePair<string, string> P_0) => P_0.Key, StringComparer.OrdinalIgnoreCase))
			{
				string text4 = item2.Key.Substring(optionalPrefix.Length).Replace('/', Path.DirectorySeparatorChar);
				if (!B(item2.Value))
				{
					throw new InvalidDataException(_"Invalid signed optional host recovery hash: " + item2.Key);
				}
				string text5 = A(text, text4);
				if (!text5.StartsWith(value, StringComparison.OrdinalIgnoreCase) || !b(text5) || !hashSet.Add(text5))
				{
					throw new InvalidDataException(_"Invalid signed optional host recovery inventory entry: " + item2.Key);
				}
				if (File.Exists(text5))
				{
					A(P_0, P_2, Path.GetRelativePath(P_2, text5), item2.Value);
				}
			}
		}
		if (hashSet.Count == 0 || !Directory.Exists(text))
		{
			return;
		}
		foreach (string item3 in Directory.EnumerateFiles(text, _"*", SearchOption.AllDirectories).Where(b))
		{
			string fullPath = Path.GetFullPath(item3);
			if (!hashSet.Contains(fullPath))
			{
				P_0.Items.Add(new HostRecoveryItem(fullPath, a(Path.GetRelativePath(P_2, fullPath)), HostRecoveryKind.Extra, null, ReleaseVerifier.a(fullPath)));
			}
		}
	}

	private static void A(HostRecoveryPlan P_0, string P_1, string P_2, string P_3)
	{
		string text = A(P_1, P_2);
		string relativePath = a(Path.GetRelativePath(P_1, text));
		if (!File.Exists(text))
		{
			P_0.Items.Add(new HostRecoveryItem(text, relativePath, HostRecoveryKind.Missing, P_3, null));
			return;
		}
		string text2 = ReleaseVerifier.a(text);
		if (!text2.Equals(P_3, StringComparison.OrdinalIgnoreCase))
		{
			P_0.Items.Add(new HostRecoveryItem(text, relativePath, HostRecoveryKind.Changed, P_3, text2));
		}
	}

	private static string A(string P_0, string P_1)
	{
		string text = Path.GetFullPath(P_0).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		string fullPath = Path.GetFullPath(Path.Combine(text, P_1));
		if (!fullPath.StartsWith(text + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidDataException(_"Signed host recovery path escaped its root: " + P_1);
		}
		return fullPath;
	}

	private static void a(string P_0, string P_1)
	{
		string text = Path.GetFullPath(P_0);
		string value = Path.GetFullPath(P_1).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		while (true)
		{
			if ((File.GetAttributes(text) & FileAttributes.ReparsePoint) != 0)
			{
				throw new InvalidDataException(_"Refusing host recovery through a redirected/reparse-point path: " + text);
			}
			if (text.Equals(value, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			string? directoryName = Path.GetDirectoryName(text);
			if (directoryName == null)
			{
				break;
			}
			text = directoryName;
		}
		throw new InvalidDataException(_"Host recovery path escaped its root.");
	}

	private static string a(string P_0)
	{
		return P_0.Replace('\\', '/');
	}

	private static bool B(string? P_0)
	{
		if (P_0 != null && P_0.Length == 64)
		{
			return P_0.All(Uri.IsHexDigit);
		}
		return false;
	}

	private static bool b(string P_0)
	{
		string extension = Path.GetExtension(P_0);
		if (!extension.Equals(_".dll", StringComparison.OrdinalIgnoreCase) && !extension.Equals(_".exe", StringComparison.OrdinalIgnoreCase) && !extension.Equals(_".winmd", StringComparison.OrdinalIgnoreCase))
		{
			return extension.Equals(_".so", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}
}
#pragma endregion

#pragma region GorillaGuard.Security/HostRecoveryItem.cs
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace GorillaGuard.Security;

internal sealed class HostRecoveryItem(string Path, string RelativePath, HostRecoveryKind Kind, string? ExpectedSha256, string? ActualSha256) : IEquatable<HostRecoveryItem>
{
		private Type EqualityContract
	{
				get
		{
			return typeof(HostRecoveryItem);
		}
	}

	public string Path { get; init; }

	public string RelativePath { get; init; }

	public HostRecoveryKind Kind { get; init; }

	public string? ExpectedSha256 { get; init; }

	public string? ActualSha256 { get; init; }

	public HostRecoveryItem(string Path, string RelativePath, HostRecoveryKind Kind, string? ExpectedSha256, string? ActualSha256)
	{
		this.Path = Path;
		this.RelativePath = RelativePath;
		this.Kind = Kind;
		this.ExpectedSha256 = ExpectedSha256;
		this.ActualSha256 = ActualSha256;
		base._002Ector();
	}

		public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(_"HostRecoveryItem");
		stringBuilder.Append(_" { ");
		if (A(stringBuilder))
		{
			stringBuilder.Append(' ');
		}
		stringBuilder.Append('}');
		return stringBuilder.ToString();
	}

		private bool A(StringBuilder P_0)
	{
		RuntimeHelpers.EnsureSufficientExecutionStack();
		P_0.Append(_"Path = ");
		P_0.Append((object?)Path);
		P_0.Append(_", RelativePath = ");
		P_0.Append((object?)RelativePath);
		P_0.Append(_", Kind = ");
		P_0.Append(Kind.ToString());
		P_0.Append(_", ExpectedSha256 = ");
		P_0.Append((object?)ExpectedSha256);
		P_0.Append(_", ActualSha256 = ");
		P_0.Append((object?)ActualSha256);
		return true;
	}

		public static bool operator !=(HostRecoveryItem? left, HostRecoveryItem? right)
	{
		return !(left == right);
	}

		public static bool operator ==(HostRecoveryItem? left, HostRecoveryItem? right)
	{
		if ((object)left != right)
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

		public override int GetHashCode()
	{
		return ((((EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Path)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(RelativePath)) * -1521134295 + EqualityComparer<HostRecoveryKind>.Default.GetHashCode(Kind)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ExpectedSha256)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ActualSha256);
	}

		public override bool Equals(object? obj)
	{
		return Equals(obj as HostRecoveryItem);
	}

		public bool Equals(HostRecoveryItem? other)
	{
		if ((object)this != other)
		{
			if ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<string>.Default.Equals(Path, other.Path) && EqualityComparer<string>.Default.Equals(RelativePath, other.RelativePath) && EqualityComparer<HostRecoveryKind>.Default.Equals(Kind, other.Kind) && EqualityComparer<string>.Default.Equals(ExpectedSha256, other.ExpectedSha256))
			{
				return EqualityComparer<string>.Default.Equals(ActualSha256, other.ActualSha256);
			}
			return false;
		}
		return true;
	}

		private HostRecoveryItem(HostRecoveryItem original)
	{
		Path = original.Path;
		RelativePath = original.RelativePath;
		Kind = original.Kind;
		ExpectedSha256 = original.ExpectedSha256;
		ActualSha256 = original.ActualSha256;
	}

		public void Deconstruct(out string Path, out string RelativePath, out HostRecoveryKind Kind, out string? ExpectedSha256, out string? ActualSha256)
	{
		Path = this.Path;
		RelativePath = this.RelativePath;
		Kind = this.Kind;
		ExpectedSha256 = this.ExpectedSha256;
		ActualSha256 = this.ActualSha256;
	}
}
#pragma endregion

#pragma region GorillaGuard.Security/HostRecoveryKind.cs
namespace GorillaGuard.Security;

internal enum HostRecoveryKind
{
	Missing,
	Changed,
	Extra
}
#pragma endregion

#pragma region GorillaGuard.Security/HostRecoveryPlan.cs
using System;
using System.Collections.Generic;

namespace GorillaGuard.Security;

internal sealed class HostRecoveryPlan
{
	internal string ExpectedBuildId { get; init; } = _"";

	internal string InstalledBuildId { get; init; } = _"";

	internal bool ManagedInventoryComplete { get; init; }

	internal bool NativeInventoryComplete { get; init; }

	internal List<HostRecoveryItem> Items { get; } = new List<HostRecoveryItem>();

	internal bool HasCompleteInventory
	{
		get
		{
			if (ManagedInventoryComplete)
			{
				return NativeInventoryComplete;
			}
			return false;
		}
	}

	internal bool BuildMatches
	{
		get
		{
			if (ExpectedBuildId.Length > 0)
			{
				return ExpectedBuildId.Equals(InstalledBuildId, StringComparison.Ordinal);
			}
			return false;
		}
	}

	internal bool CanQuarantine
	{
		get
		{
			if (HasCompleteInventory)
			{
				return BuildMatches;
			}
			return false;
		}
	}
}
#pragma endregion

#pragma region GorillaGuard.Security/InjectionPolicy.cs
using System;
using System.Collections.Generic;

namespace GorillaGuard.Security;

internal static class InjectionPolicy
{
	internal const string AbsenseSha256 = "DA9816B556B81B2444C12D42E67D5C25FB51F06E6BF884E553733770438775B3";

	internal const long AbsenseLength = 9175040L;

	internal static bool A(IEnumerable<string> P_0, IEnumerable<string> P_1)
	{
		HashSet<string> hashSet = new HashSet<string>(P_0, StringComparer.Ordinal);
		HashSet<string> hashSet2 = new HashSet<string>(P_1, StringComparer.Ordinal);
		if (hashSet.Contains(_"Absense.Injector") && hashSet.Contains(_"Absense.Memory") && hashSet.Contains(_"Absense.Native") && hashSet2.Contains(_"WriteProcessMemory") && hashSet2.Contains(_"VirtualAllocEx"))
		{
			return hashSet2.Contains(_"CreateRemoteThread");
		}
		return false;
	}

	internal static bool A(bool P_0, bool P_1, bool P_2)
	{
		if (P_0 && P_1)
		{
			return !P_2;
		}
		return false;
	}
}
#pragma endregion

#pragma region GorillaGuard.Security/KernelHealthProtocol.cs
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;

namespace GorillaGuard.Security;

internal static class KernelHealthProtocol
{
	internal const uint Version = 1u;

	internal const int QuerySize = 16;

	internal const int StatusSize = 96;

	internal const uint QueryCode = 2252800u;

	internal const uint DevelopmentFlag = 2147483648u;

	internal static byte[] A(ulong P_0)
	{
		byte[] array = new byte[16];
		BinaryPrimitives.WriteUInt32LittleEndian(array, 16u);
		BinaryPrimitives.WriteUInt32LittleEndian(array.AsSpan(4), 1u);
		BinaryPrimitives.WriteUInt64LittleEndian(array.AsSpan(8), P_0);
		return array;
	}

	internal static KernelHealthSnapshot A(ReadOnlySpan<byte> bytes, ulong P_1, KernelIdentity P_2, IReadOnlyList<KernelIdentity?> targets, bool P_4 = false)
	{
		if (bytes.Length != 96 || BinaryPrimitives.ReadUInt32LittleEndian(bytes) != 96 || BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(4)) != 1)
		{
			throw new InvalidDataException(_"Kernel status size or version mismatch.");
		}
		uint num = BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(8));
		if ((num & 3) != 3 || (num & 0x7FFFFFFC) != 0 || BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(12)) != 0)
		{
			throw new InvalidDataException(_"Kernel protection callbacks or launcher pin are not verified.");
		}
		bool flag = (num & 0x80000000u) != 0;
		if (flag && !P_4)
		{
			throw new InvalidDataException(_"Development kernel drivers cannot authorize a production session.");
		}
		if (P_1 == 0L || BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(16)) != P_1)
		{
			throw new InvalidDataException(_"Kernel status challenge mismatch.");
		}
		A(bytes.Slice(24), P_2);
		if (targets.Count != 3 || targets[0] == null)
		{
			throw new InvalidDataException(_"Expected game and VR process identities are required.");
		}
		for (int i = 0; i < targets.Count; i++)
		{
			ReadOnlySpan<byte> readOnlySpan = bytes.Slice(40 + i * 16, 16);
			KernelIdentity kernelIdentity = targets[i];
			if ((object)kernelIdentity != null)
			{
				A(readOnlySpan, kernelIdentity);
			}
			else if (BinaryPrimitives.ReadUInt64LittleEndian(readOnlySpan) != 0L || BinaryPrimitives.ReadUInt64LittleEndian(readOnlySpan.Slice(8)) != 0L)
			{
				throw new InvalidDataException(_"Kernel driver reported an unexpected protected process.");
			}
		}
		return new KernelHealthSnapshot(flag, BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(88)));
	}

	private static void A(ReadOnlySpan<byte> P_0, KernelIdentity expected)
	{
		if (expected.Pid == 0L || expected.StartedFileTime == 0L || BinaryPrimitives.ReadUInt64LittleEndian(P_0) != expected.Pid || BinaryPrimitives.ReadUInt64LittleEndian(P_0.Slice(8)) != expected.StartedFileTime)
		{
			throw new InvalidDataException(_"Kernel process identity or creation time mismatch.");
		}
	}
}
#pragma endregion

#pragma region GorillaGuard.Security/KernelHealthSnapshot.cs
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace GorillaGuard.Security;

internal sealed class KernelHealthSnapshot(bool DevelopmentOnly, ulong FilteredOperations) : IEquatable<KernelHealthSnapshot>
{
		private Type EqualityContract
	{
				get
		{
			return typeof(KernelHealthSnapshot);
		}
	}

	public bool DevelopmentOnly { get; init; }

	public ulong FilteredOperations { get; init; }

	public KernelHealthSnapshot(bool DevelopmentOnly, ulong FilteredOperations)
	{
		this.DevelopmentOnly = DevelopmentOnly;
		this.FilteredOperations = FilteredOperations;
		base._002Ector();
	}

		public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(_"KernelHealthSnapshot");
		stringBuilder.Append(_" { ");
		if (A(stringBuilder))
		{
			stringBuilder.Append(' ');
		}
		stringBuilder.Append('}');
		return stringBuilder.ToString();
	}

		private bool A(StringBuilder P_0)
	{
		RuntimeHelpers.EnsureSufficientExecutionStack();
		P_0.Append(_"DevelopmentOnly = ");
		P_0.Append(DevelopmentOnly.ToString());
		P_0.Append(_", FilteredOperations = ");
		P_0.Append(FilteredOperations.ToString());
		return true;
	}

		public static bool operator !=(KernelHealthSnapshot? left, KernelHealthSnapshot? right)
	{
		return !(left == right);
	}

		public static bool operator ==(KernelHealthSnapshot? left, KernelHealthSnapshot? right)
	{
		if ((object)left != right)
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

		public override int GetHashCode()
	{
		return (EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(DevelopmentOnly)) * -1521134295 + EqualityComparer<ulong>.Default.GetHashCode(FilteredOperations);
	}

		public override bool Equals(object? obj)
	{
		return Equals(obj as KernelHealthSnapshot);
	}

		public bool Equals(KernelHealthSnapshot? other)
	{
		if ((object)this != other)
		{
			if ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<bool>.Default.Equals(DevelopmentOnly, other.DevelopmentOnly))
			{
				return EqualityComparer<ulong>.Default.Equals(FilteredOperations, other.FilteredOperations);
			}
			return false;
		}
		return true;
	}

		private KernelHealthSnapshot(KernelHealthSnapshot original)
	{
		DevelopmentOnly = original.DevelopmentOnly;
		FilteredOperations = original.FilteredOperations;
	}

		public void Deconstruct(out bool DevelopmentOnly, out ulong FilteredOperations)
	{
		DevelopmentOnly = this.DevelopmentOnly;
		FilteredOperations = this.FilteredOperations;
	}
}
#pragma endregion

#pragma region GorillaGuard.Security/KernelIdentity.cs
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace GorillaGuard.Security;

internal sealed class KernelIdentity(ulong Pid, ulong StartedFileTime) : IEquatable<KernelIdentity>
{
		private Type EqualityContract
	{
				get
		{
			return typeof(KernelIdentity);
		}
	}

	public ulong Pid { get; init; }

	public ulong StartedFileTime { get; init; }

	public KernelIdentity(ulong Pid, ulong StartedFileTime)
	{
		this.Pid = Pid;
		this.StartedFileTime = StartedFileTime;
		base._002Ector();
	}

		public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(_"KernelIdentity");
		stringBuilder.Append(_" { ");
		if (A(stringBuilder))
		{
			stringBuilder.Append(' ');
		}
		stringBuilder.Append('}');
		return stringBuilder.ToString();
	}

		private bool A(StringBuilder P_0)
	{
		RuntimeHelpers.EnsureSufficientExecutionStack();
		P_0.Append(_"Pid = ");
		P_0.Append(Pid.ToString());
		P_0.Append(_", StartedFileTime = ");
		P_0.Append(StartedFileTime.ToString());
		return true;
	}

		public static bool operator !=(KernelIdentity? left, KernelIdentity? right)
	{
		return !(left == right);
	}

		public static bool operator ==(KernelIdentity? left, KernelIdentity? right)
	{
		if ((object)left != right)
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

		public override int GetHashCode()
	{
		return (EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<ulong>.Default.GetHashCode(Pid)) * -1521134295 + EqualityComparer<ulong>.Default.GetHashCode(StartedFileTime);
	}

		public override bool Equals(object? obj)
	{
		return Equals(obj as KernelIdentity);
	}

		public bool Equals(KernelIdentity? other)
	{
		if ((object)this != other)
		{
			if ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<ulong>.Default.Equals(Pid, other.Pid))
			{
				return EqualityComparer<ulong>.Default.Equals(StartedFileTime, other.StartedFileTime);
			}
			return false;
		}
		return true;
	}

		private KernelIdentity(KernelIdentity original)
	{
		Pid = original.Pid;
		StartedFileTime = original.StartedFileTime;
	}

		public void Deconstruct(out ulong Pid, out ulong StartedFileTime)
	{
		Pid = this.Pid;
		StartedFileTime = this.StartedFileTime;
	}
}
#pragma endregion

#pragma region GorillaGuard.Security/PipeProtocol.cs
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GorillaGuard.Security;

internal static class PipeProtocol
{
	internal static async Task<string?> A(StreamReader P_0, int P_1, TimeSpan P_2, CancellationToken P_3)
	{
		using CancellationTokenSource deadline = CancellationTokenSource.CreateLinkedTokenSource(P_3);
		deadline.CancelAfter(P_2);
		StringBuilder line = new StringBuilder(Math.Min(P_1, 256));
		char[] character = new char[1];
		try
		{
			while (await P_0.ReadAsync(character.AsMemory(), deadline.Token) != 0)
			{
				if (character[0] == '\n')
				{
					return (line.Length > 0 && line[line.Length - 1] == '\r') ? line.ToString(0, line.Length - 1) : line.ToString();
				}
				if (line.Length >= P_1)
				{
					throw new InvalidDataException(_"Pipe message exceeds the protocol size limit.");
				}
				line.Append(character[0]);
			}
			return (line.Length == 0) ? null : line.ToString();
		}
		catch (OperationCanceledException) when (!P_3.IsCancellationRequested)
		{
			throw new TimeoutException(_"The pipe client did not finish its message before the deadline.");
		}
	}
}
#pragma endregion

#pragma region GorillaGuard.Security/PrelaunchGuard.cs
using System;
using System.Collections.Generic;
using System.IO;

namespace GorillaGuard.Security;

internal static class PrelaunchGuard
{
	internal static void A(string P_0, string P_1)
	{
		string fullPath = Path.GetFullPath(P_0);
		string text = Path.Combine(fullPath, P_1);
		if (!File.Exists(text))
		{
			throw new InvalidDataException(_"Required signed code file is missing: " + text);
		}
		Stack<string> stack = new Stack<string>();
		stack.Push(fullPath);
		while (stack.Count > 0)
		{
			string text2 = stack.Pop();
			if ((File.GetAttributes(text2) & FileAttributes.ReparsePoint) != 0)
			{
				throw new InvalidDataException(_"Redirected pre-launch code directory: " + text2);
			}
			string[] directories = Directory.GetDirectories(text2);
			foreach (string item in directories)
			{
				stack.Push(item);
			}
			directories = Directory.GetFiles(text2);
			foreach (string text3 in directories)
			{
				string extension = Path.GetExtension(text3);
				if ((extension.Equals(_".dll", StringComparison.OrdinalIgnoreCase) || extension.Equals(_".exe", StringComparison.OrdinalIgnoreCase) || extension.Equals(_".winmd", StringComparison.OrdinalIgnoreCase) || extension.Equals(_".so", StringComparison.OrdinalIgnoreCase)) && ((File.GetAttributes(text3) & FileAttributes.ReparsePoint) != 0 || !string.Equals(Path.GetFullPath(text3), text, StringComparison.OrdinalIgnoreCase)))
				{
					throw new InvalidDataException(_"Unapproved code remained before launch: " + text3);
				}
			}
		}
	}
}
#pragma endregion

#pragma region GorillaGuard.Security/RecoveryPolicy.cs
using System;

namespace GorillaGuard.Security;

internal static class RecoveryPolicy
{
	internal const string FallbackVersion = "1.12.8";

	internal static string A(bool P_0)
	{
		if (!P_0)
		{
			return _"https://github.com/DevyDevelopment/GorillaGuard-Releases/releases/latest/download/GorillaGuard.update.json";
		}
		return _"https://github.com/DevyDevelopment/GorillaGuard-Releases/releases/download/v1.12.8/GorillaGuard.update.json";
	}

	internal static bool A(Version P_0, Version P_1, bool P_2, bool P_3)
	{
		if (P_0 == null || P_1 == null)
		{
			return false;
		}
		if (P_3)
		{
			return P_0 == new Version(_"1.12.8");
		}
		if (!P_2)
		{
			return P_0 > P_1;
		}
		return P_0 >= P_1;
	}
}
#pragma endregion

#pragma region GorillaGuard.Security/ReleaseFailureKind.cs
namespace GorillaGuard.Security;

internal enum ReleaseFailureKind
{
	None,
	MissingManifest,
	MissingSignature,
	InvalidSignature,
	InvalidManifest,
	VersionMismatch,
	MissingRuntime,
	RuntimeHashMismatch,
	MissingPreloader,
	PreloaderHashMismatch,
	MissingLauncher,
	LauncherHashMismatch,
	HostIntegrityMismatch,
	VerificationError
}
#pragma endregion

#pragma region GorillaGuard.Security/ReleaseVerificationResult.cs
namespace GorillaGuard.Security;

internal sealed class ReleaseVerificationResult
{
	public bool Success { get; set; }

	public ReleaseFailureKind FailureKind { get; set; }

	public string Message { get; set; }

	public string Version { get; set; }

	public string ExpectedRuntimeHash { get; set; }

	public string ActualRuntimeHash { get; set; }
}
#pragma endregion

#pragma region GorillaGuard.Security/ReleaseVerifier.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace GorillaGuard.Security;

internal static class ReleaseVerifier
{
	private sealed class HostCheck
	{
		internal string ManifestKey { get; }

		internal string Path { get; }

		internal bool IsTree { get; }

		internal string Label { get; }

		internal HostCheck(string manifestKey, string path, bool isTree, string label)
		{
			ManifestKey = manifestKey;
			Path = path;
			IsTree = isTree;
			Label = label;
		}
	}

	internal const string ExpectedVersion = "1.12.31";

	internal const string ManifestFileName = "GorillaGuard.manifest";

	internal const string SignatureFileName = "GorillaGuard.manifest.sig";

	internal const string RuntimeFileName = "GorillaGuard.dll";

	internal const string PreloaderFileName = "GorillaGuard.Preloader.dll";

	internal const string LauncherFileName = "GorillaGuard.Launcher.exe";

	internal const string BootstrapFileName = "winhttp.dll";

	internal const string DoorstopConfigFileName = "doorstop_config.ini";

	private const string PublicModulusBase64 = "2Lm8yNQ1sEC0uXbgt2O4vOqpDIVhaKSGM3Zkhj4pyxJX8slkMyECsMrSzEDhbYFrkYwj9tHTS61g8zuHgKV7GpoKNpRkz28kkJAD4ChJhMucPGnDfle1gclUL99OU357PuaZqFWoBlmXstYzQqmVPajCnwQKQSNCsOAkAEEza5ba9uIZD153eimFDqKpGAxUduST7eRgaBNZWtcdw6sTdn13/Ft/mh/gCzjGm0OFurBw4J1vq2LnHZ0B58oAivsCpTkZzCtyYScGI4afGTxP+3EddgRrjKGE82MnQrocKbVRecy5ReIsdwgq5bgb/26nS+V87FD4KH7ab8cKBSdePbLy28W2GELxQnAI+6kZAeCSpo0lmws83gZKySDjrEglJ4R1/Z4EDT28OqN+N4ntSqnTF8vAlBrtTs8uiAd1gGi2Fa2z0f7D5Y2AIerlmQMp3Jg1GgGKAoouswmDNR1Xh2MS/rOo6UklzkFsEsh/d6ZCsh6Y0UGn/SLqk/eevM7B";

	private const string PublicExponentBase64 = "AQAB";

	internal static ReleaseVerificationResult Verify(string bepInExRoot, bool verifyLauncher = true)
	{
		try
		{
			string text = Path.Combine(bepInExRoot, _"GorillaGuard.manifest");
			string text2 = Path.Combine(bepInExRoot, _"GorillaGuard.manifest.sig");
			string text3 = Path.Combine(bepInExRoot, _"plugins", _"GorillaGuard.dll");
			string text4 = Path.Combine(bepInExRoot, _"patchers", _"GorillaGuard.Preloader.dll");
			string fullName = Directory.GetParent(Path.GetFullPath(bepInExRoot)).FullName;
			string text5 = Path.Combine(fullName, _"GorillaGuard.Launcher.exe");
			string text6 = Path.Combine(fullName, _"winhttp.dll");
			string text7 = Path.Combine(fullName, _"doorstop_config.ini");
			string text8 = Path.Combine(bepInExRoot, _"core");
			if (!File.Exists(text))
			{
				return A(ReleaseFailureKind.MissingManifest, _"Missing signed release manifest: " + text);
			}
			if (!File.Exists(text2))
			{
				return A(ReleaseFailureKind.MissingSignature, _"Missing release signature: " + text2);
			}
			byte[] array = File.ReadAllBytes(text);
			byte[] array2 = File.ReadAllBytes(text2);
			if (!A(array, array2))
			{
				return A(ReleaseFailureKind.InvalidSignature, _"The release manifest signature is invalid.");
			}
			Dictionary<string, string> dictionary = a(array);
			if (!dictionary.TryGetValue(_"version", out var value) || !dictionary.TryGetValue(_"runtimeSha256", out var value2) || !dictionary.TryGetValue(_"preloaderSha256", out var value3) || !dictionary.TryGetValue(_"launcherSha256", out var value4) || !dictionary.TryGetValue(_"bootstrapSha256", out var value5) || !dictionary.TryGetValue(_"doorstopConfigSha256", out var value6) || !dictionary.TryGetValue(_"bepInExCoreTreeSha256", out var value7))
			{
				return A(ReleaseFailureKind.InvalidManifest, _"The signed release manifest is missing required fields.");
			}
			if (!value.Equals(_"1.12.31", StringComparison.Ordinal))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(57, 2);
				defaultInterpolatedStringHandler.AppendLiteral(_"Signed release version ");
				defaultInterpolatedStringHandler.AppendFormatted(value);
				defaultInterpolatedStringHandler.AppendLiteral(_" does not match required version ");
				defaultInterpolatedStringHandler.AppendFormatted(_"1.12.31");
				defaultInterpolatedStringHandler.AppendLiteral(_".");
				return A(ReleaseFailureKind.VersionMismatch, defaultInterpolatedStringHandler.ToStringAndClear());
			}
			if (!C(value2) || !C(value3) || !C(value4) || !C(value5) || !C(value6) || !C(value7))
			{
				return A(ReleaseFailureKind.InvalidManifest, _"The signed release manifest contains an invalid SHA-256 value.");
			}
			if (dictionary.TryGetValue(_"gameNativeTreeSha256Alternate", out var value8) && !C(value8))
			{
				return A(ReleaseFailureKind.InvalidManifest, _"The signed release manifest contains an invalid alternate native-code SHA-256 value.");
			}
			if (!File.Exists(text3))
			{
				return A(ReleaseFailureKind.MissingRuntime, _"Missing required runtime: " + text3, value, value2);
			}
			string text9 = a(text3);
			if (!text9.Equals(value2, StringComparison.OrdinalIgnoreCase))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(48, 3);
				defaultInterpolatedStringHandler2.AppendLiteral(_"Runtime hash mismatch: path=");
				defaultInterpolatedStringHandler2.AppendFormatted(text3);
				defaultInterpolatedStringHandler2.AppendLiteral(_"; expected=");
				defaultInterpolatedStringHandler2.AppendFormatted(value2);
				defaultInterpolatedStringHandler2.AppendLiteral(_"; actual=");
				defaultInterpolatedStringHandler2.AppendFormatted(text9);
				return A(ReleaseFailureKind.RuntimeHashMismatch, defaultInterpolatedStringHandler2.ToStringAndClear(), value, value2, text9);
			}
			if (!File.Exists(text4))
			{
				return A(ReleaseFailureKind.MissingPreloader, _"Missing required preloader: " + text4);
			}
			string text10 = a(text4);
			if (!text10.Equals(value3, StringComparison.OrdinalIgnoreCase))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(50, 3);
				defaultInterpolatedStringHandler3.AppendLiteral(_"Preloader hash mismatch: path=");
				defaultInterpolatedStringHandler3.AppendFormatted(text4);
				defaultInterpolatedStringHandler3.AppendLiteral(_"; expected=");
				defaultInterpolatedStringHandler3.AppendFormatted(value3);
				defaultInterpolatedStringHandler3.AppendLiteral(_"; actual=");
				defaultInterpolatedStringHandler3.AppendFormatted(text10);
				return A(ReleaseFailureKind.PreloaderHashMismatch, defaultInterpolatedStringHandler3.ToStringAndClear());
			}
			if (verifyLauncher)
			{
				if (!File.Exists(text5))
				{
					return A(ReleaseFailureKind.MissingLauncher, _"Missing required launcher: " + text5);
				}
				string text11 = a(text5);
				if (!text11.Equals(value4, StringComparison.OrdinalIgnoreCase))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(49, 3);
					defaultInterpolatedStringHandler4.AppendLiteral(_"Launcher hash mismatch: path=");
					defaultInterpolatedStringHandler4.AppendFormatted(text5);
					defaultInterpolatedStringHandler4.AppendLiteral(_"; expected=");
					defaultInterpolatedStringHandler4.AppendFormatted(value4);
					defaultInterpolatedStringHandler4.AppendLiteral(_"; actual=");
					defaultInterpolatedStringHandler4.AppendFormatted(text11);
					return A(ReleaseFailureKind.LauncherHashMismatch, defaultInterpolatedStringHandler4.ToStringAndClear());
				}
			}
			ReleaseVerificationResult releaseVerificationResult = A(text6, value5, _"Doorstop bootstrap");
			if (!releaseVerificationResult.Success)
			{
				return releaseVerificationResult;
			}
			ReleaseVerificationResult releaseVerificationResult2 = A(text7, value6, _"Doorstop configuration");
			if (!releaseVerificationResult2.Success)
			{
				return releaseVerificationResult2;
			}
			if (dictionary.TryGetValue(_"openvrApiSha256", out var value9))
			{
				if (!C(value9))
				{
					return A(ReleaseFailureKind.InvalidManifest, _"Invalid signed OpenVR API repair hash.");
				}
				ReleaseVerificationResult releaseVerificationResult3 = A(Path.Combine(bepInExRoot, _"repair", _"openvr_api.dll"), value9, _"OpenVR API repair payload");
				if (!releaseVerificationResult3.Success)
				{
					return releaseVerificationResult3;
				}
			}
			if (!Directory.Exists(text8))
			{
				return A(ReleaseFailureKind.HostIntegrityMismatch, _"Missing signed BepInEx core: " + text8);
			}
			string text12 = B(text8);
			ReleaseVerificationResult releaseVerificationResult4 = a(dictionary, text8);
			if (!releaseVerificationResult4.Success)
			{
				return releaseVerificationResult4;
			}
			if (!text12.Equals(value7, StringComparison.OrdinalIgnoreCase))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(60, 3);
				defaultInterpolatedStringHandler5.AppendLiteral(_"Signed BepInEx core hash mismatch: expected=");
				defaultInterpolatedStringHandler5.AppendFormatted(value7);
				defaultInterpolatedStringHandler5.AppendLiteral(_"; actual=");
				defaultInterpolatedStringHandler5.AppendFormatted(text12);
				defaultInterpolatedStringHandler5.AppendLiteral(_"; path=");
				defaultInterpolatedStringHandler5.AppendFormatted(text8);
				return A(ReleaseFailureKind.HostIntegrityMismatch, defaultInterpolatedStringHandler5.ToStringAndClear());
			}
			return new ReleaseVerificationResult
			{
				Success = true,
				FailureKind = ReleaseFailureKind.None,
				Message = _"Signed Gorilla Guard release " + value + _" verified.",
				Version = value,
				ExpectedRuntimeHash = value2,
				ActualRuntimeHash = text9
			};
		}
		catch (Exception ex)
		{
			return A(ReleaseFailureKind.VerificationError, _"Release verification error: " + ex.Message);
		}
	}

	internal static bool A(byte[] P_0, byte[] P_1)
	{
		if (P_0 == null || P_0.Length == 0 || P_1 == null || P_1.Length == 0)
		{
			return false;
		}
		using RSA rSA = RSA.Create();
		rSA.ImportParameters(new RSAParameters
		{
			Modulus = Convert.FromBase64String(_"2Lm8yNQ1sEC0uXbgt2O4vOqpDIVhaKSGM3Zkhj4pyxJX8slkMyECsMrSzEDhbYFrkYwj9tHTS61g8zuHgKV7GpoKNpRkz28kkJAD4ChJhMucPGnDfle1gclUL99OU357PuaZqFWoBlmXstYzQqmVPajCnwQKQSNCsOAkAEEza5ba9uIZD153eimFDqKpGAxUduST7eRgaBNZWtcdw6sTdn13/Ft/mh/gCzjGm0OFurBw4J1vq2LnHZ0B58oAivsCpTkZzCtyYScGI4afGTxP+3EddgRrjKGE82MnQrocKbVRecy5ReIsdwgq5bgb/26nS+V87FD4KH7ab8cKBSdePbLy28W2GELxQnAI+6kZAeCSpo0lmws83gZKySDjrEglJ4R1/Z4EDT28OqN+N4ntSqnTF8vAlBrtTs8uiAd1gGi2Fa2z0f7D5Y2AIerlmQMp3Jg1GgGKAoouswmDNR1Xh2MS/rOo6UklzkFsEsh/d6ZCsh6Y0UGn/SLqk/eevM7B"),
			Exponent = Convert.FromBase64String(_"AQAB")
		});
		return rSA.VerifyData(P_0, P_1, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
	}

	internal static IReadOnlyDictionary<string, string> A(string P_0)
	{
		ReleaseVerificationResult releaseVerificationResult = Verify(P_0);
		if (!releaseVerificationResult.Success)
		{
			throw new InvalidDataException(_"Cannot read an untrusted release manifest: " + releaseVerificationResult.Message);
		}
		return a(File.ReadAllBytes(Path.Combine(P_0, _"GorillaGuard.manifest")));
	}

	internal static ReleaseVerificationResult A(string P_0, string P_1 = null, bool P_2 = true)
	{
		try
		{
			string fullPath = Path.GetFullPath(P_0);
			string text = Path.Combine(fullPath, _"BepInEx");
			string text2 = (string.IsNullOrWhiteSpace(P_1) ? text : Path.GetFullPath(P_1));
			ReleaseVerificationResult releaseVerificationResult = Verify(text2);
			if (!releaseVerificationResult.Success)
			{
				return releaseVerificationResult;
			}
			Dictionary<string, string> dictionary = a(File.ReadAllBytes(Path.Combine(text2, _"GorillaGuard.manifest")));
			List<HostCheck> list = new List<HostCheck>
			{
				new HostCheck(_"gameExecutableSha256", Path.Combine(fullPath, _"Gorilla Tag.exe"), isTree: false, _"Gorilla Tag executable"),
				new HostCheck(_"unityPlayerSha256", Path.Combine(fullPath, _"UnityPlayer.dll"), isTree: false, _"UnityPlayer"),
				new HostCheck(_"gameManagedTreeSha256", Path.Combine(fullPath, _"Gorilla Tag_Data", _"Managed"), isTree: true, _"managed game code"),
				new HostCheck(_"gameNativeTreeSha256", Path.Combine(fullPath, _"Gorilla Tag_Data", _"Plugins"), isTree: true, _"native game code")
			};
			if (P_2)
			{
				list.Insert(0, new HostCheck(_"bepInExCoreTreeSha256", Path.Combine(text, _"core"), isTree: true, _"BepInEx core code"));
				list.Insert(0, new HostCheck(_"doorstopConfigSha256", Path.Combine(fullPath, _"doorstop_config.ini"), isTree: false, _"Doorstop configuration"));
				list.Insert(0, new HostCheck(_"bootstrapSha256", Path.Combine(fullPath, _"winhttp.dll"), isTree: false, _"Doorstop bootstrap"));
			}
			foreach (HostCheck item in list)
			{
				if (!dictionary.TryGetValue(item.ManifestKey, out var value) || !C(value))
				{
					return A(ReleaseFailureKind.InvalidManifest, _"The signed release manifest is missing a valid host baseline field: " + item.ManifestKey);
				}
				if (item.IsTree ? (!Directory.Exists(item.Path)) : (!File.Exists(item.Path)))
				{
					return A(ReleaseFailureKind.HostIntegrityMismatch, _"Required " + item.Label + _" is missing: " + item.Path);
				}
				string text3 = (item.IsTree ? B(item.Path) : a(item.Path));
				bool flag = text3.Equals(value, StringComparison.OrdinalIgnoreCase);
				if (!flag)
				{
					string[] array = new string[8]
					{
						_"Alternate",
						_"Alternate2",
						_"Alternate3",
						_"Alternate4",
						_"Alternate5",
						_"Alternate6",
						_"Alternate7",
						_"Alternate8"
					};
					foreach (string text4 in array)
					{
						if (dictionary.TryGetValue(item.ManifestKey + text4, out var value2) && C(value2) && text3.Equals(value2, StringComparison.OrdinalIgnoreCase))
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(57, 4);
					defaultInterpolatedStringHandler.AppendLiteral(_"Host integrity mismatch for ");
					defaultInterpolatedStringHandler.AppendFormatted(item.Label);
					defaultInterpolatedStringHandler.AppendLiteral(_": expected=");
					defaultInterpolatedStringHandler.AppendFormatted(value);
					defaultInterpolatedStringHandler.AppendLiteral(_"; actual=");
					defaultInterpolatedStringHandler.AppendFormatted(text3);
					defaultInterpolatedStringHandler.AppendLiteral(_"; path=");
					defaultInterpolatedStringHandler.AppendFormatted(item.Path);
					defaultInterpolatedStringHandler.AppendLiteral(_". ");
					return A(ReleaseFailureKind.HostIntegrityMismatch, defaultInterpolatedStringHandler.ToStringAndClear() + ((item.ManifestKey == _"gameManagedTreeSha256") ? A(dictionary, item.Path) : _"") + _"This can mean a different game update or extra/modified code, and is not by itself proof of cheating. Verify Gorilla Tag files through Steam and install a guard release approved for that game build.");
				}
			}
			return new ReleaseVerificationResult
			{
				Success = true,
				FailureKind = ReleaseFailureKind.None,
				Message = _"Signed Gorilla Guard host baseline 1.12.31 verified.",
				Version = _"1.12.31"
			};
		}
		catch (Exception ex)
		{
			return A(ReleaseFailureKind.VerificationError, _"Host baseline verification error: " + ex.Message);
		}
	}

	internal static string A(Dictionary<string, string> P_0, string P_1)
	{
		KeyValuePair<string, string>[] array = P_0.Where((KeyValuePair<string, string> P_0) => P_0.Key.StartsWith(_"managedFileSha256.", StringComparison.OrdinalIgnoreCase)).ToArray();
		if (array.Length == 0)
		{
			return _"File-level inventory unavailable in this older package. ";
		}
		string text = Path.GetFullPath(P_1).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		List<string> list = new List<string>();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		KeyValuePair<string, string>[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			KeyValuePair<string, string> keyValuePair = array2[i];
			string text2 = keyValuePair.Key.Substring(_"managedFileSha256.".Length).Replace('/', Path.DirectorySeparatorChar);
			string fullPath = Path.GetFullPath(Path.Combine(text, text2));
			if (!fullPath.StartsWith(text, StringComparison.OrdinalIgnoreCase) || !b(fullPath) || !C(keyValuePair.Value) || !hashSet.Add(fullPath))
			{
				throw new InvalidDataException(_"Invalid signed managed-file inventory: " + keyValuePair.Key);
			}
			if (!File.Exists(fullPath))
			{
				num2++;
				if (list.Count < 12)
				{
					list.Add(_"MISSING " + text2);
				}
			}
			else if (!string.Equals(a(fullPath), keyValuePair.Value, StringComparison.OrdinalIgnoreCase))
			{
				num++;
				if (list.Count < 12)
				{
					list.Add(_"CHANGED " + text2);
				}
			}
		}
		foreach (string item in Directory.GetFiles(P_1, _"*", SearchOption.AllDirectories).Where(b))
		{
			if (!hashSet.Contains(Path.GetFullPath(item)))
			{
				num3++;
				if (list.Count < 12)
				{
					list.Add(_"EXTRA " + Path.GetRelativePath(P_1, item));
				}
			}
		}
		DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 3);
		defaultInterpolatedStringHandler.AppendLiteral(_"Managed-file diagnosis: changed=");
		defaultInterpolatedStringHandler.AppendFormatted(num);
		defaultInterpolatedStringHandler.AppendLiteral(_", missing=");
		defaultInterpolatedStringHandler.AppendFormatted(num2);
		defaultInterpolatedStringHandler.AppendLiteral(_", extra=");
		defaultInterpolatedStringHandler.AppendFormatted(num3);
		defaultInterpolatedStringHandler.AppendLiteral(_". ");
		return defaultInterpolatedStringHandler.ToStringAndClear() + string.Join(_"; ", list) + ((num + num2 + num3 > list.Count) ? _"; additional differences omitted" : _"") + _". ";
	}

	internal static string a(string P_0)
	{
		using SHA256 sHA = SHA256.Create();
		using FileStream inputStream = new FileStream(P_0, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
		byte[] array = sHA.ComputeHash(inputStream);
		StringBuilder stringBuilder = new StringBuilder(array.Length * 2);
		byte[] array2 = array;
		foreach (byte b in array2)
		{
			stringBuilder.Append(b.ToString(_"X2"));
		}
		return stringBuilder.ToString();
	}

	private static ReleaseVerificationResult A(string P_0, string P_1, string P_2)
	{
		if (!File.Exists(P_0))
		{
			return A(ReleaseFailureKind.HostIntegrityMismatch, _"Missing signed " + P_2 + _": " + P_0);
		}
		string text = a(P_0);
		if (!text.Equals(P_1, StringComparison.OrdinalIgnoreCase))
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(48, 4);
			defaultInterpolatedStringHandler.AppendLiteral(_"Signed ");
			defaultInterpolatedStringHandler.AppendFormatted(P_2);
			defaultInterpolatedStringHandler.AppendLiteral(_" hash mismatch: expected=");
			defaultInterpolatedStringHandler.AppendFormatted(P_1);
			defaultInterpolatedStringHandler.AppendLiteral(_"; actual=");
			defaultInterpolatedStringHandler.AppendFormatted(text);
			defaultInterpolatedStringHandler.AppendLiteral(_"; path=");
			defaultInterpolatedStringHandler.AppendFormatted(P_0);
			return A(ReleaseFailureKind.HostIntegrityMismatch, defaultInterpolatedStringHandler.ToStringAndClear());
		}
		return new ReleaseVerificationResult
		{
			Success = true,
			FailureKind = ReleaseFailureKind.None
		};
	}

	internal static string B(string P_0)
	{
		string fullRoot = Path.GetFullPath(P_0).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		if ((File.GetAttributes(fullRoot) & FileAttributes.ReparsePoint) != 0)
		{
			throw new InvalidDataException(_"A protected code root cannot be a reparse point: " + fullRoot);
		}
		List<string> list = new List<string>();
		Stack<string> stack = new Stack<string>();
		stack.Push(fullRoot);
		while (stack.Count > 0)
		{
			string path = stack.Pop();
			foreach (string item in Directory.EnumerateDirectories(path, _"*", SearchOption.TopDirectoryOnly))
			{
				if ((File.GetAttributes(item) & FileAttributes.ReparsePoint) != 0)
				{
					throw new InvalidDataException(_"A protected code directory cannot contain a reparse point: " + item);
				}
				stack.Push(item);
			}
			foreach (string item2 in Directory.EnumerateFiles(path, _"*", SearchOption.TopDirectoryOnly))
			{
				if (b(item2))
				{
					if ((File.GetAttributes(item2) & FileAttributes.ReparsePoint) != 0)
					{
						throw new InvalidDataException(_"A protected code file cannot be a reparse point: " + item2);
					}
					list.Add(item2);
				}
			}
		}
		list.Sort((string P_0, string P_1) => StringComparer.Ordinal.Compare(A(fullRoot, P_0), A(fullRoot, P_1)));
		using SHA256 sHA = SHA256.Create();
		using CryptoStream cryptoStream = new CryptoStream(Stream.Null, sHA, CryptoStreamMode.Write);
		using BinaryWriter binaryWriter = new BinaryWriter(cryptoStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), leaveOpen: true);
		foreach (string item3 in list)
		{
			string s = A(fullRoot, item3);
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			binaryWriter.Write(bytes.Length);
			binaryWriter.Write(bytes);
			binaryWriter.Write(new FileInfo(item3).Length);
			binaryWriter.Flush();
			using FileStream fileStream = new FileStream(item3, FileMode.Open, FileAccess.Read, FileShare.Read);
			fileStream.CopyTo(cryptoStream);
		}
		binaryWriter.Flush();
		cryptoStream.FlushFinalBlock();
		return A(sHA.Hash);
	}

	private static ReleaseVerificationResult a(Dictionary<string, string> P_0, string P_1)
	{
		KeyValuePair<string, string>[] array = P_0.Where((KeyValuePair<string, string> P_0) => P_0.Key.StartsWith(_"coreFileSha256.", StringComparison.OrdinalIgnoreCase)).ToArray();
		if (array.Length == 0)
		{
			return new ReleaseVerificationResult
			{
				Success = true
			};
		}
		string text = Path.GetFullPath(P_1).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		KeyValuePair<string, string>[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			KeyValuePair<string, string> keyValuePair = array2[i];
			string path = keyValuePair.Key.Substring(_"coreFileSha256.".Length).Replace('/', Path.DirectorySeparatorChar);
			string fullPath = Path.GetFullPath(Path.Combine(text, path));
			if (!fullPath.StartsWith(text, StringComparison.OrdinalIgnoreCase) || !b(fullPath) || !C(keyValuePair.Value) || !hashSet.Add(fullPath))
			{
				return A(ReleaseFailureKind.InvalidManifest, _"Invalid signed core file inventory entry: " + keyValuePair.Key);
			}
			if (!File.Exists(fullPath))
			{
				return A(ReleaseFailureKind.HostIntegrityMismatch, _"Missing signed core file: " + fullPath);
			}
			string text2 = a(fullPath);
			if (!string.Equals(text2, keyValuePair.Value, StringComparison.OrdinalIgnoreCase))
			{
				return A(ReleaseFailureKind.HostIntegrityMismatch, _"Core file hash mismatch: path=" + fullPath + _"; expected=" + keyValuePair.Value + _"; actual=" + text2);
			}
		}
		foreach (string item in Directory.GetFiles(P_1, _"*", SearchOption.AllDirectories).Where(b))
		{
			if (!hashSet.Contains(Path.GetFullPath(item)))
			{
				return A(ReleaseFailureKind.HostIntegrityMismatch, _"Unexpected executable code in signed core: " + item);
			}
		}
		return new ReleaseVerificationResult
		{
			Success = true
		};
	}

	private static string A(string P_0, string P_1)
	{
		return Path.GetRelativePath(P_0, P_1).Replace('\\', '/').ToLowerInvariant();
	}

	private static bool b(string P_0)
	{
		string extension = Path.GetExtension(P_0);
		if (!extension.Equals(_".dll", StringComparison.OrdinalIgnoreCase) && !extension.Equals(_".exe", StringComparison.OrdinalIgnoreCase) && !extension.Equals(_".winmd", StringComparison.OrdinalIgnoreCase))
		{
			return extension.Equals(_".so", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static string A(byte[] P_0)
	{
		StringBuilder stringBuilder = new StringBuilder(P_0.Length * 2);
		foreach (byte b in P_0)
		{
			stringBuilder.Append(b.ToString(_"X2"));
		}
		return stringBuilder.ToString();
	}

	private static Dictionary<string, string> a(byte[] P_0)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
		string[] array = Encoding.UTF8.GetString(P_0).Split(new string[2]
		{
			_"\r\n",
			_"\n"
		}, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
		{
			int num = text.IndexOf('=');
			if (num <= 0 || num == text.Length - 1)
			{
				throw new InvalidDataException(_"Malformed release manifest line.");
			}
			string text2 = text.Substring(0, num).Trim();
			string value = text.Substring(num + 1).Trim();
			if (!dictionary.TryAdd(text2, value))
			{
				throw new InvalidDataException(_"Duplicate release manifest field: " + text2);
			}
		}
		return dictionary;
	}

	private static bool C(string P_0)
	{
		if (P_0 == null || P_0.Length != 64)
		{
			return false;
		}
		foreach (char c in P_0)
		{
			if ((c < '0' || c > '9') && (c < 'a' || c > 'f') && (c < 'A' || c > 'F'))
			{
				return false;
			}
		}
		return true;
	}

	private static ReleaseVerificationResult A(ReleaseFailureKind P_0, string P_1, string P_2 = null, string P_3 = null, string P_4 = null)
	{
		return new ReleaseVerificationResult
		{
			Success = false,
			FailureKind = P_0,
			Message = P_1,
			Version = P_2,
			ExpectedRuntimeHash = P_3,
			ActualRuntimeHash = P_4
		};
	}
}
#pragma endregion

#pragma region GorillaGuard.Security/SecureDownloads.cs
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace GorillaGuard.Security;

internal static class SecureDownloads
{
	internal static HttpClient A()
	{
		return new HttpClient(new HttpClientHandler
		{
			AllowAutoRedirect = false,
			SslProtocols = (SslProtocols.Tls12 | SslProtocols.Tls13)
		})
		{
			Timeout = TimeSpan.FromMinutes(3.0)
		};
	}

	internal static async Task<HttpResponseMessage> A(HttpClient P_0, string P_1, CancellationToken P_2 = default(CancellationToken))
	{
		Uri address = new Uri(P_1, UriKind.Absolute);
		for (int redirects = 0; redirects <= 5; redirects++)
		{
			if (address.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(address.UserInfo))
			{
				throw new InvalidDataException(_"Downloads require HTTPS without embedded credentials, including every redirect.");
			}
			HttpResponseMessage httpResponseMessage = await P_0.GetAsync(address, HttpCompletionOption.ResponseHeadersRead, P_2);
			HttpStatusCode statusCode = httpResponseMessage.StatusCode;
			if (((uint)(statusCode - 301) <= 2u || (uint)(statusCode - 307) <= 1u) ? true : false)
			{
				Uri location = httpResponseMessage.Headers.Location;
				httpResponseMessage.Dispose();
				if (location == null)
				{
					throw new InvalidDataException(_"Download redirect has no destination.");
				}
				address = new Uri(address, location);
				continue;
			}
			return httpResponseMessage;
		}
		throw new InvalidDataException(_"Download exceeded the redirect limit.");
	}
}
#pragma endregion

#pragma region GorillaGuard.Security/SessionLocation.cs
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace GorillaGuard.Security;

internal static class SessionLocation
{
	internal static string A(string P_0)
	{
		string s = Path.GetFullPath(P_0).TrimEnd('\\', '/').ToUpperInvariant();
		using SHA256 sHA = SHA256.Create();
		string text = BitConverter.ToString(sHA.ComputeHash(Encoding.UTF8.GetBytes(s))).Replace(_"-", _"");
		return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _"GorillaGuard", _"sessions", text, _"GorillaGuard.session");
	}
}
#pragma endregion

#pragma region GorillaGuard.Security/WarningGate.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GorillaGuard.Security;

internal sealed class WarningGate
{
	private readonly Dictionary<string, long> last = new Dictionary<string, long>(StringComparer.Ordinal);

	internal bool A(string P_0)
	{
		long timestamp = Stopwatch.GetTimestamp();
		lock (last)
		{
			if (last.TryGetValue(P_0, out var value) && (double)(timestamp - value) / (double)Stopwatch.Frequency < 15.0)
			{
				return false;
			}
			if (last.Count >= 512)
			{
				last.Clear();
			}
			last[P_0] = timestamp;
			return true;
		}
	}
}
#pragma endregion

#pragma region GorillaGuard.Security/WorkQueue.cs
using System;
using System.Collections.Generic;

namespace GorillaGuard.Security;

internal sealed class WorkQueue<T>
{
	private readonly Queue<T> items = new Queue<T>();

	private readonly int capacity;

	private long dropped;

	internal long Dropped
	{
		get
		{
			lock (items)
			{
				return dropped;
			}
		}
	}

	internal int Count
	{
		get
		{
			lock (items)
			{
				return items.Count;
			}
		}
	}

	internal bool IsEmpty => Count == 0;

	internal WorkQueue(int capacity)
	{
		if (capacity < 1)
		{
			throw new ArgumentOutOfRangeException(_"capacity");
		}
		this.capacity = capacity;
	}

	internal void A(T P_0)
	{
		lock (items)
		{
			if (items.Count >= capacity)
			{
				dropped++;
			}
			else
			{
				items.Enqueue(P_0);
			}
		}
	}

	internal bool A(out T P_0)
	{
		lock (items)
		{
			if (items.Count == 0)
			{
				P_0 = default(T);
				return false;
			}
			P_0 = items.Dequeue();
			return true;
		}
	}
}
#pragma endregion

#pragma region Properties/AssemblyInfo.cs
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

[assembly: AssemblyCompany("GorillaGuard.Launcher")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyDescription("Gorilla Guard — local game integrity protection")]
[assembly: AssemblyFileVersion("1.12.31.0")]
[assembly: AssemblyInformationalVersion("1.12.31")]
[assembly: AssemblyProduct("Gorilla Guard")]
[assembly: AssemblyTitle("GorillaGuard.Launcher")]
[assembly: TargetPlatform("Windows7.0")]
[assembly: SupportedOSPlatform("Windows7.0")]
[assembly: AssemblyVersion("1.12.31.0")]
[module: RefSafetyRules(11)]
#pragma endregion
