using System;

namespace BepInEx.Configuration;

[Obsolete("Use ConfigFile from new Bind overloads instead")]
public sealed class ConfigWrapper<T>
{

	public ConfigEntry<T> ConfigEntry { get; }

	public ConfigDefinition Definition => ConfigEntry.Definition;

	public ConfigFile ConfigFile => ConfigEntry.ConfigFile;

	public T Value
	{
		get
		{
			return ConfigEntry.Value;
		}
		set
		{
			ConfigEntry.Value = value;
		}
	}

	public event EventHandler SettingChanged;

	internal ConfigWrapper(ConfigEntry<T> configEntry)
	{
		ConfigWrapper<T> configWrapper = this;
		ConfigEntry = configEntry ?? throw new ArgumentNullException("configEntry");
		configEntry.ConfigFile.SettingChanged += delegate(object sender, SettingChangedEventArgs args)
		{
			if (args.ChangedSetting == configEntry)
			{
				configWrapper.SettingChanged?.Invoke(sender, args);
			}
		};
	}
}
