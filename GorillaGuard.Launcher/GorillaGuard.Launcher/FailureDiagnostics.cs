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
