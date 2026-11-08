using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using BepInEx.Logging;

namespace BepInEx.Configuration;

public static class TomlTypeConverter
{
	private static bool _lazyLoadedConverters;

	private static Dictionary<Type, TypeConverter> TypeConverters { get; } = new Dictionary<Type, TypeConverter>
	{
		[typeof(string)] = new TypeConverter
		{
			ConvertToString = (object obj, Type type) => ((string)obj).Escape(),
			ConvertToObject = (string str, Type type) => Regex.IsMatch(str, "^\"?\\w:\\\\(?!\\\\)(?!.+\\\\\\\\)") ? str : str.Unescape()
		},
		[typeof(bool)] = new TypeConverter
		{
			ConvertToString = (object obj, Type type) => obj.ToString().ToLowerInvariant(),
			ConvertToObject = (string str, Type type) => bool.Parse(str)
		},
		[typeof(byte)] = new TypeConverter
		{
			ConvertToString = (object obj, Type type) => obj.ToString(),
			ConvertToObject = (string str, Type type) => byte.Parse(str)
		},
		[typeof(sbyte)] = new TypeConverter
		{
			ConvertToString = (object obj, Type type) => obj.ToString(),
			ConvertToObject = (string str, Type type) => sbyte.Parse(str)
		},
		[typeof(byte)] = new TypeConverter
		{
			ConvertToString = (object obj, Type type) => obj.ToString(),
			ConvertToObject = (string str, Type type) => byte.Parse(str)
		},
		[typeof(short)] = new TypeConverter
		{
			ConvertToString = (object obj, Type type) => obj.ToString(),
			ConvertToObject = (string str, Type type) => short.Parse(str)
		},
		[typeof(ushort)] = new TypeConverter
		{
			ConvertToString = (object obj, Type type) => obj.ToString(),
			ConvertToObject = (string str, Type type) => ushort.Parse(str)
		},
		[typeof(int)] = new TypeConverter
		{
			ConvertToString = (object obj, Type type) => obj.ToString(),
			ConvertToObject = (string str, Type type) => int.Parse(str)
		},
		[typeof(uint)] = new TypeConverter
		{
			ConvertToString = (object obj, Type type) => obj.ToString(),
			ConvertToObject = (string str, Type type) => uint.Parse(str)
		},
		[typeof(long)] = new TypeConverter
		{
			ConvertToString = (object obj, Type type) => obj.ToString(),
			ConvertToObject = (string str, Type type) => long.Parse(str)
		},
		[typeof(ulong)] = new TypeConverter
		{
			ConvertToString = (object obj, Type type) => obj.ToString(),
			ConvertToObject = (string str, Type type) => ulong.Parse(str)
		},
		[typeof(float)] = new TypeConverter
		{
			ConvertToString = (object obj, Type type) => ((float)obj).ToString(NumberFormatInfo.InvariantInfo),
			ConvertToObject = (string str, Type type) => float.Parse(str, NumberFormatInfo.InvariantInfo)
		},
		[typeof(double)] = new TypeConverter
		{
			ConvertToString = (object obj, Type type) => ((double)obj).ToString(NumberFormatInfo.InvariantInfo),
			ConvertToObject = (string str, Type type) => double.Parse(str, NumberFormatInfo.InvariantInfo)
		},
		[typeof(decimal)] = new TypeConverter
		{
			ConvertToString = (object obj, Type type) => ((decimal)obj).ToString(NumberFormatInfo.InvariantInfo),
			ConvertToObject = (string str, Type type) => decimal.Parse(str, NumberFormatInfo.InvariantInfo)
		},
		[typeof(Enum)] = new TypeConverter
		{
			ConvertToString = (object obj, Type type) => obj.ToString(),
			ConvertToObject = (string str, Type type) => Enum.Parse(type, str, ignoreCase: true)
		}
	};

	public static string ConvertToString(object value, Type valueType)
	{
		return (GetConverter(valueType) ?? throw new InvalidOperationException($"Cannot convert from type {valueType}")).ConvertToString(value, valueType);
	}

	public static T ConvertToValue<T>(string value)
	{
		return (T)ConvertToValue(value, typeof(T));
	}

	public static object ConvertToValue(string value, Type valueType)
	{
		return (GetConverter(valueType) ?? throw new InvalidOperationException("Cannot convert to type " + valueType.Name)).ConvertToObject(value, valueType);
	}

	public static TypeConverter GetConverter(Type valueType)
	{
		if (valueType == null)
		{
			throw new ArgumentNullException("valueType");
		}
		if (valueType.IsEnum)
		{
			return TypeConverters[typeof(Enum)];
		}
		if (!TypeConverters.TryGetValue(valueType, out var value) && !_lazyLoadedConverters)
		{
			_lazyLoadedConverters = true;
			LazyLoadConverters();
			TypeConverters.TryGetValue(valueType, out value);
		}
		return value;
	}

	public static bool AddConverter(Type type, TypeConverter converter)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (converter == null)
		{
			throw new ArgumentNullException("converter");
		}
		if (CanConvert(type))
		{
			Logger.LogWarning("Tried to add a TomlConverter when one already exists for type " + type.FullName);
			return false;
		}
		TypeConverters.Add(type, converter);
		return true;
	}

	public static bool CanConvert(Type type)
	{
		return GetConverter(type) != null;
	}

	public static IEnumerable<Type> GetSupportedTypes()
	{
		return TypeConverters.Keys;
	}

	private static void LazyLoadConverters()
	{
		try
		{
			LazyTomlConverterLoader.AddUnityEngineConverters();
		}
		catch (Exception ex)
		{
			Logger.LogWarning("Failed to load UnityEngine Toml converters - " + ex.Message);
		}
	}

	private static string Escape(this string txt)
	{
		if (string.IsNullOrEmpty(txt))
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder(txt.Length + 2);
		foreach (char c in txt)
		{
			switch (c)
			{
			case '\0':
				stringBuilder.Append("\\0");
				break;
			case '\a':
				stringBuilder.Append("\\a");
				break;
			case '\b':
				stringBuilder.Append("\\b");
				break;
			case '\t':
				stringBuilder.Append("\\t");
				break;
			case '\n':
				stringBuilder.Append("\\n");
				break;
			case '\v':
				stringBuilder.Append("\\v");
				break;
			case '\f':
				stringBuilder.Append("\\f");
				break;
			case '\r':
				stringBuilder.Append("\\r");
				break;
			case '\'':
				stringBuilder.Append("\\'");
				break;
			case '\\':
				stringBuilder.Append("\\");
				break;
			case '"':
				stringBuilder.Append("\\\"");
				break;
			default:
				stringBuilder.Append(c);
				break;
			}
		}
		return stringBuilder.ToString();
	}

	private static string Unescape(this string txt)
	{
		if (string.IsNullOrEmpty(txt))
		{
			return txt;
		}
		StringBuilder stringBuilder = new StringBuilder(txt.Length);
		int num = 0;
		while (num < txt.Length)
		{
			int num2 = txt.IndexOf('\\', num);
			if (num2 < 0 || num2 == txt.Length - 1)
			{
				num2 = txt.Length;
			}
			stringBuilder.Append(txt, num, num2 - num);
			if (num2 >= txt.Length)
			{
				break;
			}
			char c = txt[num2 + 1];
			switch (c)
			{
			case '0':
				stringBuilder.Append('\0');
				break;
			case 'a':
				stringBuilder.Append('\a');
				break;
			case 'b':
				stringBuilder.Append('\b');
				break;
			case 't':
				stringBuilder.Append('\t');
				break;
			case 'n':
				stringBuilder.Append('\n');
				break;
			case 'v':
				stringBuilder.Append('\v');
				break;
			case 'f':
				stringBuilder.Append('\f');
				break;
			case 'r':
				stringBuilder.Append('\r');
				break;
			case '\'':
				stringBuilder.Append('\'');
				break;
			case '"':
				stringBuilder.Append('"');
				break;
			case '\\':
				stringBuilder.Append('\\');
				break;
			default:
				stringBuilder.Append('\\').Append(c);
				break;
			}
			num = num2 + 2;
		}
		return stringBuilder.ToString();
	}
}
