
#pragma region Properties/AssemblyInfo.cs
﻿using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

[module: RefSafetyRules(11)]
#pragma endregion

#pragma region GorillaGuard.Security/FrameRatePolicy.cs
﻿using System;

namespace GorillaGuard.Security;

internal static class FrameRatePolicy
{
	private static readonly float[] Allowed = new float[4] { 72f, 80f, 90f, 120f };

	internal static float A(float P_0)
	{
		float num = Allowed[0];
		float[] allowed = Allowed;
		foreach (float num2 in allowed)
		{
			if (Math.Abs(num2 - P_0) < Math.Abs(num - P_0))
			{
				num = num2;
			}
		}
		return num;
	}

	internal static bool a(float P_0)
	{
		if (!float.IsNaN(P_0) && !float.IsInfinity(P_0) && P_0 > 0f)
		{
			return Math.Abs(P_0 - A(P_0)) <= 3f;
		}
		return false;
	}

	internal static int A(int P_0)
	{
		if (P_0 > 0 && !a(P_0))
		{
			return (int)A((float)P_0);
		}
		return P_0;
	}
}
#pragma endregion

#pragma region GorillaGuard.Security/InjectionPolicy.cs
﻿using System;
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
		if (hashSet.Contains("Absense.Injector") && hashSet.Contains("Absense.Memory") && hashSet.Contains("Absense.Native") && hashSet2.Contains("WriteProcessMemory") && hashSet2.Contains("VirtualAllocEx"))
		{
			return hashSet2.Contains("CreateRemoteThread");
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

#pragma region GorillaGuard.Security/ProtectionStatus.cs
﻿
namespace GorillaGuard.Security;

internal static class ProtectionStatus
{
	internal static string A(bool P_0, bool P_1, bool P_2, bool P_3, bool P_4, bool P_5 = false, bool P_6 = false, bool P_7 = false)
	{
		if (!P_0)
		{
			return "Integrity failed — repair required";
		}
		if (!P_1)
		{
			return "Waiting for launcher verification";
		}
		if (P_6)
		{
			return "Value correction incomplete — restart required";
		}
		if (P_5)
		{
			return "Monitoring incomplete — restart required";
		}
		if (P_3)
		{
			return "Removal incomplete — restart required";
		}
		if (P_7)
		{
			return "Repeated protected-value changes detected";
		}
		if (P_2)
		{
			if (!P_4)
			{
				return "Mod disabled";
			}
			return "Mod disabled — restart required";
		}
		if (!P_4)
		{
			return "Verified";
		}
		return "Restart required";
	}
}
#pragma endregion

#pragma region GorillaGuard.Security/ReleaseFailureKind.cs
﻿namespace GorillaGuard.Security;

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
﻿namespace GorillaGuard.Security;

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
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
			string text = Path.Combine(bepInExRoot, "GorillaGuard.manifest");
			string text2 = Path.Combine(bepInExRoot, "GorillaGuard.manifest.sig");
			string text3 = Path.Combine(bepInExRoot, "plugins", "GorillaGuard.dll");
			string text4 = Path.Combine(bepInExRoot, "patchers", "GorillaGuard.Preloader.dll");
			string fullName = Directory.GetParent(Path.GetFullPath(bepInExRoot)).FullName;
			string text5 = Path.Combine(fullName, "GorillaGuard.Launcher.exe");
			string text6 = Path.Combine(fullName, "winhttp.dll");
			string text7 = Path.Combine(fullName, "doorstop_config.ini");
			string text8 = Path.Combine(bepInExRoot, "core");
			if (!File.Exists(text))
			{
				return A(ReleaseFailureKind.MissingManifest, "Missing signed release manifest: " + text);
			}
			if (!File.Exists(text2))
			{
				return A(ReleaseFailureKind.MissingSignature, "Missing release signature: " + text2);
			}
			byte[] array = File.ReadAllBytes(text);
			byte[] array2 = File.ReadAllBytes(text2);
			if (!A(array, array2))
			{
				return A(ReleaseFailureKind.InvalidSignature, "The release manifest signature is invalid.");
			}
			Dictionary<string, string> dictionary = a(array);
			if (!dictionary.TryGetValue("version", out var value) || !dictionary.TryGetValue("runtimeSha256", out var value2) || !dictionary.TryGetValue("preloaderSha256", out var value3) || !dictionary.TryGetValue("launcherSha256", out var value4) || !dictionary.TryGetValue("bootstrapSha256", out var value5) || !dictionary.TryGetValue("doorstopConfigSha256", out var value6) || !dictionary.TryGetValue("bepInExCoreTreeSha256", out var value7))
			{
				return A(ReleaseFailureKind.InvalidManifest, "The signed release manifest is missing required fields.");
			}
			if (!value.Equals("1.12.31", StringComparison.Ordinal))
			{
				return A(ReleaseFailureKind.VersionMismatch, "Signed release version " + value + " does not match required version 1.12.31.");
			}
			if (!C(value2) || !C(value3) || !C(value4) || !C(value5) || !C(value6) || !C(value7))
			{
				return A(ReleaseFailureKind.InvalidManifest, "The signed release manifest contains an invalid SHA-256 value.");
			}
			if (dictionary.TryGetValue("gameNativeTreeSha256Alternate", out var value8) && !C(value8))
			{
				return A(ReleaseFailureKind.InvalidManifest, "The signed release manifest contains an invalid alternate native-code SHA-256 value.");
			}
			if (!File.Exists(text3))
			{
				return A(ReleaseFailureKind.MissingRuntime, "Missing required runtime: " + text3, value, value2);
			}
			string text9 = a(text3);
			if (!text9.Equals(value2, StringComparison.OrdinalIgnoreCase))
			{
				return A(ReleaseFailureKind.RuntimeHashMismatch, "Runtime hash mismatch: path=" + text3 + "; expected=" + value2 + "; actual=" + text9, value, value2, text9);
			}
			if (!File.Exists(text4))
			{
				return A(ReleaseFailureKind.MissingPreloader, "Missing required preloader: " + text4);
			}
			string text10 = a(text4);
			if (!text10.Equals(value3, StringComparison.OrdinalIgnoreCase))
			{
				return A(ReleaseFailureKind.PreloaderHashMismatch, "Preloader hash mismatch: path=" + text4 + "; expected=" + value3 + "; actual=" + text10);
			}
			if (verifyLauncher)
			{
				if (!File.Exists(text5))
				{
					return A(ReleaseFailureKind.MissingLauncher, "Missing required launcher: " + text5);
				}
				string text11 = a(text5);
				if (!text11.Equals(value4, StringComparison.OrdinalIgnoreCase))
				{
					return A(ReleaseFailureKind.LauncherHashMismatch, "Launcher hash mismatch: path=" + text5 + "; expected=" + value4 + "; actual=" + text11);
				}
			}
			ReleaseVerificationResult releaseVerificationResult = A(text6, value5, "Doorstop bootstrap");
			if (!releaseVerificationResult.Success)
			{
				return releaseVerificationResult;
			}
			ReleaseVerificationResult releaseVerificationResult2 = A(text7, value6, "Doorstop configuration");
			if (!releaseVerificationResult2.Success)
			{
				return releaseVerificationResult2;
			}
			if (dictionary.TryGetValue("openvrApiSha256", out var value9))
			{
				if (!C(value9))
				{
					return A(ReleaseFailureKind.InvalidManifest, "Invalid signed OpenVR API repair hash.");
				}
				ReleaseVerificationResult releaseVerificationResult3 = A(Path.Combine(bepInExRoot, "repair", "openvr_api.dll"), value9, "OpenVR API repair payload");
				if (!releaseVerificationResult3.Success)
				{
					return releaseVerificationResult3;
				}
			}
			if (!Directory.Exists(text8))
			{
				return A(ReleaseFailureKind.HostIntegrityMismatch, "Missing signed BepInEx core: " + text8);
			}
			string text12 = B(text8);
			ReleaseVerificationResult releaseVerificationResult4 = a(dictionary, text8);
			if (!releaseVerificationResult4.Success)
			{
				return releaseVerificationResult4;
			}
			if (!text12.Equals(value7, StringComparison.OrdinalIgnoreCase))
			{
				return A(ReleaseFailureKind.HostIntegrityMismatch, "Signed BepInEx core hash mismatch: expected=" + value7 + "; actual=" + text12 + "; path=" + text8);
			}
			return new ReleaseVerificationResult
			{
				Success = true,
				FailureKind = ReleaseFailureKind.None,
				Message = "Signed Gorilla Guard release " + value + " verified.",
				Version = value,
				ExpectedRuntimeHash = value2,
				ActualRuntimeHash = text9
			};
		}
		catch (Exception ex)
		{
			return A(ReleaseFailureKind.VerificationError, "Release verification error: " + ex.Message);
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
			Modulus = Convert.FromBase64String("2Lm8yNQ1sEC0uXbgt2O4vOqpDIVhaKSGM3Zkhj4pyxJX8slkMyECsMrSzEDhbYFrkYwj9tHTS61g8zuHgKV7GpoKNpRkz28kkJAD4ChJhMucPGnDfle1gclUL99OU357PuaZqFWoBlmXstYzQqmVPajCnwQKQSNCsOAkAEEza5ba9uIZD153eimFDqKpGAxUduST7eRgaBNZWtcdw6sTdn13/Ft/mh/gCzjGm0OFurBw4J1vq2LnHZ0B58oAivsCpTkZzCtyYScGI4afGTxP+3EddgRrjKGE82MnQrocKbVRecy5ReIsdwgq5bgb/26nS+V87FD4KH7ab8cKBSdePbLy28W2GELxQnAI+6kZAeCSpo0lmws83gZKySDjrEglJ4R1/Z4EDT28OqN+N4ntSqnTF8vAlBrtTs8uiAd1gGi2Fa2z0f7D5Y2AIerlmQMp3Jg1GgGKAoouswmDNR1Xh2MS/rOo6UklzkFsEsh/d6ZCsh6Y0UGn/SLqk/eevM7B"),
			Exponent = Convert.FromBase64String("AQAB")
		});
		return rSA.VerifyData(P_0, P_1, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
	}

	internal static IReadOnlyDictionary<string, string> A(string P_0)
	{
		ReleaseVerificationResult releaseVerificationResult = Verify(P_0);
		if (!releaseVerificationResult.Success)
		{
			throw new InvalidDataException("Cannot read an untrusted release manifest: " + releaseVerificationResult.Message);
		}
		return a(File.ReadAllBytes(Path.Combine(P_0, "GorillaGuard.manifest")));
	}

	internal static ReleaseVerificationResult A(string P_0, string P_1 = null, bool P_2 = true)
	{
		try
		{
			string fullPath = Path.GetFullPath(P_0);
			string text = Path.Combine(fullPath, "BepInEx");
			string text2 = (string.IsNullOrWhiteSpace(P_1) ? text : Path.GetFullPath(P_1));
			ReleaseVerificationResult releaseVerificationResult = Verify(text2);
			if (!releaseVerificationResult.Success)
			{
				return releaseVerificationResult;
			}
			Dictionary<string, string> dictionary = a(File.ReadAllBytes(Path.Combine(text2, "GorillaGuard.manifest")));
			List<HostCheck> list = new List<HostCheck>
			{
				new HostCheck("gameExecutableSha256", Path.Combine(fullPath, "Gorilla Tag.exe"), isTree: false, "Gorilla Tag executable"),
				new HostCheck("unityPlayerSha256", Path.Combine(fullPath, "UnityPlayer.dll"), isTree: false, "UnityPlayer"),
				new HostCheck("gameManagedTreeSha256", Path.Combine(fullPath, "Gorilla Tag_Data", "Managed"), isTree: true, "managed game code"),
				new HostCheck("gameNativeTreeSha256", Path.Combine(fullPath, "Gorilla Tag_Data", "Plugins"), isTree: true, "native game code")
			};
			if (P_2)
			{
				list.Insert(0, new HostCheck("bepInExCoreTreeSha256", Path.Combine(text, "core"), isTree: true, "BepInEx core code"));
				list.Insert(0, new HostCheck("doorstopConfigSha256", Path.Combine(fullPath, "doorstop_config.ini"), isTree: false, "Doorstop configuration"));
				list.Insert(0, new HostCheck("bootstrapSha256", Path.Combine(fullPath, "winhttp.dll"), isTree: false, "Doorstop bootstrap"));
			}
			foreach (HostCheck item in list)
			{
				if (!dictionary.TryGetValue(item.ManifestKey, out var value) || !C(value))
				{
					return A(ReleaseFailureKind.InvalidManifest, "The signed release manifest is missing a valid host baseline field: " + item.ManifestKey);
				}
				if (item.IsTree ? (!Directory.Exists(item.Path)) : (!File.Exists(item.Path)))
				{
					return A(ReleaseFailureKind.HostIntegrityMismatch, "Required " + item.Label + " is missing: " + item.Path);
				}
				string text3 = (item.IsTree ? B(item.Path) : a(item.Path));
				bool flag = text3.Equals(value, StringComparison.OrdinalIgnoreCase);
				if (!flag)
				{
					string[] array = new string[8]
					{
						"Alternate",
						"Alternate2",
						"Alternate3",
						"Alternate4",
						"Alternate5",
						"Alternate6",
						"Alternate7",
						"Alternate8"
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
					return A(ReleaseFailureKind.HostIntegrityMismatch, "Host integrity mismatch for " + item.Label + ": expected=" + value + "; actual=" + text3 + "; path=" + item.Path + ". " + ((item.ManifestKey == "gameManagedTreeSha256") ? A(dictionary, item.Path) : "") + "This can mean a different game update or extra/modified code, and is not by itself proof of cheating. Verify Gorilla Tag files through Steam and install a guard release approved for that game build.");
				}
			}
			return new ReleaseVerificationResult
			{
				Success = true,
				FailureKind = ReleaseFailureKind.None,
				Message = "Signed Gorilla Guard host baseline 1.12.31 verified.",
				Version = "1.12.31"
			};
		}
		catch (Exception ex)
		{
			return A(ReleaseFailureKind.VerificationError, "Host baseline verification error: " + ex.Message);
		}
	}

	internal static string A(Dictionary<string, string> P_0, string P_1)
	{
		KeyValuePair<string, string>[] array = P_0.Where((KeyValuePair<string, string> P_0) => P_0.Key.StartsWith("managedFileSha256.", StringComparison.OrdinalIgnoreCase)).ToArray();
		if (array.Length == 0)
		{
			return "File-level inventory unavailable in this older package. ";
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
			string text2 = keyValuePair.Key.Substring("managedFileSha256.".Length).Replace('/', Path.DirectorySeparatorChar);
			string fullPath = Path.GetFullPath(Path.Combine(text, text2));
			if (!fullPath.StartsWith(text, StringComparison.OrdinalIgnoreCase) || !b(fullPath) || !C(keyValuePair.Value) || !hashSet.Add(fullPath))
			{
				throw new InvalidDataException("Invalid signed managed-file inventory: " + keyValuePair.Key);
			}
			if (!File.Exists(fullPath))
			{
				num2++;
				if (list.Count < 12)
				{
					list.Add("MISSING " + text2);
				}
			}
			else if (!string.Equals(a(fullPath), keyValuePair.Value, StringComparison.OrdinalIgnoreCase))
			{
				num++;
				if (list.Count < 12)
				{
					list.Add("CHANGED " + text2);
				}
			}
		}
		foreach (string item in Directory.GetFiles(P_1, "*", SearchOption.AllDirectories).Where(b))
		{
			if (!hashSet.Contains(Path.GetFullPath(item)))
			{
				num3++;
				if (list.Count < 12)
				{
					list.Add("EXTRA " + Path.GetRelativePath(P_1, item));
				}
			}
		}
		return string.Format("Managed-file diagnosis: changed={0}, missing={1}, extra={2}. ", num, num2, num3) + string.Join("; ", list) + ((num + num2 + num3 > list.Count) ? "; additional differences omitted" : "") + ". ";
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
			stringBuilder.Append(b.ToString("X2"));
		}
		return stringBuilder.ToString();
	}

	private static ReleaseVerificationResult A(string P_0, string P_1, string P_2)
	{
		if (!File.Exists(P_0))
		{
			return A(ReleaseFailureKind.HostIntegrityMismatch, "Missing signed " + P_2 + ": " + P_0);
		}
		string text = a(P_0);
		if (!text.Equals(P_1, StringComparison.OrdinalIgnoreCase))
		{
			return A(ReleaseFailureKind.HostIntegrityMismatch, "Signed " + P_2 + " hash mismatch: expected=" + P_1 + "; actual=" + text + "; path=" + P_0);
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
			throw new InvalidDataException("A protected code root cannot be a reparse point: " + fullRoot);
		}
		List<string> list = new List<string>();
		Stack<string> stack = new Stack<string>();
		stack.Push(fullRoot);
		while (stack.Count > 0)
		{
			string path = stack.Pop();
			foreach (string item in Directory.EnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly))
			{
				if ((File.GetAttributes(item) & FileAttributes.ReparsePoint) != 0)
				{
					throw new InvalidDataException("A protected code directory cannot contain a reparse point: " + item);
				}
				stack.Push(item);
			}
			foreach (string item2 in Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly))
			{
				if (b(item2))
				{
					if ((File.GetAttributes(item2) & FileAttributes.ReparsePoint) != 0)
					{
						throw new InvalidDataException("A protected code file cannot be a reparse point: " + item2);
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
		KeyValuePair<string, string>[] array = P_0.Where((KeyValuePair<string, string> P_0) => P_0.Key.StartsWith("coreFileSha256.", StringComparison.OrdinalIgnoreCase)).ToArray();
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
			string path = keyValuePair.Key.Substring("coreFileSha256.".Length).Replace('/', Path.DirectorySeparatorChar);
			string fullPath = Path.GetFullPath(Path.Combine(text, path));
			if (!fullPath.StartsWith(text, StringComparison.OrdinalIgnoreCase) || !b(fullPath) || !C(keyValuePair.Value) || !hashSet.Add(fullPath))
			{
				return A(ReleaseFailureKind.InvalidManifest, "Invalid signed core file inventory entry: " + keyValuePair.Key);
			}
			if (!File.Exists(fullPath))
			{
				return A(ReleaseFailureKind.HostIntegrityMismatch, "Missing signed core file: " + fullPath);
			}
			string text2 = a(fullPath);
			if (!string.Equals(text2, keyValuePair.Value, StringComparison.OrdinalIgnoreCase))
			{
				return A(ReleaseFailureKind.HostIntegrityMismatch, "Core file hash mismatch: path=" + fullPath + "; expected=" + keyValuePair.Value + "; actual=" + text2);
			}
		}
		foreach (string item in Directory.GetFiles(P_1, "*", SearchOption.AllDirectories).Where(b))
		{
			if (!hashSet.Contains(Path.GetFullPath(item)))
			{
				return A(ReleaseFailureKind.HostIntegrityMismatch, "Unexpected executable code in signed core: " + item);
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
		if (!extension.Equals(".dll", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".exe", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".winmd", StringComparison.OrdinalIgnoreCase))
		{
			return extension.Equals(".so", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static string A(byte[] P_0)
	{
		StringBuilder stringBuilder = new StringBuilder(P_0.Length * 2);
		foreach (byte b in P_0)
		{
			stringBuilder.Append(b.ToString("X2"));
		}
		return stringBuilder.ToString();
	}

	private static Dictionary<string, string> a(byte[] P_0)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
		string[] array = Encoding.UTF8.GetString(P_0).Split(new string[2]
		{
			"\r\n",
			"\n"
		}, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
		{
			int num = text.IndexOf('=');
			if (num <= 0 || num == text.Length - 1)
			{
				throw new InvalidDataException("Malformed release manifest line.");
			}
			string text2 = text.Substring(0, num).Trim();
			string value = text.Substring(num + 1).Trim();
			if (!dictionary.TryAdd(text2, value))
			{
				throw new InvalidDataException("Duplicate release manifest field: " + text2);
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

#pragma region GorillaGuard.Security/SessionLocation.cs
﻿using System;
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
		string text = BitConverter.ToString(sHA.ComputeHash(Encoding.UTF8.GetBytes(s))).Replace("-", "");
		return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GorillaGuard", "sessions", text, "GorillaGuard.session");
	}
}
#pragma endregion

#pragma region GorillaGuard.Security/TrackingAnomaly.cs
﻿using System;

namespace GorillaGuard.Security;

[Flags]
internal enum TrackingAnomaly
{
	None = 0,
	InvalidSample = 1,
	LeftReach = 2,
	RightReach = 4,
	HeadJump = 8,
	LeftHandJump = 0x10,
	RightHandJump = 0x20,
	HeadSpeed = 0x40,
	LeftHandSpeed = 0x80,
	RightHandSpeed = 0x100,
	HeadAcceleration = 0x200,
	LeftHandAcceleration = 0x400,
	RightHandAcceleration = 0x800
}
#pragma endregion

#pragma region GorillaGuard.Security/TrackingAnomalyWindow.cs
﻿namespace GorillaGuard.Security;

internal sealed class TrackingAnomalyWindow
{
	internal const double MaximumReachMeters = 1.85;

	internal const double MaximumHeadJumpMeters = 0.75;

	internal const double MaximumHandJumpMeters = 1.25;

	internal const double MaximumHeadSpeedMetersPerSecond = 15.0;

	internal const double MaximumHandSpeedMetersPerSecond = 35.0;

	internal const double MaximumHeadAccelerationMetersPerSecondSquared = 180.0;

	internal const double MaximumHandAccelerationMetersPerSecondSquared = 450.0;

	internal const double MaximumSampleGapSeconds = 0.5;

	private TrackingPoseSample _previous;

	private TrackingPoint _previousHeadVelocity;

	private TrackingPoint _previousLeftVelocity;

	private TrackingPoint _previousRightVelocity;

	private bool _hasSample;

	private bool _hasVelocity;

	internal void A()
	{
		_previous = default(TrackingPoseSample);
		_previousHeadVelocity = default(TrackingPoint);
		_previousLeftVelocity = default(TrackingPoint);
		_previousRightVelocity = default(TrackingPoint);
		_hasSample = false;
		_hasVelocity = false;
	}

	internal TrackingAnomaly A(TrackingPoseSample P_0)
	{
		if (!P_0.IsFinite)
		{
			A();
			return TrackingAnomaly.InvalidSample;
		}
		TrackingAnomaly trackingAnomaly = TrackingAnomaly.None;
		if ((P_0.LeftHand - P_0.Head).Magnitude > 1.85)
		{
			trackingAnomaly |= TrackingAnomaly.LeftReach;
		}
		if ((P_0.RightHand - P_0.Head).Magnitude > 1.85)
		{
			trackingAnomaly |= TrackingAnomaly.RightReach;
		}
		if (!_hasSample)
		{
			_previous = P_0;
			_hasSample = true;
			return trackingAnomaly;
		}
		double num = P_0.Time - _previous.Time;
		if (!double.IsFinite(num) || num <= 0.0 || num > 0.5)
		{
			_previous = P_0;
			_hasVelocity = false;
			return trackingAnomaly;
		}
		TrackingPoint trackingPoint = P_0.Head - P_0.Body - (_previous.Head - _previous.Body);
		TrackingPoint trackingPoint2 = P_0.LeftHand - P_0.Head - (_previous.LeftHand - _previous.Head);
		TrackingPoint trackingPoint3 = P_0.RightHand - P_0.Head - (_previous.RightHand - _previous.Head);
		if (trackingPoint.Magnitude > 0.75)
		{
			trackingAnomaly |= TrackingAnomaly.HeadJump;
		}
		if (trackingPoint2.Magnitude > 1.25)
		{
			trackingAnomaly |= TrackingAnomaly.LeftHandJump;
		}
		if (trackingPoint3.Magnitude > 1.25)
		{
			trackingAnomaly |= TrackingAnomaly.RightHandJump;
		}
		TrackingPoint trackingPoint4 = trackingPoint.A(num);
		TrackingPoint trackingPoint5 = trackingPoint2.A(num);
		TrackingPoint trackingPoint6 = trackingPoint3.A(num);
		if (trackingPoint4.Magnitude > 15.0)
		{
			trackingAnomaly |= TrackingAnomaly.HeadSpeed;
		}
		if (trackingPoint5.Magnitude > 35.0)
		{
			trackingAnomaly |= TrackingAnomaly.LeftHandSpeed;
		}
		if (trackingPoint6.Magnitude > 35.0)
		{
			trackingAnomaly |= TrackingAnomaly.RightHandSpeed;
		}
		if (_hasVelocity)
		{
			if (trackingPoint4.Magnitude > 5.0 && (trackingPoint4 - _previousHeadVelocity).A(num).Magnitude > 180.0)
			{
				trackingAnomaly |= TrackingAnomaly.HeadAcceleration;
			}
			if (trackingPoint5.Magnitude > 11.666666666666666 && (trackingPoint5 - _previousLeftVelocity).A(num).Magnitude > 450.0)
			{
				trackingAnomaly |= TrackingAnomaly.LeftHandAcceleration;
			}
			if (trackingPoint6.Magnitude > 11.666666666666666 && (trackingPoint6 - _previousRightVelocity).A(num).Magnitude > 450.0)
			{
				trackingAnomaly |= TrackingAnomaly.RightHandAcceleration;
			}
		}
		_previous = P_0;
		_previousHeadVelocity = trackingPoint4;
		_previousLeftVelocity = trackingPoint5;
		_previousRightVelocity = trackingPoint6;
		_hasVelocity = true;
		return trackingAnomaly;
	}

	internal static int A(TrackingAnomaly P_0)
	{
		if (P_0 == TrackingAnomaly.None || P_0 == TrackingAnomaly.InvalidSample)
		{
			return 0;
		}
		int num = 0;
		if ((P_0 & (TrackingAnomaly.LeftReach | TrackingAnomaly.RightReach)) != 0)
		{
			num++;
		}
		if ((P_0 & (TrackingAnomaly.HeadJump | TrackingAnomaly.LeftHandJump | TrackingAnomaly.RightHandJump)) != 0)
		{
			num += 3;
		}
		if ((P_0 & (TrackingAnomaly.HeadSpeed | TrackingAnomaly.LeftHandSpeed | TrackingAnomaly.RightHandSpeed)) != 0)
		{
			num += 2;
		}
		if ((P_0 & (TrackingAnomaly.HeadAcceleration | TrackingAnomaly.LeftHandAcceleration | TrackingAnomaly.RightHandAcceleration)) != 0)
		{
			num++;
		}
		return num;
	}
}
#pragma endregion

#pragma region GorillaGuard.Security/TrackingEvidenceWindow.cs
﻿using System;

namespace GorillaGuard.Security;

internal sealed class TrackingEvidenceWindow
{
	private double _lastTime = double.NaN;

	internal double Score { get; private set; }

	internal void A()
	{
		Score = 0.0;
		_lastTime = double.NaN;
	}

	internal double A(double P_0, int P_1)
	{
		if (!double.IsFinite(P_0))
		{
			A();
			return Score;
		}
		double num = P_0 - _lastTime;
		_lastTime = P_0;
		if (!double.IsFinite(num) || num <= 0.0 || num > 0.5)
		{
			Score = 0.0;
			return Score;
		}
		Score = Math.Max(0.0, Math.Min(20.0, Score + num * (double)((P_1 > 0) ? (P_1 * 20) : (-5))));
		return Score;
	}
}
#pragma endregion

#pragma region GorillaGuard.Security/TrackingPoint.cs
﻿using System;

namespace GorillaGuard.Security;

internal readonly struct TrackingPoint
{
	internal double X { get; }

	internal double Y { get; }

	internal double Z { get; }

	internal bool IsFinite
	{
		get
		{
			if (double.IsFinite(X) && double.IsFinite(Y))
			{
				return double.IsFinite(Z);
			}
			return false;
		}
	}

	internal double Magnitude => Math.Sqrt(X * X + Y * Y + Z * Z);

	internal TrackingPoint(double x, double y, double z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	public static TrackingPoint operator -(TrackingPoint left, TrackingPoint right)
	{
		return new TrackingPoint(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
	}

	internal TrackingPoint A(double P_0)
	{
		return new TrackingPoint(X / P_0, Y / P_0, Z / P_0);
	}
}
#pragma endregion

#pragma region GorillaGuard.Security/TrackingPoseSample.cs
﻿namespace GorillaGuard.Security;

internal readonly struct TrackingPoseSample
{
	internal double Time { get; }

	internal TrackingPoint Body { get; }

	internal TrackingPoint Head { get; }

	internal TrackingPoint LeftHand { get; }

	internal TrackingPoint RightHand { get; }

	internal bool IsFinite
	{
		get
		{
			if (double.IsFinite(Time) && Body.IsFinite && Head.IsFinite && LeftHand.IsFinite)
			{
				return RightHand.IsFinite;
			}
			return false;
		}
	}

	internal TrackingPoseSample(double time, TrackingPoint body, TrackingPoint head, TrackingPoint leftHand, TrackingPoint rightHand)
	{
		Time = time;
		Body = body;
		Head = head;
		LeftHand = leftHand;
		RightHand = rightHand;
	}
}
#pragma endregion

#pragma region GorillaGuard.Security/ValueDriftWindow.cs
﻿using System.Collections.Generic;

namespace GorillaGuard.Security;

internal sealed class ValueDriftWindow
{
	private readonly Queue<double> observations = new Queue<double>(3);

	private double last;

	internal bool A(double P_0)
	{
		if (P_0 < last)
		{
			observations.Clear();
		}
		last = P_0;
		while (observations.Count > 0 && P_0 - observations.Peek() > 10.0)
		{
			observations.Dequeue();
		}
		if (observations.Count == 3)
		{
			observations.Dequeue();
		}
		observations.Enqueue(P_0);
		return observations.Count == 3;
	}
}
#pragma endregion

#pragma region GorillaGuard.Security/ValueProtection.cs
﻿using System;

namespace GorillaGuard.Security;

internal static class ValueProtection
{
	internal static bool A(float P_0, float P_1)
	{
		if (!float.IsNaN(P_0) && !float.IsInfinity(P_0) && !float.IsNaN(P_1) && !float.IsInfinity(P_1))
		{
			return Math.Abs(P_0 - P_1) <= 0.0001f;
		}
		return false;
	}

	internal static bool A(float P_0, Action<float> P_1, Func<float> P_2, out float P_3)
	{
		P_1(P_0);
		P_3 = P_2();
		return A(P_3, P_0);
	}
}
#pragma endregion

#pragma region GorillaGuard.Security/WarningGate.cs
﻿using System;
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
﻿using System;
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
			throw new ArgumentOutOfRangeException("capacity");
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

#pragma region GorillaGuard/NativeLoadNotifications.cs
﻿using System;
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
		callback = A;
		int num = LdrRegisterDllNotification(0u, callback, IntPtr.Zero, out cookie);
		if (num != 0)
		{
			throw new InvalidOperationException("DLL notifications unavailable: NTSTATUS=" + num.ToString("X8"));
		}
	}

	private void A(uint P_0, IntPtr P_1, IntPtr P_2)
	{
		if (P_0 == 1 || P_0 == 2)
		{
			Interlocked.Exchange(ref pending, 1);
		}
	}

	internal bool A()
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
#pragma endregion

#pragma region GorillaGuard/Plugin.cs
﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using BepInEx;
using BepInEx.Bootstrap;
using GorillaGuard.Security;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

namespace GorillaGuard;

[BepInPlugin("com.local.gorillaguard", "Gorilla Guard", "1.12.31")]
[DefaultExecutionOrder(int.MaxValue)]
public sealed class Plugin : BaseUnityPlugin
{
	private sealed class MemberTarget
	{
		public Object Target { get; }

		public string MemberName { get; }

		public MemberInfo Member { get; }

		public MemberInfo StiltMember { get; }

		public MemberTarget(Object target, string memberName)
		{
			Target = target;
			MemberName = memberName;
			Member = A(((object)target).GetType(), memberName);
			StiltMember = A(((object)target).GetType(), "maxStiltTagDistance");
		}
	}

	private sealed class LauncherSession
	{
		public string PipeName { get; }

		public string Nonce { get; }

		public int LauncherPid { get; }

		public LauncherSession(string pipeName, string nonce, int launcherPid)
		{
			PipeName = pipeName;
			Nonce = nonce;
			LauncherPid = launcherPid;
		}
	}

	private sealed class LauncherAlert
	{
		public string Category { get; }

		public string Details { get; }

		public LauncherAlert(string category, string details)
		{
			Category = category;
			Details = details;
		}
	}

	private sealed class LauncherNotice
	{
		public string Category { get; }

		public string Details { get; }

		public LauncherNotice(string category, string details)
		{
			Category = category;
			Details = details;
		}
	}

	private sealed class NativeModuleObservation
	{
		public string Name { get; }

		public string Path { get; }

		public string Error { get; }

		public NativeModuleObservation(string name, string path, string error)
		{
			Name = name;
			Path = path;
			Error = error;
		}
	}

	private sealed class IntegrityObservation
	{
		public ReleaseVerificationResult Result { get; }

		public string RestartMarker { get; }

		public string RestoredTargets { get; }

		public string RestorationMessage { get; }

		public IntegrityObservation(ReleaseVerificationResult result, string restartMarker, string restoredTargets, string restorationMessage)
		{
			Result = result;
			RestartMarker = restartMarker;
			RestoredTargets = restoredTargets;
			RestorationMessage = restorationMessage;
		}
	}

	private readonly List<InputDevice> headsetDevices = new List<InputDevice>();

	private string headsetStatus = "WAITING FOR XR";

	private string headsetRuntime = "Unknown";

	private string headsetRefresh = "Unknown";

	private float nextHeadsetAudit;

	private const float PlayspaceRadius = 0.5f;

	private const float PlayspaceChaseSpeed = 35f;

	private const float PlayspaceSnapThreshold = 10f;

	private MonoBehaviour playspace;

	private Type playspaceType;

	private FieldInfo playspaceHead;

	private readonly MemberInfo[] playspaceMembers = new MemberInfo[5];

	private static readonly string[] PlayspaceFields = new string[5]
	{
		"_sphereRadius",
		"_defaultChaseSpeed",
		"_snapToThreshold",
		"_sqrSphereRadius",
		"_sqrSnapToThreshold"
	};

	private static readonly float[] PlayspaceValues = new float[5] { 0.5f, 35f, 10f, 0.25f, 100f };

	private string playspaceStatus = "Waiting for local rig";

	private float nextPlayspaceStatusAt;

	public const string PluginGuid = "com.local.gorillaguard";

	public const string PluginName = "Gorilla Guard";

	public const string PluginVersion = "1.12.31";

	private const float MaxTagDistance = 1.2f;

	private const float MaxStiltTagDistance = 3.2f;

	private const float MaxArmLength = 1.5f;

	private const float LowerSlip = 0.55f;

	private const float UpperSlip = 1f;

	private const float AuditIntervalSeconds = 20f;

	private const float IntegrityIntervalSeconds = 5f;

	private const float FrameRateStartupDelaySeconds = 3f;

	private const float FrameRateAuditIntervalSeconds = 0.5f;

	private const string GuardRuntimeFileName = "GorillaGuard.dll";

	private const string GuardPreloaderFileName = "GorillaGuard.Preloader.dll";

	private const double LauncherGraceSeconds = 45.0;

	private const double LauncherTimeoutSeconds = 15.0;

	private const string LowerSlipPath = "Environment Objects/LocalObjects_Prefab/Forest/Terrain/pitgeo/pit lower slippery wall";

	private const string UpperSlipPath = "Environment Objects/LocalObjects_Prefab/Forest/Terrain/pitgeo/pit upper slippery wall";

	private readonly Dictionary<string, float> lastDriftLog = new Dictionary<string, float>(StringComparer.Ordinal);

	private readonly HashSet<string> knownAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly HashSet<string> knownModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly HashSet<string> startupCodeFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly HashSet<string> observedNewCodeFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly HashSet<string> reportedUnapprovedPlugins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly HashSet<string> reportedHarmonyPatches = new HashSet<string>(StringComparer.Ordinal);

	private readonly Dictionary<string, int> quarantineAttempts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, float> quarantineRetryAt = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

	private readonly List<MemberTarget> tagDistanceTargets = new List<MemberTarget>();

	private readonly List<FileSystemWatcher> codeFileWatchers = new List<FileSystemWatcher>();

	private readonly Queue<string> recentBlocks = new Queue<string>();

	private readonly WorkQueue<string> pendingCodeFiles = new WorkQueue<string>(1024);

	private readonly WorkQueue<NativeModuleObservation> pendingNativeModules = new WorkQueue<NativeModuleObservation>(1024);

	private readonly WorkQueue<string> pendingGuardLogLines = new WorkQueue<string>(1024);

	private readonly WorkQueue<IntegrityObservation> pendingIntegrityResults = new WorkQueue<IntegrityObservation>(1024);

	private readonly WorkQueue<LauncherAlert> pendingLauncherAlerts = new WorkQueue<LauncherAlert>(1024);

	private readonly WorkQueue<LauncherAlert> pendingLauncherBlocks = new WorkQueue<LauncherAlert>(1024);

	private readonly WorkQueue<LauncherNotice> pendingLauncherNotices = new WorkQueue<LauncherNotice>(1024);

	private static readonly ConcurrentDictionary<string, MemberInfo> FloatMemberCache = new ConcurrentDictionary<string, MemberInfo>(StringComparer.Ordinal);

	private Type playerType;

	private Object player;

	private GameObject lowerSlipObject;

	private GameObject upperSlipObject;

	private Component lowerSlipComponent;

	private Component upperSlipComponent;

	private MemberInfo armReachMember;

	private MemberInfo lowerSlipMember;

	private MemberInfo upperSlipMember;

	private float nextObjectDiscoveryAt;

	private float nextComponentScanAt;

	private float nextOverlayRefreshAt;

	private string overlayTitle = "GORILLA GUARD";

	private string overlayBody = "Starting guard...";

	private float nextAuditAt;

	private float nextTargetDiscoveryAt;

	private float nextIntegrityCheckAt;

	private float nextFrameRateAuditAt;

	private float configuredRefreshRate;

	private float enforcedFrameRate = 90f;

	private float smoothedFrameRate;

	private int frameRateSampleCount;

	private bool frameRateVerified;

	private string frameRateStatus = "WAITING FOR XR";

	private int correctedValues;

	private int unapprovedPlugins;

	private int newAssemblies;

	private int newNativeModules;

	private int blockedComponents;

	private int disabledPlugins;

	private int quarantinedFiles;

	private int blockedEvents;

	private string recentBlocksText = "None this session";

	private GUIStyle panelStyle;

	private GUIStyle titleStyle;

	private GUIStyle bodyStyle;

	private string guardLogPath;

	private readonly object assemblyLock = new object();

	private readonly object fileGuardLock = new object();

	private readonly object moduleLock = new object();

	private volatile bool componentScanPending = true;

	private volatile bool fileRescanPending;

	private int nativeAuditRunning;

	private int guardLogWriterRunning;

	private string guardLogWriteError;

	private int integrityCheckRunning;

	private volatile bool integrityCheckRequested;

	private bool integrityFailure = true;

	private string integrityStatus = "VERIFYING SIGNED RELEASE";

	private string lastIntegrityFingerprint;

	private byte[] trustedRuntimeBytes;

	private byte[] trustedPreloaderBytes;

	private byte[] trustedManifestBytes;

	private byte[] trustedSignatureBytes;

	private volatile bool launcherHeartbeatRunning;

	private volatile bool launcherStopRequested;

	private long lastLauncherHeartbeatUtcTicks;

	private float launcherDeadline;

	private string launcherStatus = "CONNECTING";

	private bool launcherFailureHandled;

	private string lastLauncherHeartbeatError;

	private volatile bool harmonyAuditPending = true;

	private bool harmonyAuditFailureReported;

	private bool modRemovalIncomplete;

	private bool moduleMonitoringIncomplete;

	private bool modRestartRequired;

	private bool modWasDisabled;

	private readonly Dictionary<Assembly, Type[]> componentTypes = new Dictionary<Assembly, Type[]>();

	private readonly Queue<Type> pendingComponentTypes = new Queue<Type>();

	private Object[] pendingComponentObjects;

	private int pendingComponentIndex;

	private int auditPhase;

	private long auditWorkTicks;

	private long auditMaxPhaseTicks;

	private long maxComponentSliceTicks;

	private int auditCorrectionsBefore;

	private Type cachedHarmonyType;

	private Queue<MethodBase> pendingHarmonyMethods;

	private volatile bool harmonyDiscoveryRequested = true;

	private volatile bool componentDiscoveryRequested = true;

	private bool harmonyRescanRequested;

	private long maxHarmonyMethodTicks;

	private string integrityDetails = "Verifying signed release files";

	private NativeLoadNotifications nativeLoadNotifications;

	private readonly ConditionalWeakTable<object, Dictionary<MemberInfo, ValueDriftWindow>> valueDrift = new ConditionalWeakTable<object, Dictionary<MemberInfo, ValueDriftWindow>>();

	private bool valueCorrectionFailed;

	private bool repeatedValueChanges;

	private readonly WarningGate warningGate = new WarningGate();

	private readonly TrackingAnomalyWindow trackingAnomalies = new TrackingAnomalyWindow();

	private Object trackingBoundPlayer;

	private FieldInfo trackingHeadColliderField;

	private FieldInfo trackingBodyColliderField;

	private MethodInfo trackingControllerTransformMethod;

	private float trackingGraceUntil;

	private float nextTrackingWarningAt;

	private double trackingEvidence;

	private readonly TrackingEvidenceWindow trackingEvidenceWindow = new TrackingEvidenceWindow();

	private float nextTrackingSampleAt;

	private int trackingWarnings;

	private string trackingStatus = "Waiting for local player";

	private bool trackingMonitoringIncomplete = true;

	private void A()
	{

		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (realtimeSinceStartup < nextHeadsetAudit)
		{
			return;
		}
		nextHeadsetAudit = realtimeSinceStartup + 0.5f;
		try
		{
			string text = a(a(a(a(D("GorillaTagger"), "Instance"), "activeXRDisplay"), "subsystemDescriptor"), "id") as string;
			headsetRuntime = (string.IsNullOrWhiteSpace(text) ? "Unknown" : A(text, 48));
			headsetRefresh = (A(out var num) ? (num.ToString("0.0") + " Hz") : "Unknown");
			headsetDevices.Clear();
			InputDevices.GetDevicesWithCharacteristics((InputDeviceCharacteristics)1, headsetDevices);
			List<string> list = new List<string>();
			foreach (InputDevice headsetDevice in headsetDevices)
			{
				InputDevice current = headsetDevice;
				if (((InputDevice)(ref current)).isValid)
				{
					string text2 = (string.IsNullOrWhiteSpace(((InputDevice)(ref current)).name) ? "Unknown" : ((InputDevice)(ref current)).name);
					list.Add(A(text2, 60));
				}
			}
			headsetStatus = ((list.Count == 0) ? "Unknown" : string.Join(", ", list));
		}
		catch (Exception)
		{
			headsetStatus = "Unknown";
			headsetRuntime = "Unknown";
			headsetRefresh = "Unknown";
		}
	}

	private void a()
	{

		if ((Object)(object)playspace != (Object)null)
		{
			return;
		}
		Object obj = player;
		Component val = (Component)(object)((obj is Component) ? obj : null);
		if (val == null)
		{
			return;
		}
		playspaceType = playspaceType ?? D("GorillaLocomotion.Playspace");
		if (playspaceType == null)
		{
			playspaceStatus = "Unavailable: game component missing";
			return;
		}
		playspaceHead = playspaceHead ?? playspaceType.GetField("_localGorillaHead", BindingFlags.Instance | BindingFlags.NonPublic);
		if (playspaceHead == null)
		{
			playspaceStatus = "Unavailable: rig binding changed";
			return;
		}
		Object[] array = Resources.FindObjectsOfTypeAll(playspaceType);
		foreach (Object obj2 in array)
		{
			MonoBehaviour val2 = (MonoBehaviour)(object)((obj2 is MonoBehaviour) ? obj2 : null);
			if ((Object)(object)val2 == (Object)null)
			{
				continue;
			}
			Scene scene = ((Component)val2).gameObject.scene;
			if (!((Scene)(ref scene)).IsValid() || (Object) != (Object)(object)val.gameObject)
			{
				continue;
			}
			for (int j = 0; j < PlayspaceFields.Length; j++)
			{
				playspaceMembers[j] = A(playspaceType, PlayspaceFields[j]);
				if (playspaceMembers[j] == null)
				{
					playspaceStatus = "Unavailable: game fields changed";
					return;
				}
			}
			playspace = val2;
			playspaceStatus = "Bound to local rig; checking defaults";
			return;
		}
		playspaceStatus = "Unavailable: local playspace not found";
	}

	private void A(bool P_0)
	{
		if ((Object)(object)playspace == (Object)null)
		{
			return;
		}
		try
		{
			Object obj = player;
			Component val = (Component)(object)((obj is Component) ? obj : null);
			if (val == null || (Object) != (Object)(object)val.gameObject)
			{
				playspace = null;
				playspaceStatus = "Unavailable: local rig reference changed";
				return;
			}
			if (!((Component)playspace).gameObject.activeInHierarchy)
			{
				playspaceStatus = "Inactive rig — not verified";
				return;
			}
			if (!((Behaviour)playspace).enabled)
			{
				((Behaviour)playspace).enabled = true;
				if (P_0 && c("playspace-disabled"))
				{
					A("PLAYSPACE", "game limiter re-enabled", "The existing local Playspace component was re-enabled; no transforms were rewritten.");
				}
			}
			for (int i = 0; i < playspaceMembers.Length; i++)
			{
				A(playspace, playspaceMembers[i], PlayspaceValues[i], P_0, PlayspaceFields[i]);
			}
			if (!(Time.realtimeSinceStartup < nextPlayspaceStatusAt))
			{
				nextPlayspaceStatusAt = Time.realtimeSinceStartup + 0.5f;
				bool flag = ((Behaviour)playspace).enabled;
				for (int j = 0; j < playspaceMembers.Length; j++)
				{
					float num = A(playspace, playspaceMembers[j]);
					flag &= !float.IsNaN(num) && !float.IsInfinity(num) && Math.Abs(num - PlayspaceValues[j]) <= 0.0001f;
				}
				playspaceStatus = (flag ? "Verified defaults: radius 0.5 / chase 35 / snap 10" : "Correction incomplete");
			}
		}
		catch (Exception ex)
		{
			playspaceStatus = "Correction incomplete";
			if (c("playspace-error"))
			{
				B("WARNING", "Playspace check failed: " + ex.GetBaseException().Message);
			}
		}
	}

	private void Awake()
	{
		guardLogPath = Path.Combine(Paths.PluginPath, "GorillaGuard.log");
		A(B(false), true);
		D();
		F();
		try
		{
			nativeLoadNotifications = new NativeLoadNotifications();
		}
		catch (Exception ex)
		{
			B("WARNING", "DLL load notifications unavailable; periodic audits remain active: " + ex.Message);
		}
		g();
		AppDomain.CurrentDomain.AssemblyLoad += A;
		nextAuditAt = Time.realtimeSinceStartup + 1f;
		nextTargetDiscoveryAt = Time.realtimeSinceStartup;
		nextIntegrityCheckAt = Time.realtimeSinceStartup + 5f;
		nextFrameRateAuditAt = Time.realtimeSinceStartup;
		launcherDeadline = Time.realtimeSinceStartup + (integrityFailure ? 5f : 45f);
		n();
		B("INFO", "=== New runtime session === Gorilla Guard 1.12.31 active. Exclusive BepInEx mode and continuous value enforcement are enabled; there are no configuration bypass switches.");
	}

	private void OnDestroy()
	{
		nativeLoadNotifications?.Dispose();
		AppDomain.CurrentDomain.AssemblyLoad -= A;
		launcherStopRequested = true;
		foreach (FileSystemWatcher codeFileWatcher in codeFileWatchers)
		{
			codeFileWatcher.Dispose();
		}
		codeFileWatchers.Clear();
		B("INFO", "Runtime guard shutting down.");
	}

	private void Update()
	{
		if (nativeLoadNotifications != null && nativeLoadNotifications.A())
		{
			nextAuditAt = Math.Min(nextAuditAt, Time.realtimeSinceStartup);
		}
		if (pendingCodeFiles.Dropped > 0 || pendingLauncherAlerts.Dropped > 0 || pendingIntegrityResults.Dropped > 0)
		{
			if (!launcherFailureHandled)
			{
				launcherFailureHandled = true;
				integrityFailure = true;
				b("MONITORING OVERFLOW", "Security event capacity exceeded; restarting is required.");
				Application.Quit();
			}
			return;
		}
		if (pendingGuardLogLines.Dropped > 0 || pendingNativeModules.Dropped > 0 || pendingLauncherNotices.Dropped > 0 || pendingLauncherBlocks.Dropped > 0)
		{
			moduleMonitoringIncomplete = true;
		}
		J();
		A();
		A(true, false);
		l();
		P();
		p();
		N();
		G();
		I();
		if (harmonyAuditPending)
		{
			harmonyAuditPending = false;
			E();
		}
		if (fileRescanPending)
		{
			fileRescanPending = false;
			h();
		}
		if (componentScanPending && Time.realtimeSinceStartup >= nextComponentScanAt)
		{
			componentScanPending = false;
			nextComponentScanAt = Time.realtimeSinceStartup + 1f;
			j();
		}
		B();
		a(true);
		K();
		e();
		C();
		Q();
		if (integrityCheckRequested || Time.realtimeSinceStartup >= nextIntegrityCheckAt)
		{
			integrityCheckRequested = false;
			nextIntegrityCheckAt = Time.realtimeSinceStartup + 5f;
			M();
		}
		if (Time.realtimeSinceStartup >= nextAuditAt)
		{
			b();
			nextAuditAt = Time.realtimeSinceStartup + 20f;
		}
	}

	private void FixedUpdate()
	{
		a(true);
	}

	private void LateUpdate()
	{
		a(true);
		R();
	}

	private void B()
	{
		if (!(Time.realtimeSinceStartup < nextObjectDiscoveryAt))
		{
			nextObjectDiscoveryAt = Time.realtimeSinceStartup + 1f;
			if (player == (Object)null)
			{
				playerType = playerType ?? D("GorillaLocomotion.GTPlayer");
				ref Object reference = ref player;
				object obj = a(playerType, "Instance");
				reference = (Object)((obj is Object) ? obj : null);
				armReachMember = ((player == (Object)null) ? null : A(((object)player).GetType(), "MaxArmLengthAllowed"));
			}
			a();
			if ((Object)(object)lowerSlipComponent == (Object)null)
			{
				lowerSlipObject = GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest/Terrain/pitgeo/pit lower slippery wall");
				lowerSlipComponent = A(lowerSlipObject, "slidePercentageOverride");
				lowerSlipMember = (((Object)(object)lowerSlipComponent == (Object)null) ? null : A(((object)lowerSlipComponent).GetType(), "slidePercentageOverride"));
			}
			if ((Object)(object)upperSlipComponent == (Object)null)
			{
				upperSlipObject = GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest/Terrain/pitgeo/pit upper slippery wall");
				upperSlipComponent = A(upperSlipObject, "slidePercentageOverride");
				upperSlipMember = (((Object)(object)upperSlipComponent == (Object)null) ? null : A(((object)upperSlipComponent).GetType(), "slidePercentageOverride"));
			}
			if (Time.realtimeSinceStartup >= nextTargetDiscoveryAt)
			{
				i();
				nextTargetDiscoveryAt = Time.realtimeSinceStartup + 20f;
			}
		}
	}

	private void a(bool P_0)
	{
		A(P_0);
		if (player != (Object)null)
		{
			A(player, armReachMember, 1.5f, P_0, "maximum arm reach");
		}
		if ((Object)(object)lowerSlipComponent != (Object)null)
		{
			A(lowerSlipComponent, lowerSlipMember, 0.55f, P_0, "lower slippery wall");
		}
		if ((Object)(object)upperSlipComponent != (Object)null)
		{
			A(upperSlipComponent, upperSlipMember, 1f, P_0, "upper slippery wall");
		}
		for (int num = tagDistanceTargets.Count - 1; num >= 0; num--)
		{
			MemberTarget memberTarget = tagDistanceTargets[num];
			if (memberTarget.Target == (Object)null)
			{
				tagDistanceTargets.RemoveAt(num);
			}
			else
			{
				A(memberTarget.Target, memberTarget.Member, 1.2f, P_0);
				A(memberTarget.Target, memberTarget.StiltMember, 3.2f, P_0, "maximum stilt tag distance");
			}
		}
	}

	private void b()
	{
		if (auditPhase == 0)
		{
			auditCorrectionsBefore = correctedValues;
			auditWorkTicks = (auditMaxPhaseTicks = 0L);
			auditPhase = 1;
		}
	}

	private void C()
	{
		if (auditPhase != 0)
		{
			long timestamp = Stopwatch.GetTimestamp();
			int num = auditPhase;
			d("AUDIT_STAGE_BEGIN phase=" + num);
			switch (auditPhase++)
			{
			case 1:
				c();
				break;
			case 2:
				j();
				break;
			case 3:
				E();
				break;
			case 4:
				f();
				break;
			case 5:
				A(true, true);
				break;
			default:
				auditPhase = 0;
				d(string.Format("Integrity audit complete: mainWorkMs={0:0.00}; maxStageMs={1:0.00}; maxComponentSliceMs={2:0.00}; maxHarmonyMethodMs={3:0.00}; componentScanPending={4}; harmonyPending={5}; corrected={6}; unapprovedPlugins={7}; blockedComponents={8}; protected={9}", 1000.0 * (double)auditWorkTicks / (double)Stopwatch.Frequency, 1000.0 * (double)auditMaxPhaseTicks / (double)Stopwatch.Frequency, 1000.0 * (double)maxComponentSliceTicks / (double)Stopwatch.Frequency, 1000.0 * (double)maxHarmonyMethodTicks / (double)Stopwatch.Frequency, pendingComponentTypes.Count > 0 || pendingComponentObjects != null, pendingHarmonyMethods?.Count ?? 0, correctedValues - auditCorrectionsBefore, unapprovedPlugins, blockedComponents, k()));
				maxComponentSliceTicks = 0L;
				maxHarmonyMethodTicks = 0L;
				return;
			case 6:
				break;
			}
			long num2 = Stopwatch.GetTimestamp() - timestamp;
			auditWorkTicks += num2;
			auditMaxPhaseTicks = Math.Max(auditMaxPhaseTicks, num2);
			d(string.Format("AUDIT_STAGE_END phase={0}; workMs={1:0.00}", num, 1000.0 * (double)num2 / (double)Stopwatch.Frequency));
		}
	}

	private void c()
	{
		int num = 0;
		foreach (KeyValuePair<string, PluginInfo> pluginInfo in Chainloader.PluginInfos)
		{
			string text = pluginInfo.Key ?? string.Empty;
			if ((Object)(object)pluginInfo.Value?.Instance == (Object)(object)this)
			{
				continue;
			}
			num++;
			modRestartRequired = true;
			BaseUnityPlugin baseUnityPlugin = pluginInfo.Value?.Instance;
			if ((Object)(object)baseUnityPlugin != (Object)null)
			{
				string text2 = pluginInfo.Value.Metadata?.Name ?? text;
				Assembly assembly = ((object)baseUnityPlugin).GetType().Assembly;
				bool flag = false;
				try
				{
					((Behaviour)baseUnityPlugin).enabled = false;
					((MonoBehaviour)baseUnityPlugin).StopAllCoroutines();
					((MonoBehaviour)baseUnityPlugin).CancelInvoke();
					Object.Destroy((Object)(object)baseUnityPlugin);
					modWasDisabled = true;
					modRestartRequired = true;
					flag = true;
				}
				catch (Exception ex)
				{
					modRemovalIncomplete = true;
					B("WARNING", "Plugin cleanup incomplete for " + text + ": " + ex.GetBaseException().Message);
				}
				if (flag)
				{
					disabledPlugins++;
				}
				A("PLUGIN", text2 + " (" + text + ")", "name=" + text2 + "; guid=" + text + "; assembly=" + assembly.GetName().Name + "; location=" + c(assembly) + "; action=" + (flag ? "component disabled and destruction requested" : "cleanup incomplete") + "; assembly remains loaded until restart");
				C("PLUGIN", "Unapproved BepInEx plugin " + (flag ? "disabled" : "cleanup incomplete") + ": name=" + text2 + "; guid=" + text + "; location=" + c(assembly));
				reportedUnapprovedPlugins.Add(text);
			}
			else if (reportedUnapprovedPlugins.Add(text))
			{
				modRemovalIncomplete = true;
				B("WARNING", "Unapproved BepInEx plugin present: " + text + " (" + pluginInfo.Value?.Metadata?.Name + ")");
			}
		}
		unapprovedPlugins = num;
	}

	private void D()
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			knownAssemblies.Add(C(assembly));
			string text = c(assembly);
			if (a(assembly, text))
			{
				string text2 = assembly.GetName().Name ?? "<unknown>";
				string text3 = "Pre-existing unapproved managed assembly detected: name=" + text2 + "; location=" + text;
				A("MANAGED ASSEMBLY", text2, text3);
				A(assembly, text, text3);
			}
		}
	}

	private void d()
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			string item = C(assembly);
			bool flag;
			lock (assemblyLock)
			{
				flag = knownAssemblies.Add(item);
			}
			if (flag)
			{
				A(assembly, "audit");
			}
		}
	}

	private void A(object P_0, AssemblyLoadEventArgs P_1)
	{
		harmonyDiscoveryRequested = true;
		componentDiscoveryRequested = true;
		Assembly loadedAssembly = P_1.LoadedAssembly;
		string item = C(loadedAssembly);
		bool flag;
		lock (assemblyLock)
		{
			flag = knownAssemblies.Add(item);
		}
		if (flag)
		{
			A(loadedAssembly, "AssemblyLoad event");
		}
		if (A(loadedAssembly))
		{
			componentScanPending = true;
		}
		if (string.Equals(loadedAssembly.GetName().Name, "0Harmony", StringComparison.OrdinalIgnoreCase) || loadedAssembly.GetType("HarmonyLib.Harmony", throwOnError: false) != null)
		{
			harmonyAuditPending = true;
		}
	}

	private bool A(Assembly P_0)
	{
		if (P_0 == typeof(Plugin).Assembly)
		{
			return false;
		}
		string text = c(P_0);
		if (B(text))
		{
			return false;
		}
		if (text.StartsWith("<", StringComparison.Ordinal))
		{
			return b(P_0);
		}
		string fullPath;
		try
		{
			fullPath = Path.GetFullPath(text);
		}
		catch
		{
			return true;
		}
		lock (fileGuardLock)
		{
			if (observedNewCodeFiles.Contains(fullPath))
			{
				return true;
			}
		}
		if (!A(fullPath, Paths.ManagedPath))
		{
			return !A(fullPath, Path.Combine(Paths.BepInExRootPath, "core"));
		}
		return false;
	}

	private void A(Assembly P_0, string P_1)
	{
		newAssemblies++;
		string text = c(P_0);
		string text2 = P_0.GetName().Name ?? "<unknown>";
		B("WARNING", "New managed assembly observed (" + P_1 + "): " + text2 + "; location=" + text);
		if (a(P_0, text))
		{
			A(P_0, text, "Unapproved managed assembly observed: name=" + text2 + "; location=" + text + "; source=" + P_1);
		}
	}

	private void A(Assembly P_0, string P_1, string P_2)
	{
		modRestartRequired = true;
		if (InjectionPolicy.A(a(P_0, P_1), P_1.StartsWith("<", StringComparison.Ordinal), P_0.IsDynamic))
		{
			componentScanPending = true;
			harmonyAuditPending = true;
			modRestartRequired = true;
			modRemovalIncomplete = true;
			B("WARNING", "UNAPPROVED MEMORY IMAGE: " + P_2 + "; action=disable components and remove patches; assembly unload requires restart");
			C("MEMORY IMAGE", P_2 + "; full unload requires restart; session kept running");
		}
		else
		{
			C("MANAGED ASSEMBLY", P_2);
		}
	}

	private static bool a(Assembly P_0, string P_1)
	{
		if (A(P_1))
		{
			return true;
		}
		if (P_1.StartsWith("<", StringComparison.Ordinal) && P_0 != typeof(Plugin).Assembly && !a(P_0))
		{
			return b(P_0);
		}
		return false;
	}

	private void E()
	{
		if (pendingHarmonyMethods != null && pendingHarmonyMethods.Count > 0)
		{
			harmonyRescanRequested = true;
		}
		else
		{
			if (cachedHarmonyType == null && !harmonyDiscoveryRequested)
			{
				return;
			}
			harmonyDiscoveryRequested = false;
			Type type = cachedHarmonyType;
			Assembly[] array = ((type == null) ? AppDomain.CurrentDomain.GetAssemblies() : Array.Empty<Assembly>());
			foreach (Assembly assembly in array)
			{
				try
				{
					type = assembly.GetType("HarmonyLib.Harmony", throwOnError: false);
					if (type != null)
					{
						break;
					}
				}
				catch
				{
				}
			}
			if (type == null)
			{
				return;
			}
			cachedHarmonyType = type;
			try
			{
				MethodInfo? method = type.GetMethod("GetAllPatchedMethods", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (method == null)
				{
					throw new MissingMethodException(type.FullName, "GetAllPatchedMethods");
				}
				if (!(method.Invoke(null, null) is IEnumerable source))
				{
					throw new InvalidOperationException("Harmony returned no inspectable patch collection.");
				}
				pendingHarmonyMethods = new Queue<MethodBase>(source.Cast<object>().OfType<MethodBase>());
			}
			catch (Exception ex)
			{
				if (!harmonyAuditFailureReported)
				{
					harmonyAuditFailureReported = true;
					modRemovalIncomplete = true;
					modRestartRequired = true;
					string text = "Harmony runtime was loaded but its patch table could not be inspected: " + ex.GetBaseException().Message;
					B("ERROR", text);
					C("PATCH AUDIT FAILED", text + "; removal could not be verified; session kept running");
				}
			}
		}
	}

	private void e()
	{
		if (pendingHarmonyMethods == null || pendingHarmonyMethods.Count == 0)
		{
			if (harmonyRescanRequested)
			{
				harmonyRescanRequested = false;
				harmonyAuditPending = true;
			}
			return;
		}
		long timestamp = Stopwatch.GetTimestamp();
		try
		{
			MethodBase methodBase = pendingHarmonyMethods.Dequeue();
			Type type = cachedHarmonyType;
			string text = methodBase.DeclaringType?.Assembly?.GetName()?.Name ?? "<unknown assembly>";
			string text2 = methodBase.DeclaringType?.FullName ?? "<global>";
			HashSet<string> hashSet;
			List<MethodInfo> list;
			HashSet<string> hashSet2 = A(type, methodBase, out hashSet, out list);
			string text3 = text + "|" + text2 + "|" + methodBase.Name + "|owners=" + string.Join(",", hashSet2.OrderBy<string, string>((string P_0) => P_0, StringComparer.Ordinal)) + "|patchers=" + string.Join(",", hashSet.OrderBy<string, string>((string P_0) => P_0, StringComparer.Ordinal));
			bool flag = reportedHarmonyPatches.Add(text3);
			if (A(text, text2, methodBase.Name, hashSet))
			{
				if (flag)
				{
					d("Trusted BepInEx runtime patch verified: method=" + text2 + "." + methodBase.Name + "; patchers=" + string.Join(",", hashSet));
				}
				return;
			}
			string text4;
			int num = A(type, methodBase, list, out text4);
			modRestartRequired = true;
			if (!string.IsNullOrEmpty(text4))
			{
				modRemovalIncomplete = true;
			}
			else if (num > 0)
			{
				modWasDisabled = true;
			}
			string text5 = (string.IsNullOrEmpty(text4) ? string.Format("unapproved patch methods removed={0}", num) : string.Format("patch removal incomplete; removed={0}; error={1}", num, text4));
			string text6 = "Harmony patch detected: assembly=" + text + "; method=" + text2 + "." + methodBase.Name + "; owners=" + string.Join(",", hashSet2) + "; patchers=" + string.Join(",", hashSet) + "; action=" + text5;
			if (flag || c("reapplied-patch:" + text3))
			{
				A("METHOD PATCH", text2 + "." + methodBase.Name, text6);
				C("METHOD PATCH", text6);
			}
		}
		catch (Exception ex)
		{
			modRemovalIncomplete = (modRestartRequired = true);
			d("Harmony inspection/removal incomplete: " + ex.GetBaseException().Message);
			C("PATCH AUDIT FAILED", ex.GetBaseException().Message);
		}
		finally
		{
			maxHarmonyMethodTicks = Math.Max(maxHarmonyMethodTicks, Stopwatch.GetTimestamp() - timestamp);
		}
	}

	private static HashSet<string> A(Type P_0, MethodBase P_1, out HashSet<string> P_2, out List<MethodInfo> P_3)
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
		P_2 = new HashSet<string>(StringComparer.Ordinal);
		P_3 = new List<MethodInfo>();
		MethodInfo? method = P_0.GetMethod("GetPatchInfo", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(MethodBase) }, null);
		if (method == null)
		{
			throw new MissingMethodException(P_0.FullName, "GetPatchInfo");
		}
		object obj = method.Invoke(null, new object[1] { P_1 });
		if (obj == null)
		{
			return hashSet;
		}
		if ((obj?.GetType().GetProperty("Owners", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))?.GetValue(obj, null) is IEnumerable enumerable)
		{
			foreach (object item in enumerable)
			{
				if (item is string text && !string.IsNullOrWhiteSpace(text))
				{
					hashSet.Add(text);
				}
			}
		}
		string[] array = new string[5]
		{
			"Prefixes",
			"Postfixes",
			"Transpilers",
			"Finalizers",
			"ILManipulators"
		};
		foreach (string text2 in array)
		{
			PropertyInfo propertyInfo = obj?.GetType().GetProperty(text2, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			FieldInfo fieldInfo = obj?.GetType().GetField(text2, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (propertyInfo == null && fieldInfo == null)
			{
				if (!(text2 == "ILManipulators"))
				{
					throw new MissingMemberException(obj.GetType().FullName, text2);
				}
				continue;
			}
			foreach (object item2 in (((propertyInfo != null) ? propertyInfo.GetValue(obj, null) : fieldInfo?.GetValue(obj)) as IEnumerable) ?? throw new InvalidOperationException("Patch collection could not be inspected: " + text2))
			{
				if (item2 != null)
				{
					if (!(item2.GetType().GetProperty("PatchMethod", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(item2, null) is MethodInfo methodInfo))
					{
						throw new InvalidOperationException("Patch method could not be inspected.");
					}
					P_3.Add(methodInfo);
					string text3 = methodInfo.DeclaringType?.Assembly?.GetName()?.Name ?? "<unknown assembly>";
					string text4 = methodInfo.DeclaringType?.FullName ?? "<global>";
					P_2.Add(text3 + "|" + text4);
				}
			}
		}
		return hashSet;
	}

	private static int A(Type P_0, MethodBase P_1, IEnumerable<MethodInfo> P_2, out string P_3)
	{
		int num = 0;
		P_3 = null;
		try
		{
			ConstructorInfo constructor = P_0.GetConstructor(new Type[1] { typeof(string) });
			MethodInfo method = P_0.GetMethod("Unpatch", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[2]
			{
				typeof(MethodBase),
				typeof(MethodInfo)
			}, null);
			if (constructor == null || method == null)
			{
				throw new MissingMethodException(P_0.FullName, "Unpatch(MethodBase, MethodInfo)");
			}
			object obj = constructor.Invoke(new object[1] { "com.local.gorillaguard.enforcement" });
			foreach (MethodInfo item in P_2.Distinct())
			{
				if (!A(P_1, item))
				{
					method.Invoke(obj, new object[2] { P_1, item });
					num++;
				}
			}
			A(P_0, P_1, out var _, out var source);
			if (source.Any((MethodInfo P_0) => !A(P_1, P_0)))
			{
				P_3 = "Unapproved patch methods remain after removal; restart required.";
			}
		}
		catch (Exception ex)
		{
			P_3 = A(ex.GetBaseException().Message, 256);
		}
		return num;
	}

	private static bool A(MethodInfo P_0)
	{
		string? obj = P_0.DeclaringType?.Assembly?.GetName()?.Name ?? string.Empty;
		string text = P_0.DeclaringType?.FullName ?? string.Empty;
		if (!obj.Equals("BepInEx.Preloader", StringComparison.Ordinal))
		{
			return false;
		}
		if (!text.Equals("BepInEx.Preloader.RuntimeFixes.ConsoleSetOutFix", StringComparison.Ordinal) && !text.Equals("BepInEx.Preloader.RuntimeFixes.HarmonyInteropFix", StringComparison.Ordinal))
		{
			return text.Equals("BepInEx.Preloader.RuntimeFixes.UnityPatches", StringComparison.Ordinal);
		}
		return true;
	}

	private static bool A(MethodBase P_0, MethodInfo P_1)
	{
		if (!A(P_1))
		{
			return false;
		}
		string item = P_1.DeclaringType.Assembly.GetName().Name + "|" + P_1.DeclaringType.FullName;
		return A(P_0.DeclaringType?.Assembly.GetName().Name ?? "", P_0.DeclaringType?.FullName ?? "", P_0.Name, new HashSet<string>(StringComparer.Ordinal) { item });
	}

	private static bool A(string P_0, string P_1, string P_2, HashSet<string> P_3)
	{
		if (!P_0.Equals("mscorlib", StringComparison.Ordinal) || P_3.Count == 0)
		{
			return false;
		}
		if (P_1.Equals("System.Console", StringComparison.Ordinal) && P_2.Equals("SetOut", StringComparison.Ordinal))
		{
			return P_3.SetEquals(new string[1] { "BepInEx.Preloader|BepInEx.Preloader.RuntimeFixes.ConsoleSetOutFix" });
		}
		if (P_1.Equals("System.Reflection.Assembly", StringComparison.Ordinal) && (P_2.Equals("LoadFile", StringComparison.Ordinal) || P_2.Equals("LoadFrom", StringComparison.Ordinal)))
		{
			return P_3.SetEquals(new string[1] { "BepInEx.Preloader|BepInEx.Preloader.RuntimeFixes.HarmonyInteropFix" });
		}
		if (P_1.Equals("System.Reflection.Assembly", StringComparison.Ordinal) && (P_2.Equals("get_Location", StringComparison.Ordinal) || P_2.Equals("get_CodeBase", StringComparison.Ordinal)))
		{
			return P_3.SetEquals(new string[1] { "BepInEx.Preloader|BepInEx.Preloader.RuntimeFixes.UnityPatches" });
		}
		return false;
	}

	private static bool a(Assembly P_0)
	{
		if (P_0 != null)
		{
			if (!(P_0 == typeof(Object).Assembly) && !(P_0 == typeof(GUI).Assembly) && !(P_0 == typeof(Rigidbody).Assembly))
			{
				return P_0 == typeof(InputDevices).Assembly;
			}
			return true;
		}
		return false;
	}

	private static bool A(string P_0)
	{
		if (string.IsNullOrWhiteSpace(P_0) || P_0.StartsWith("<", StringComparison.Ordinal))
		{
			return false;
		}
		try
		{
			string fullPath = Path.GetFullPath(P_0);
			if (A(fullPath, Paths.PluginPath) || A(fullPath, Path.Combine(Paths.BepInExRootPath, "patchers")))
			{
				return !B(fullPath);
			}
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			return A(fullPath, Path.Combine(folderPath, "Downloads")) || A(fullPath, Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)) || A(fullPath, Path.GetTempPath());
		}
		catch
		{
			return true;
		}
	}

	private void F()
	{
		try
		{
			foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
			{
				knownModules.Add(module.FileName ?? module.ModuleName);
			}
		}
		catch (Exception ex)
		{
			B("WARNING", "Could not snapshot native modules: " + ex.Message);
		}
	}

	private void f()
	{
		if (Interlocked.CompareExchange(ref nativeAuditRunning, 1, 0) != 0)
		{
			return;
		}
		ThreadPool.QueueUserWorkItem(delegate
		{
			try
			{
				foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
				{
					string text = module.FileName ?? module.ModuleName;
					bool flag;
					lock (moduleLock)
					{
						flag = knownModules.Add(text);
					}
					if (flag)
					{
						pendingNativeModules.A(new NativeModuleObservation(module.ModuleName, text, null));
					}
				}
			}
			catch (Exception ex)
			{
				pendingNativeModules.A(new NativeModuleObservation(null, null, ex.Message));
			}
			finally
			{
				Interlocked.Exchange(ref nativeAuditRunning, 0);
			}
		});
	}

	private void G()
	{
		int num = 0;
		NativeModuleObservation nativeModuleObservation;
		while (num < 4 && pendingNativeModules.A(out nativeModuleObservation))
		{
			num++;
			if (nativeModuleObservation.Error != null)
			{
				moduleMonitoringIncomplete = true;
				B("WARNING", "Native monitoring incomplete; restart required: " + nativeModuleObservation.Error);
			}
			else
			{
				newNativeModules++;
				B("WARNING", "New native module observed: " + nativeModuleObservation.Name + "; location=" + nativeModuleObservation.Path);
			}
		}
	}

	private void g()
	{
		A(Paths.GameRootPath, false);
		A(Paths.ManagedPath, true);
		A(Path.Combine(Paths.GameRootPath, "Gorilla Tag_Data", "Plugins"), true);
		A(Paths.PluginPath, true);
		A(Path.Combine(Paths.BepInExRootPath, "patchers"), true);
		A(Path.Combine(Paths.BepInExRootPath, "core"), true);
		H();
		B("INFO", string.Format("Code-file guard active: baselineFiles={0}, watchedLocations={1}", startupCodeFiles.Count, codeFileWatchers.Count));
	}

	private void H()
	{
		if (!Directory.Exists(Paths.PluginPath))
		{
			return;
		}
		int num = 0;
		try
		{
			string[] files = Directory.GetFiles(Paths.PluginPath, "*", SearchOption.AllDirectories);
			foreach (string text in files)
			{
				if (!C(text) || a(text))
				{
					continue;
				}
				string fullPath = Path.GetFullPath(text);
				lock (fileGuardLock)
				{
					startupCodeFiles.Remove(fullPath);
					if (!observedNewCodeFiles.Add(fullPath))
					{
						continue;
					}
				}
				pendingCodeFiles.A(fullPath);
				num++;
			}
		}
		catch (Exception ex)
		{
			B("ERROR", "Could not enumerate the exclusive plugin directory: " + ex.Message);
		}
		if (num > 0)
		{
			B("WARNING", string.Format("Exclusive mode queued {0} pre-existing non-guard executable file(s) for quarantine.", num));
		}
	}

	private static bool a(string P_0)
	{
		try
		{
			string fullPath = Path.GetFullPath(Path.Combine(Paths.PluginPath, "GorillaGuard.dll"));
			return Path.GetFullPath(P_0).Equals(fullPath, StringComparison.OrdinalIgnoreCase);
		}
		catch
		{
			return false;
		}
	}

	private static bool B(string P_0)
	{
		try
		{
			string fullPath = Path.GetFullPath(P_0);
			string fullPath2 = Path.GetFullPath(Path.Combine(Paths.PluginPath, "GorillaGuard.dll"));
			string fullPath3 = Path.GetFullPath(Path.Combine(Paths.BepInExRootPath, "patchers", "GorillaGuard.Preloader.dll"));
			return fullPath.Equals(fullPath2, StringComparison.OrdinalIgnoreCase) || fullPath.Equals(fullPath3, StringComparison.OrdinalIgnoreCase);
		}
		catch
		{
			return false;
		}
	}

	private void A(string P_0, bool P_1)
	{
		if (string.IsNullOrEmpty(P_0) || !Directory.Exists(P_0))
		{
			return;
		}
		SearchOption searchOption = (P_1 ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
		try
		{
			string[] files = Directory.GetFiles(P_0, "*", searchOption);
			foreach (string text in files)
			{
				if (C(text))
				{
					startupCodeFiles.Add(Path.GetFullPath(text));
				}
			}
			FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(P_0)
			{
				IncludeSubdirectories = P_1,
				NotifyFilter = (NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.CreationTime),
				Filter = "*",
				EnableRaisingEvents = false
			};
			fileSystemWatcher.Created += A;
			fileSystemWatcher.Renamed += A;
			fileSystemWatcher.Changed += a;
			fileSystemWatcher.Error += A;
			fileSystemWatcher.EnableRaisingEvents = true;
			codeFileWatchers.Add(fileSystemWatcher);
		}
		catch (Exception ex)
		{
			B("ERROR", "Could not guard code directory " + P_0 + ": " + ex.Message);
		}
	}

	private void A(object P_0, FileSystemEventArgs P_1)
	{
		E(P_1.FullPath);
		b(P_1.FullPath);
	}

	private void A(object P_0, RenamedEventArgs P_1)
	{
		E(P_1.FullPath);
		b(P_1.FullPath);
	}

	private void a(object P_0, FileSystemEventArgs P_1)
	{
		E(P_1.FullPath);
	}

	private void A(object P_0, ErrorEventArgs P_1)
	{
		fileRescanPending = true;
	}

	private void h()
	{
		foreach (FileSystemWatcher codeFileWatcher in codeFileWatchers)
		{
			try
			{
				SearchOption searchOption = (codeFileWatcher.IncludeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
				string[] files = Directory.GetFiles(codeFileWatcher.Path, "*", searchOption);
				foreach (string text in files)
				{
					b(text);
				}
			}
			catch (Exception ex)
			{
				B("ERROR", "Could not reconcile guarded directory " + codeFileWatcher.Path + ": " + ex.Message);
			}
		}
	}

	private void b(string P_0)
	{
		if (!C(P_0))
		{
			return;
		}
		string fullPath;
		try
		{
			fullPath = Path.GetFullPath(P_0);
		}
		catch
		{
			return;
		}
		lock (fileGuardLock)
		{
			if (startupCodeFiles.Contains(fullPath) || !observedNewCodeFiles.Add(fullPath))
			{
				return;
			}
		}
		pendingCodeFiles.A(fullPath);
		componentDiscoveryRequested = true;
	}

	private void I()
	{
		int num = 0;
		string text;
		while (num < 16 && pendingCodeFiles.A(out text))
		{
			num++;
			if (!File.Exists(text) || !A(text, Paths.GameRootPath))
			{
				continue;
			}
			lock (fileGuardLock)
			{
				if (startupCodeFiles.Contains(text))
				{
					continue;
				}
			}
			if (quarantineRetryAt.TryGetValue(text, out var value) && Time.realtimeSinceStartup < value)
			{
				pendingCodeFiles.A(text);
				break;
			}
			try
			{
				string text2 = Path.Combine(Paths.BepInExRootPath, "quarantine");
				Directory.CreateDirectory(text2);
				string text3 = Path.Combine(text2, DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff") + "-" + Path.GetFileName(text) + ".blocked");
				File.Move(text, text3);
				quarantineAttempts.Remove(text);
				quarantineRetryAt.Remove(text);
				quarantinedFiles++;
				A("FILE", Path.GetFileName(text), "source=" + text + "; action=quarantined; destination=" + text3);
			}
			catch (IOException ex)
			{
				int value2;
				int num2 = ((!quarantineAttempts.TryGetValue(text, out value2)) ? 1 : (value2 + 1));
				quarantineAttempts[text] = num2;
				if (num2 < 20)
				{
					quarantineRetryAt[text] = Time.realtimeSinceStartup + 0.25f;
					pendingCodeFiles.A(text);
				}
				else
				{
					B("ERROR", string.Format("Could not quarantine new code file after {0} attempts: {1}; {2}", num2, text, ex.Message));
					C("FILE CONTAINMENT", string.Format("Could not quarantine executable code after {0} attempts; continuing component/value enforcement: path={1}; error={2}", num2, text, ex.Message));
				}
			}
			catch (UnauthorizedAccessException ex2)
			{
				B("ERROR", "Could not quarantine new code file " + text + ": " + ex2.Message);
				C("FILE CONTAINMENT", "Access denied while quarantining executable code; continuing component/value enforcement: path=" + text + "; error=" + ex2.Message);
			}
		}
	}

	private static bool C(string P_0)
	{
		string extension = Path.GetExtension(P_0);
		if (!extension.Equals(".dll", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".exe", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".winmd", StringComparison.OrdinalIgnoreCase))
		{
			return extension.Equals(".so", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private void i()
	{
		tagDistanceTargets.Clear();
		object obj = a(D("GorillaTagger"), "Instance");
		Object val = (Object)((obj is Object) ? obj : null);
		if (val != (Object)null && A(((object)val).GetType(), "maxTagDistance") != null)
		{
			tagDistanceTargets.Add(new MemberTarget(val, "maxTagDistance"));
		}
	}

	private void J()
	{
		float unscaledDeltaTime = Time.unscaledDeltaTime;
		if (!(unscaledDeltaTime <= 0.0001f) && !(unscaledDeltaTime > 1f))
		{
			float num = Mathf.Clamp(1f / unscaledDeltaTime, 1f, 500f);
			if (frameRateSampleCount == 0)
			{
				smoothedFrameRate = num;
			}
			else
			{
				smoothedFrameRate = Mathf.Lerp(smoothedFrameRate, num, 0.05f);
			}
			frameRateSampleCount++;
		}
	}

	private void A(bool P_0, bool P_1 = false)
	{
		if (!P_1 && Time.realtimeSinceStartup < nextFrameRateAuditAt)
		{
			return;
		}
		nextFrameRateAuditAt = Time.realtimeSinceStartup + 0.5f;
		if (!A(out var num))
		{
			configuredRefreshRate = 0f;
			frameRateVerified = false;
			frameRateStatus = string.Format("WAITING FOR XR / measured {0:0.0} FPS", smoothedFrameRate);
			return;
		}
		configuredRefreshRate = num;
		bool flag = FrameRatePolicy.a(num);
		int targetFrameRate = Application.targetFrameRate;
		int num2 = FrameRatePolicy.A(targetFrameRate);
		if (Time.realtimeSinceStartup >= 3f && num2 != targetFrameRate)
		{
			Application.targetFrameRate = num2;
			if (P_0 && c("frame-rate-policy"))
			{
				correctedValues++;
				A("FRAME RATE", string.Format("unsupported target {0} -> {1}", targetFrameRate, num2), string.Format("previousTargetFps={0}; correctedTargetFps={1}; headset refresh and game physics untouched", targetFrameRate, num2));
			}
		}
		int targetFrameRate2 = Application.targetFrameRate;
		enforcedFrameRate = ((targetFrameRate2 > 0) ? ((float)targetFrameRate2) : num);
		frameRateVerified = flag;
		string text = ((targetFrameRate2 > 0) ? targetFrameRate2.ToString() : "game/XR controlled");
		frameRateStatus = string.Format("{0} {1:0.0}Hz / target {2} / measured {3:0.0} FPS", flag ? "VERIFIED" : "UNSUPPORTED XR", num, text, smoothedFrameRate);
	}

	private static bool A(out float P_0)
	{
		P_0 = 0f;
		try
		{
			object obj = a(a(D("GorillaTagger"), "Instance"), "activeXRDisplay");
			if (obj == null)
			{
				return false;
			}
			MethodInfo method = obj.GetType().GetMethod("TryGetDisplayRefreshRate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(float).MakeByRefType() }, null);
			if (method == null)
			{
				return false;
			}
			object[] array = new object[1] { 0f };
			object obj2 = method.Invoke(obj, array);
			bool flag = default(bool);
			int num;
			if (obj2 is bool)
			{
				flag = (bool)obj2;
				num = 1;
			}
			else
			{
				num = 0;
			}
			if (((uint)num & (flag ? 1u : 0u)) == 0 || !(array[0] is float num2) || !A(num2))
			{
				return false;
			}
			P_0 = num2;
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static bool A(float P_0)
	{
		if (!float.IsNaN(P_0) && !float.IsInfinity(P_0))
		{
			return P_0 > 0f;
		}
		return false;
	}

	private static string A(object P_0)
	{

		if (P_0 is float num)
		{
			return num.ToString("R");
		}
		if (P_0 is Vector2 val)
		{
			return string.Format("({0:R},{1:R})", val.x, val.y);
		}
		if (!(P_0 is Vector3 val2))
		{
			return P_0?.ToString() ?? "<null>";
		}
		return string.Format("({0:R},{1:R},{2:R})", val2.x, val2.y, val2.z);
	}

	private void j()
	{
		if (pendingComponentTypes.Count > 0 || pendingComponentObjects != null)
		{
			componentScanPending = true;
			return;
		}
		if (!componentDiscoveryRequested)
		{
			foreach (Type[] value2 in componentTypes.Values)
			{
				foreach (Type item in value2)
				{
					pendingComponentTypes.Enqueue(item);
				}
			}
			return;
		}
		componentDiscoveryRequested = false;
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			if (B(assembly))
			{
				continue;
			}
			if (!componentTypes.TryGetValue(assembly, out var value))
			{
				try
				{
					value = assembly.GetTypes();
				}
				catch (ReflectionTypeLoadException ex)
				{
					value = ex.Types;
				}
				catch (Exception ex2)
				{
					modRemovalIncomplete = (modRestartRequired = true);
					B("WARNING", "Could not inspect component types: " + ex2.Message);
					continue;
				}
				value = value.Where((Type P_0) => P_0 != null && !P_0.IsAbstract && !P_0.ContainsGenericParameters && typeof(MonoBehaviour).IsAssignableFrom(P_0)).ToArray();
				componentTypes[assembly] = value;
			}
			Type[] current = value;
			foreach (Type item2 in current)
			{
				pendingComponentTypes.Enqueue(item2);
			}
		}
	}

	private void K()
	{
		long timestamp = Stopwatch.GetTimestamp();
		if (pendingComponentObjects == null)
		{
			if (pendingComponentTypes.Count == 0)
			{
				return;
			}
			try
			{
				pendingComponentObjects = Resources.FindObjectsOfTypeAll(pendingComponentTypes.Dequeue());
			}
			catch (Exception ex)
			{
				modRemovalIncomplete = (modRestartRequired = true);
				B("WARNING", "Targeted component query failed: " + ex.Message);
				return;
			}
			pendingComponentIndex = 0;
		}
		int num = 0;
		while (pendingComponentIndex < pendingComponentObjects.Length && num++ < 32 && (double)(Stopwatch.GetTimestamp() - timestamp) * 1000.0 / (double)Stopwatch.Frequency < 1.0)
		{
			Object obj = pendingComponentObjects[pendingComponentIndex++];
			MonoBehaviour val = (MonoBehaviour)(object)((obj is MonoBehaviour) ? obj : null);
			if (!((Object)(object)val == (Object)null))
			{
				Type type = ((object)val).GetType();
				Assembly assembly = type.Assembly;
				try
				{
					((Behaviour)val).enabled = false;
					val.StopAllCoroutines();
					val.CancelInvoke();
					Object.Destroy((Object)(object)val);
					modWasDisabled = true;
					modRestartRequired = true;
				}
				catch (Exception ex2)
				{
					B("WARNING", "Component cleanup incomplete for " + type.FullName + ": " + ex2.GetBaseException().Message);
					modRemovalIncomplete = true;
					modRestartRequired = true;
					continue;
				}
				blockedComponents++;
				string text = c(assembly);
				A("COMPONENT", type.FullName ?? type.Name, "type=" + type.FullName + "; assembly=" + assembly.GetName().Name + "; location=" + text + "; action=disabled and destroyed");
				C("COMPONENT", "Unapproved Unity component disabled and destroyed: type=" + type.FullName + "; assembly=" + assembly.GetName().Name + "; location=" + text);
			}
		}
		if (pendingComponentIndex >= pendingComponentObjects.Length)
		{
			pendingComponentObjects = null;
		}
		maxComponentSliceTicks = Math.Max(maxComponentSliceTicks, Stopwatch.GetTimestamp() - timestamp);
	}

	private bool B(Assembly P_0)
	{
		if (P_0 == typeof(Plugin).Assembly)
		{
			return true;
		}
		string text = c(P_0);
		if (B(text))
		{
			return true;
		}
		if (text.StartsWith("<", StringComparison.Ordinal))
		{
			if (!a(P_0))
			{
				return !b(P_0);
			}
			return true;
		}
		string fullPath;
		try
		{
			fullPath = Path.GetFullPath(text);
		}
		catch
		{
			return false;
		}
		lock (fileGuardLock)
		{
			if (observedNewCodeFiles.Contains(fullPath))
			{
				return false;
			}
		}
		if (!A(text, Paths.ManagedPath))
		{
			return A(text, Path.Combine(Paths.BepInExRootPath, "core"));
		}
		return true;
	}

	private static bool b(Assembly P_0)
	{
		try
		{
			AssemblyName[] referencedAssemblies = P_0.GetReferencedAssemblies();
			foreach (AssemblyName assemblyName in referencedAssemblies)
			{
				if (assemblyName.Name == "Assembly-CSharp" || assemblyName.Name == "BepInEx")
				{
					return true;
				}
			}
		}
		catch
		{
			return true;
		}
		return false;
	}

	private static bool A(string P_0, string P_1)
	{
		try
		{
			string fullPath = Path.GetFullPath(P_0);
			string value = Path.GetFullPath(P_1).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
			return fullPath.StartsWith(value, StringComparison.OrdinalIgnoreCase);
		}
		catch
		{
			return false;
		}
	}

	private string k()
	{

		List<string> list = new List<string>
		{
			string.Format("timeScale={0:R} (game-controlled)", Time.timeScale),
			"gravity=" + A((object)Physics.gravity) + " (game-controlled)",
			"frameRate=" + frameRateStatus
		};
		if (player != (Object)null)
		{
			list.Add(A(player, "MaxArmLengthAllowed", 1.5f));
			list.Add(A((object)player, "jumpMultiplier"));
			list.Add(A((object)player, "maxJumpSpeed"));
		}
		if ((Object)(object)lowerSlipComponent != (Object)null)
		{
			list.Add(A(lowerSlipComponent, "slidePercentageOverride", 0.55f));
		}
		if ((Object)(object)upperSlipComponent != (Object)null)
		{
			list.Add(A(upperSlipComponent, "slidePercentageOverride", 1f));
		}
		foreach (MemberTarget tagDistanceTarget in tagDistanceTargets)
		{
			if (tagDistanceTarget.Target != (Object)null)
			{
				list.Add(A(tagDistanceTarget.Target, tagDistanceTarget.MemberName, 1.2f));
			}
		}
		return string.Join(", ", list.Where((string P_0) => P_0 != null));
	}

	private static string A(object P_0, string P_1)
	{
		if (P_0 == null)
		{
			return null;
		}
		MemberInfo memberInfo = A(P_0.GetType(), P_1);
		if (memberInfo == null)
		{
			return null;
		}
		try
		{
			return string.Format("{0}.{1}={2:R}", P_0.GetType().Name, P_1, A(P_0, memberInfo));
		}
		catch
		{
			return P_0.GetType().Name + "." + P_1 + "=<unreadable>";
		}
	}

	private static string A(object P_0, string P_1, float P_2)
	{
		if (P_0 == null)
		{
			return null;
		}
		MemberInfo memberInfo = A(P_0.GetType(), P_1);
		if (memberInfo == null)
		{
			return null;
		}
		try
		{
			return string.Format("{0}.{1}={2:R}/{3:R}", P_0.GetType().Name, P_1, A(P_0, memberInfo), P_2);
		}
		catch
		{
			return string.Format("{0}.{1}=<unreadable>/{2:R}", P_0.GetType().Name, P_1, P_2);
		}
	}

	private static Component A(GameObject P_0, string P_1)
	{
		if ((Object)(object)P_0 == (Object)null)
		{
			return null;
		}
		Component[] components = P_0.GetComponents<Component>();
		foreach (Component val in components)
		{
			if ((Object)(object)val != (Object)null && A(((object)val).GetType(), P_1) != null)
			{
				return val;
			}
		}
		return null;
	}

	private void A(object P_0, MemberInfo P_1, float P_2, bool P_3, string P_4 = null)
	{
		if (P_1 == null || P_0 == null)
		{
			return;
		}
		string text = P_0.GetType().FullName + "." + P_1.Name;
		string text2 = P_4 ?? text;
		try
		{
			float num = A(P_0, P_1);
			if (ValueProtection.A(num, P_2))
			{
				return;
			}
			Dictionary<MemberInfo, ValueDriftWindow> orCreateValue = valueDrift.GetOrCreateValue(P_0);
			if (!orCreateValue.TryGetValue(P_1, out var value))
			{
				value = (orCreateValue[P_1] = new ValueDriftWindow());
			}
			if (value.A(Time.realtimeSinceStartup))
			{
				repeatedValueChanges = true;
				if (c("repeated-value:" + text))
				{
					string text3 = "member=" + text + "; label=" + text2 + "; at least 3 drift observations within 10 seconds; source unknown; not proof of cheating";
					A("REPEATED VALUE", text2 + " repeatedly changed", text3);
					C("REPEATED VALUE", text3);
				}
			}
			if (!ValueProtection.A(P_2, delegate(float P_0)
			{
				A(P_0, P_1, P_0);
			}, () => A(P_0, P_1), out var num2))
			{
				a(text, string.Format("{0}: requested={1:R}; observed={2:R}; correction did not verify", text2, P_2, num2));
				return;
			}
			correctedValues++;
			if (P_3 && c(text))
			{
				A("VALUE", string.Format("{0} {1:R} -> {2:R}", text2, num, P_2), string.Format("member={0}; observed={1:R}; restored={2:R}; action=correction verified by readback", text, num, num2));
			}
		}
		catch (Exception ex)
		{
			a(text, text2 + ": could not read, write, or verify protected value: " + ex.GetBaseException().Message);
		}
	}

	private void a(string P_0, string P_1)
	{
		valueCorrectionFailed = true;
		if (c("failed-value:" + P_0))
		{
			B("ERROR", P_1);
			A("VALUE INCOMPLETE", "Correction unverified; restart required", P_1);
			C("VALUE INCOMPLETE", P_1);
		}
	}

	private bool c(string P_0)
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (lastDriftLog.TryGetValue(P_0, out var value) && realtimeSinceStartup - value < 5f)
		{
			return false;
		}
		lastDriftLog[P_0] = realtimeSinceStartup;
		return true;
	}

	private static MemberInfo A(Type P_0, string P_1)
	{
		string key = (P_0.AssemblyQualifiedName ?? P_0.FullName ?? P_0.Name) + "|" + P_1;
		if (FloatMemberCache.TryGetValue(key, out var value))
		{
			return value;
		}
		PropertyInfo property = P_0.GetProperty(P_1, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (property != null && property.PropertyType == typeof(float) && property.CanRead && property.CanWrite)
		{
			FloatMemberCache.TryAdd(key, property);
			return property;
		}
		FieldInfo field = P_0.GetField(P_1, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (field != null && field.FieldType == typeof(float))
		{
			FloatMemberCache.TryAdd(key, field);
			return field;
		}
		return null;
	}

	private static float A(object P_0, MemberInfo P_1)
	{
		if (P_1 is PropertyInfo propertyInfo)
		{
			return (float)propertyInfo.GetValue(P_0, null);
		}
		return (float)((FieldInfo)P_1).GetValue(P_0);
	}

	private static void A(object P_0, MemberInfo P_1, float P_2)
	{
		if (P_1 is PropertyInfo propertyInfo)
		{
			propertyInfo.SetValue(P_0, P_2, null);
		}
		else
		{
			((FieldInfo)P_1).SetValue(P_0, P_2);
		}
	}

	private static Type D(string P_0)
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			if (string.Equals(assembly.GetName().Name, "Assembly-CSharp", StringComparison.Ordinal))
			{
				Type type = assembly.GetType(P_0, throwOnError: false);
				if (type != null)
				{
					return type;
				}
			}
		}
		return null;
	}

	private static object a(Type P_0, string P_1)
	{
		if (P_0 == null)
		{
			return null;
		}
		PropertyInfo property = P_0.GetProperty(P_1, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (property != null)
		{
			return property.GetValue(null, null);
		}
		return P_0.GetField(P_1, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null);
	}

	private static object a(object P_0, string P_1)
	{
		if (P_0 == null)
		{
			return null;
		}
		Type type = P_0.GetType();
		PropertyInfo property = type.GetProperty(P_1, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (property != null)
		{
			return property.GetValue(P_0, null);
		}
		return type.GetField(P_1, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(P_0);
	}

	private static string C(Assembly P_0)
	{
		return P_0.FullName + "|" + c(P_0);
	}

	private static string c(Assembly P_0)
	{
		try
		{
			return string.IsNullOrEmpty(P_0.Location) ? "<dynamic/no location>" : P_0.Location;
		}
		catch
		{
			return "<unavailable>";
		}
	}

	private void B(string P_0, string P_1)
	{
		if (P_0 == "WARNING" && !warningGate.A(P_1))
		{
			return;
		}
		if (!(P_0 == "ERROR"))
		{
			if (P_0 == "WARNING")
			{
				base.Logger.LogWarning(P_1);
			}
			else
			{
				base.Logger.LogInfo(P_1);
			}
		}
		else
		{
			base.Logger.LogError(P_1);
		}
		pendingGuardLogLines.A(string.Format("[{0:O}] [{1}] {2}{3}", DateTimeOffset.Now, P_0, P_1, Environment.NewLine));
		L();
	}

	private void d(string P_0)
	{
		pendingGuardLogLines.A(string.Format("[{0:O}] [INFO] {1}{2}", DateTimeOffset.Now, P_0, Environment.NewLine));
		L();
	}

	private void L()
	{
		if (Interlocked.CompareExchange(ref guardLogWriterRunning, 1, 0) != 0)
		{
			return;
		}
		ThreadPool.QueueUserWorkItem(delegate
		{
			do
			{
				try
				{
					List<string> list = new List<string>();
					string item;
					while (pendingGuardLogLines.A(out item))
					{
						list.Add(item);
					}
					if (list.Count > 0)
					{
						File.AppendAllText(guardLogPath, string.Concat(list));
					}
				}
				catch (Exception ex)
				{
					Interlocked.Exchange(ref guardLogWriteError, ex.Message);
				}
				Interlocked.Exchange(ref guardLogWriterRunning, 0);
			}
			while (!pendingGuardLogLines.IsEmpty && Interlocked.CompareExchange(ref guardLogWriterRunning, 1, 0) == 0);
		});
	}

	private void l()
	{
		string text = Interlocked.Exchange(ref guardLogWriteError, null);
		if (!string.IsNullOrEmpty(text))
		{
			base.Logger.LogError("Could not write dedicated guard log: " + text);
		}
	}

	private void E(string P_0)
	{
		try
		{
			string fullPath = Path.GetFullPath(P_0);
			string fullPath2 = Path.GetFullPath(Path.Combine(Paths.PluginPath, "GorillaGuard.dll"));
			string fullPath3 = Path.GetFullPath(Path.Combine(Paths.BepInExRootPath, "patchers", "GorillaGuard.Preloader.dll"));
			if (fullPath.Equals(fullPath2, StringComparison.OrdinalIgnoreCase) || fullPath.Equals(fullPath3, StringComparison.OrdinalIgnoreCase))
			{
				integrityCheckRequested = true;
			}
		}
		catch
		{
		}
	}

	private void M()
	{
		if (Interlocked.CompareExchange(ref integrityCheckRunning, 1, 0) != 0)
		{
			return;
		}
		ThreadPool.QueueUserWorkItem(delegate
		{
			try
			{
				pendingIntegrityResults.A(B(true));
			}
			finally
			{
				Interlocked.Exchange(ref integrityCheckRunning, 0);
			}
		});
	}

	private IntegrityObservation B(bool P_0)
	{
		ReleaseVerificationResult releaseVerificationResult = ReleaseVerifier.Verify(Paths.BepInExRootPath, !P_0);
		string restoredTargets = null;
		string restorationMessage = null;
		if (P_0 && trustedRuntimeBytes != null && !releaseVerificationResult.Success && A(releaseVerificationResult.FailureKind, out restoredTargets, out restorationMessage))
		{
			releaseVerificationResult = ReleaseVerifier.Verify(Paths.BepInExRootPath, verifyLauncher: false);
		}
		if (releaseVerificationResult.Success)
		{
			string fullPath = Path.GetFullPath(Path.Combine(Paths.PluginPath, "GorillaGuard.dll"));
			string text = c(typeof(Plugin).Assembly);
			if (!string.Equals(text, fullPath, StringComparison.OrdinalIgnoreCase) || typeof(Plugin).Assembly.GetName().Version?.ToString(3) != "1.12.31")
			{
				releaseVerificationResult.Success = false;
				releaseVerificationResult.FailureKind = ReleaseFailureKind.RuntimeHashMismatch;
				releaseVerificationResult.Message = "Loaded Guard assembly origin/version differs from the verified installed runtime: " + text;
			}
		}
		string restartMarker = m();
		return new IntegrityObservation(releaseVerificationResult, restartMarker, restoredTargets, restorationMessage);
	}

	private bool A(ReleaseFailureKind P_0, out string P_1, out string P_2)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		try
		{
			switch (P_0)
			{
			case ReleaseFailureKind.MissingRuntime:
			case ReleaseFailureKind.RuntimeHashMismatch:
				A(Path.Combine(Paths.PluginPath, "GorillaGuard.dll"), trustedRuntimeBytes, "runtime", list, list2);
				break;
			case ReleaseFailureKind.MissingPreloader:
			case ReleaseFailureKind.PreloaderHashMismatch:
				A(Path.Combine(Paths.BepInExRootPath, "patchers", "GorillaGuard.Preloader.dll"), trustedPreloaderBytes, "preloader", list, list2);
				break;
			case ReleaseFailureKind.MissingLauncher:
			case ReleaseFailureKind.LauncherHashMismatch:
				P_1 = null;
				P_2 = "The external launcher cannot be repaired safely from inside the game.";
				return false;
			case ReleaseFailureKind.MissingSignature:
				A(Path.Combine(Paths.BepInExRootPath, "GorillaGuard.manifest.sig"), trustedSignatureBytes, "signature", list, list2);
				break;
			case ReleaseFailureKind.MissingManifest:
			case ReleaseFailureKind.InvalidSignature:
			case ReleaseFailureKind.InvalidManifest:
			case ReleaseFailureKind.VersionMismatch:
				A(Path.Combine(Paths.BepInExRootPath, "GorillaGuard.manifest"), trustedManifestBytes, "manifest", list, list2);
				A(Path.Combine(Paths.BepInExRootPath, "GorillaGuard.manifest.sig"), trustedSignatureBytes, "signature", list, list2);
				break;
			default:
				P_1 = null;
				P_2 = null;
				return false;
			}
			P_1 = string.Join(",", list);
			P_2 = string.Join("; ", list2);
			return true;
		}
		catch (Exception ex)
		{
			P_1 = ((list.Count == 0) ? null : string.Join(",", list));
			P_2 = ex.Message;
			return false;
		}
	}

	private static void A(string P_0, byte[] P_1, string P_2, List<string> P_3, List<string> P_4)
	{
		if (P_1 == null)
		{
			throw new InvalidOperationException("No trusted bytes were cached for " + P_2 + ".");
		}
		string text = P_0 + ".restore-" + Guid.NewGuid().ToString("N") + ".tmp";
		try
		{
			string text2 = Path.Combine(Paths.BepInExRootPath, "quarantine", "integrity-swaps");
			Directory.CreateDirectory(text2);
			string text3 = Path.Combine(text2, DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff") + "-" + P_2 + "-" + Guid.NewGuid().ToString("N") + ".tampered");
			string directoryName = Path.GetDirectoryName(P_0);
			if (!string.IsNullOrEmpty(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			if (File.Exists(P_0))
			{
				File.Move(P_0, text3);
			}
			File.WriteAllBytes(text, P_1);
			File.Move(text, P_0);
			P_3.Add(P_2);
			P_4.Add(P_2 + ": tampered=" + text3 + "; restored=" + P_0);
		}
		finally
		{
			try
			{
				if (File.Exists(text))
				{
					File.Delete(text);
				}
			}
			catch
			{
			}
		}
	}

	private static string m()
	{
		try
		{
			string path = Path.Combine(Paths.BepInExRootPath, "GorillaGuard.restart-required");
			if (!File.Exists(path))
			{
				return null;
			}
			using StreamReader streamReader = new StreamReader(path);
			return streamReader.ReadLine();
		}
		catch (Exception ex)
		{
			return "Could not read exclusive-mode marker: " + ex.Message;
		}
	}

	private void N()
	{
		IntegrityObservation integrityObservation;
		while (pendingIntegrityResults.A(out integrityObservation))
		{
			A(integrityObservation, false);
		}
	}

	private void A(IntegrityObservation P_0, bool P_1)
	{
		bool flag = P_0.Result.Success && string.IsNullOrEmpty(P_0.RestartMarker);
		string text = (P_0.Result.Success ? P_0.Result.Message : (P_0.Result.Message ?? "Unknown verification failure"));
		if (!string.IsNullOrEmpty(P_0.RestartMarker))
		{
			text = "Restart required: " + P_0.RestartMarker;
		}
		integrityFailure = !flag;
		integrityDetails = (flag ? "Signed files match the release manifest" : A(text, 2048));
		integrityStatus = (flag ? ("VERIFIED " + P_0.Result.Version) : ("FAILED: " + A(text, 70)));
		string text2 = (flag ? "OK|" : "FAIL|") + text;
		if (P_1 || !text2.Equals(lastIntegrityFingerprint, StringComparison.Ordinal))
		{
			lastIntegrityFingerprint = text2;
			B(flag ? "INFO" : "ERROR", flag ? ("SIGNED RELEASE VERIFIED: " + text) : ("INTEGRITY FAILURE: " + text));
			if (!flag)
			{
				b("INTEGRITY", text);
			}
		}
		if (flag && trustedRuntimeBytes == null)
		{
			try
			{
				trustedRuntimeBytes = File.ReadAllBytes(Path.Combine(Paths.PluginPath, "GorillaGuard.dll"));
				trustedPreloaderBytes = File.ReadAllBytes(Path.Combine(Paths.BepInExRootPath, "patchers", "GorillaGuard.Preloader.dll"));
				trustedManifestBytes = File.ReadAllBytes(Path.Combine(Paths.BepInExRootPath, "GorillaGuard.manifest"));
				trustedSignatureBytes = File.ReadAllBytes(Path.Combine(Paths.BepInExRootPath, "GorillaGuard.manifest.sig"));
			}
			catch (Exception ex)
			{
				integrityFailure = true;
				integrityStatus = "FAILED: could not cache trusted runtime";
				B("ERROR", "INTEGRITY FAILURE: Could not cache trusted runtime bytes: " + ex.Message);
				b("INTEGRITY", "Could not cache trusted runtime bytes: " + ex.Message);
			}
		}
		if (!string.IsNullOrEmpty(P_0.RestoredTargets))
		{
			A("FILE SWAP", P_0.RestoredTargets + " restored", "Detected signed release file replacement; targets=" + P_0.RestoredTargets + "; action=quarantined and restored trusted signed bytes; " + P_0.RestorationMessage);
		}
		else if (!string.IsNullOrEmpty(P_0.RestorationMessage) && !flag)
		{
			B("ERROR", "RUNTIME RESTORE FAILED: " + P_0.RestorationMessage);
			modRemovalIncomplete = true;
		}
	}

	private void A(string P_0, string P_1, string P_2)
	{
		string text = A(P_0, 24);
		string arg = A(P_1, 64);
		string text2 = A(P_2, 2048);
		string item = string.Format("{0:HH:mm:ss} {1}: {2}", DateTime.Now, text, arg);
		blockedEvents++;
		recentBlocks.Enqueue(item);
		while (recentBlocks.Count > 3)
		{
			recentBlocks.Dequeue();
		}
		recentBlocksText = string.Join("\n", recentBlocks.ToArray());
		B("WARNING", "BLOCKED [" + text + "] " + text2);
	}

	private void b(string P_0, string P_1)
	{
		if (pendingLauncherAlerts.Count < 16)
		{
			pendingLauncherAlerts.A(new LauncherAlert(A(P_0, 40), A(P_1, 1000)));
		}
	}

	private void C(string P_0, string P_1)
	{
		if (pendingLauncherBlocks.Count < 32)
		{
			pendingLauncherBlocks.A(new LauncherAlert(A(P_0, 40), A(P_1, 1000)));
		}
	}

	private static string A(string P_0, int P_1)
	{
		string text = (string.IsNullOrWhiteSpace(P_0) ? "<unknown>" : P_0.Replace('\r', ' ').Replace('\n', ' ').Trim());
		if (text.Length > P_1)
		{
			return text.Substring(0, P_1 - 3) + "...";
		}
		return text;
	}

	private void n()
	{
		if (launcherHeartbeatRunning)
		{
			return;
		}
		launcherHeartbeatRunning = true;
		ThreadPool.QueueUserWorkItem(delegate
		{
			while (!launcherStopRequested)
			{
				try
				{
					LauncherSession launcherSession = O();
					if (launcherSession == null)
					{
						launcherStatus = "MISSING";
						Thread.Sleep(1000);
					}
					else
					{
						using NamedPipeClientStream namedPipeClientStream = new NamedPipeClientStream(".", launcherSession.PipeName, PipeDirection.InOut, PipeOptions.None);
						launcherStatus = "CONNECTING";
						namedPipeClientStream.Connect(2000);
						using StreamReader streamReader = new StreamReader(namedPipeClientStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, 1024, leaveOpen: true);
						using StreamWriter streamWriter = new StreamWriter(namedPipeClientStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 1024, leaveOpen: true)
						{
							AutoFlush = true
						};
						streamWriter.WriteLine("HELLO|" + launcherSession.Nonce + "|1.12.31");
						string text = streamReader.ReadLine();
						if (!string.Equals(text, "OK|" + launcherSession.Nonce, StringComparison.Ordinal))
						{
							throw new InvalidDataException("Launcher rejected the authenticated runtime handshake.");
						}
						Interlocked.Exchange(ref lastLauncherHeartbeatUtcTicks, DateTime.UtcNow.Ticks);
						launcherStatus = "CONNECTED / LOCKS ACTIVE";
						lastLauncherHeartbeatError = null;
						B("INFO", string.Format("Launcher supervisor authenticated; pid={0}; pipe={1}", launcherSession.LauncherPid, launcherSession.PipeName));
						while (!launcherStopRequested && namedPipeClientStream.IsConnected)
						{
							if (pendingLauncherAlerts.A(out var launcherAlert))
							{
								string text2 = Convert.ToBase64String(Encoding.UTF8.GetBytes(launcherAlert.Details));
								streamWriter.WriteLine("ALERT|" + launcherSession.Nonce + "|" + launcherAlert.Category + "|" + text2);
								text = streamReader.ReadLine();
								if (!string.Equals(text, "ACK|" + launcherSession.Nonce, StringComparison.Ordinal))
								{
									throw new IOException("Launcher did not acknowledge the protected-session alert.");
								}
								launcherStatus = "VIOLATION REPORTED — CLOSING";
								throw new IOException("Protected-session violation was reported to the launcher.");
							}
							if (pendingLauncherBlocks.A(out var launcherAlert2))
							{
								string text3 = Convert.ToBase64String(Encoding.UTF8.GetBytes(launcherAlert2.Details));
								streamWriter.WriteLine("BLOCK|" + launcherSession.Nonce + "|" + launcherAlert2.Category + "|" + text3);
								text = streamReader.ReadLine();
								if (!string.Equals(text, "ACK|" + launcherSession.Nonce, StringComparison.Ordinal))
								{
									throw new IOException("Launcher did not acknowledge the runtime block report.");
								}
								launcherStatus = "CONNECTED / LOCAL ACTION REPORTED";
							}
							streamWriter.WriteLine("PING|" + launcherSession.Nonce);
							text = streamReader.ReadLine();
							if (text != null && text.StartsWith("EVENT|" + launcherSession.Nonce + "|", StringComparison.Ordinal))
							{
								string[] array = text.Split(new char[1] { '|' }, 4);
								string category = ((array.Length > 2) ? A(array[2], 40) : "LAUNCHER");
								string text4 = "Launcher reported a block.";
								if (array.Length > 3)
								{
									try
									{
										text4 = Encoding.UTF8.GetString(Convert.FromBase64String(array[3]));
									}
									catch
									{
										text4 = "Launcher event payload was malformed.";
									}
								}
								pendingLauncherNotices.A(new LauncherNotice(category, A(text4, 1000)));
							}
							else if (!string.Equals(text, "PONG|" + launcherSession.Nonce, StringComparison.Ordinal))
							{
								throw new IOException("Launcher heartbeat ended or returned an invalid response.");
							}
							Interlocked.Exchange(ref lastLauncherHeartbeatUtcTicks, DateTime.UtcNow.Ticks);
							Thread.Sleep(2000);
						}
					}
				}
				catch (Exception ex)
				{
					launcherStatus = "DISCONNECTED";
					string text5 = A(ex.Message, 256);
					if (!string.Equals(text5, lastLauncherHeartbeatError, StringComparison.Ordinal))
					{
						lastLauncherHeartbeatError = text5;
						B("WARNING", "Launcher heartbeat unavailable: " + text5);
					}
					Thread.Sleep(1000);
				}
			}
			launcherHeartbeatRunning = false;
		});
	}

	private static LauncherSession O()
	{
		string path = SessionLocation.GetSessionFilePath(Paths.GameRootPath);
		if (!File.Exists(path))
		{
			return null;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
		string[] array = File.ReadAllLines(path);
		foreach (string text in array)
		{
			int num = text.IndexOf('=');
			if (num > 0)
			{
				dictionary[text.Substring(0, num)] = text.Substring(num + 1);
			}
		}
		if (!dictionary.TryGetValue("version", out var value) || value != "1.12.31" || !dictionary.TryGetValue("pipe", out var value2) || string.IsNullOrWhiteSpace(value2) || !dictionary.TryGetValue("nonce", out var value3) || value3.Length < 32 || !dictionary.TryGetValue("launcherPid", out var value4) || !int.TryParse(value4, out var result) || !dictionary.TryGetValue("createdUtcTicks", out var value5) || !long.TryParse(value5, out var result2))
		{
			throw new InvalidDataException("The launcher session file is malformed or from another release.");
		}
		double totalSeconds = new TimeSpan(DateTime.UtcNow.Ticks - result2).TotalSeconds;
		if (totalSeconds < -30.0 || totalSeconds > 600.0)
		{
			throw new InvalidDataException("The launcher session file is stale.");
		}
		Process processById;
		try
		{
			processById = Process.GetProcessById(result);
		}
		catch
		{
			return null;
		}
		using (processById)
		{
			if (processById.HasExited)
			{
				return null;
			}
			string fullPath;
			try
			{
				fullPath = Path.GetFullPath(processById.MainModule.FileName);
			}
			catch
			{
				return null;
			}
			string fullPath2 = Path.GetFullPath(Path.Combine(Paths.GameRootPath, "GorillaGuard.Launcher.exe"));
			if (!fullPath.Equals(fullPath2, StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException("The launcher session belongs to an unexpected executable.");
			}
		}
		return new LauncherSession(value2, value3, result);
	}

	private bool o()
	{
		long num = Interlocked.Read(in lastLauncherHeartbeatUtcTicks);
		if (num > 0)
		{
			return new TimeSpan(DateTime.UtcNow.Ticks - num).TotalSeconds <= 15.0;
		}
		return false;
	}

	private void P()
	{
		int num = 0;
		LauncherNotice launcherNotice;
		while (num < 8 && pendingLauncherNotices.A(out launcherNotice))
		{
			num++;
			if (launcherNotice.Category.Equals("MONITORING INCOMPLETE", StringComparison.OrdinalIgnoreCase))
			{
				moduleMonitoringIncomplete = true;
			}
			if (launcherNotice.Category.Equals("FILE CONTAINMENT", StringComparison.OrdinalIgnoreCase) || launcherNotice.Category.Equals("NATIVE MODULE", StringComparison.OrdinalIgnoreCase))
			{
				modRemovalIncomplete = true;
				modRestartRequired = true;
			}
			if (launcherNotice.Category.Equals("FILE", StringComparison.OrdinalIgnoreCase))
			{
				quarantinedFiles++;
			}
			A("LAUNCHER " + launcherNotice.Category, launcherNotice.Details, launcherNotice.Details);
		}
	}

	private void p()
	{
		if (!launcherFailureHandled && !(Time.realtimeSinceStartup < launcherDeadline) && !o())
		{
			launcherFailureHandled = true;
			launcherStatus = "REQUIRED — CLOSING GAME";
			A("LAUNCHER", "supervisor heartbeat missing", "Authenticated launcher heartbeat was not present; action=close unprotected game session");
			Application.Quit();
		}
	}

	private void OnGUI()
	{

		if ((int)Event.current.type == 7)
		{
			q();
			bool flag = o();
			bool num = integrityFailure || !flag || trackingMonitoringIncomplete || blockedEvents > 0 || unapprovedPlugins > 0;
			Color color = GUI.color;
			GUI.color = (num ? new Color(1f, 0.72f, 0.25f) : new Color(0.35f, 1f, 0.55f));
			GUI.Box(new Rect(18f, 18f, 620f, 400f), GUIContent.none, panelStyle);
			GUI.Label(new Rect(32f, 28f, 590f, 26f), overlayTitle, titleStyle);
			GUI.color = Color.white;
			GUI.Label(new Rect(32f, 56f, 590f, 350f), overlayBody, bodyStyle);
			GUI.color = color;
		}
	}

	private void Q()
	{
		if (!(Time.realtimeSinceStartup < nextOverlayRefreshAt))
		{
			nextOverlayRefreshAt = Time.realtimeSinceStartup + 0.25f;
			if (integrityFailure || valueCorrectionFailed || repeatedValueChanges || trackingMonitoringIncomplete || !o() || blockedEvents > 0)
			{
				_ = 1;
			}
			else
				_ = unapprovedPlugins > 0;
			string text = ProtectionStatus.A(!integrityFailure, o(), modWasDisabled, modRemovalIncomplete, modRestartRequired, moduleMonitoringIncomplete || trackingMonitoringIncomplete, valueCorrectionFailed, repeatedValueChanges);
			overlayTitle = "GORILLA GUARD — " + text;
			float num = Mathf.Max(0f, nextAuditAt - Time.realtimeSinceStartup);
			overlayBody = "Local protection / event uploads disabled\nLauncher supervisor: " + launcherStatus + "\nRelease integrity: " + integrityStatus + "\nFile check: " + A(integrityDetails, 150) + "\nValues: " + (valueCorrectionFailed ? "CORRECTION INCOMPLETE" : (repeatedValueChanges ? "REPEATED CHANGES" : "guarded tag / reach / slips")) + "\nFrames: " + frameRateStatus + "\nHeadset: " + headsetStatus + "\nPlayspace: " + playspaceStatus + "\n" + string.Format("Tracking: {0} / Warnings: {1}\n", trackingStatus, trackingWarnings) + "VR runtime: " + headsetRuntime + " / Refresh: " + headsetRefresh + "\n" + ((modRestartRequired || modRemovalIncomplete) ? "Loaded code may remain; restart for full unload." : "Pre-launch quarantine and signed-file checks active.") + "\n" + (integrityFailure ? "Repair: close the game, run the signed installer or launcher --repair. See log for full path/hash." : "") + "\n" + string.Format("Corrections: {0}   Next audit: {1:0}s\n", correctedValues, num) + string.Format("Block events: {0}   Plugins: {1}   Components: {2}   Files: {3}\n", blockedEvents, disabledPlugins, blockedComponents, quarantinedFiles) + "Recent blocks:\n" + recentBlocksText;
		}
	}

	private void q()
	{

		if (panelStyle == null)
		{
			GUIStyle val = new GUIStyle(GUI.skin.box);
			val.normal.background = Texture2D.whiteTexture;
			panelStyle = val;
			GUIStyle val2 = new GUIStyle(GUI.skin.label)
			{
				fontSize = 14,
				fontStyle = (FontStyle)1
			};
			val2.normal.textColor = Color.white;
			titleStyle = val2;
			GUIStyle val3 = new GUIStyle(GUI.skin.label)
			{
				fontSize = 12
			};
			val3.normal.textColor = Color.white;
			bodyStyle = val3;
		}
	}

	private void R()
	{

		if (Time.realtimeSinceStartup < nextTrackingSampleAt)
		{
			return;
		}
		nextTrackingSampleAt = Time.realtimeSinceStartup + 0.05f;
		trackingMonitoringIncomplete = true;
		if (player == (Object)null)
		{
			e("Waiting for local player");
			return;
		}
		try
		{
			if (trackingBoundPlayer != player)
			{
				r();
			}
			if (trackingHeadColliderField == null || trackingBodyColliderField == null || trackingControllerTransformMethod == null)
			{
				trackingStatus = "Unavailable: game tracking bindings changed";
				return;
			}
			object? value = trackingHeadColliderField.GetValue(player);
			SphereCollider val = (SphereCollider)((value is SphereCollider) ? value : null);
			object? value2 = trackingBodyColliderField.GetValue(player);
			CapsuleCollider val2 = (CapsuleCollider)((value2 is CapsuleCollider) ? value2 : null);
			object? obj = trackingControllerTransformMethod.Invoke(player, new object[1] { true });
			Transform val3 = (Transform)((obj is Transform) ? obj : null);
			object? obj2 = trackingControllerTransformMethod.Invoke(player, new object[1] { false });
			Transform val4 = (Transform)((obj2 is Transform) ? obj2 : null);
			if ((Object)(object)val == (Object)null || (Object)(object)val2 == (Object)null || (Object)(object)val3 == (Object)null || (Object)(object)val4 == (Object)null || !((Component)val).gameObject.activeInHierarchy || !((Component)val2).gameObject.activeInHierarchy)
			{
				trackingAnomalies.A();
				trackingEvidenceWindow.A();
				trackingEvidence = 0.0;
				trackingStatus = "Inactive rig — not sampled";
				return;
			}
			Vector3 val5 = ((Component)val).transform.TransformPoint(val.center);
			Vector3 val6 = ((Component)val2).transform.TransformPoint(val2.center);
			TrackingPoseSample trackingPoseSample = new TrackingPoseSample(Time.realtimeSinceStartup, A(val6), A(val5), A(val3.position), A(val4.position));
			TrackingAnomaly trackingAnomaly = trackingAnomalies.A(trackingPoseSample);
			if (Time.realtimeSinceStartup < trackingGraceUntil)
			{
				trackingEvidence = 0.0;
				trackingEvidenceWindow.A();
				trackingStatus = "Stabilizing local tracking";
				return;
			}
			int num = TrackingAnomalyWindow.A(trackingAnomaly);
			trackingEvidence = trackingEvidenceWindow.A(trackingPoseSample.Time, num);
			if (trackingAnomaly == TrackingAnomaly.InvalidSample)
			{
				trackingEvidenceWindow.A();
				trackingEvidence = 0.0;
				trackingStatus = "Invalid sample — observation window reset";
				return;
			}
			trackingMonitoringIncomplete = false;
			if (trackingEvidence < 5.0)
			{
				trackingStatus = ((trackingEvidence == 0.0) ? "Relative pose monitoring active" : "Tracking evidence accumulating");
				return;
			}
			trackingStatus = "Anomaly observed — see local log";
			if (!(Time.realtimeSinceStartup < nextTrackingWarningAt))
			{
				nextTrackingWarningAt = Time.realtimeSinceStartup + 15f;
				trackingWarnings++;
				double magnitude = (trackingPoseSample.LeftHand - trackingPoseSample.Head).Magnitude;
				double magnitude2 = (trackingPoseSample.RightHand - trackingPoseSample.Head).Magnitude;
				B("WARNING", string.Format("TRACKING ANOMALY observation only: flags={0}; evidence={1}; ", trackingAnomaly, trackingEvidence) + string.Format("leftReach={0:0.000}m; rightReach={1:0.000}m; ", magnitude, magnitude2) + "relative head/hand motion exceeded conservative local thresholds; no tracking transform was changed and this warning alone is not proof of cheating.");
			}
		}
		catch (Exception ex)
		{
			trackingAnomalies.A();
			trackingEvidenceWindow.A();
			trackingEvidence = 0.0;
			trackingStatus = "Monitoring unavailable";
			if (c("tracking-observer-error"))
			{
				B("WARNING", "Tracking observation failed: " + ex.GetBaseException().Message);
			}
		}
	}

	private void r()
	{
		trackingBoundPlayer = player;
		Type type = ((object)player).GetType();
		trackingHeadColliderField = type.GetField("headCollider", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		trackingBodyColliderField = type.GetField("bodyCollider", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		trackingControllerTransformMethod = type.GetMethod("GetControllerTransform", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(bool) }, null);
		trackingAnomalies.A();
		trackingEvidenceWindow.A();
		trackingEvidence = 0.0;
		trackingGraceUntil = Time.realtimeSinceStartup + 3f;
		trackingStatus = ((trackingHeadColliderField != null && trackingBodyColliderField != null && trackingControllerTransformMethod != null) ? "Stabilizing local tracking" : "Unavailable: game tracking bindings changed");
	}

	private void e(string P_0)
	{
		if (!(trackingBoundPlayer == (Object)null) || !(trackingStatus == P_0))
		{
			trackingBoundPlayer = null;
			trackingHeadColliderField = null;
			trackingBodyColliderField = null;
			trackingControllerTransformMethod = null;
			trackingAnomalies.A();
			trackingEvidenceWindow.A();
			trackingEvidence = 0.0;
			trackingStatus = P_0;
		}
	}

	private static TrackingPoint A(Vector3 P_0)
	{

		return new TrackingPoint(P_0.x, P_0.y, P_0.z);
	}

	[CompilerGenerated]
	private void a(object P_0)
	{
		try
		{
			foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
			{
				string text = module.FileName ?? module.ModuleName;
				bool flag;
				lock (moduleLock)
				{
					flag = knownModules.Add(text);
				}
				if (flag)
				{
					pendingNativeModules.A(new NativeModuleObservation(module.ModuleName, text, null));
				}
			}
		}
		catch (Exception ex)
		{
			pendingNativeModules.A(new NativeModuleObservation(null, null, ex.Message));
		}
		finally
		{
			Interlocked.Exchange(ref nativeAuditRunning, 0);
		}
	}

	[CompilerGenerated]
	private void B(object P_0)
	{
		do
		{
			try
			{
				List<string> list = new List<string>();
				string item;
				while (pendingGuardLogLines.A(out item))
				{
					list.Add(item);
				}
				if (list.Count > 0)
				{
					File.AppendAllText(guardLogPath, string.Concat(list));
				}
			}
			catch (Exception ex)
			{
				Interlocked.Exchange(ref guardLogWriteError, ex.Message);
			}
			Interlocked.Exchange(ref guardLogWriterRunning, 0);
		}
		while (!pendingGuardLogLines.IsEmpty && Interlocked.CompareExchange(ref guardLogWriterRunning, 1, 0) == 0);
	}

	[CompilerGenerated]
	private void b(object P_0)
	{
		try
		{
			pendingIntegrityResults.A(B(true));
		}
		finally
		{
			Interlocked.Exchange(ref integrityCheckRunning, 0);
		}
	}

	[CompilerGenerated]
	private void C(object P_0)
	{
		while (!launcherStopRequested)
		{
			try
			{
				LauncherSession launcherSession = O();
				if (launcherSession == null)
				{
					launcherStatus = "MISSING";
					Thread.Sleep(1000);
					continue;
				}
				using NamedPipeClientStream namedPipeClientStream = new NamedPipeClientStream(".", launcherSession.PipeName, PipeDirection.InOut, PipeOptions.None);
				launcherStatus = "CONNECTING";
				namedPipeClientStream.Connect(2000);
				using StreamReader streamReader = new StreamReader(namedPipeClientStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, 1024, leaveOpen: true);
				using StreamWriter streamWriter = new StreamWriter(namedPipeClientStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 1024, leaveOpen: true)
				{
					AutoFlush = true
				};
				streamWriter.WriteLine("HELLO|" + launcherSession.Nonce + "|1.12.31");
				string text = streamReader.ReadLine();
				if (!string.Equals(text, "OK|" + launcherSession.Nonce, StringComparison.Ordinal))
				{
					throw new InvalidDataException("Launcher rejected the authenticated runtime handshake.");
				}
				Interlocked.Exchange(ref lastLauncherHeartbeatUtcTicks, DateTime.UtcNow.Ticks);
				launcherStatus = "CONNECTED / LOCKS ACTIVE";
				lastLauncherHeartbeatError = null;
				B("INFO", string.Format("Launcher supervisor authenticated; pid={0}; pipe={1}", launcherSession.LauncherPid, launcherSession.PipeName));
				while (!launcherStopRequested && namedPipeClientStream.IsConnected)
				{
					if (pendingLauncherAlerts.A(out var launcherAlert))
					{
						string text2 = Convert.ToBase64String(Encoding.UTF8.GetBytes(launcherAlert.Details));
						streamWriter.WriteLine("ALERT|" + launcherSession.Nonce + "|" + launcherAlert.Category + "|" + text2);
						text = streamReader.ReadLine();
						if (!string.Equals(text, "ACK|" + launcherSession.Nonce, StringComparison.Ordinal))
						{
							throw new IOException("Launcher did not acknowledge the protected-session alert.");
						}
						launcherStatus = "VIOLATION REPORTED — CLOSING";
						throw new IOException("Protected-session violation was reported to the launcher.");
					}
					if (pendingLauncherBlocks.A(out var launcherAlert2))
					{
						string text3 = Convert.ToBase64String(Encoding.UTF8.GetBytes(launcherAlert2.Details));
						streamWriter.WriteLine("BLOCK|" + launcherSession.Nonce + "|" + launcherAlert2.Category + "|" + text3);
						text = streamReader.ReadLine();
						if (!string.Equals(text, "ACK|" + launcherSession.Nonce, StringComparison.Ordinal))
						{
							throw new IOException("Launcher did not acknowledge the runtime block report.");
						}
						launcherStatus = "CONNECTED / LOCAL ACTION REPORTED";
					}
					streamWriter.WriteLine("PING|" + launcherSession.Nonce);
					text = streamReader.ReadLine();
					if (text != null && text.StartsWith("EVENT|" + launcherSession.Nonce + "|", StringComparison.Ordinal))
					{
						string[] array = text.Split(new char[1] { '|' }, 4);
						string category = ((array.Length > 2) ? A(array[2], 40) : "LAUNCHER");
						string text4 = "Launcher reported a block.";
						if (array.Length > 3)
						{
							try
							{
								text4 = Encoding.UTF8.GetString(Convert.FromBase64String(array[3]));
							}
							catch
							{
								text4 = "Launcher event payload was malformed.";
							}
						}
						pendingLauncherNotices.A(new LauncherNotice(category, A(text4, 1000)));
					}
					else if (!string.Equals(text, "PONG|" + launcherSession.Nonce, StringComparison.Ordinal))
					{
						throw new IOException("Launcher heartbeat ended or returned an invalid response.");
					}
					Interlocked.Exchange(ref lastLauncherHeartbeatUtcTicks, DateTime.UtcNow.Ticks);
					Thread.Sleep(2000);
				}
			}
			catch (Exception ex)
			{
				launcherStatus = "DISCONNECTED";
				string text5 = A(ex.Message, 256);
				if (!string.Equals(text5, lastLauncherHeartbeatError, StringComparison.Ordinal))
				{
					lastLauncherHeartbeatError = text5;
					B("WARNING", "Launcher heartbeat unavailable: " + text5);
				}
				Thread.Sleep(1000);
			}
		}
		launcherHeartbeatRunning = false;
	}
}
#pragma endregion
