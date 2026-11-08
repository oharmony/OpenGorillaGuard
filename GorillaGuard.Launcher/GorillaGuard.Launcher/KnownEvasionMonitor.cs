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
