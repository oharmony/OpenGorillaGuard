using System.Collections.Generic;

namespace GorillaGuard.Security;

internal sealed class ValueDriftWindow
{
	private readonly Queue<double> observations = new Queue<double>(3);

	private double last;

	internal bool A(double P_0)
	{
		if (P_0 < last)
		{
			observations.Clear();
		}
		last = P_0;
		while (observations.Count > 0 && P_0 - observations.Peek() > 10.0)
		{
			observations.Dequeue();
		}
		if (observations.Count == 3)
		{
			observations.Dequeue();
		}
		observations.Enqueue(P_0);
		return observations.Count == 3;
	}
}
