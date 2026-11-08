using System;

namespace GorillaGuard.Security;

internal static class FrameRatePolicy
{
	private static readonly float[] Allowed = new float[4] { 72f, 80f, 90f, 120f };

	internal static float A(float P_0)
	{
		float num = Allowed[0];
		float[] allowed = Allowed;
		foreach (float num2 in allowed)
		{
			if (Math.Abs(num2 - P_0) < Math.Abs(num - P_0))
			{
				num = num2;
			}
		}
		return num;
	}

	internal static bool a(float P_0)
	{
		if (!float.IsNaN(P_0) && !float.IsInfinity(P_0) && P_0 > 0f)
		{
			return Math.Abs(P_0 - A(P_0)) <= 3f;
		}
		return false;
	}

	internal static int NormalizeTarget(int currentTarget)
	{
		if (P_0 > 0 && !a(P_0))
		{
			return (int)A((float)P_0);
		}
		return currentTarget;
	}
}
