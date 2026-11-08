namespace GorillaGuard.Security;

internal sealed class ReleaseVerificationResult
{
	public bool Success { get; set; }

	public ReleaseFailureKind FailureKind { get; set; }

	public string Message { get; set; }

	public string Version { get; set; }

	public string ExpectedRuntimeHash { get; set; }

	public string ActualRuntimeHash { get; set; }
}
