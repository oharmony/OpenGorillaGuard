using System;
using System.Linq;

namespace BepInEx.Configuration;

public class ConfigDefinition : IEquatable<ConfigDefinition>
{
	private static readonly char[] _invalidConfigChars = new char[8] { '=', '\n', '\t', '\\', '"', '\'', '[', ']' };

	public string Section { get; }

	public string Key { get; }

	public ConfigDefinition(string section, string key)
	{
		CheckInvalidConfigChars(section, "section");
		CheckInvalidConfigChars(key, "key");
		Key = key;
		Section = section;
	}

	private static void CheckInvalidConfigChars(string val, string name)
	{
		if (val == null)
		{
			throw new ArgumentNullException(name);
		}
		if (val != val.Trim())
		{
			throw new ArgumentException("Cannot use whitespace characters at start or end of section and key names", name);
		}
		if (val.Any((char c) => _invalidConfigChars.Contains(c)))
		{
			throw new ArgumentException("Cannot use any of the following characters in section and key names: = \\n \\t \\ \" ' [ ]", name);
		}
	}

	[Obsolete("description argument is no longer used, put it in a ConfigDescription instead")]
	public ConfigDefinition(string section, string key, string description)
	{
		Key = key ?? "";
		Section = section ?? "";
	}

	public bool Equals(ConfigDefinition other)
	{
		if (other == null)
		{
			return false;
		}
		if (string.Equals(Key, other.Key))
		{
			return string.Equals(Section, other.Section);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		return Equals(obj as ConfigDefinition);
	}

	public override int GetHashCode()
	{
		return (((Key != null) ? Key.GetHashCode() : 0) * 397) ^ ((Section != null) ? Section.GetHashCode() : 0);
	}

	public static bool operator ==(ConfigDefinition left, ConfigDefinition right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(ConfigDefinition left, ConfigDefinition right)
	{
		return !object.Equals(left, right);
	}

	public override string ToString()
	{
		return Section + "." + Key;
	}
}
