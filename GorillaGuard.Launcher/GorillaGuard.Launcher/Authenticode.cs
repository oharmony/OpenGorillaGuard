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
