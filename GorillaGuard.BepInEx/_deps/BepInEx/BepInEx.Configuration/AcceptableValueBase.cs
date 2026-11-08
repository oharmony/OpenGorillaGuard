using System;

namespace BepInEx.Configuration;

public abstract class AcceptableValueBase
{

	public Type ValueType { get; }

	protected AcceptableValueBase(Type valueType)
	{
		ValueType = valueType;
	}

	public abstract object Clamp(object value);

	public abstract bool IsValid(object value);

	public abstract string ToDescriptionString();
}
