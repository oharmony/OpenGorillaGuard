using System;

namespace BepInEx.Configuration;

public class AcceptableValueRange<T> : AcceptableValueBase where T : IComparable
{

	public virtual T MinValue { get; }

	public virtual T MaxValue { get; }

	public AcceptableValueRange(T minValue, T maxValue)
		: base(typeof(T))
	{
		if (maxValue == null)
		{
			throw new ArgumentNullException("maxValue");
		}
		if (minValue == null)
		{
			throw new ArgumentNullException("minValue");
		}
		if (minValue.CompareTo(maxValue) >= 0)
		{
			throw new ArgumentException("minValue has to be lower than maxValue");
		}
		MinValue = minValue;
		MaxValue = maxValue;
	}

	public override object Clamp(object value)
	{
		if (MinValue.CompareTo(value) > 0)
		{
			return MinValue;
		}
		if (MaxValue.CompareTo(value) < 0)
		{
			return MaxValue;
		}
		return value;
	}

	public override bool IsValid(object value)
	{
		if (MinValue.CompareTo(value) <= 0)
		{
			return MaxValue.CompareTo(value) >= 0;
		}
		return false;
	}

	public override string ToDescriptionString()
	{
		return $"# Acceptable value range: From {MinValue} to {MaxValue}";
	}
}
