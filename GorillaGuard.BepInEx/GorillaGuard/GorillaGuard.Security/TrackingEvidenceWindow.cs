using System;

namespace GorillaGuard.Security;

internal sealed class TrackingEvidenceWindow
{
	private double _lastTime = double.NaN;

	internal double Score { get; private set; }

	internal void Reset()
	{
		Score = 0.0;
		_lastTime = double.NaN;
	}

	internal double Update(double time, int anomalyScore)
	{
		if (!double.IsFinite(time))
		{
			Reset();
			return Score;
		}
		double num = time - _lastTime;
		_lastTime = time;
		if (!double.IsFinite(num) || num <= 0.0 || num > 0.5)
		{
			Score = 0.0;
			return Score;
		}
		Score = Math.Max(0.0, Math.Min(20.0, Score + num * (double)((anomalyScore > 0) ? (anomalyScore * 20) : (-5))));
		return Score;
	}
}
