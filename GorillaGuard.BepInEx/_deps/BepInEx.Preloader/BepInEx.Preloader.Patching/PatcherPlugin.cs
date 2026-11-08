using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.Bootstrap;

namespace BepInEx.Preloader.Patching;

public class PatcherPlugin : ICacheable
{

	public Func<IEnumerable<string>> TargetDLLs { get; set; }

	public Action Initializer { get; set; }

	public Action Finalizer { get; set; }

	public AssemblyPatcherDelegate Patcher { get; set; }

	public string TypeName { get; set; } = string.Empty;

	public void Save(BinaryWriter bw)
	{
		bw.Write(TypeName);
	}

	public void Load(BinaryReader br)
	{
		TypeName = br.ReadString();
	}
}
