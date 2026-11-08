using System.Collections.Generic;

namespace GorillaGuard.Launcher;

internal sealed class UninstallPlan
{
	public string ApprovalManifest { get; set; } = _"";

	public string ApprovalSignature { get; set; } = _"";

	public string GameRoot { get; set; } = _"";

	public string BackupRoot { get; set; } = _"";

	public List<InstalledFile> Files { get; set; } = new List<InstalledFile>();

	public List<CleanupFile> Extras { get; set; } = new List<CleanupFile>();

	public int ParentPid { get; set; }

	public long ParentStartedUtcTicks { get; set; }

	public int GamePid { get; set; }

	public long GameStartedUtcTicks { get; set; }
}
