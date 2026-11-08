using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace GorillaGuard.Security;

internal static class SessionLocation
{
	internal static string GetSessionFilePath(string gameRootPath)
	{
		string s = Path.GetFullPath(gameRootPath).TrimEnd('\\', '/').ToUpperInvariant();
		using SHA256 sHA = SHA256.Create();
		string text = BitConverter.ToString(sHA.ComputeHash(Encoding.UTF8.GetBytes(s))).Replace("-", "");
		return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GorillaGuard", "sessions", text, "GorillaGuard.session");
	}
}
