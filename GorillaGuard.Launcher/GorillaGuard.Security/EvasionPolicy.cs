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
