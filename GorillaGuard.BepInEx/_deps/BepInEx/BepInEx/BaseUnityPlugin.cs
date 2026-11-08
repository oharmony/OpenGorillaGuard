using System;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

namespace BepInEx;

public abstract class BaseUnityPlugin : MonoBehaviour
{

	public PluginInfo Info { get; }

	protected ManualLogSource Logger { get; }

	public ConfigFile Config { get; }

	protected BaseUnityPlugin()
	{
		BepInPlugin metadata = MetadataHelper.GetMetadata(this);
		if (metadata == null)
		{
			throw new InvalidOperationException("Can't create an instance of " + ((object)this).GetType().FullName + " because it inherits from BaseUnityPlugin and the BepInPlugin attribute is missing.");
		}
		if (!Chainloader.IsEditor && Chainloader.PluginInfos.TryGetValue(metadata.GUID, out var value))
		{
			Info = value;
		}
		else
		{
			Info = new PluginInfo
			{
				Metadata = metadata,
				Instance = this,
				Dependencies = MetadataHelper.GetDependencies(((object)this).GetType()),
				Processes = MetadataHelper.GetAttributes<BepInProcess>(((object)this).GetType()),
				Location = ((object)this).GetType().Assembly.Location
			};
		}
		Logger = BepInEx.Logging.Logger.CreateLogSource(metadata.Name);
		string text = (Chainloader.IsEditor ? "." : Paths.ConfigPath);
		Config = new ConfigFile(Utility.CombinePaths(text, metadata.GUID + ".cfg"), saveOnInit: false, metadata);
	}
}
