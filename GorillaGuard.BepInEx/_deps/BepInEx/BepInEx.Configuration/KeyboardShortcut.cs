using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using UnityEngine;

namespace BepInEx.Configuration;

public struct KeyboardShortcut
{

	public static readonly KeyboardShortcut Empty;

	[Obsolete("Use UnityInput.Current.SupportedKeyCodes instead")]
	public static readonly IEnumerable<KeyCode> AllKeyCodes;

	private static KeyCode[] _modifierBlockKeyCodes;

	private readonly KeyCode[] _allKeys;

	public KeyCode MainKey
	{
		get
		{
			if (_allKeys != null && _allKeys.Length != 0)
			{
				return _allKeys[0];
			}
			return (KeyCode)0;
		}
	}

	public IEnumerable<KeyCode> Modifiers => _allKeys?.Skip(1) ?? Enumerable.Empty<KeyCode>();

	static KeyboardShortcut()
	{
		Empty = default(KeyboardShortcut);
		AllKeyCodes = (KeyCode[])Enum.GetValues(typeof(KeyCode));
		TomlTypeConverter.AddConverter(typeof(KeyboardShortcut), new TypeConverter
		{
			ConvertToString = (object o, Type type) => ((KeyboardShortcut)o).Serialize(),
			ConvertToObject = (string s, Type type) => Deserialize(s)
		});
	}

	public KeyboardShortcut(KeyCode mainKey, params KeyCode[] modifiers)
		: this(((IEnumerable<KeyCode>)(object)new KeyCode[1] { (KeyCode)(int)mainKey }).Concat(modifiers).ToArray())
	{

		if ((int)mainKey == 0 && modifiers.Any())
		{
			throw new ArgumentException("Can't set mainKey to KeyCode.None if there are any modifiers");
		}
	}

	private KeyboardShortcut(KeyCode[] keys)
	{
		_allKeys = SanitizeKeys(keys);
	}

	private static KeyCode[] SanitizeKeys(params KeyCode[] keys)
	{
		if (keys.Length == 0 || (int)keys[0] == 0)
		{
			return (KeyCode[])(object)new KeyCode[1];
		}
		return ((IEnumerable<KeyCode>)(object)new KeyCode[1] { keys[0] }).Concat(from x in keys.Skip(1).Distinct()
			where (int)x != (int)keys[0]
			orderby (int)x
			select x).ToArray();
	}

	public static KeyboardShortcut Deserialize(string str)
	{
		try
		{
			return new KeyboardShortcut(((IEnumerable<string>)str.Split(new char[5] { ' ', '+', ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)).Select((Func<string, KeyCode>)((string x) => (KeyCode)Enum.Parse(typeof(KeyCode), x))).ToArray());
		}
		catch (SystemException ex)
		{
			Logger.Log(LogLevel.Error, "Failed to read keybind from settings: " + ex.Message);
			return Empty;
		}
	}

	public string Serialize()
	{
		if (_allKeys == null)
		{
			return string.Empty;
		}
		return string.Join(" + ", _allKeys.Select((KeyCode x) => ((object)(KeyCode)(ref x)).ToString()).ToArray());
	}

	public bool IsDown()
	{

		KeyCode mainKey = MainKey;
		if ((int)mainKey == 0)
		{
			return false;
		}
		if (UnityInput.Current.GetKeyDown(mainKey))
		{
			return ModifierKeyTest();
		}
		return false;
	}

	public bool IsPressed()
	{

		KeyCode mainKey = MainKey;
		if ((int)mainKey == 0)
		{
			return false;
		}
		if (UnityInput.Current.GetKey(mainKey))
		{
			return ModifierKeyTest();
		}
		return false;
	}

	public bool IsUp()
	{

		KeyCode mainKey = MainKey;
		if ((int)mainKey == 0)
		{
			return false;
		}
		if (UnityInput.Current.GetKeyUp(mainKey))
		{
			return ModifierKeyTest();
		}
		return false;
	}

	private bool ModifierKeyTest()
	{

		KeyCode[] allKeys = _allKeys;
		KeyCode mainKey = MainKey;
		if (!allKeys.All((KeyCode c) => c == mainKey || UnityInput.Current.GetKey(c)))
		{
			return false;
		}
		if (_modifierBlockKeyCodes == null)
		{
			_modifierBlockKeyCodes = UnityInput.Current.SupportedKeyCodes.Except((IEnumerable<KeyCode>)(object)new KeyCode[8]
			{
				(KeyCode)323,
				(KeyCode)324,
				(KeyCode)325,
				(KeyCode)326,
				(KeyCode)327,
				(KeyCode)328,
				(KeyCode)329,
				default(KeyCode)
			}).ToArray();
		}
		return _modifierBlockKeyCodes.All((KeyCode c) => !UnityInput.Current.GetKey(c) || allKeys.Contains(c));
	}

	public override string ToString()
	{

		if ((int)MainKey == 0)
		{
			return "Not set";
		}
		return string.Join(" + ", _allKeys.Select((KeyCode c) => ((object)(KeyCode)(ref c)).ToString()).ToArray());
	}

	public override bool Equals(object obj)
	{

		if (obj is KeyboardShortcut keyboardShortcut && MainKey == keyboardShortcut.MainKey)
		{
			return Modifiers.SequenceEqual(keyboardShortcut.Modifiers);
		}
		return false;
	}

	public override int GetHashCode()
	{

		if ((int)MainKey == 0)
		{
			return 0;
		}
		return _allKeys.Aggregate(_allKeys.Length, (int current, KeyCode item) => current * 31 + item);
	}
}
