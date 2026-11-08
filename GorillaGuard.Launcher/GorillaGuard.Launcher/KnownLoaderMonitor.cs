using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using GorillaGuard.Security;

namespace GorillaGuard.Launcher;

internal sealed class KnownLoaderMonitor
{
	private sealed class CacheEntry(long Length, DateTime Written, long Checked, LoaderFinding? Finding) : IEquatable<CacheEntry>
	{
				private Type EqualityContract
		{
						get
			{
				return typeof(CacheEntry);
			}
		}

		public long Length { get; init; }

		public DateTime Written { get; init; }

		public long Checked { get; init; }

		public LoaderFinding? Finding { get; init; }

		public CacheEntry(long Length, DateTime Written, long Checked, LoaderFinding? Finding)
		{
			this.Length = Length;
			this.Written = Written;
			this.Checked = Checked;
			this.Finding = Finding;
			base._002Ector();
		}

				public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(_"CacheEntry");
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
			P_0.Append(_", Finding = ");
			P_0.Append(Finding);
			return true;
		}

				public static bool operator !=(CacheEntry? left, CacheEntry? right)
		{
			return !(left == right);
		}

				public static bool operator ==(CacheEntry? left, CacheEntry? right)
		{
			if ((object)left != right)
			{
				return left?.Equals(right) ?? false;
			}
			return true;
		}

				public override int GetHashCode()
		{
			return (((EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<long>.Default.GetHashCode(Length)) * -1521134295 + EqualityComparer<DateTime>.Default.GetHashCode(Written)) * -1521134295 + EqualityComparer<long>.Default.GetHashCode(Checked)) * -1521134295 + EqualityComparer<LoaderFinding>.Default.GetHashCode(Finding);
		}

				public override bool Equals(object? obj)
		{
			return Equals(obj as CacheEntry);
		}

				public bool Equals(CacheEntry? other)
		{
			if ((object)this != other)
			{
				if ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<long>.Default.Equals(Length, other.Length) && EqualityComparer<DateTime>.Default.Equals(Written, other.Written) && EqualityComparer<long>.Default.Equals(Checked, other.Checked))
				{
					return EqualityComparer<LoaderFinding>.Default.Equals(Finding, other.Finding);
				}
				return false;
			}
			return true;
		}

				private CacheEntry(CacheEntry original)
		{
			Length = original.Length;
			Written = original.Written;
			Checked = original.Checked;
			Finding = original.Finding;
		}

				public void Deconstruct(out long Length, out DateTime Written, out long Checked, out LoaderFinding? Finding)
		{
			Length = this.Length;
			Written = this.Written;
			Checked = this.Checked;
			Finding = this.Finding;
		}
	}

	private readonly Dictionary<string, CacheEntry> _cache = new Dictionary<string, CacheEntry>(StringComparer.OrdinalIgnoreCase);

	private readonly Action<string, string> _log;

	internal KnownLoaderMonitor(Action<string, string> log)
	{
		_log = log;
	}

	internal LoaderFinding? A()
	{
		Process[] processes = Process.GetProcesses();
		try
		{
			Process[] array = processes;
			foreach (Process process in array)
			{
				try
				{
					string text = A(process.Id);
					if (text != null)
					{
						LoaderFinding loaderFinding = A(text);
						if (loaderFinding != null)
						{
							LoaderFinding loaderFinding2 = loaderFinding._003CClone_003E_0024();
							loaderFinding2.ProcessId = process.Id;
							return loaderFinding2;
						}
					}
				}
				catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException || ex is Win32Exception || ex is InvalidOperationException) ? 1 : 0) != 0)
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
		if (_cache.Count > 4096)
		{
			_cache.Clear();
		}
		return null;
	}

	internal LoaderFinding? A(string P_0)
	{
		FileInfo fileInfo = new FileInfo(P_0);
		long timestamp = Stopwatch.GetTimestamp();
		if (_cache.TryGetValue(P_0, out CacheEntry value) && value.Length == fileInfo.Length && value.Written == fileInfo.LastWriteTimeUtc && Stopwatch.GetElapsedTime(value.Checked, timestamp).TotalSeconds < 10.0)
		{
			return value.Finding;
		}
		LoaderFinding loaderFinding = a(P_0);
		_cache[P_0] = new CacheEntry(fileInfo.Length, fileInfo.LastWriteTimeUtc, timestamp, loaderFinding);
		if (loaderFinding != null)
		{
			_log(_"ERROR", _"Known loader evidence: " + loaderFinding.Evidence + _"; path=" + P_0);
		}
		return loaderFinding;
	}

	internal static LoaderFinding? a(string P_0)
	{
		using FileStream fileStream = new FileStream(P_0, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
		if (fileStream.Length == 9175040)
		{
			string text = Convert.ToHexString(SHA256.HashData(fileStream));
			if (text.Equals(_"DA9816B556B81B2444C12D42E67D5C25FB51F06E6BF884E553733770438775B3", StringComparison.OrdinalIgnoreCase))
			{
				return new LoaderFinding(_"Absense specimen SHA-256=" + text, P_0);
			}
			fileStream.Position = 0L;
		}
		if (fileStream.Length < 128 || fileStream.Length > 134217728)
		{
			return null;
		}
		try
		{
			using PEReader pEReader = new PEReader(fileStream, PEStreamOptions.LeaveOpen);
			if (!pEReader.HasMetadata || pEReader.PEHeaders.CorHeader.MetadataDirectory.Size > 16777216)
			{
				return null;
			}
			MetadataReader metadataReader = pEReader.GetMetadataReader();
			if (metadataReader.TypeDefinitions.Count > 100000 || metadataReader.MethodDefinitions.Count > 200000)
			{
				return null;
			}
			List<string> list = new List<string>();
			foreach (TypeDefinitionHandle typeDefinition2 in metadataReader.TypeDefinitions)
			{
				TypeDefinition typeDefinition = metadataReader.GetTypeDefinition(typeDefinition2);
				string @string = metadataReader.GetString(typeDefinition.Namespace);
				if (@string == _"Absense")
				{
					list.Add(@string + _"." + metadataReader.GetString(typeDefinition.Name));
				}
			}
			if (list.Count < 3)
			{
				return null;
			}
			List<string> list2 = new List<string>();
			foreach (MethodDefinitionHandle methodDefinition2 in metadataReader.MethodDefinitions)
			{
				MethodDefinition methodDefinition = metadataReader.GetMethodDefinition(methodDefinition2);
				if ((methodDefinition.Attributes & MethodAttributes.PinvokeImpl) != 0)
				{
					MethodImport import = methodDefinition.GetImport();
					if (!import.Module.IsNil && !import.Name.IsNil && metadataReader.GetString(metadataReader.GetModuleReference(import.Module).Name).Equals(_"kernel32.dll", StringComparison.OrdinalIgnoreCase))
					{
						list2.Add(metadataReader.GetString(import.Name));
					}
				}
			}
			return InjectionPolicy.A(list, list2) ? new LoaderFinding(_"Absense injector type/import combination (static family match)", P_0) : null;
		}
		catch (Exception ex) when (((ex is BadImageFormatException || ex is InvalidOperationException || ex is ArgumentException || ex is OverflowException || ex is IndexOutOfRangeException) ? 1 : 0) != 0)
		{
			return null;
		}
	}

	private static string? A(int P_0)
	{
		nint num = OpenProcess(4096u, inherit: false, P_0);
		if (num == IntPtr.Zero)
		{
			return null;
		}
		try
		{
			StringBuilder stringBuilder = new StringBuilder(32768);
			int size = stringBuilder.Capacity;
			return QueryFullProcessImageNameW(num, 0u, stringBuilder, ref size) ? stringBuilder.ToString() : null;
		}
		finally
		{
			CloseHandle(num);
		}
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern nint OpenProcess(uint access, bool inherit, int pid);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool QueryFullProcessImageNameW(nint process, uint flags, StringBuilder name, ref int size);

	[DllImport("kernel32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool CloseHandle(nint handle);
}
