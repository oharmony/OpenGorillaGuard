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

	internal static bool VerifyManifestSignature(byte[] manifestBytes, byte[] signatureBytes)
	{
		if (manifestBytes == null || manifestBytes.Length == 0 || signatureBytes == null || signatureBytes.Length == 0)
		{
			return false;
		}
		using RSA rSA = RSA.Create();
		rSA.ImportParameters(new RSAParameters
		{
			Modulus = Convert.FromBase64String(_"2Lm8yNQ1sEC0uXbgt2O4vOqpDIVhaKSGM3Zkhj4pyxJX8slkMyECsMrSzEDhbYFrkYwj9tHTS61g8zuHgKV7GpoKNpRkz28kkJAD4ChJhMucPGnDfle1gclUL99OU357PuaZqFWoBlmXstYzQqmVPajCnwQKQSNCsOAkAEEza5ba9uIZD153eimFDqKpGAxUduST7eRgaBNZWtcdw6sTdn13/Ft/mh/gCzjGm0OFurBw4J1vq2LnHZ0B58oAivsCpTkZzCtyYScGI4afGTxP+3EddgRrjKGE82MnQrocKbVRecy5ReIsdwgq5bgb/26nS+V87FD4KH7ab8cKBSdePbLy28W2GELxQnAI+6kZAeCSpo0lmws83gZKySDjrEglJ4R1/Z4EDT28OqN+N4ntSqnTF8vAlBrtTs8uiAd1gGi2Fa2z0f7D5Y2AIerlmQMp3Jg1GgGKAoouswmDNR1Xh2MS/rOo6UklzkFsEsh/d6ZCsh6Y0UGn/SLqk/eevM7B"),
			Exponent = Convert.FromBase64String(_"AQAB")
		});
		return rSA.VerifyData(manifestBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
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
			string fullPath = Path.GetFullPath(gameRootPath);
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

	internal static string ComputeFileSha256(string path)
	{
		using SHA256 sHA = SHA256.Create();
		using FileStream inputStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
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

	internal static string ComputeDirectoryTreeSha256(string directoryPath)
	{
		string fullRoot = Path.GetFullPath(gameRootPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
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
