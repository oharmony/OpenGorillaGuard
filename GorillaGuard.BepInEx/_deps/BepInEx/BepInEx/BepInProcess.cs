using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace BepInEx;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class BepInProcess : Attribute
{

	public string ProcessName { get; protected set; }

	public BepInProcess(string ProcessName)
	{
		this.ProcessName = ProcessName;
	}

	internal static List<BepInProcess> FromCecilType(TypeDefinition td)
	{
		return (from customAttribute in MetadataHelper.GetCustomAttributes<BepInProcess>(td, inherit: true)
			select new BepInProcess((string)customAttribute.ConstructorArguments[0].Value)).ToList();
	}
}
