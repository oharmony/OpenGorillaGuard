using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GorillaGuard.Security;

namespace GorillaGuard.Launcher;

internal static class SignedUpdateClient
{
	private sealed class UpdateDescriptor
	{
		public string Version { get; set; } = _"";

		public string PackageUrl { get; set; } = _"";

		public string PackageSha256 { get; set; } = _"";
	}

	private const long MaxMetadataBytes = 1048576L;

	private const long MaxPackageBytes = 104857600L;

	private const long MaxExtractedBytes = 314572800L;

	private const int MaxArchiveEntries = 4096;

	internal static async Task<bool> A(RemoteSettings P_0, string P_1, Action<string, string> P_2, bool P_3 = false, bool P_4 = false)
	{
		if (!P_0.HasUpdateChannel)
		{
			return false;
		}
		try
		{
			using HttpClient http = SecureDownloads.A();
			http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(_"GorillaGuard", _"1.12.31"));
			byte[] metadataBytes = await A(http, P_0.UpdateManifestUrl, 1048576L);
			if (!ReleaseVerifier.A(metadataBytes, await A(http, P_0.UpdateSignatureUrl, 1048576L)))
			{
				throw new InvalidDataException(_"The remote update descriptor signature is invalid.");
			}
			UpdateDescriptor descriptor = JsonSerializer.Deserialize<UpdateDescriptor>(metadataBytes, RemoteSettings.A()) ?? throw new InvalidDataException(_"The signed update descriptor was empty.");
			if (!Version.TryParse(descriptor.Version, out Version remoteVersion) || !Version.TryParse(_"1.12.31", out Version result))
			{
				throw new InvalidDataException(_"The signed update descriptor contains an invalid version.");
			}
			if (P_3 && !P_4 && remoteVersion < result)
			{
				throw new InvalidDataException(_"Repair cannot silently downgrade the release. Use the explicit rollback installer instead.");
			}
			if (!RecoveryPolicy.A(remoteVersion, result, P_3, P_4))
			{
				if (P_4)
				{
					throw new InvalidDataException(_"The rollback descriptor does not match the pinned previous release.");
				}
				return false;
			}
			if (!Uri.TryCreate(descriptor.PackageUrl, UriKind.Absolute, out Uri result2) || !result2.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) || !string.IsNullOrEmpty(result2.UserInfo))
			{
				throw new InvalidDataException(_"The signed update package URL is not valid HTTPS.");
			}
			if (!A(descriptor.PackageSha256))
			{
				throw new InvalidDataException(_"The signed update descriptor contains an invalid package SHA-256.");
			}
			string arg = _"INFO";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(72, 1);
			defaultInterpolatedStringHandler.AppendLiteral(_"Signed Gorilla Guard update ");
			defaultInterpolatedStringHandler.AppendFormatted(remoteVersion);
			defaultInterpolatedStringHandler.AppendLiteral(_" is available; downloading verified package.");
			P_2(arg, defaultInterpolatedStringHandler.ToStringAndClear());
			byte[] array = await A(http, descriptor.PackageUrl, 104857600L);
			string text = Convert.ToHexString(SHA256.HashData(array));
			if (!text.Equals(descriptor.PackageSha256, StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException(_"Downloaded update hash mismatch: expected=" + descriptor.PackageSha256 + _"; actual=" + text);
			}
			string updateRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _"GorillaGuard", _"updates", remoteVersion?.ToString() + _"-" + Guid.NewGuid().ToString(_"N"));
			Directory.CreateDirectory(updateRoot);
			string zipPath = Path.Combine(updateRoot, _"package.zip");
			await File.WriteAllBytesAsync(zipPath, array);
			string text2 = Path.Combine(updateRoot, _"package");
			Directory.CreateDirectory(text2);
			ExtractSafely(zipPath, text2);
			string[] files = Directory.GetFiles(text2, _"GorillaGuard.Launcher.exe", SearchOption.AllDirectories);
			if (files.Length != 1)
			{
				throw new InvalidDataException(_"The signed update package must contain exactly one launcher.");
			}
			string directoryName = Path.GetDirectoryName(files[0]);
			if (!Directory.Exists(Path.Combine(directoryName, _"BepInEx")))
			{
				throw new InvalidDataException(_"The signed update package has no BepInEx payload beside its launcher.");
			}
			string text3 = Path.Combine(P_1, _"GorillaGuard.remote.json");
			if (!P_3 && !P_4 && File.Exists(text3))
			{
				File.Copy(text3, Path.Combine(directoryName, _"GorillaGuard.remote.json"), overwrite: true);
			}
			BootstrapReceipt value = new BootstrapReceipt
			{
				Root = updateRoot,
				Files = (from P_0 in Directory.EnumerateFiles(updateRoot, _"*", SearchOption.AllDirectories)
					select new CleanupFile
					{
						Path = Path.GetRelativePath(updateRoot, P_0),
						Hash = ReleaseVerifier.a(P_0),
						Kind = _"cache"
					}).ToList()
			};
			File.WriteAllText(Path.Combine(directoryName, _"GorillaGuard.bootstrap.json"), JsonSerializer.Serialize(value));
			if (Process.Start(new ProcessStartInfo
			{
				FileName = files[0],
				Arguments = _"--update-parent " + Environment.ProcessId + ((P_3 || P_4) ? _" --install-only" : _""),
				WorkingDirectory = directoryName,
				UseShellExecute = true
			}) == null)
			{
				throw new InvalidOperationException(_"Windows did not start the verified update installer.");
			}
			string arg2 = _"INFO";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(76, 1);
			defaultInterpolatedStringHandler2.AppendLiteral(_"Verified update ");
			defaultInterpolatedStringHandler2.AppendFormatted(remoteVersion);
			defaultInterpolatedStringHandler2.AppendLiteral(_" staged; handing off installation after this launcher exits.");
			P_2(arg2, defaultInterpolatedStringHandler2.ToStringAndClear());
			return true;
		}
		catch (Exception ex)
		{
			P_2(P_0.RequireUpdateCheck ? _"ERROR" : _"WARNING", _"Remote update check failed: " + ex.Message);
			if (P_0.RequireUpdateCheck)
			{
				throw;
			}
			return false;
		}
	}

	private static async Task<byte[]> A(HttpClient P_0, string P_1, long P_2)
	{
		using CancellationTokenSource deadline = new CancellationTokenSource(TimeSpan.FromMinutes(3.0));
		using HttpResponseMessage response = await SecureDownloads.A(P_0, P_1, deadline.Token);
		Uri uri = response.RequestMessage?.RequestUri;
		if ((object)uri == null || !uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidDataException(_"Remote content resolved to a non-HTTPS address.");
		}
		response.EnsureSuccessStatusCode();
		long? contentLength = response.Content.Headers.ContentLength;
		if (contentLength.HasValue)
		{
			long valueOrDefault = contentLength.GetValueOrDefault();
			if (valueOrDefault > P_2)
			{
				throw new InvalidDataException(_"Remote content exceeds the allowed size.");
			}
		}
		byte[] result;
		await using (Stream input = await response.Content.ReadAsStreamAsync())
		{
			using MemoryStream output = new MemoryStream();
			byte[] buffer = new byte[81920];
			while (true)
			{
				int num = await input.ReadAsync(buffer, deadline.Token);
				if (num == 0)
				{
					break;
				}
				if (output.Length + num > P_2)
				{
					throw new InvalidDataException(_"Remote content exceeds the allowed size.");
				}
				output.Write(buffer, 0, num);
			}
			result = output.ToArray();
		}
		return result;
	}

	private static void ExtractSafely(string zipPath, string destinationRoot)
	{
		string value = Path.GetFullPath(destinationRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
		long num = 0L;
		using ZipArchive zipArchive = ZipFile.OpenRead(zipPath);
		if (zipArchive.Entries.Count > 4096)
		{
			throw new InvalidDataException(_"The update archive contains too many entries.");
		}
		foreach (ZipArchiveEntry entry in zipArchive.Entries)
		{
			string fullPath = Path.GetFullPath(Path.Combine(destinationRoot, entry.FullName));
			if (!fullPath.StartsWith(value, StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException(_"The update archive contains an unsafe path.");
			}
			if (string.IsNullOrEmpty(entry.Name))
			{
				Directory.CreateDirectory(fullPath);
				continue;
			}
			num += entry.Length;
			if (entry.Length > 104857600 || num > 314572800)
			{
				throw new InvalidDataException(_"The update archive expands beyond the allowed size.");
			}
			Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
			using Stream stream = entry.Open();
			using FileStream destination = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
			stream.CopyTo(destination);
		}
	}

	private static bool A(string? P_0)
	{
		if (P_0 != null && P_0.Length == 64)
		{
			return P_0.All(Uri.IsHexDigit);
		}
		return false;
	}
}
