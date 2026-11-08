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
