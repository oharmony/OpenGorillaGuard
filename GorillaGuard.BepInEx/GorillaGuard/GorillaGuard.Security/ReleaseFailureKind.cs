namespace GorillaGuard.Security;

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
