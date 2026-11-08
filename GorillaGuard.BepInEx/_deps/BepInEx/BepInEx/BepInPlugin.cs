using System;
using System.Linq;
using Mono.Cecil;

namespace BepInEx;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class BepInPlugin : Attribute
{

	public string GUID { get; protected set; }

	public string Name { get; protected set; }

	public Version Version { get; protected set; }

	public BepInPlugin(string GUID, string Name, string Version)
	{
		this.GUID = GUID;
		this.Name = Name;
		try
		{
			this.Version = new Version(Version);
		}
		catch
		{
			this.Version = null;
		}
	}

	internal static BepInPlugin FromCecilType(TypeDefinition td)
	{
		CustomAttribute customAttribute = MetadataHelper.GetCustomAttributes<BepInPlugin>(td, inherit: false).FirstOrDefault();
		if (customAttribute == null)
		{
			return null;
		}
		return new BepInPlugin((string)customAttribute.ConstructorArguments[0].Value, (string)customAttribute.ConstructorArguments[1].Value, (string)customAttribute.ConstructorArguments[2].Value);
	}
}
