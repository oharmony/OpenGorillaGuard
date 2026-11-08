using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using GorillaGuard.Security;
using Microsoft.Win32;

namespace GorillaGuard.Launcher;

internal sealed class VrPreflight : IDisposable
{
	private sealed class ModuleApprovalRoot
	{
		internal string Path { get; }

		internal HashSet<string> Publishers { get; }

		internal ModuleApprovalRoot(string path, HashSet<string> publishers)
		{
			Path = path;
			Publishers = publishers;
		}
	}

		private sealed class _003CReadPaths_003Ed__35 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private string _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private JsonElement root;

		public JsonElement _003C_003E3__root;

		private string key;

		public string _003C_003E3__key;

		private JsonElement.ArrayEnumerator _003C_003E7__wrap1;

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

				public _003CReadPaths_003Ed__35(int _003C_003E1__state)
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
			_003C_003E7__wrap1 = default(JsonElement.ArrayEnumerator);
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
				{
					_003C_003E1__state = -1;
					if (root.ValueKind != JsonValueKind.Object)
					{
						throw new InvalidDataException(_"OpenVR paths JSON must be an object.");
					}
					if (!root.TryGetProperty(key, out var value) || value.ValueKind == JsonValueKind.Null)
					{
						return false;
					}
					if (value.ValueKind != JsonValueKind.Array)
					{
						throw new InvalidDataException(_"OpenVR " + key + _" must be an array.");
					}
					if (value.GetArrayLength() > 128)
					{
						throw new InvalidDataException(_"Too many OpenVR paths.");
					}
					_003C_003E7__wrap1 = value.EnumerateArray().GetEnumerator();
					_003C_003E1__state = -3;
					break;
				}
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				if (_003C_003E7__wrap1.MoveNext())
				{
					JsonElement current = _003C_003E7__wrap1.Current;
					string text = ((current.ValueKind == JsonValueKind.String) ? current.GetString() : null);
					if (string.IsNullOrWhiteSpace(text) || !Path.IsPathFullyQualified(text))
					{
						throw new InvalidDataException(_"OpenVR " + key + _" contains an invalid absolute path.");
					}
					_003C_003E2__current = Path.GetFullPath(text);
					_003C_003E1__state = 1;
					return true;
				}
				a();
				_003C_003E7__wrap1 = default(JsonElement.ArrayEnumerator);
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
			((IDisposable)_003C_003E7__wrap1).Dispose();
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
			_003CReadPaths_003Ed__35 _003CReadPaths_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CReadPaths_003Ed__ = this;
			}
			else
			{
				_003CReadPaths_003Ed__ = new _003CReadPaths_003Ed__35(0);
			}
			_003CReadPaths_003Ed__.root = _003C_003E3__root;
			_003CReadPaths_003Ed__.key = _003C_003E3__key;
			return _003CReadPaths_003Ed__;
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

		private sealed class _003CWalkFiles_003Ed__66 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private string _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private string root;

		public string _003C_003E3__root;

		private Stack<string> _003Cpending_003E5__2;

		private int _003Centries_003E5__3;

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

				public _003CWalkFiles_003Ed__66(int _003C_003E1__state)
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
					goto IL_0195;
				}
				_003C_003E1__state = -1;
				e(root);
				_003Cpending_003E5__2 = new Stack<string>();
				_003Cpending_003E5__2.Push(root);
				_003Centries_003E5__3 = 0;
				goto IL_01bb;
				IL_0195:
				while (_003C_003E7__wrap3.MoveNext())
				{
					string current = _003C_003E7__wrap3.Current;
					if (++_003Centries_003E5__3 > 20000)
					{
						throw new InvalidDataException(_"VR directory exceeds the audit limit: " + root);
					}
					FileAttributes attributes = File.GetAttributes(current);
					if ((attributes & FileAttributes.ReparsePoint) != 0)
					{
						throw new InvalidDataException(_"Redirected VR entry: " + current);
					}
					if ((attributes & FileAttributes.Directory) != 0)
					{
						_003Cpending_003E5__2.Push(current);
						continue;
					}
					_003C_003E2__current = current;
					_003C_003E1__state = 1;
					return true;
				}
				a();
				_003C_003E7__wrap3 = null;
				goto IL_01bb;
				IL_01bb:
				if (_003Cpending_003E5__2.Count != 0)
				{
					_003C_003E7__wrap3 = Directory.EnumerateFileSystemEntries(_003Cpending_003E5__2.Pop()).GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_0195;
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
			_003CWalkFiles_003Ed__66 _003CWalkFiles_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CWalkFiles_003Ed__ = this;
			}
			else
			{
				_003CWalkFiles_003Ed__ = new _003CWalkFiles_003Ed__66(0);
			}
			_003CWalkFiles_003Ed__.root = _003C_003E3__root;
			return _003CWalkFiles_003Ed__;
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

	private readonly string _gameRoot;

	private readonly string[] _steamRoots;

	private readonly Action<string, string> _log;

	private readonly List<FileStream> _locks = new List<FileStream>();

	private readonly HashSet<string> _lockedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly List<(string Path, string Backup, string SanitizedHash)> _restores = new List<(string, string, string)>();

	private readonly HashSet<string> _approvedHashes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly HashSet<string> _revokedHashes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly HashSet<string> _approvedSpaceCalibratorDrivers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, string> _registrationHashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private readonly HashSet<string> _absentRegistrationPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, Dictionary<string, string>> _securityTrees = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

	private readonly HashSet<string> _configurationPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly HashSet<string> _initialConfigurationPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private string? _xrRegistrationBaseline;

	private readonly Dictionary<string, string> _layerManifestHashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private readonly HashSet<string> _vrServerPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly HashSet<string> _vrCompositorPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly List<ModuleApprovalRoot> _moduleRoots = new List<ModuleApprovalRoot>();

	private readonly HashSet<string> _approvedLoadedModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly VrProcessContinuity _serverContinuity = new VrProcessContinuity();

	private readonly VrProcessContinuity _compositorContinuity = new VrProcessContinuity();

	private readonly List<VrCodeWatcher> _codeWatchers = new List<VrCodeWatcher>();

	private readonly Dictionary<string, string> _manifest;

	private readonly string _manifestRoot;

	private readonly string _backupRoot;

	private static readonly HashSet<string> ValvePublishers = new HashSet<string>(StringComparer.Ordinal)
	{
		_"Valve",
		_"Valve Corp.",
		_"Valve Corporation"
	};

	private static readonly HashSet<string> HeadsetPublishers = new HashSet<string>(StringComparer.Ordinal)
	{
		_"Valve",
		_"Valve Corp.",
		_"Valve Corporation",
		_"Oculus VR, LLC",
		_"Oculus VR, Inc.",
		_"Meta Platforms Technologies, LLC",
		_"HTC Corporation",
		_"Microsoft Corporation"
	};

	private static readonly JsonDocumentOptions JsonOptions = new JsonDocumentOptions
	{
		AllowTrailingCommas = true,
		CommentHandling = JsonCommentHandling.Skip,
		MaxDepth = 64
	};

	internal VrProcessIdentity? ServerIdentity => _serverContinuity.Bound;

	internal VrProcessIdentity? CompositorIdentity => _compositorContinuity.Bound;

	internal VrPreflight(string gameRoot, IEnumerable<string> steamRoots, Action<string, string> log, string? trustedManifestRoot = null, string? backupRoot = null)
	{
		_gameRoot = gameRoot;
		_steamRoots = steamRoots.ToArray();
		_log = log;
		_backupRoot = backupRoot ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _"GorillaGuard", _"backups");
		string text = (_manifestRoot = trustedManifestRoot ?? Path.Combine(gameRoot, _"BepInEx"));
		ReleaseVerificationResult releaseVerificationResult = ReleaseVerifier.Verify(text);
		if (!releaseVerificationResult.Success)
		{
			throw new InvalidDataException(releaseVerificationResult.Message);
		}
		byte[] array = File.ReadAllBytes(Path.Combine(text, _"GorillaGuard.manifest"));
		if (!ReleaseVerifier.A(array, File.ReadAllBytes(Path.Combine(text, _"GorillaGuard.manifest.sig"))))
		{
			throw new InvalidDataException(_"VR policy manifest signature failed.");
		}
		_manifest = (from P_0 in Encoding.UTF8.GetString(array).Split('\n', StringSplitOptions.RemoveEmptyEntries)
			select P_0.Trim().Split('=', 2) into P_0
			where P_0.Length == 2
			select P_0).ToDictionary<string[], string, string>((string[] P_0) => P_0[0], (string[] P_0) => P_0[1], StringComparer.Ordinal);
		string[] array2;
		if (_manifest.TryGetValue(_"vrRevokedCodeSha256", out string value))
		{
			array2 = value.Split(';', StringSplitOptions.RemoveEmptyEntries);
			foreach (string text2 in array2)
			{
				if (text2.Length != 64 || !text2.All(Uri.IsHexDigit))
				{
					throw new InvalidDataException(_"Invalid signed VR revocation.");
				}
				_revokedHashes.Add(text2);
			}
		}
		if (_manifest.TryGetValue(_"vrApprovedCodeSha256", out string value2))
		{
			array2 = value2.Split(';', StringSplitOptions.RemoveEmptyEntries);
			foreach (string text3 in array2)
			{
				if (text3.Length != 64 || !text3.All(Uri.IsHexDigit))
				{
					throw new InvalidDataException(_"Invalid signed VR code approval.");
				}
				_approvedHashes.Add(text3);
			}
		}
		if (!_manifest.TryGetValue(_"spaceCalibratorDriverSha256", out string value3))
		{
			return;
		}
		array2 = value3.Split(';', StringSplitOptions.RemoveEmptyEntries);
		foreach (string text4 in array2)
		{
			if (text4.Length != 64 || !text4.All(Uri.IsHexDigit))
			{
				throw new InvalidDataException(_"Invalid signed SpaceCalibrator driver approval.");
			}
			_approvedSpaceCalibratorDrivers.Add(text4);
		}
	}

	internal void A(bool P_0 = true)
	{
		b();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		HashSet<string> hashSet2 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _"openvr");
		string[] array = new string[2]
		{
			_"openvrpaths.vrpath",
			_"openvrpaths.vrpaths"
		};
		foreach (string path2 in array)
		{
			string text = Path.Combine(path, path2);
			if (!File.Exists(text))
			{
				_absentRegistrationPaths.Add(Path.GetFullPath(text));
				continue;
			}
			a(text);
			using JsonDocument jsonDocument = d(text);
			foreach (string item2 in A(jsonDocument.RootElement, _"runtime"))
			{
				hashSet.Add(item2);
			}
			foreach (string item3 in A(jsonDocument.RootElement, _"config"))
			{
				hashSet2.Add(Path.Combine(item3, _"steamvr.vrsettings"));
			}
			string[] array2 = new string[2]
			{
				_"external_drivers",
				_"externaldrivers"
			};
			foreach (string text2 in array2)
			{
				foreach (string item4 in A(jsonDocument.RootElement, text2))
				{
					A(item4, HeadsetPublishers, true);
				}
			}
		}
		array = _steamRoots;
		foreach (string text3 in array)
		{
			A(text3, ValvePublishers);
			hashSet2.Add(Path.Combine(text3, _"config", _"steamvr.vrsettings"));
			string text4 = Path.Combine(text3, _"steamapps", _"common", _"SteamVR");
			if (Directory.Exists(text4))
			{
				hashSet.Add(text4);
			}
		}
		foreach (string item5 in hashSet)
		{
			string text5 = Path.Combine(item5, _"bin", _"win64", _"vrserver.exe");
			a(text5, ValvePublishers);
			_vrServerPaths.Add(Path.GetFullPath(text5));
			string text6 = Path.Combine(item5, _"bin", _"win64", _"vrcompositor.exe");
			a(text6, ValvePublishers);
			_vrCompositorPaths.Add(Path.GetFullPath(text6));
			string text7 = Path.Combine(item5, _"bin");
			A(text7, F(text7).ToArray());
			A(item5, ValvePublishers);
			A(Path.Combine(item5, _"drivers"), ValvePublishers, true);
			string item = Path.Combine(item5, _"resources", _"settings", _"default.vrsettings");
			hashSet2.Add(item);
		}
		_xrRegistrationBaseline = B();
		c();
		if (_xrRegistrationBaseline != B())
		{
			throw new InvalidDataException(_"OpenXR registration changed during preflight; retry protected startup.");
		}
		a(P_0);
		foreach (string item6 in hashSet2)
		{
			_configurationPaths.Add(Path.GetFullPath(item6));
			if (File.Exists(item6))
			{
				a(item6, P_0);
				_initialConfigurationPaths.Add(Path.GetFullPath(item6));
			}
		}
		_log(_"INFO", _"VR PREFLIGHT PASS: registered runtimes, drivers, OpenXR layers, prediction settings, and continuous VR session baselines audited.");
	}

	[IteratorStateMachine(typeof(_003CReadPaths_003Ed__35))]
	internal static IEnumerable<string> A(JsonElement P_0, string P_1)
	{

		return new _003CReadPaths_003Ed__35(-2)
		{
			_003C_003E3__root = P_0,
			_003C_003E3__key = P_1
		};
	}

	private void A(string P_0, HashSet<string> P_1, bool P_2)
	{
		int num = 0;
		string[] array = F(P_0).ToArray();
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (Path.GetFileName(text).StartsWith(_"driver", StringComparison.OrdinalIgnoreCase) && text.EndsWith(_".dll", StringComparison.OrdinalIgnoreCase))
			{
				a(text, P_1, true);
				num++;
			}
		}
		if (P_2 && num == 0)
		{
			throw new InvalidDataException(_"VR driver directory has no verifiable driver binaries: " + P_0);
		}
		A(P_0, P_1);
		A(P_0, array);
	}

	internal void A(string P_0)
	{
		a(P_0, ValvePublishers, true);
	}

	internal void a(string P_0, HashSet<string> P_1, bool P_2 = false)
	{
		e(P_0);
		FileStream fileStream = new FileStream(P_0, FileMode.Open, FileAccess.Read, FileShare.Read);
		try
		{
			string text = Convert.ToHexString(SHA256.HashData(fileStream));
			A(text, _revokedHashes, P_0);
			bool flag = P_2 && Path.GetFileName(P_0).Equals(_"driver_01spacecalibrator.dll", StringComparison.OrdinalIgnoreCase) && _approvedSpaceCalibratorDrivers.Contains(text);
			bool flag2 = _approvedHashes.Contains(text) || flag;
			if (!flag2)
			{
				string text2 = Authenticode.A(P_0);
				if (!P_1.Contains(text2))
				{
					throw new InvalidDataException(_"VR binary publisher is not approved: " + text2 + _"; path=" + P_0 + _"; sha256=" + text);
				}
				flag2 = true;
			}
			A(P_0, fileStream);
			if (flag)
			{
				_log(_"INFO", _"SpaceCalibrator approved by signed upstream-release hash; driver and calibration settings left unchanged: " + P_0);
			}
			if (flag2)
			{
				_log(_"INFO", _"VR binary verified: " + P_0 + _"; sha256=" + text);
			}
		}
		catch
		{
			fileStream.Dispose();
			throw;
		}
	}

	internal void A()
	{
		b();
		foreach (VrCodeWatcher codeWatcher in _codeWatchers)
		{
			string failure = codeWatcher.Failure;
			if (failure != null)
			{
				throw new InvalidDataException(failure);
			}
		}
		if (_xrRegistrationBaseline != null && _xrRegistrationBaseline != B())
		{
			throw new InvalidDataException(_"OpenXR runtime or layer registration changed; restart the protected session.");
		}
		foreach (KeyValuePair<string, string> layerManifestHash in _layerManifestHashes)
		{
			if (!C(layerManifestHash.Key).Equals(layerManifestHash.Value, StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException(_"OpenXR layer manifest changed: " + layerManifestHash.Key);
			}
		}
		foreach (string absentRegistrationPath in _absentRegistrationPaths)
		{
			if (File.Exists(absentRegistrationPath))
			{
				throw new InvalidDataException(_"OpenVR registration appeared during protected play: " + absentRegistrationPath);
			}
		}
		foreach (KeyValuePair<string, string> registrationHash in _registrationHashes)
		{
			if (!File.Exists(registrationHash.Key))
			{
				throw new InvalidDataException(_"OpenVR registration disappeared during protected play: " + registrationHash.Key);
			}
			if (!C(registrationHash.Key).Equals(registrationHash.Value, StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException(_"OpenVR registration changed during protected play: " + registrationHash.Key);
			}
		}
		foreach (KeyValuePair<string, Dictionary<string, string>> securityTree in _securityTrees)
		{
			string text = a(securityTree.Key, securityTree.Value.Keys);
			if (text != null)
			{
				throw new InvalidDataException(text);
			}
		}
		foreach (string configurationPath in _configurationPaths)
		{
			A(configurationPath, _initialConfigurationPaths.Contains(configurationPath));
		}
		C();
		A(_"vrcompositor", _vrCompositorPaths, _compositorContinuity);
	}

	internal static void A(string P_0, IEnumerable<string> P_1, string P_2)
	{
		if (P_1.Contains<string>(P_0, StringComparer.OrdinalIgnoreCase))
		{
			throw new InvalidDataException(_"VR code was revoked by the signed release policy: " + P_2 + _"; sha256=" + P_0);
		}
	}

	internal static void A(string P_0, bool P_1)
	{
		if (!File.Exists(P_0))
		{
			if (P_1)
			{
				throw new InvalidDataException(_"SteamVR configuration disappeared during protected play: " + P_0);
			}
			return;
		}
		ReadOnlySpan<byte> utf8Json = E(P_0);
		JsonDocumentOptions jsonOptions = JsonOptions;
		if (!A((JsonNode.Parse(utf8Json, null, jsonOptions) as JsonObject) ?? throw new InvalidDataException(_"SteamVR settings must be a JSON object: " + P_0)))
		{
			return;
		}
		throw new InvalidDataException(_"Restricted SteamVR prediction override appeared during protected play: " + P_0);
	}

	internal static void a()
	{
		string[] array = new string[2]
		{
			_"vrserver",
			_"vrcompositor"
		};
		for (int i = 0; i < array.Length; i++)
		{
			Process[] processesByName = Process.GetProcessesByName(array[i]);
			try
			{
				if (processesByName.Length != 0)
				{
					throw new InvalidDataException(_"Close SteamVR before starting Gorilla Guard. Protected play requires a fresh verified VR session.");
				}
			}
			finally
			{
				Process[] array2 = processesByName;
				for (int j = 0; j < array2.Length; j++)
				{
					array2[j].Dispose();
				}
			}
		}
	}

	private static string B()
	{
		List<string> list = new List<string>();
		RegistryHive[] array = new RegistryHive[2]
		{
			RegistryHive.LocalMachine,
			RegistryHive.CurrentUser
		};
		for (int i = 0; i < array.Length; i++)
		{
			RegistryHive registryHive = array[i];
			RegistryView[] array2 = new RegistryView[2]
			{
				RegistryView.Registry64,
				RegistryView.Registry32
			};
			for (int j = 0; j < array2.Length; j++)
			{
				RegistryView view = array2[j];
				string[] array3 = new string[3]
				{
					_"SOFTWARE\\Khronos\\OpenXR\\1",
					_"SOFTWARE\\Khronos\\OpenXR\\1\\ApiLayers\\Implicit",
					_"SOFTWARE\\Khronos\\OpenXR\\1\\ApiLayers\\Explicit"
				};
				foreach (string text in array3)
				{
					using RegistryKey registryKey = RegistryKey.OpenBaseKey(registryHive, view);
					using RegistryKey registryKey2 = registryKey.OpenSubKey(text);
					list.Add(registryHive.ToString() + _"|" + view.ToString() + _"|" + text + _"|present=" + (registryKey2 != null));
					if (registryKey2 == null)
					{
						continue;
					}
					foreach (string item in registryKey2.GetValueNames().OrderBy<string, string>((string P_0) => P_0, StringComparer.Ordinal))
					{
						if (!text.EndsWith(_"OpenXR\\1", StringComparison.Ordinal) || !(item != _"ActiveRuntime"))
						{
							object value = registryKey2.GetValue(item, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
							list.Add(JsonSerializer.Serialize(new
							{
								hive = registryHive,
								view = view,
								subkey = text,
								name = item,
								kind = registryKey2.GetValueKind(item),
								value = value
							}));
						}
					}
				}
			}
		}
		return string.Join(_"\n", list);
	}

	private static void b()
	{
		string[] array = new string[5]
		{
			_"VR_PATHREG_OVERRIDE",
			_"VR_OVERRIDE",
			_"VR_DRIVER_PATH",
			_"XR_API_LAYER_PATH",
			_"XR_ENABLE_API_LAYERS"
		};
		foreach (string text in array)
		{
			if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(text)))
			{
				throw new InvalidDataException(_"VR preflight: remove the runtime/layer override " + text + _" before protected play.");
			}
		}
	}

	private void a(string P_0)
	{
		string fullPath = Path.GetFullPath(P_0);
		e(fullPath);
		FileStream fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
		try
		{
			_registrationHashes[fullPath] = Convert.ToHexString(SHA256.HashData(fileStream));
			A(fullPath, fileStream);
		}
		catch
		{
			fileStream.Dispose();
			throw;
		}
	}

	private void A(string P_0, IEnumerable<string> P_1)
	{
		string fullPath = Path.GetFullPath(P_0);
		_codeWatchers.Add(new VrCodeWatcher(fullPath));
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach (string item in P_1.Where(b))
		{
			string fullPath2 = Path.GetFullPath(item);
			FileStream fileStream = new FileStream(fullPath2, FileMode.Open, FileAccess.Read, FileShare.Read);
			try
			{
				dictionary[fullPath2] = Convert.ToHexString(SHA256.HashData(fileStream));
				A(fullPath2, fileStream);
			}
			catch
			{
				fileStream.Dispose();
				throw;
			}
		}
		_securityTrees[fullPath] = dictionary;
	}

	private static HashSet<string> B(string P_0)
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (string item in F(P_0).Where(b))
		{
			hashSet.Add(Path.GetFullPath(item));
		}
		return hashSet;
	}

	internal static string? a(string P_0, IEnumerable<string> P_1)
	{
		HashSet<string> current = B(P_0);
		HashSet<string> expected = new HashSet<string>(P_1.Select(Path.GetFullPath), StringComparer.OrdinalIgnoreCase);
		string text = expected.FirstOrDefault((string P_0) => !current.Contains(P_0));
		if (text != null)
		{
			return _"Protected VR code or manifest disappeared during play: " + text;
		}
		string text2 = current.FirstOrDefault((string P_0) => !expected.Contains(P_0));
		if (text2 != null)
		{
			return _"New VR code or manifest appeared during protected play: " + text2;
		}
		return null;
	}

	private static bool b(string P_0)
	{
		string extension = Path.GetExtension(P_0);
		if (!extension.Equals(_".dll", StringComparison.OrdinalIgnoreCase) && !extension.Equals(_".exe", StringComparison.OrdinalIgnoreCase))
		{
			return extension.Equals(_".vrdrivermanifest", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static string C(string P_0)
	{
		e(P_0);
		using FileStream source = new FileStream(P_0, FileMode.Open, FileAccess.Read, FileShare.Read);
		return Convert.ToHexString(SHA256.HashData(source));
	}

	private void A(string P_0, FileStream P_1)
	{
		string fullPath = Path.GetFullPath(P_0);
		if (!_lockedPaths.Add(fullPath))
		{
			P_1.Dispose();
		}
		else
		{
			_locks.Add(P_1);
		}
	}

	private void A(string P_0, HashSet<string> P_1)
	{
		if (Directory.Exists(P_0))
		{
			string full = Path.GetFullPath(P_0).TrimEnd(Path.DirectorySeparatorChar);
			ModuleApprovalRoot moduleApprovalRoot = _moduleRoots.FirstOrDefault((ModuleApprovalRoot P_0) => P_0.Path.Equals(full, StringComparison.OrdinalIgnoreCase));
			if (moduleApprovalRoot != null)
			{
				moduleApprovalRoot.Publishers.UnionWith(P_1);
			}
			else
			{
				_moduleRoots.Add(new ModuleApprovalRoot(full, new HashSet<string>(P_1, StringComparer.Ordinal)));
			}
		}
	}

	private void C()
	{
		A(_"vrserver", _vrServerPaths, _serverContinuity);
	}

	private void A(string P_0, HashSet<string> P_1, VrProcessContinuity P_2)
	{
		Process[] processesByName = Process.GetProcessesByName(P_0);
		try
		{
			List<VrProcessIdentity> list = new List<VrProcessIdentity>();
			Process[] array = processesByName;
			foreach (Process process in array)
			{
				process.Refresh();
				string fullPath = Path.GetFullPath(process.MainModule?.FileName ?? throw new InvalidDataException(_"SteamVR vrserver executable path is unavailable."));
				if (!P_1.Contains(fullPath))
				{
					throw new InvalidDataException(_"Unapproved SteamVR vrserver process: pid=" + process.Id + _"; path=" + fullPath);
				}
				list.Add(new VrProcessIdentity(process.Id, process.StartTime.ToUniversalTime().Ticks, fullPath));
				HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				foreach (ProcessModule module in process.Modules)
				{
					string fileName = module.FileName;
					if (string.IsNullOrWhiteSpace(fileName))
					{
						throw new InvalidDataException(_"SteamVR vrserver returned a module without a readable path; pid=" + process.Id);
					}
					hashSet.Add(Path.GetFullPath(fileName));
				}
				if (hashSet.Count == 0)
				{
					throw new InvalidDataException(_"SteamVR vrserver returned no readable modules; pid=" + process.Id);
				}
				foreach (string item in hashSet)
				{
					c(item);
				}
			}
			string text = P_2.A(list);
			if (text != null)
			{
				throw new InvalidDataException(P_0 + _": " + text);
			}
		}
		catch (Win32Exception ex)
		{
			throw new InvalidDataException(_"SteamVR vrserver module inspection failed: " + ex.Message, ex);
		}
		finally
		{
			Process[] array = processesByName;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Dispose();
			}
		}
	}

	private void c(string P_0)
	{
		e(P_0);
		if (_vrServerPaths.Contains(P_0) || _vrCompositorPaths.Contains(P_0))
		{
			return;
		}
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
		if (_approvedLoadedModules.Contains(P_0))
		{
			return;
		}
		HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
		if (A(P_0, folderPath))
		{
			hashSet.UnionWith(new string[9]
			{
				_"Microsoft Windows",
				_"Microsoft Corporation",
				_"Microsoft Windows Publisher",
				_"Microsoft Windows Software Compatibility Publisher",
				_"Microsoft Windows Hardware Compatibility Publisher",
				_"NVIDIA Corporation",
				_"Advanced Micro Devices",
				_"Advanced Micro Devices, Inc.",
				_"Intel Corporation"
			});
		}
		foreach (ModuleApprovalRoot moduleRoot in _moduleRoots)
		{
			if (A(P_0, moduleRoot.Path))
			{
				hashSet.UnionWith(moduleRoot.Publishers);
			}
		}
		if (hashSet.Count == 0)
		{
			throw new InvalidDataException(_"SteamVR vrserver loaded a module outside approved Windows/SteamVR/driver roots: " + P_0);
		}
		e(P_0);
		FileStream fileStream = new FileStream(P_0, FileMode.Open, FileAccess.Read, FileShare.Read);
		try
		{
			string text = Convert.ToHexString(SHA256.HashData(fileStream));
			A(text, _revokedHashes, P_0);
			if (!_approvedHashes.Contains(text) && (!Path.GetFileName(P_0).Equals(_"driver_01spacecalibrator.dll", StringComparison.OrdinalIgnoreCase) || !_approvedSpaceCalibratorDrivers.Contains(text)))
			{
				string text2 = Authenticode.A(P_0);
				if (!hashSet.Contains(text2))
				{
					throw new InvalidDataException(_"SteamVR vrserver module publisher is not approved: " + text2 + _"; path=" + P_0 + _"; sha256=" + text);
				}
			}
			A(P_0, fileStream);
			_approvedLoadedModules.Add(P_0);
			_log(_"INFO", _"SteamVR vrserver module approved and locked: " + P_0 + _"; sha256=" + text);
		}
		catch
		{
			fileStream.Dispose();
			throw;
		}
	}

	private static bool A(string P_0, string P_1)
	{
		string fullPath = Path.GetFullPath(P_0);
		string value = Path.GetFullPath(P_1).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
		return fullPath.StartsWith(value, StringComparison.OrdinalIgnoreCase);
	}

	private void c()
	{
		RegistryHive[] array = new RegistryHive[2]
		{
			RegistryHive.LocalMachine,
			RegistryHive.CurrentUser
		};
		foreach (RegistryHive hKey in array)
		{
			RegistryView[] array2 = new RegistryView[2]
			{
				RegistryView.Registry64,
				RegistryView.Registry32
			};
			foreach (RegistryView view in array2)
			{
				using RegistryKey registryKey = RegistryKey.OpenBaseKey(hKey, view);
				using RegistryKey registryKey2 = registryKey.OpenSubKey(_"SOFTWARE\\Khronos\\OpenXR\\1\\ApiLayers\\Implicit");
				if (registryKey2 == null)
				{
					continue;
				}
				string[] valueNames = registryKey2.GetValueNames();
				int num = 0;
				while (num < valueNames.Length)
				{
					string text = valueNames[num];
					if (registryKey2.GetValueKind(text) == RegistryValueKind.DWord)
					{
						object value = registryKey2.GetValue(text);
						if (value is int)
						{
							if ((int)value == 0)
							{
								if (!Path.IsPathFullyQualified(text))
								{
									throw new InvalidDataException(_"OpenXR layer manifest must have an absolute path: " + text);
								}
								using JsonDocument jsonDocument = d(text);
								string path = jsonDocument.RootElement.GetProperty(_"api_layer").GetProperty(_"library_path").GetString() ?? throw new InvalidDataException(_"OpenXR layer library is missing: " + text);
								string fullPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(text), path));
								if (!_approvedHashes.Contains(ReleaseVerifier.a(fullPath)))
								{
									throw new InvalidDataException(_"Unapproved active OpenXR implicit layer: " + text + _". Disable it in its owning application before protected play.");
								}
								a(fullPath, new HashSet<string>(StringComparer.Ordinal));
								a(text);
								_layerManifestHashes[Path.GetFullPath(text)] = C(text);
								_log(_"INFO", _"Approved OpenXR implicit layer: " + text);
							}
							num++;
							continue;
						}
					}
					throw new InvalidDataException(_"Malformed OpenXR implicit layer registration: " + text);
				}
			}
		}
	}

	internal void a(bool P_0 = true)
	{
		string text = Path.Combine(_gameRoot, _"Gorilla Tag_Data", _"Plugins", _"x86_64", _"openvr_api.dll");
		if (!_manifest.TryGetValue(_"openvrApiSha256", out string value))
		{
			_log(_"INFO", _"OpenVR API integrity is covered by the signed native-game tree; this release has no separate repair payload.");
			return;
		}
		if (value.Length != 64 || !value.All(Uri.IsHexDigit))
		{
			throw new InvalidDataException(_"Invalid signed OpenVR API hash.");
		}
		if (File.Exists(text) && ReleaseVerifier.a(text).Equals(value, StringComparison.OrdinalIgnoreCase))
		{
			return;
		}
		if (!P_0)
		{
			throw new InvalidDataException(_"OpenVR API does not match the signed release; a normal protected launch will attempt authenticated repair.");
		}
		string text2 = Path.Combine(_manifestRoot, _"repair", _"openvr_api.dll");
		e(text2);
		e(Path.GetDirectoryName(text));
		if (!ReleaseVerifier.a(text2).Equals(value, StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidDataException(_"OpenVR API repair payload does not match the signed release. Reinstall the current signed package.");
		}
		using FileStream fileStream = new FileStream(text2, FileMode.Open, FileAccess.Read, FileShare.Read);
		if (!Convert.ToHexString(SHA256.HashData(fileStream)).Equals(value, StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidDataException(_"OpenVR repair payload changed during verification.");
		}
		fileStream.Position = 0L;
		if (File.Exists(text))
		{
			e(text);
			File.Copy(text, D(_"openvr_api.dll"), overwrite: false);
		}
		string text3 = text + _".guard-repair-" + Guid.NewGuid().ToString(_"N");
		try
		{
			using (FileStream destination = new FileStream(text3, FileMode.CreateNew, FileAccess.Write, FileShare.None))
			{
				fileStream.CopyTo(destination);
			}
			File.Move(text3, text, overwrite: true);
		}
		finally
		{
			if (File.Exists(text3))
			{
				File.Delete(text3);
			}
		}
		_log(_"WARNING", _"Repaired openvr_api.dll from the authenticated release payload; previous file backed up in LocalAppData/GorillaGuard/backups.");
	}

	internal static bool A(JsonObject P_0)
	{
		bool result = false;
		KeyValuePair<string, JsonNode>[] array = P_0.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<string, JsonNode> keyValuePair = array[i];
			if (keyValuePair.Key != _"steamvr" && keyValuePair.Key != _"steam.app.1533390")
			{
				continue;
			}
			if (!(keyValuePair.Value is JsonObject jsonObject))
			{
				throw new InvalidDataException(_"SteamVR settings section must be an object: " + keyValuePair.Key);
			}
			string[] array2 = new string[4]
			{
				_"poseTimeOffset",
				_"predictionTime",
				_"predictionTimeOffset",
				_"secondsFromVsyncToPhotons"
			};
			foreach (string propertyName in array2)
			{
				if (jsonObject.TryGetPropertyValue(propertyName, out JsonNode jsonNode) && (!(jsonNode is JsonValue jsonValue) || !jsonValue.TryGetValue<double>(out var value) || !double.IsFinite(value) || value != 0.0))
				{
					jsonObject.Remove(propertyName);
					result = true;
				}
			}
		}
		return result;
	}

	internal void a(string P_0, bool P_1)
	{
		e(P_0);
		byte[] array = E(P_0);
		ReadOnlySpan<byte> utf8Json = array;
		JsonDocumentOptions jsonOptions = JsonOptions;
		JsonObject jsonObject = (JsonNode.Parse(utf8Json, null, jsonOptions) as JsonObject) ?? throw new InvalidDataException(_"SteamVR settings must be a JSON object: " + P_0);
		if (!A(jsonObject))
		{
			return;
		}
		if (!P_1)
		{
			throw new InvalidDataException(_"Prediction overrides require cleanup: " + P_0);
		}
		string[] array2 = new string[2]
		{
			_"vrserver",
			_"vrcompositor"
		};
		for (int i = 0; i < array2.Length; i++)
		{
			Process[] processesByName = Process.GetProcessesByName(array2[i]);
			bool flag = processesByName.Length != 0;
			Process[] array3 = processesByName;
			for (int j = 0; j < array3.Length; j++)
			{
				array3[j].Dispose();
			}
			if (flag)
			{
				throw new InvalidDataException(_"Close SteamVR before cleaning prediction overrides in " + P_0 + _". Then run Gorilla Guard again.");
			}
		}
		string text = D(Path.GetFileName(P_0));
		byte[] bytes = Encoding.UTF8.GetBytes(jsonObject.ToJsonString(new JsonSerializerOptions
		{
			WriteIndented = true
		}));
		File.WriteAllBytes(text, array);
		using (FileStream fileStream = new FileStream(P_0, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
		{
			if (!SHA256.HashData(fileStream).SequenceEqual(SHA256.HashData(array)))
			{
				throw new IOException(_"SteamVR configuration changed during audit; retry launch: " + P_0);
			}
			try
			{
				fileStream.Position = 0L;
				fileStream.Write(bytes);
				fileStream.SetLength(bytes.Length);
				fileStream.Flush(flushToDisk: true);
			}
			catch
			{
				try
				{
					fileStream.Position = 0L;
					fileStream.Write(array);
					fileStream.SetLength(array.Length);
					fileStream.Flush(flushToDisk: true);
				}
				catch (Exception ex)
				{
					_log(_"ERROR", _"Configuration write and rollback failed: " + ex.Message + _"; original backup=" + text);
				}
				throw;
			}
		}
		_restores.Add((P_0, text, Convert.ToHexString(SHA256.HashData(bytes))));
		_log(_"WARNING", _"Temporarily removed prediction overrides: " + P_0 + _"; backup=" + text);
	}

	private string D(string P_0)
	{
		string text = Path.Combine(_backupRoot, Guid.NewGuid().ToString(_"N"));
		Directory.CreateDirectory(text);
		return Path.Combine(text, P_0);
	}

	internal static JsonDocument d(string P_0)
	{
		return JsonDocument.Parse(E(P_0), JsonOptions);
	}

	private static byte[] E(string P_0)
	{
		e(P_0);
		using FileStream fileStream = new FileStream(P_0, FileMode.Open, FileAccess.Read, FileShare.Read);
		if (fileStream.Length > 2097152)
		{
			throw new InvalidDataException(_"VR JSON exceeds the 2 MB limit: " + P_0);
		}
		using MemoryStream memoryStream = new MemoryStream();
		fileStream.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}

	internal static void e(string P_0)
	{
		for (string text = Path.GetFullPath(P_0); text != null; text = Path.GetDirectoryName(text))
		{
			if ((File.GetAttributes(text) & FileAttributes.ReparsePoint) != 0)
			{
				throw new InvalidDataException(_"Redirected VR paths are not allowed: " + text);
			}
		}
	}

	[IteratorStateMachine(typeof(_003CWalkFiles_003Ed__66))]
	internal static IEnumerable<string> F(string P_0)
	{

		return new _003CWalkFiles_003Ed__66(-2)
		{
			_003C_003E3__root = P_0
		};
	}

	public void Dispose()
	{
		foreach (VrCodeWatcher codeWatcher in _codeWatchers)
		{
			codeWatcher.Dispose();
		}
		_codeWatchers.Clear();
		foreach (FileStream @lock in _locks)
		{
			@lock.Dispose();
		}
		_locks.Clear();
		_lockedPaths.Clear();
		_approvedLoadedModules.Clear();
		foreach (var restore in _restores)
		{
			try
			{
				e(restore.Path);
				using FileStream fileStream = new FileStream(restore.Path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
				if (!Convert.ToHexString(SHA256.HashData(fileStream)).Equals(restore.SanitizedHash, StringComparison.Ordinal))
				{
					_log(_"WARNING", _"SteamVR settings were edited during play; preserving edits. Original backup: " + restore.Backup);
					continue;
				}
				byte[] array = File.ReadAllBytes(restore.Backup);
				fileStream.Position = 0L;
				fileStream.Write(array);
				fileStream.SetLength(array.Length);
				fileStream.Flush(flushToDisk: true);
				_log(_"INFO", _"Restored SteamVR configuration: " + restore.Path);
			}
			catch (Exception ex)
			{
				_log(_"WARNING", _"Could not restore SteamVR settings: " + ex.Message + _"; backup=" + restore.Backup);
			}
		}
		_restores.Clear();
	}
}
