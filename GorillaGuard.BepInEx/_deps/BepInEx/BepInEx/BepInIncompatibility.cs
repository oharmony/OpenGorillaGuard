using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Bootstrap;
using Mono.Cecil;

namespace BepInEx;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class BepInIncompatibility : Attribute, ICacheable
{

	public string IncompatibilityGUID { get; protected set; }

	public BepInIncompatibility(string IncompatibilityGUID)
	{
		this.IncompatibilityGUID = IncompatibilityGUID;
	}

	internal static IEnumerable<BepInIncompatibility> FromCecilType(TypeDefinition td)
	{
		return (from customAttribute in MetadataHelper.GetCustomAttributes<BepInIncompatibility>(td, inherit: true)
			select new BepInIncompatibility((string)customAttribute.ConstructorArguments[0].Value)).ToList();
	}

	void ICacheable.Save(BinaryWriter bw)
	{
		bw.Write(IncompatibilityGUID);
	}

	void ICacheable.Load(BinaryReader br)
	{
		IncompatibilityGUID = br.ReadString();
	}
}
