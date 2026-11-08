using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Bootstrap;
using Mono.Cecil;

namespace BepInEx;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class BepInDependency : Attribute, ICacheable
{

	[Flags]
	public enum DependencyFlags
	{

		HardDependency = 1,

		SoftDependency = 2
	}

	public string DependencyGUID { get; protected set; }

	public DependencyFlags Flags { get; protected set; }

	public Version MinimumVersion { get; protected set; }

	public BepInDependency(string DependencyGUID, DependencyFlags Flags = DependencyFlags.HardDependency)
	{
		this.DependencyGUID = DependencyGUID;
		this.Flags = Flags;
		MinimumVersion = new Version();
	}

	public BepInDependency(string DependencyGUID, string MinimumDependencyVersion)
		: this(DependencyGUID)
	{
		MinimumVersion = new Version(MinimumDependencyVersion);
	}

	internal static IEnumerable<BepInDependency> FromCecilType(TypeDefinition td)
	{
		return MetadataHelper.GetCustomAttributes<BepInDependency>(td, inherit: true).Select(delegate(CustomAttribute customAttribute)
		{
			string dependencyGUID = (string)customAttribute.ConstructorArguments[0].Value;
			object value = customAttribute.ConstructorArguments[1].Value;
			return (value is string minimumDependencyVersion) ? new BepInDependency(dependencyGUID, minimumDependencyVersion) : new BepInDependency(dependencyGUID, (DependencyFlags)value);
		}).ToList();
	}

	void ICacheable.Save(BinaryWriter bw)
	{
		bw.Write(DependencyGUID);
		bw.Write((int)Flags);
		bw.Write(MinimumVersion.ToString());
	}

	void ICacheable.Load(BinaryReader br)
	{
		DependencyGUID = br.ReadString();
		Flags = (DependencyFlags)br.ReadInt32();
		MinimumVersion = new Version(br.ReadString());
	}
}
