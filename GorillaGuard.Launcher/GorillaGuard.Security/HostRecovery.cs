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
