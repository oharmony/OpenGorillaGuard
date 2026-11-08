using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GorillaGuard.Security;

internal sealed class WarningGate
{
	private readonly Dictionary<string, long> last = new Dictionary<string, long>(StringComparer.Ordinal);

	internal bool A(string P_0)
	{
		long timestamp = Stopwatch.GetTimestamp();
		lock (last)
		{
			if (last.TryGetValue(P_0, out var value) && (double)(timestamp - value) / (double)Stopwatch.Frequency < 15.0)
			{
				return false;
			}
			if (last.Count >= 512)
			{
				last.Clear();
			}
			last[P_0] = timestamp;
			return true;
		}
	}
}
