namespace GorillaGuard.Security;

internal sealed class TrackingAnomalyWindow
{
	internal const double MaximumReachMeters = 1.85;

	internal const double MaximumHeadJumpMeters = 0.75;

	internal const double MaximumHandJumpMeters = 1.25;

	internal const double MaximumHeadSpeedMetersPerSecond = 15.0;

	internal const double MaximumHandSpeedMetersPerSecond = 35.0;

	internal const double MaximumHeadAccelerationMetersPerSecondSquared = 180.0;

	internal const double MaximumHandAccelerationMetersPerSecondSquared = 450.0;

	internal const double MaximumSampleGapSeconds = 0.5;

	private TrackingPoseSample _previous;

	private TrackingPoint _previousHeadVelocity;

	private TrackingPoint _previousLeftVelocity;

	private TrackingPoint _previousRightVelocity;

	private bool _hasSample;

	private bool _hasVelocity;

	internal void Reset()
	{
		_previous = default(TrackingPoseSample);
		_previousHeadVelocity = default(TrackingPoint);
		_previousLeftVelocity = default(TrackingPoint);
		_previousRightVelocity = default(TrackingPoint);
		_hasSample = false;
		_hasVelocity = false;
	}

	internal TrackingAnomaly Evaluate(TrackingPoseSample sample)
	{
		if (!sample.IsFinite)
		{
			Reset();
			return TrackingAnomaly.InvalidSample;
		}
		TrackingAnomaly trackingAnomaly = TrackingAnomaly.None;
		if ((sample.LeftHand - sample.Head).Magnitude > 1.85)
		{
			trackingAnomaly |= TrackingAnomaly.LeftReach;
		}
		if ((sample.RightHand - sample.Head).Magnitude > 1.85)
		{
			trackingAnomaly |= TrackingAnomaly.RightReach;
		}
		if (!_hasSample)
		{
			_previous = sample;
			_hasSample = true;
			return trackingAnomaly;
		}
		double num = sample.Time - _previous.Time;
		if (!double.IsFinite(num) || num <= 0.0 || num > 0.5)
		{
			_previous = sample;
			_hasVelocity = false;
			return trackingAnomaly;
		}
		TrackingPoint trackingPoint = sample.Head - sample.Body - (_previous.Head - _previous.Body);
		TrackingPoint trackingPoint2 = sample.LeftHand - sample.Head - (_previous.LeftHand - _previous.Head);
		TrackingPoint trackingPoint3 = sample.RightHand - sample.Head - (_previous.RightHand - _previous.Head);
		if (trackingPoint.Magnitude > 0.75)
		{
			trackingAnomaly |= TrackingAnomaly.HeadJump;
		}
		if (trackingPoint2.Magnitude > 1.25)
		{
			trackingAnomaly |= TrackingAnomaly.LeftHandJump;
		}
		if (trackingPoint3.Magnitude > 1.25)
		{
			trackingAnomaly |= TrackingAnomaly.RightHandJump;
		}
		TrackingPoint trackingPoint4 = trackingPoint.Scale(num);
		TrackingPoint trackingPoint5 = trackingPoint2.Scale(num);
		TrackingPoint trackingPoint6 = trackingPoint3.Scale(num);
		if (trackingPoint4.Magnitude > 15.0)
		{
			trackingAnomaly |= TrackingAnomaly.HeadSpeed;
		}
		if (trackingPoint5.Magnitude > 35.0)
		{
			trackingAnomaly |= TrackingAnomaly.LeftHandSpeed;
		}
		if (trackingPoint6.Magnitude > 35.0)
		{
			trackingAnomaly |= TrackingAnomaly.RightHandSpeed;
		}
		if (_hasVelocity)
		{
			if (trackingPoint4.Magnitude > 5.0 && (trackingPoint4 - _previousHeadVelocity).Scale(num).Magnitude > 180.0)
			{
				trackingAnomaly |= TrackingAnomaly.HeadAcceleration;
			}
			if (trackingPoint5.Magnitude > 11.666666666666666 && (trackingPoint5 - _previousLeftVelocity).Scale(num).Magnitude > 450.0)
			{
				trackingAnomaly |= TrackingAnomaly.LeftHandAcceleration;
			}
			if (trackingPoint6.Magnitude > 11.666666666666666 && (trackingPoint6 - _previousRightVelocity).Scale(num).Magnitude > 450.0)
			{
				trackingAnomaly |= TrackingAnomaly.RightHandAcceleration;
			}
		}
		_previous = sample;
		_previousHeadVelocity = trackingPoint4;
		_previousLeftVelocity = trackingPoint5;
		_previousRightVelocity = trackingPoint6;
		_hasVelocity = true;
		return trackingAnomaly;
	}

	internal static int GetAnomalyScore(TrackingAnomaly anomaly)
	{
		if (anomaly == TrackingAnomaly.None || anomaly == TrackingAnomaly.InvalidSample)
		{
			return 0;
		}
		int num = 0;
		if ((anomaly & (TrackingAnomaly.LeftReach | TrackingAnomaly.RightReach)) != 0)
		{
			num++;
		}
		if ((anomaly & (TrackingAnomaly.HeadJump | TrackingAnomaly.LeftHandJump | TrackingAnomaly.RightHandJump)) != 0)
		{
			num += 3;
		}
		if ((anomaly & (TrackingAnomaly.HeadSpeed | TrackingAnomaly.LeftHandSpeed | TrackingAnomaly.RightHandSpeed)) != 0)
		{
			num += 2;
		}
		if ((anomaly & (TrackingAnomaly.HeadAcceleration | TrackingAnomaly.LeftHandAcceleration | TrackingAnomaly.RightHandAcceleration)) != 0)
		{
			num++;
		}
		return num;
	}
}
