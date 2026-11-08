using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using BepInEx;
using BepInEx.Logging;
using GorillaGuard.Security;
using Mono.Cecil;

namespace GorillaGuard.Preloader;

public static class Patcher
{
	private sealed class LauncherSession
	{
		public string PipeName { get; }

		public string Nonce { get; }

		public LauncherSession(string pipeName, string nonce)
		{
			PipeName = pipeName;
			Nonce = nonce;
		}
	}

	[CompilerGenerated]
	private sealed class _003CAllTypes_003Ed__22 : IEnumerable<TypeDefinition>, IEnumerable, IEnumerator<TypeDefinition>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private TypeDefinition _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private IEnumerable<TypeDefinition> roots;

		public IEnumerable<TypeDefinition> _003C_003E3__roots;

		private IEnumerator<TypeDefinition> _003C_003E7__wrap1;

		private TypeDefinition _003Ctype_003E5__3;

		private IEnumerator<TypeDefinition> _003C_003E7__wrap3;

		TypeDefinition IEnumerator<TypeDefinition>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CAllTypes_003Ed__22(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		private void A()
		{
			int num = _003C_003E1__state;
			if ((uint)(num - -4) <= 1u || (uint)(num - 1) <= 1u)
			{
				try
				{
					if (num == -4 || num == 2)
					{
						try
						{
						}
						finally
						{
							B();
						}
					}
				}
				finally
				{
					a();
				}
			}
			_003C_003E7__wrap1 = null;
			_003Ctype_003E5__3 = null;
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
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003C_003E7__wrap1 = roots.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					_003C_003E7__wrap3 = Patcher.EnumerateTypes((IEnumerable<TypeDefinition>)_003Ctype_003E5__3.NestedTypes).GetEnumerator();
					_003C_003E1__state = -4;
					goto IL_0138;
				case 2:
					{
						_003C_003E1__state = -4;
						goto IL_0138;
					}
					IL_0138:
					if (_003C_003E7__wrap3.MoveNext())
					{
						TypeDefinition current = _003C_003E7__wrap3.Current;
						_003C_003E2__current = current;
						_003C_003E1__state = 2;
						return true;
					}
					B();
					_003C_003E7__wrap3 = null;
					_003Ctype_003E5__3 = null;
					break;
				}
				if (_003C_003E7__wrap1.MoveNext())
				{
					_003Ctype_003E5__3 = _003C_003E7__wrap1.Current;
					_003C_003E2__current = _003Ctype_003E5__3;
					_003C_003E1__state = 1;
					return true;
				}
				a();
				_003C_003E7__wrap1 = null;
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
			if (_003C_003E7__wrap1 != null)
			{
				_003C_003E7__wrap1.Dispose();
			}
		}

		private void B()
		{
			_003C_003E1__state = -3;
			if (_003C_003E7__wrap3 != null)
			{
				_003C_003E7__wrap3.Dispose();
			}
		}

		[DebuggerHidden]
		private void b()
		{
			throw new NotSupportedException();
		}

		void IEnumerator.Reset()
		{

			this.b();
		}

		[DebuggerHidden]
		private IEnumerator<TypeDefinition> C()
		{
			_003CAllTypes_003Ed__22 _003CAllTypes_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CAllTypes_003Ed__ = this;
			}
			else
			{
				_003CAllTypes_003Ed__ = new _003CAllTypes_003Ed__22(0);
			}
			_003CAllTypes_003Ed__.roots = _003C_003E3__roots;
			return _003CAllTypes_003Ed__;
		}

		IEnumerator<TypeDefinition> IEnumerable<TypeDefinition>.GetEnumerator()
		{

			return this.C();
		}

		[DebuggerHidden]
		private IEnumerator c()
		{
			return C();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{

			return this.c();
		}
	}

	[CompilerGenerated]
	private sealed class _003Cget_TargetDLLs_003Ed__7 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private string _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		string IEnumerator<string>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003Cget_TargetDLLs_003Ed__7(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		private void A()
		{
			_003C_003E1__state = -2;
		}

		void IDisposable.Dispose()
		{

			this.A();
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				Patcher.EnsureExclusiveModePrepared();
				_003C_003E2__current = "Assembly-CSharp.dll";
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				return false;
			}
		}

		bool IEnumerator.MoveNext()
		{

			return this.MoveNext();
		}

		[DebuggerHidden]
		private void a()
		{
			throw new NotSupportedException();
		}

		void IEnumerator.Reset()
		{

			this.a();
		}

		[DebuggerHidden]
		private IEnumerator<string> B()
		{
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				return this;
			}
			return new _003Cget_TargetDLLs_003Ed__7(0);
		}

		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{

			return this.B();
		}

		[DebuggerHidden]
		private IEnumerator b()
		{
			return B();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{

			return this.b();
		}
	}

	private static readonly ManualLogSource Log = Logger.CreateLogSource("GorillaGuard.Preloader");

	private static readonly object FileLogLock = new object();

	private static readonly object ExclusiveLock = new object();

	private static bool ExclusiveModePrepared;

	private static string QuarantineSessionDirectory;

	public static IEnumerable<string> TargetDLLs
	{
		[IteratorStateMachine(typeof(_003Cget_TargetDLLs_003Ed__7))]
		get
		{

			return new _003Cget_TargetDLLs_003Ed__7(-2);
		}
	}

	public static void Initialize()
	{
		EnsureExclusiveModePrepared();
	}

	public static void Patch(AssemblyDefinition assembly)
	{
		EnsureExclusiveModePrepared();
		B("INFO", "=== New preload target === Gorilla Guard verifying " + assembly.Name.Name + " before it executes.");
		FieldDefinition fieldDefinition = FindSingleField(FindType(assembly.MainModule, "GorillaTagger"), "maxTagDistance");
		LogGuard("INFO", (fieldDefinition == null) ? "Preload verification failed: GorillaTagger.maxTagDistance was not found; no game code was modified." : "Preload verification passed: GorillaTagger.maxTagDistance found; no game code was modified.");
	}

	private static void A()
	{
		lock (ExclusiveLock)
		{
			if (ExclusiveModePrepared)
			{
				return;
			}
			ExclusiveModePrepared = true;
			ClearRestartRequiredMarker();
			ReleaseVerificationResult releaseVerificationResult = ReleaseVerifier.Verify(Paths.BepInExRootPath);
			if (releaseVerificationResult.Success)
			{
				LogGuard("INFO", releaseVerificationResult.Message);
			}
			else
			{
				LogGuard("ERROR", "SIGNED RELEASE VERIFICATION FAILED: " + releaseVerificationResult.Message);
				WriteRestartRequiredMarker("Signed release verification failed: " + releaseVerificationResult.Message);
				QuarantineFile(Path.Combine(Paths.PluginPath, "GorillaGuard.dll"), Paths.PluginPath, "integrity-failure");
			}
			if (!TryVerifyLauncherSession(out var text, out var launcherSession))
			{
				LogGuard("ERROR", "LAUNCHER SESSION VERIFICATION FAILED: " + text);
				WriteRestartRequiredMarker("Launcher session verification failed: " + text);
			}
			else
			{
				LogGuard("INFO", text);
				if (!TrySendPreloaderProof(launcherSession, out var text2))
				{
					LogGuard("ERROR", "PRELOADER PROOF FAILED: " + text2);
					WriteRestartRequiredMarker("Preloader proof failed: " + text2);
				}
				else
				{
					LogGuard("INFO", text2);
				}
			}
			a();
			B();
		}
	}

	private static void QuarantineOtherPlugins()
	{
		if (!Directory.Exists(Paths.PluginPath))
		{
			return;
		}
		string text = Path.Combine(Paths.PluginPath, "GorillaGuard.dll");
		string[] files = Directory.GetFiles(Paths.PluginPath, "*", SearchOption.AllDirectories);
		foreach (string text2 in files)
		{
			if (a(text2) && !A(text2, text))
			{
				QuarantineFile(text2, Paths.PluginPath, "plugins");
			}
		}
	}

	private static void QuarantineOtherPatchers()
	{
		string text = Path.Combine(Paths.BepInExRootPath, "patchers");
		if (!Directory.Exists(text))
		{
			return;
		}
		string text2 = Path.Combine(text, "GorillaGuard.Preloader.dll");
		string[] files = Directory.GetFiles(text, "*", SearchOption.AllDirectories);
		foreach (string text3 in files)
		{
			if (a(text3) && !A(text3, text2))
			{
				WriteRestartRequiredMarker("Another BepInEx patcher was present and may already have initialized: " + text3);
				QuarantineFile(text3, text, "patchers");
			}
		}
	}

	private static bool TryVerifyLauncherSession(out string message, out LauncherSession session)
	{
		session = null;
		try
		{
			string path = SessionLocation.GetSessionFilePath(Paths.GameRootPath);
			if (!File.Exists(path))
			{
				message = "No launcher session file was present. Start the game with GorillaGuard.Launcher.exe.";
				return false;
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
				message = "The launcher session file was malformed or from another version.";
				return false;
			}
			double totalSeconds = new TimeSpan(DateTime.UtcNow.Ticks - result2).TotalSeconds;
			if (totalSeconds < -30.0 || totalSeconds > 600.0)
			{
				message = "The launcher session file was stale."";
				return false;
			}
			using (Process process = Process.GetProcessById(result))
			{
				if (process.HasExited)
				{
					message = "The launcher process had already exited."";
					return false;
				}
				string fullPath = Path.GetFullPath(Path.Combine(Paths.GameRootPath, "GorillaGuard.Launcher.exe"));
				if (!Path.GetFullPath(process.MainModule.FileName).Equals(fullPath, StringComparison.OrdinalIgnoreCase))
				{
					message = "The session process was not the installed Gorilla Guard launcher."";
					return false;
				}
			}
			session = new LauncherSession(value2, value3);
			message = string.Format("Launcher session verified before game assembly load; pid={0}.", result);
			return true;
		}
		catch (Exception ex)
		{
			message = ex.Message;
			return false;
		}
	}

	private static bool TrySendPreloaderProof(LauncherSession session, out string message)
	{
		DateTime dateTime = DateTime.UtcNow.AddSeconds(12.0);
		Exception ex = null;
		while (DateTime.UtcNow < dateTime)
		{
			try
			{
				using NamedPipeClientStream namedPipeClientStream = new NamedPipeClientStream(".", session.PipeName, PipeDirection.InOut, PipeOptions.None);
				namedPipeClientStream.Connect(1000);
				using StreamReader streamReader = new StreamReader(namedPipeClientStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, 1024, leaveOpen: true);
				using StreamWriter streamWriter = new StreamWriter(namedPipeClientStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 1024, leaveOpen: true)
				{
					AutoFlush = true
				};
				streamWriter.WriteLine("PRELOAD|" + session.Nonce + "|1.12.31");
				if (streamReader.ReadLine() == "PRELOAD_OK|" + session.Nonce)
				{
					message = "Kernel PID-bound preloader proof accepted by the launcher.";
					return true;
				}
				ex = new InvalidDataException("The launcher rejected the preloader proof.");
			}
			catch (Exception ex2)
			{
				ex = ex2;
			}
			Thread.Sleep(200);
		}
		message = ((ex == null) ? "The launcher did not accept the preloader proof before its deadline." : ("The launcher did not accept the preloader proof: " + ex.GetBaseException().Message));
		return false;
	}

	private static void QuarantineFile(string sourcePath, string guardedRoot, string category)
	{
		if (!File.Exists(sourcePath))
		{
			return;
		}
		try
		{
			string text = Path.GetFullPath(guardedRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string fullPath = Path.GetFullPath(sourcePath);
			string text2 = text + Path.DirectorySeparatorChar;
			if (!fullPath.StartsWith(text2, StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidOperationException("Refusing to quarantine a file outside its guarded root.");
			}
			string path = fullPath.Substring(text2.Length);
			string text3 = Path.Combine(b(), P_2, path);
			Directory.CreateDirectory(Path.GetDirectoryName(text3));
			File.Move(fullPath, text3);
			LogGuard("WARNING", "BLOCKED [BEPINEX FILE] source=" + fullPath + "; action=preload quarantine; destination=" + text3);
		}
		catch (Exception ex)
		{
			LogGuard("ERROR", "Could not quarantine exclusive-mode file " + sourcePath + ": " + ex.Message);
			WriteRestartRequiredMarker("Could not quarantine exclusive-mode file: " + sourcePath + "; " + ex.Message);
		}
	}

	private static string GetOrCreateQuarantineSessionDirectory()
	{
		if (QuarantineSessionDirectory == null)
		{
			QuarantineSessionDirectory = Path.Combine(Paths.BepInExRootPath, "quarantine", "exclusive", DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff"));
			Directory.CreateDirectory(QuarantineSessionDirectory);
		}
		return QuarantineSessionDirectory;
	}

	private static void ClearRestartRequiredMarker()
	{
		try
		{
			string path = Path.Combine(Paths.BepInExRootPath, "GorillaGuard.restart-required");
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}
		catch (Exception ex)
		{
			LogGuard("ERROR", "Could not clear exclusive-mode marker: " + ex.Message);
		}
	}

	private static void WriteRestartRequiredMarker(string reason)
	{
		try
		{
			File.AppendAllText(Path.Combine(Paths.BepInExRootPath, "GorillaGuard.restart-required"), string.Format(arg1: (reason ?? "Unknown exclusive-mode failure").Replace('\r', ' ').Replace('\n', ' '), format: "[{0:O}] {1}{2}", arg0: DateTimeOffset.Now, arg2: Environment.NewLine));
		}
		catch (Exception ex)
		{
			LogGuard("ERROR", "Could not write exclusive-mode marker: " + ex.Message);
		}
	}

	private static bool PathsEqual(string left, string right)
	{
		try
		{
			return Path.GetFullPath(left).Equals(Path.GetFullPath(right), StringComparison.OrdinalIgnoreCase);
		}
		catch
		{
			return false;
		}
	}

	private static bool IsCodeFileExtension(string path)
	{
		string extension = Path.GetExtension(path);
		if (!extension.Equals(".dll", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".exe", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".winmd", StringComparison.OrdinalIgnoreCase))
		{
			return extension.Equals(".so", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static TypeDefinition FindType(ModuleDefinition module, string fullName)
	{
		foreach (TypeDefinition item in EnumerateTypes(module.Types))
		{
			if (item.FullName == fullName)
			{
				return item;
			}
		}
		LogGuard("WARNING", "Type not found; early lock skipped: " + fullName);
		return null;
	}

	private static FieldDefinition FindSingleField(TypeDefinition type, string fieldName)
	{
		if (type != null)
		{
			foreach (FieldDefinition field in type.Fields)
			{
				if (field.Name == fieldName && field.FieldType.MetadataType == MetadataType.Single)
				{
					return field;
				}
			}
		}
		LogGuard("WARNING", "Field not found; early lock skipped: " + (type?.FullName ?? "<missing type>") + "." + fieldName);
		return null;
	}

	[IteratorStateMachine(typeof(_003CAllTypes_003Ed__22))]
	private static IEnumerable<TypeDefinition> EnumerateTypes(IEnumerable<TypeDefinition> roots)
	{

		return new _003CAllTypes_003Ed__22(-2)
		{
			_003C_003E3__roots = roots
		};
	}

	private static void LogGuard(string level, string message)
	{
		if (level == "ERROR")
		{
			Log.LogError(message);
		}
		else if (level == "WARNING")
		{
			Log.LogWarning(message);
		}
		else
		{
			Log.LogInfo(message);
		}
		B(P_0, P_1);
	}

	private static void WriteDedicatedGuardLog(string level, string message)
	{
		try
		{
			string path = Path.Combine(Paths.PluginPath, "GorillaGuard.log");
			string contents = string.Format("[{0:O}] [PRELOADER/{1}] {2}{3}", DateTimeOffset.Now, P_0, P_1, Environment.NewLine);
			lock (FileLogLock)
			{
				File.AppendAllText(path, contents);
			}
		}
		catch (Exception ex)
		{
			Log.LogError("Could not write dedicated guard log: " + ex.Message);
		}
	}
}
