using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Bootstrap;

namespace BepInEx;

public class PluginInfo : ICacheable
{

	public BepInPlugin Metadata { get; internal set; }

	public IEnumerable<BepInProcess> Processes { get; internal set; }

	public IEnumerable<BepInDependency> Dependencies { get; internal set; }

	public IEnumerable<BepInIncompatibility> Incompatibilities { get; internal set; }

	public string Location { get; internal set; }

	public BaseUnityPlugin Instance { get; internal set; }

	internal string TypeName { get; set; }

	internal Version TargettedBepInExVersion { get; set; }

	void ICacheable.Save(BinaryWriter bw)
	{
		bw.Write(TypeName);
		bw.Write(Metadata.GUID);
		bw.Write(Metadata.Name);
		bw.Write(Metadata.Version.ToString());
		List<BepInProcess> list = Processes.ToList();
		bw.Write(list.Count);
		foreach (BepInProcess item in list)
		{
			bw.Write(item.ProcessName);
		}
		List<BepInDependency> list2 = Dependencies.ToList();
		bw.Write(list2.Count);
		foreach (BepInDependency item2 in list2)
		{
			((ICacheable)item2).Save(bw);
		}
		List<BepInIncompatibility> list3 = Incompatibilities.ToList();
		bw.Write(list3.Count);
		foreach (BepInIncompatibility item3 in list3)
		{
			((ICacheable)item3).Save(bw);
		}
		bw.Write(TargettedBepInExVersion.ToString(4));
	}

	void ICacheable.Load(BinaryReader br)
	{
		TypeName = br.ReadString();
		Metadata = new BepInPlugin(br.ReadString(), br.ReadString(), br.ReadString());
		int num = br.ReadInt32();
		List<BepInProcess> list = new List<BepInProcess>(num);
		for (int i = 0; i < num; i++)
		{
			list.Add(new BepInProcess(br.ReadString()));
		}
		Processes = list;
		int num2 = br.ReadInt32();
		List<BepInDependency> list2 = new List<BepInDependency>(num2);
		for (int j = 0; j < num2; j++)
		{
			BepInDependency bepInDependency = new BepInDependency("");
			((ICacheable)bepInDependency).Load(br);
			list2.Add(bepInDependency);
		}
		Dependencies = list2;
		int num3 = br.ReadInt32();
		List<BepInIncompatibility> list3 = new List<BepInIncompatibility>(num3);
		for (int k = 0; k < num3; k++)
		{
			BepInIncompatibility bepInIncompatibility = new BepInIncompatibility("");
			((ICacheable)bepInIncompatibility).Load(br);
			list3.Add(bepInIncompatibility);
		}
		Incompatibilities = list3;
		TargettedBepInExVersion = new Version(br.ReadString());
	}

	public override string ToString()
	{
		return $"{Metadata?.Name} {Metadata?.Version}";
	}
}
