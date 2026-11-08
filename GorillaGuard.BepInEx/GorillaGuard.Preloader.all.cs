
#pragma region Properties/AssemblyInfo.cs
﻿using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

[module: RefSafetyRules(11)]
#pragma endregion

#pragma region GorillaGuard.Preloader/Patcher.cs
﻿using System;
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
				return Failure(ReleaseFailureKind.MissingManifest, "Missing signed release manifest: " + text);
			}
			if (!File.Exists(text2))
			{
				return Failure(ReleaseFailureKind.MissingSignature, "Missing release signature: " + text2);
			}
			byte[] array = File.ReadAllBytes(text);
			byte[] array2 = File.ReadAllBytes(text2);
			if (!VerifyManifestSignature(array, array2))
			{
				return Failure(ReleaseFailureKind.InvalidSignature, "The release manifest signature is invalid.");
			}
			Dictionary<string, string> dictionary = ParseManifest(array);
			if (!dictionary.TryGetValue("version", out var value) || !dictionary.TryGetValue("runtimeSha256", out var value2) || !dictionary.TryGetValue("preloaderSha256", out var value3) || !dictionary.TryGetValue("launcherSha256", out var value4) || !dictionary.TryGetValue("bootstrapSha256", out var value5) || !dictionary.TryGetValue("doorstopConfigSha256", out var value6) || !dictionary.TryGetValue("bepInExCoreTreeSha256", out var value7))
			{
				return Failure(ReleaseFailureKind.InvalidManifest, "The signed release manifest is missing required fields.");
			}
			if (!value.Equals("1.12.31", StringComparison.Ordinal))
			{
				return Failure(ReleaseFailureKind.VersionMismatch, "Signed release version " + value + " does not match required version 1.12.31.");
			}
			if (!IsSha256Hex(value2) || !C(value3) || !C(value4) || !C(value5) || !C(value6) || !C(value7))
			{
				return Failure(ReleaseFailureKind.InvalidManifest, "The signed release manifest contains an invalid SHA-256 value.");
			}
			if (dictionary.TryGetValue("gameNativeTreeSha256Alternate", out var value8) && !C(value8))
			{
				return Failure(ReleaseFailureKind.InvalidManifest, "The signed release manifest contains an invalid alternate native-code SHA-256 value.");
			}
			if (!File.Exists(text3))
			{
				return Failure(ReleaseFailureKind.MissingRuntime, "Missing required runtime: " + text3, value, value2);
			}
			string text9 = ComputeFileSha256(text3);
			if (!text9.Equals(value2, StringComparison.OrdinalIgnoreCase))
			{
				return Failure(ReleaseFailureKind.RuntimeHashMismatch, "Runtime hash mismatch: path=" + text3 + "; expected=" + value2 + "; actual=" + text9, value, value2, text9);
			}
			if (!File.Exists(text4))
			{
				return Failure(ReleaseFailureKind.MissingPreloader, "Missing required preloader: " + text4);
			}
			string text10 = ComputeFileSha256(text4);
			if (!text10.Equals(value3, StringComparison.OrdinalIgnoreCase))
			{
				return Failure(ReleaseFailureKind.PreloaderHashMismatch, "Preloader hash mismatch: path=" + text4 + "; expected=" + value3 + "; actual=" + text10);
			}
			if (verifyLauncher)
			{
				if (!File.Exists(text5))
				{
					return Failure(ReleaseFailureKind.MissingLauncher, "Missing required launcher: " + text5);
				}
				string text11 = ComputeFileSha256(text5);
				if (!text11.Equals(value4, StringComparison.OrdinalIgnoreCase))
				{
					return Failure(ReleaseFailureKind.LauncherHashMismatch, "Launcher hash mismatch: path=" + text5 + "; expected=" + value4 + "; actual=" + text11);
				}
			}
			ReleaseVerificationResult releaseVerificationResult = VerifyFileHash(text6, value5, "Doorstop bootstrap");
			if (!releaseVerificationResult.Success)
			{
				return releaseVerificationResult;
			}
			ReleaseVerificationResult releaseVerificationResult2 = VerifyFileHash(text7, value6, "Doorstop configuration");
			if (!releaseVerificationResult2.Success)
			{
				return releaseVerificationResult2;
			}
			if (dictionary.TryGetValue("openvrApiSha256", out var value9))
			{
				if (!C(value9))
				{
					return Failure(ReleaseFailureKind.InvalidManifest, "Invalid signed OpenVR API repair hash.");
				}
				ReleaseVerificationResult releaseVerificationResult3 = VerifyFileHash(Path.Combine(bepInExRoot, "repair", "openvr_api.dll"), value9, "OpenVR API repair payload");
				if (!releaseVerificationResult3.Success)
				{
					return releaseVerificationResult3;
				}
			}
			if (!Directory.Exists(text8))
			{
				return Failure(ReleaseFailureKind.HostIntegrityMismatch, "Missing signed BepInEx core: " + text8);
			}
			string text12 = ComputeDirectoryTreeSha256(text8);
			ReleaseVerificationResult releaseVerificationResult4 = VerifyCoreFileInventory(dictionary, text8);
			if (!releaseVerificationResult4.Success)
			{
				return releaseVerificationResult4;
			}
			if (!text12.Equals(value7, StringComparison.OrdinalIgnoreCase))
			{
				return Failure(ReleaseFailureKind.HostIntegrityMismatch, "Signed BepInEx core hash mismatch: expected=" + value7 + "; actual=" + text12 + "; path=" + text8);
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
			return Failure(ReleaseFailureKind.VerificationError, "Release verification error: " + ex.Message);
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
			Modulus = Convert.FromBase64String("2Lm8yNQ1sEC0uXbgt2O4vOqpDIVhaKSGM3Zkhj4pyxJX8slkMyECsMrSzEDhbYFrkYwj9tHTS61g8zuHgKV7GpoKNpRkz28kkJAD4ChJhMucPGnDfle1gclUL99OU357PuaZqFWoBlmXstYzQqmVPajCnwQKQSNCsOAkAEEza5ba9uIZD153eimFDqKpGAxUduST7eRgaBNZWtcdw6sTdn13/Ft/mh/gCzjGm0OFurBw4J1vq2LnHZ0B58oAivsCpTkZzCtyYScGI4afGTxP+3EddgRrjKGE82MnQrocKbVRecy5ReIsdwgq5bgb/26nS+V87FD4KH7ab8cKBSdePbLy28W2GELxQnAI+6kZAeCSpo0lmws83gZKySDjrEglJ4R1/Z4EDT28OqN+N4ntSqnTF8vAlBrtTs8uiAd1gGi2Fa2z0f7D5Y2AIerlmQMp3Jg1GgGKAoouswmDNR1Xh2MS/rOo6UklzkFsEsh/d6ZCsh6Y0UGn/SLqk/eevM7B"),
			Exponent = Convert.FromBase64String("AQAB")
		});
		return rSA.VerifyData(manifestBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
	}

	internal static IReadOnlyDictionary<string, string> ReadVerifiedManifest(string bepInExRoot)
	{
		ReleaseVerificationResult releaseVerificationResult = Verify(bepInExRoot);
		if (!releaseVerificationResult.Success)
		{
			throw new InvalidDataException("Cannot read an untrusted release manifest: " + releaseVerificationResult.Message);
		}
		return ParseManifest(File.ReadAllBytes(Path.Combine(bepInExRoot, "GorillaGuard.manifest")));
	}

	internal static ReleaseVerificationResult VerifyHostBaseline(string gameRootPath, string bepInExRootOverride = null, bool verifyBepInExHost = true)
	{
		try
		{
			string fullPath = Path.GetFullPath(gameRootPath);
			string text = Path.Combine(fullPath, "BepInEx");
			string text2 = (string.IsNullOrWhiteSpace(bepInExRootOverride) ? text : Path.GetFullPath(bepInExRootOverride));
			ReleaseVerificationResult releaseVerificationResult = Verify(text2);
			if (!releaseVerificationResult.Success)
			{
				return releaseVerificationResult;
			}
			Dictionary<string, string> dictionary = ParseManifest(File.ReadAllBytes(Path.Combine(text2, "GorillaGuard.manifest")));
			List<HostCheck> list = new List<HostCheck>
			{
				new HostCheck("gameExecutableSha256", Path.Combine(fullPath, "Gorilla Tag.exe"), isTree: false, "Gorilla Tag executable"),
				new HostCheck("unityPlayerSha256", Path.Combine(fullPath, "UnityPlayer.dll"), isTree: false, "UnityPlayer"),
				new HostCheck("gameManagedTreeSha256", Path.Combine(fullPath, "Gorilla Tag_Data", "Managed"), isTree: true, "managed game code"),
				new HostCheck("gameNativeTreeSha256", Path.Combine(fullPath, "Gorilla Tag_Data", "Plugins"), isTree: true, "native game code")
			};
			if (verifyBepInExHost)
			{
				list.Insert(0, new HostCheck("bepInExCoreTreeSha256", Path.Combine(text, "core"), isTree: true, "BepInEx core code"));
				list.Insert(0, new HostCheck("doorstopConfigSha256", Path.Combine(fullPath, "doorstop_config.ini"), isTree: false, "Doorstop configuration"));
				list.Insert(0, new HostCheck("bootstrapSha256", Path.Combine(fullPath, "winhttp.dll"), isTree: false, "Doorstop bootstrap"));
			}
			foreach (HostCheck item in list)
			{
				if (!dictionary.TryGetValue(item.ManifestKey, out var value) || !IsSha256Hex(value))
				{
					return Failure(ReleaseFailureKind.InvalidManifest, "The signed release manifest is missing a valid host baseline field: " + item.ManifestKey);
				}
				if (item.IsTree ? (!Directory.Exists(item.Path)) : (!File.Exists(item.Path)))
				{
					return Failure(ReleaseFailureKind.HostIntegrityMismatch, "Required " + item.Label + " is missing: " + item.Path);
				}
				string text3 = (item.IsTree ? ComputeDirectoryTreeSha256(item.Path) : ComputeFileSha256(item.Path));
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
						if (dictionary.TryGetValue(item.ManifestKey + text4, out var value2) && IsSha256Hex(value2) && text3.Equals(value2, StringComparison.OrdinalIgnoreCase))
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					return Failure(ReleaseFailureKind.HostIntegrityMismatch, "Host integrity mismatch for " + item.Label + ": expected=" + value + "; actual=" + text3 + "; path=" + item.Path + ". " + ((item.ManifestKey == "gameManagedTreeSha256") ? A(dictionary, item.Path) : "") + "This can mean a different game update or extra/modified code, and is not by itself proof of cheating. Verify Gorilla Tag files through Steam and install a guard release approved for that game build.");
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
			return Failure(ReleaseFailureKind.VerificationError, "Host baseline verification error: " + ex.Message);
		}
	}

	internal static string DiagnoseManagedFileDifferences(Dictionary<string, string> manifest, string managedRootPath)
	{
		KeyValuePair<string, string>[] array = manifest.Where((KeyValuePair<string, string> entry) => entry.Key.StartsWith("managedFileSha256.", StringComparison.OrdinalIgnoreCase)).ToArray();
		if (array.Length == 0)
		{
			return "File-level inventory unavailable in this older package. ";
		}
		string text = Path.GetFullPath(managedRootPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
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
			if (!fullPath.StartsWith(text, StringComparison.OrdinalIgnoreCase) || !IsCodeFile(fullPath) || !IsSha256Hex(keyValuePair.Value) || !hashSet.Add(fullPath))
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
			else if (!string.Equals(ComputeFileSha256(fullPath), keyValuePair.Value, StringComparison.OrdinalIgnoreCase))
			{
				num++;
				if (list.Count < 12)
				{
					list.Add("CHANGED " + text2);
				}
			}
		}
		foreach (string item in Directory.GetFiles(managedRootPath, "*", SearchOption.AllDirectories).Where(IsCodeFile))
		{
			if (!hashSet.Contains(Path.GetFullPath(item)))
			{
				num3++;
				if (list.Count < 12)
				{
					list.Add("EXTRA " + Path.GetRelativePath(managedRootPath, item));
				}
			}
		}
		return string.Format("Managed-file diagnosis: changed={0}, missing={1}, extra={2}. ", num, num2, num3) + string.Join("; ", list) + ((num + num2 + num3 > list.Count) ? "; additional differences omitted" : "") + ". ";
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
			stringBuilder.Append(b.ToString("X2"));
		}
		return stringBuilder.ToString();
	}

	private static ReleaseVerificationResult VerifyFileHash(string path, string expectedHash, string label)
	{
		if (!File.Exists(P_0))
		{
			return Failure(ReleaseFailureKind.HostIntegrityMismatch, "Missing signed " + P_2 + ": " + P_0);
		}
		string text = a(P_0);
		if (!text.Equals(P_1, StringComparison.OrdinalIgnoreCase))
		{
			return Failure(ReleaseFailureKind.HostIntegrityMismatch, "Signed " + P_2 + " hash mismatch: expected=" + P_1 + "; actual=" + text + "; path=" + P_0);
		}
		return new ReleaseVerificationResult
		{
			Success = true,
			FailureKind = ReleaseFailureKind.None
		};
	}

	internal static string ComputeDirectoryTreeSha256(string directoryPath)
	{
		string fullRoot = Path.GetFullPath(directoryPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
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
				if (IsCodeFile(item2))
				{
					if ((File.GetAttributes(item2) & FileAttributes.ReparsePoint) != 0)
					{
						throw new InvalidDataException("A protected code file cannot be a reparse point: " + item2);
					}
					list.Add(item2);
				}
			}
		}
		list.Sort((string left, string right) => StringComparer.Ordinal.Compare(GetNormalizedRelativePath(fullRoot, left), GetNormalizedRelativePath(fullRoot, right)));
		using SHA256 sHA = SHA256.Create();
		using CryptoStream cryptoStream = new CryptoStream(Stream.Null, sHA, CryptoStreamMode.Write);
		using BinaryWriter binaryWriter = new BinaryWriter(cryptoStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), leaveOpen: true);
		foreach (string item3 in list)
		{
			string s = GetNormalizedRelativePath(fullRoot, item3);
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
		return ToHex(sHA.Hash);
	}

	private static ReleaseVerificationResult VerifyCoreFileInventory(Dictionary<string, string> manifest, string coreRootPath)
	{
		KeyValuePair<string, string>[] array = manifest.Where((KeyValuePair<string, string> entry) => P_0.Key.StartsWith("coreFileSha256.", StringComparison.OrdinalIgnoreCase)).ToArray();
		if (array.Length == 0)
		{
			return new ReleaseVerificationResult
			{
				Success = true
			};
		}
		string text = Path.GetFullPath(managedRootPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		KeyValuePair<string, string>[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			KeyValuePair<string, string> keyValuePair = array2[i];
			string path = keyValuePair.Key.Substring("coreFileSha256.".Length).Replace('/', Path.DirectorySeparatorChar);
			string fullPath = Path.GetFullPath(Path.Combine(text, path));
			if (!fullPath.StartsWith(text, StringComparison.OrdinalIgnoreCase) || !IsCodeFile(fullPath) || !IsSha256Hex(keyValuePair.Value) || !hashSet.Add(fullPath))
			{
				return Failure(ReleaseFailureKind.InvalidManifest, "Invalid signed core file inventory entry: " + keyValuePair.Key);
			}
			if (!File.Exists(fullPath))
			{
				return Failure(ReleaseFailureKind.HostIntegrityMismatch, "Missing signed core file: " + fullPath);
			}
			string text2 = ComputeFileSha256(fullPath);
			if (!string.Equals(text2, keyValuePair.Value, StringComparison.OrdinalIgnoreCase))
			{
				return Failure(ReleaseFailureKind.HostIntegrityMismatch, "Core file hash mismatch: path=" + fullPath + "; expected=" + keyValuePair.Value + "; actual=" + text2);
			}
		}
		foreach (string item in Directory.GetFiles(managedRootPath, "*", SearchOption.AllDirectories).Where(IsCodeFile))
		{
			if (!hashSet.Contains(Path.GetFullPath(item)))
			{
				return Failure(ReleaseFailureKind.HostIntegrityMismatch, "Unexpected executable code in signed core: " + item);
			}
		}
		return new ReleaseVerificationResult
		{
			Success = true
		};
	}

	private static string GetNormalizedRelativePath(string rootPath, string path)
	{
		return Path.GetRelativePath(rootPath, path).Replace('\\', '/').ToLowerInvariant();
	}

	private static bool IsCodeFile(string path)
	{
		string extension = Path.GetExtension(path);
		if (!extension.Equals(".dll", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".exe", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".winmd", StringComparison.OrdinalIgnoreCase))
		{
			return extension.Equals(".so", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static string ToHex(byte[] bytes)
	{
		StringBuilder stringBuilder = new StringBuilder(bytes.Length * 2);
		foreach (byte b in bytes)
		{
			stringBuilder.Append(b.ToString("X2"));
		}
		return stringBuilder.ToString();
	}

	private static Dictionary<string, string> ParseManifest(byte[] manifestBytes)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
		string[] array = Encoding.UTF8.GetString(manifestBytes).Split(new string[2]
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

	private static bool IsSha256Hex(string value)
	{
		if (value == null || value.Length != 64)
		{
			return false;
		}
		foreach (char c in value)
		{
			if ((c < '0' || c > '9') && (c < 'a' || c > 'f') && (c < 'A' || c > 'F'))
			{
				return false;
			}
		}
		return true;
	}

	private static ReleaseVerificationResult Failure(ReleaseFailureKind kind, string message, string version = null, string expectedRuntimeHash = null, string actualRuntimeHash = null)
	{
		return new ReleaseVerificationResult
		{
			Success = false,
			FailureKind = kind,
			Message = message,
			Version = version,
			ExpectedRuntimeHash = expectedRuntimeHash,
			ActualRuntimeHash = actualRuntimeHash
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
	internal static string GetSessionFilePath(string gameRootPath)
	{
		string s = Path.GetFullPath(gameRootPath).TrimEnd('\\', '/').ToUpperInvariant();
		using SHA256 sHA = SHA256.Create();
		string text = BitConverter.ToString(sHA.ComputeHash(Encoding.UTF8.GetBytes(s))).Replace("-", "");
		return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GorillaGuard", "sessions", text, "GorillaGuard.session");
	}
}
#pragma endregion
