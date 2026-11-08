namespace GorillaGuard.Security;

internal readonly struct TrackingPoseSample
{
	internal double Time { get; }

	internal TrackingPoint Body { get; }

	internal TrackingPoint Head { get; }

	internal TrackingPoint LeftHand { get; }

	internal TrackingPoint RightHand { get; }

	internal bool IsFinite
	{
		get
		{
			if (double.IsFinite(Time) && Body.IsFinite && Head.IsFinite && LeftHand.IsFinite)
			{
				return RightHand.IsFinite;
			}
			return false;
		}
	}

	internal TrackingPoseSample(double time, TrackingPoint body, TrackingPoint head, TrackingPoint leftHand, TrackingPoint rightHand)
	{
		Time = time;
		Body = body;
		Head = head;
		LeftHand = leftHand;
		RightHand = rightHand;
	}
}
