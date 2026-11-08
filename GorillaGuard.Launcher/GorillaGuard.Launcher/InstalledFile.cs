
namespace GorillaGuard.Launcher;

internal sealed class InstalledFile
{
	public string Relative { get; set; } = _"";

	public string? BackupName { get; set; }

	public string? OriginalHash { get; set; }

	public string? InstalledHash { get; set; }
}
