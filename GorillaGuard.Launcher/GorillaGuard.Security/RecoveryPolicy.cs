using System;

namespace GorillaGuard.Security;

internal static class RecoveryPolicy
{
	internal const string FallbackVersion = "1.12.8";

	internal static string A(bool P_0)
	{
		if (!P_0)
		{
			return _"https://github.com/DevyDevelopment/GorillaGuard-Releases/releases/latest/download/GorillaGuard.update.json";
		}
		return _"https://github.com/DevyDevelopment/GorillaGuard-Releases/releases/download/v1.12.8/GorillaGuard.update.json";
	}

	internal static bool A(Version P_0, Version P_1, bool P_2, bool P_3)
	{
		if (P_0 == null || P_1 == null)
		{
			return false;
		}
		if (P_3)
		{
			return P_0 == new Version(_"1.12.8");
		}
		if (!P_2)
		{
			return P_0 > P_1;
		}
		return P_0 >= P_1;
	}
}
