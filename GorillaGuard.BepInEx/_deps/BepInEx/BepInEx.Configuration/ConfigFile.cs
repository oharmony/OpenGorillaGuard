using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using BepInEx.Logging;

namespace BepInEx.Configuration;

public class ConfigFile : IDictionary<ConfigDefinition, ConfigEntryBase>, ICollection<KeyValuePair<ConfigDefinition, ConfigEntryBase>>, IEnumerable<KeyValuePair<ConfigDefinition, ConfigEntryBase>>, IEnumerable
{
	private readonly BepInPlugin _ownerMetadata;

	private readonly object _ioLock = new object();

	internal static ConfigFile CoreConfig { get; } = new ConfigFile(Paths.BepInExConfigPath, saveOnInit: true);

	protected Dictionary<ConfigDefinition, ConfigEntryBase> Entries { get; } = new Dictionary<ConfigDefinition, ConfigEntryBase>();

	private Dictionary<ConfigDefinition, string> OrphanedEntries { get; } = new Dictionary<ConfigDefinition, string>();

	[Obsolete("Use Keys instead")]
	public ReadOnlyCollection<ConfigDefinition> ConfigDefinitions
	{
		get
		{
			lock (_ioLock)
			{
				return Entries.Keys.ToList().AsReadOnly();
			}
		}
	}

	public string ConfigFilePath { get; }

	public bool SaveOnConfigSet { get; set; } = true;

	public int Count
	{
		get
		{
			lock (_ioLock)
			{
				return Entries.Count;
			}
		}
	}

	public bool IsReadOnly => false;

	ConfigEntryBase IDictionary<ConfigDefinition, ConfigEntryBase>.this[ConfigDefinition key]
	{
		get
		{
			lock (_ioLock)
			{
				return Entries[key];
			}
		}
		set
		{
			throw new InvalidOperationException("Directly setting a config entry is not supported");
		}
	}

	public ConfigEntryBase this[ConfigDefinition key]
	{
		get
		{
			lock (_ioLock)
			{
				return Entries[key];
			}
		}
	}

	public ConfigEntryBase this[string section, string key] => this[new ConfigDefinition(section, key)];

	public ICollection<ConfigDefinition> Keys
	{
		get
		{
			lock (_ioLock)
			{
				return Entries.Keys.ToArray();
			}
		}
	}

	ICollection<ConfigEntryBase> IDictionary<ConfigDefinition, ConfigEntryBase>.Values
	{
		get
		{
			lock (_ioLock)
			{
				return Entries.Values.ToArray();
			}
		}
	}

	public event EventHandler ConfigReloaded;

	public event EventHandler<SettingChangedEventArgs> SettingChanged;

	[Obsolete("Use Values instead")]
	public ConfigEntryBase[] GetConfigEntries()
	{
		lock (_ioLock)
		{
			return Entries.Values.ToArray();
		}
	}

	public ConfigFile(string configPath, bool saveOnInit)
		: this(configPath, saveOnInit, null)
	{
	}

	public ConfigFile(string configPath, bool saveOnInit, BepInPlugin ownerMetadata)
	{
		_ownerMetadata = ownerMetadata;
		if (configPath == null)
		{
			throw new ArgumentNullException("configPath");
		}
		configPath = Path.GetFullPath(configPath);
		ConfigFilePath = configPath;
		if (File.Exists(ConfigFilePath))
		{
			Reload();
		}
		else if (saveOnInit)
		{
			Save();
		}
	}

	public void Reload()
	{
		lock (_ioLock)
		{
			OrphanedEntries.Clear();
			string section = string.Empty;
			string[] array = File.ReadAllLines(ConfigFilePath);
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i].Trim();
				if (text.StartsWith("#"))
				{
					continue;
				}
				if (text.StartsWith("[") && text.EndsWith("]"))
				{
					section = text.Substring(1, text.Length - 2);
					continue;
				}
				string[] array2 = text.Split(new char[1] { '=' }, 2);
				if (array2.Length == 2)
				{
					string key = array2[0].Trim();
					string text2 = array2[1].Trim();
					ConfigDefinition key2 = new ConfigDefinition(section, key);
					Entries.TryGetValue(key2, out var value);
					if (value != null)
					{
						value.SetSerializedValue(text2);
					}
					else
					{
						OrphanedEntries[key2] = text2;
					}
				}
			}
		}
		OnConfigReloaded();
	}

	public void Save()
	{
		lock (_ioLock)
		{
			string directoryName = Path.GetDirectoryName(ConfigFilePath);
			if (directoryName != null)
			{
				Directory.CreateDirectory(directoryName);
			}
			using StreamWriter streamWriter = new StreamWriter(ConfigFilePath, append: false, Utility.UTF8NoBom);
			if (_ownerMetadata != null)
			{
				streamWriter.WriteLine($"## Settings file was created by plugin {_ownerMetadata.Name} v{_ownerMetadata.Version}");
				streamWriter.WriteLine("## Plugin GUID: " + _ownerMetadata.GUID);
				streamWriter.WriteLine();
			}
			foreach (var item in from x in Entries.Select((KeyValuePair<ConfigDefinition, ConfigEntryBase> x) => new
				{
					Key = x.Key,
					entry = x.Value,
					value = x.Value.GetSerializedValue()
				}).Concat(OrphanedEntries.Select((KeyValuePair<ConfigDefinition, string> x) => new
				{
					Key = x.Key,
					entry = (ConfigEntryBase)null,
					value = x.Value
				}))
				group x by x.Key.Section into x
				orderby x.Key
				select x)
			{
				streamWriter.WriteLine("[" + item.Key + "]");
				foreach (var item2 in item)
				{
					streamWriter.WriteLine();
					item2.entry?.WriteDescription(streamWriter);
					streamWriter.WriteLine(item2.Key.Key + " = " + item2.value);
				}
				streamWriter.WriteLine();
			}
		}
	}

	[Obsolete("Use ConfigFile[key] or TryGetEntry instead")]
	public ConfigEntry<T> GetSetting<T>(ConfigDefinition configDefinition)
	{
		if (!TryGetEntry(configDefinition, out ConfigEntry<T> entry))
		{
			return null;
		}
		return entry;
	}

	[Obsolete("Use ConfigFile[key] or TryGetEntry instead")]
	public ConfigEntry<T> GetSetting<T>(string section, string key)
	{
		if (!TryGetEntry(section, key, out ConfigEntry<T> entry))
		{
			return null;
		}
		return entry;
	}

	public bool TryGetEntry<T>(ConfigDefinition configDefinition, out ConfigEntry<T> entry)
	{
		lock (_ioLock)
		{
			if (Entries.TryGetValue(configDefinition, out var value))
			{
				entry = (ConfigEntry<T>)value;
				return true;
			}
			entry = null;
			return false;
		}
	}

	public bool TryGetEntry<T>(string section, string key, out ConfigEntry<T> entry)
	{
		return TryGetEntry(new ConfigDefinition(section, key), out entry);
	}

	public ConfigEntry<T> Bind<T>(ConfigDefinition configDefinition, T defaultValue, ConfigDescription configDescription = null)
	{
		if (!TomlTypeConverter.CanConvert(typeof(T)))
		{
			throw new ArgumentException(string.Format("Type {0} is not supported by the config system. Supported types: {1}", typeof(T), string.Join(", ", (from x in TomlTypeConverter.GetSupportedTypes()
				select x.Name).ToArray())));
		}
		lock (_ioLock)
		{
			if (Entries.TryGetValue(configDefinition, out var value))
			{
				return (ConfigEntry<T>)value;
			}
			ConfigEntry<T> configEntry = new ConfigEntry<T>(this, configDefinition, defaultValue, configDescription);
			Entries[configDefinition] = configEntry;
			if (OrphanedEntries.TryGetValue(configDefinition, out var value2))
			{
				configEntry.SetSerializedValue(value2);
				OrphanedEntries.Remove(configDefinition);
			}
			if (SaveOnConfigSet)
			{
				Save();
			}
			return configEntry;
		}
	}

	public ConfigEntry<T> Bind<T>(string section, string key, T defaultValue, ConfigDescription configDescription = null)
	{
		return Bind(new ConfigDefinition(section, key), defaultValue, configDescription);
	}

	public ConfigEntry<T> Bind<T>(string section, string key, T defaultValue, string description)
	{
		return Bind(new ConfigDefinition(section, key), defaultValue, new ConfigDescription(description, null));
	}

	[Obsolete("Use Bind instead")]
	public ConfigEntry<T> AddSetting<T>(ConfigDefinition configDefinition, T defaultValue, ConfigDescription configDescription = null)
	{
		return Bind(configDefinition, defaultValue, configDescription);
	}

	[Obsolete("Use Bind instead")]
	public ConfigEntry<T> AddSetting<T>(string section, string key, T defaultValue, ConfigDescription configDescription = null)
	{
		return Bind(new ConfigDefinition(section, key), defaultValue, configDescription);
	}

	[Obsolete("Use Bind instead")]
	public ConfigEntry<T> AddSetting<T>(string section, string key, T defaultValue, string description)
	{
		return Bind(new ConfigDefinition(section, key), defaultValue, new ConfigDescription(description, null));
	}

	[Obsolete("Use Bind instead")]
	public ConfigWrapper<T> Wrap<T>(string section, string key, string description = null, T defaultValue = default(T))
	{
		lock (_ioLock)
		{
			ConfigDefinition configDefinition = new ConfigDefinition(section, key, description);
			return new ConfigWrapper<T>(Bind(configDefinition, defaultValue, string.IsNullOrEmpty(description) ? null : new ConfigDescription(description, null)));
		}
	}

	[Obsolete("Use Bind instead")]
	public ConfigWrapper<T> Wrap<T>(ConfigDefinition configDefinition, T defaultValue = default(T))
	{
		return Wrap(configDefinition.Section, configDefinition.Key, null, defaultValue);
	}

	internal void OnSettingChanged(object sender, ConfigEntryBase changedEntryBase)
	{
		if (changedEntryBase == null)
		{
			throw new ArgumentNullException("changedEntryBase");
		}
		if (SaveOnConfigSet)
		{
			Save();
		}
		EventHandler<SettingChangedEventArgs> eventHandler = this.SettingChanged;
		if (eventHandler == null)
		{
			return;
		}
		SettingChangedEventArgs e = new SettingChangedEventArgs(changedEntryBase);
		foreach (EventHandler<SettingChangedEventArgs> item in eventHandler.GetInvocationList().Cast<EventHandler<SettingChangedEventArgs>>())
		{
			try
			{
				item(sender, e);
			}
			catch (Exception data)
			{
				Logger.Log(LogLevel.Error, data);
			}
		}
	}

	private void OnConfigReloaded()
	{
		EventHandler eventHandler = this.ConfigReloaded;
		if (eventHandler == null)
		{
			return;
		}
		foreach (EventHandler item in eventHandler.GetInvocationList().Cast<EventHandler>())
		{
			try
			{
				item(this, EventArgs.Empty);
			}
			catch (Exception data)
			{
				Logger.Log(LogLevel.Error, data);
			}
		}
	}

	public IEnumerator<KeyValuePair<ConfigDefinition, ConfigEntryBase>> GetEnumerator()
	{
		return Entries.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	void ICollection<KeyValuePair<ConfigDefinition, ConfigEntryBase>>.Add(KeyValuePair<ConfigDefinition, ConfigEntryBase> item)
	{
		lock (_ioLock)
		{
			Entries.Add(item.Key, item.Value);
		}
	}

	public bool Contains(KeyValuePair<ConfigDefinition, ConfigEntryBase> item)
	{
		lock (_ioLock)
		{
			return ((ICollection<KeyValuePair<ConfigDefinition, ConfigEntryBase>>)Entries).Contains(item);
		}
	}

	void ICollection<KeyValuePair<ConfigDefinition, ConfigEntryBase>>.CopyTo(KeyValuePair<ConfigDefinition, ConfigEntryBase>[] array, int arrayIndex)
	{
		lock (_ioLock)
		{
			((ICollection<KeyValuePair<ConfigDefinition, ConfigEntryBase>>)Entries).CopyTo(array, arrayIndex);
		}
	}

	bool ICollection<KeyValuePair<ConfigDefinition, ConfigEntryBase>>.Remove(KeyValuePair<ConfigDefinition, ConfigEntryBase> item)
	{
		lock (_ioLock)
		{
			return Entries.Remove(item.Key);
		}
	}

	public bool ContainsKey(ConfigDefinition key)
	{
		lock (_ioLock)
		{
			return Entries.ContainsKey(key);
		}
	}

	public void Add(ConfigDefinition key, ConfigEntryBase value)
	{
		throw new InvalidOperationException("Directly adding a config entry is not supported");
	}

	public bool Remove(ConfigDefinition key)
	{
		lock (_ioLock)
		{
			return Entries.Remove(key);
		}
	}

	public void Clear()
	{
		lock (_ioLock)
		{
			Entries.Clear();
		}
	}

	bool IDictionary<ConfigDefinition, ConfigEntryBase>.TryGetValue(ConfigDefinition key, out ConfigEntryBase value)
	{
		lock (_ioLock)
		{
			return Entries.TryGetValue(key, out value);
		}
	}
}
