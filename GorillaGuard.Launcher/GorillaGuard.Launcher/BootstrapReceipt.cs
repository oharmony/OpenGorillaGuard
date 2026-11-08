using System.Collections.Generic;

namespace GorillaGuard.Launcher;

internal sealed class BootstrapReceipt
{
	public string Root { get; set; } = _"";

	public string BootstrapPath { get; set; } = _"";

	public string BootstrapHash { get; set; } = _"";

	public List<CleanupFile> Files { get; set; } = new List<CleanupFile>();
}
