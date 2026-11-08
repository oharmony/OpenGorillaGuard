using System;

namespace BepInEx;

internal class BuildInfoAttribute : Attribute
{
	public string Info { get; }

	public BuildInfoAttribute(string info)
	{
		Info = info;
	}
}
