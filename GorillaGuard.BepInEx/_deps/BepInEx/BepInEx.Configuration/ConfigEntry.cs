using System;

namespace BepInEx.Configuration;

public sealed class ConfigEntry<T> : ConfigEntryBase
{
	private T _typedValue;

	public T Value
	{
		get
		{
			return _typedValue;
		}
		set
		{
			value = ClampValue(value);
			if (!object.Equals(_typedValue, value))
			{
				_typedValue = value;
				OnSettingChanged(this);
			}
		}
	}

	public override object BoxedValue
	{
		get
		{
			return Value;
		}
		set
		{
			Value = (T)value;
		}
	}

	public event EventHandler SettingChanged;

	internal ConfigEntry(ConfigFile configFile, ConfigDefinition definition, T defaultValue, ConfigDescription configDescription)
		: base(configFile, definition, typeof(T), defaultValue, configDescription)
	{
		configFile.SettingChanged += delegate(object sender, SettingChangedEventArgs args)
		{
			if (args.ChangedSetting == this)
			{
				this.SettingChanged?.Invoke(sender, args);
			}
		};
	}
}
