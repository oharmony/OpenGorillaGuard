using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace GorillaGuard.Launcher;

internal sealed class RemoteSettings
{
	internal const string FileName = "GorillaGuard.remote.json";

	public string UpdateManifestUrl { get; set; } = _"";

	public string UpdateSignatureUrl { get; set; } = _"";

	public string EventEndpoint { get; set; } = _"";

	public string RelayToken { get; set; } = _"";

	public bool RequireUpdateCheck { get; set; }

	public bool RequireReadyAcknowledgement { get; set; }

	internal bool HasUpdateChannel => !string.IsNullOrWhiteSpace(UpdateManifestUrl);

	internal bool HasEventEndpoint => false;

	internal static RemoteSettings A(string P_0)
	{
		string path = Path.Combine(P_0, _"GorillaGuard.remote.json");
		if (!File.Exists(path))
		{
			return new RemoteSettings();
		}
		RemoteSettings remoteSettings = JsonSerializer.Deserialize<RemoteSettings>(File.ReadAllText(path, Encoding.UTF8), A()) ?? throw new InvalidDataException(_"Remote settings JSON was empty.");
		remoteSettings.EventEndpoint = _"";
		remoteSettings.RelayToken = _"";
		remoteSettings.RequireReadyAcknowledgement = false;
		if (remoteSettings.HasUpdateChannel)
		{
			A(remoteSettings.UpdateManifestUrl, _"UpdateManifestUrl", false);
			A(remoteSettings.UpdateSignatureUrl, _"UpdateSignatureUrl", false);
		}
		else if (!string.IsNullOrWhiteSpace(remoteSettings.UpdateSignatureUrl))
		{
			throw new InvalidDataException(_"UpdateSignatureUrl requires UpdateManifestUrl.");
		}
		if (remoteSettings.HasEventEndpoint)
		{
			A(remoteSettings.EventEndpoint, _"EventEndpoint", false);
		}
		string relayToken = remoteSettings.RelayToken;
		if (relayToken != null && relayToken.Length > 512)
		{
			throw new InvalidDataException(_"RelayToken is too long.");
		}
		return remoteSettings;
	}

	internal static JsonSerializerOptions A()
	{
		return new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			WriteIndented = true
		};
	}

	private static void A(string P_0, string P_1, bool P_2)
	{
		if (!Uri.TryCreate(P_0, UriKind.Absolute, out Uri result) || !result.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(result.Host) || !string.IsNullOrEmpty(result.UserInfo))
		{
			throw new InvalidDataException(P_1 + _" must be an absolute HTTPS URL without embedded credentials.");
		}
		if (!P_2 && (result.Host.Equals(_"discord.com", StringComparison.OrdinalIgnoreCase) || result.Host.EndsWith(_".discord.com", StringComparison.OrdinalIgnoreCase) || result.Host.Equals(_"discordapp.com", StringComparison.OrdinalIgnoreCase) || result.Host.EndsWith(_".discordapp.com", StringComparison.OrdinalIgnoreCase)))
		{
			throw new InvalidDataException(P_1 + _" must point to your relay, not directly to a Discord webhook.");
		}
	}
}
