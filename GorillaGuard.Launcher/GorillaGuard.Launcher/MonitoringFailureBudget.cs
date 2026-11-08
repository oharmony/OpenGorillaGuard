using System;

namespace GorillaGuard.Launcher;

internal sealed class MonitoringFailureBudget
{
	private int _failures;

	internal bool A(bool P_0)
	{
		_failures = ((!P_0) ? Math.Min(3, _failures + 1) : 0);
		return _failures >= 3;
	}
}
