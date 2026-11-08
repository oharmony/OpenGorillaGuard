using System;
using System.Linq;

namespace BepInEx.Configuration;

public class AcceptableValueList<T> : AcceptableValueBase where T : IEquatable<T>
{

	public virtual T[] AcceptableValues { get; }

	public AcceptableValueList(params T[] acceptableValues)
		: base(typeof(T))
	{
		if (acceptableValues == null)
		{
			throw new ArgumentNullException("acceptableValues");
		}
		if (acceptableValues.Length == 0)
		{
			throw new ArgumentException("At least one acceptable value is needed", "acceptableValues");
		}
		AcceptableValues = acceptableValues;
	}

	public override object Clamp(object value)
	{
		if (IsValid(value))
		{
			return value;
		}
		return AcceptableValues[0];
	}

	public override bool IsValid(object value)
	{
		if (value is T)
		{
			T v = (T)value;
			return AcceptableValues.Any((T x) => x.Equals(v));
		}
		return false;
	}

	public override string ToDescriptionString()
	{
		return "# Acceptable values: " + string.Join(", ", AcceptableValues.Select((T x) => x.ToString()).ToArray());
	}
}
