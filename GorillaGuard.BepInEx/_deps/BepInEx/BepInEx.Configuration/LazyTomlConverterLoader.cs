using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BepInEx.Configuration;

internal static class LazyTomlConverterLoader
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static void AddUnityEngineConverters()
	{
		TypeConverter converter = new TypeConverter
		{
			ConvertToString = (object obj, Type type) => ColorUtility.ToHtmlStringRGBA((Color)obj),
			ConvertToObject = delegate(string str, Type type)
			{

				Color val = default(Color);
				if (!ColorUtility.TryParseHtmlString("#" + str.Trim('#', ' '), ref val))
				{
					throw new FormatException("Invalid color string, expected hex #RRGGBBAA");
				}
				return val;
			}
		};
		TomlTypeConverter.AddConverter(typeof(Color), converter);
		TypeConverter converter2 = new TypeConverter
		{
			ConvertToString = (object obj, Type type) => JsonUtility.ToJson(obj),
			ConvertToObject = (string str, Type type) => JsonUtility.FromJson(str, type)
		};
		TomlTypeConverter.AddConverter(typeof(Vector2), converter2);
		TomlTypeConverter.AddConverter(typeof(Vector3), converter2);
		TomlTypeConverter.AddConverter(typeof(Vector4), converter2);
		TomlTypeConverter.AddConverter(typeof(Quaternion), converter2);
		TomlTypeConverter.AddConverter(typeof(Rect), new TypeConverter
		{
			ConvertToObject = StringToRect,
			ConvertToString = RectToString
		});
	}

	private static object StringToRect(string s, Type type)
	{

		Rect val = default(Rect);
		if (s == null)
		{
			return val;
		}
		string[] array = s.Trim('{', '}').Replace(" ", "").Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(':');
			if (array2.Length == 2 && float.TryParse(array2[1], out var result))
			{
				switch (array2[0].Trim('"'))
				{
				case "x":
					((Rect)(ref val)).x = result;
					break;
				case "y":
					((Rect)(ref val)).y = result;
					break;
				case "width":
				case "z":
					((Rect)(ref val)).width = result;
					break;
				case "height":
				case "w":
					((Rect)(ref val)).height = result;
					break;
				}
			}
		}
		return val;
	}

	private static string RectToString(object o, Type type)
	{

		Rect val = (Rect)o;
		return string.Format(CultureInfo.InvariantCulture, "{{ \"x\":{0}, \"y\":{1}, \"width\":{2}, \"height\":{3} }}", ((Rect)(ref val)).x, ((Rect)(ref val)).y, ((Rect)(ref val)).width, ((Rect)(ref val)).height);
	}
}
