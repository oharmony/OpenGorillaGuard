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
