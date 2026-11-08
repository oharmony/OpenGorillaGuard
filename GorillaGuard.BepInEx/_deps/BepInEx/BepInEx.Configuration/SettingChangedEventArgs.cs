using System;

namespace BepInEx.Configuration;

public sealed class SettingChangedEventArgs : EventArgs
{

	public ConfigEntryBase ChangedSetting { get; }

	public SettingChangedEventArgs(ConfigEntryBase changedSetting)
	{
		ChangedSetting = changedSetting;
	}
}
