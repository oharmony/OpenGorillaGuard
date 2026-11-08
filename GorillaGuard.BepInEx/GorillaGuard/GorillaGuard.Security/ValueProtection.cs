using System;

namespace GorillaGuard.Security;

internal static class ValueProtection
{
	internal static bool HasMeaningfulDrift(float expected, float actual)
	{
		if (!float.IsNaN(P_0) && !float.IsInfinity(P_0) && !float.IsNaN(P_1) && !float.IsInfinity(P_1))
		{
			return Math.Abs(P_0 - P_1) <= 0.0001f;
		}
		return false;
	}

	internal static bool A(float P_0, Action<float> P_1, Func<float> P_2, out float P_3)
	{
		P_1(P_0);
		P_3 = P_2();
		return A(P_3, P_0);
	}
}
