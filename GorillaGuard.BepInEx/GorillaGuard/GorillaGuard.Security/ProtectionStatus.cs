
namespace GorillaGuard.Security;

internal static class ProtectionStatus
{
	internal static string Describe(bool integrityOk, bool launcherVerified, bool modDisabled, bool removalIncomplete, bool restartRequired, bool monitoringIncomplete = false, bool valueCorrectionIncomplete = false, bool repeatedProtectedValueChanges = false)
	{
		if (!integrityOk)
		{
			return "Integrity failed — repair required";
		}
		if (!launcherVerified)
		{
			return "Waiting for launcher verification";
		}
		if (valueCorrectionIncomplete)
		{
			return "Value correction incomplete — restart required";
		}
		if (monitoringIncomplete)
		{
			return "Monitoring incomplete — restart required";
		}
		if (removalIncomplete)
		{
			return "Removal incomplete — restart required";
		}
		if (repeatedProtectedValueChanges)
		{
			return "Repeated protected-value changes detected";
		}
		if (modDisabled)
		{
			if (!restartRequired)
			{
				return "Mod disabled";
			}
			return "Mod disabled — restart required";
		}
		if (!restartRequired)
		{
			return "Verified";
		}
		return "Restart required";
	}
}
