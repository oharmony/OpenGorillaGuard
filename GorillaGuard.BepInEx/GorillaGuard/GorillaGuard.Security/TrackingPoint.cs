using System;

namespace GorillaGuard.Security;

internal readonly struct TrackingPoint
{
	internal double X { get; }

	internal double Y { get; }

	internal double Z { get; }

	internal bool IsFinite
	{
		get
		{
			if (double.IsFinite(X) && double.IsFinite(Y))
			{
				return double.IsFinite(Z);
			}
			return false;
		}
	}

	internal double Magnitude => Math.Sqrt(X * X + Y * Y + Z * Z);

	internal TrackingPoint(double x, double y, double z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	public static TrackingPoint operator -(TrackingPoint left, TrackingPoint right)
	{
		return new TrackingPoint(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
	}

	internal TrackingPoint Scale(double factor)
	{
		return new TrackingPoint(X / factor, Y / factor, Z / factor);
	}
}
