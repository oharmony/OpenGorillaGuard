using System;
using System.Reflection;

namespace UnityInjector.ConsoleUtil;

internal static class SafeConsole
{
	private delegate ConsoleColor GetColorDelegate();

	private delegate void SetColorDelegate(ConsoleColor value);

	private delegate string GetStringDelegate();

	private delegate void SetStringDelegate(string value);

	private static GetColorDelegate _getBackgroundColor;

	private static SetColorDelegate _setBackgroundColor;

	private static GetColorDelegate _getForegroundColor;

	private static SetColorDelegate _setForegroundColor;

	private static GetStringDelegate _getTitle;

	private static SetStringDelegate _setTitle;

	public static bool BackgroundColorExists { get; private set; }

	public static ConsoleColor BackgroundColor
	{
		get
		{
			return _getBackgroundColor();
		}
		set
		{
			_setBackgroundColor(value);
		}
	}

	public static bool ForegroundColorExists { get; private set; }

	public static ConsoleColor ForegroundColor
	{
		get
		{
			return _getForegroundColor();
		}
		set
		{
			_setForegroundColor(value);
		}
	}

	public static bool TitleExists { get; private set; }

	public static string Title
	{
		get
		{
			return _getTitle();
		}
		set
		{
			_setTitle(value);
		}
	}

	static SafeConsole()
	{
		InitColors(typeof(Console));
	}

	private static void InitColors(Type tConsole)
	{
		MethodInfo method = tConsole.GetMethod("get_ForegroundColor", BindingFlags.Static | BindingFlags.Public);
		MethodInfo method2 = tConsole.GetMethod("set_ForegroundColor", BindingFlags.Static | BindingFlags.Public);
		MethodInfo method3 = tConsole.GetMethod("get_BackgroundColor", BindingFlags.Static | BindingFlags.Public);
		MethodInfo method4 = tConsole.GetMethod("set_BackgroundColor", BindingFlags.Static | BindingFlags.Public);
		MethodInfo method5 = tConsole.GetMethod("get_Title", BindingFlags.Static | BindingFlags.Public);
		MethodInfo method6 = tConsole.GetMethod("set_Title", BindingFlags.Static | BindingFlags.Public);
		_setForegroundColor = ((method2 != null) ? ((SetColorDelegate)Delegate.CreateDelegate(typeof(SetColorDelegate), method2)) : ((SetColorDelegate)delegate
		{
		}));
		_setBackgroundColor = ((method4 != null) ? ((SetColorDelegate)Delegate.CreateDelegate(typeof(SetColorDelegate), method4)) : ((SetColorDelegate)delegate
		{
		}));
		_getForegroundColor = ((method != null) ? ((GetColorDelegate)Delegate.CreateDelegate(typeof(GetColorDelegate), method)) : ((GetColorDelegate)(() => ConsoleColor.Gray)));
		_getBackgroundColor = ((method3 != null) ? ((GetColorDelegate)Delegate.CreateDelegate(typeof(GetColorDelegate), method3)) : ((GetColorDelegate)(() => ConsoleColor.Black)));
		_getTitle = ((method5 != null) ? ((GetStringDelegate)Delegate.CreateDelegate(typeof(GetStringDelegate), method5)) : ((GetStringDelegate)(() => string.Empty)));
		_setTitle = ((method6 != null) ? ((SetStringDelegate)Delegate.CreateDelegate(typeof(SetStringDelegate), method6)) : ((SetStringDelegate)delegate
		{
		}));
		BackgroundColorExists = _setBackgroundColor != null && _getBackgroundColor != null;
		ForegroundColorExists = _setForegroundColor != null && _getForegroundColor != null;
		TitleExists = _setTitle != null && _getTitle != null;
	}
}
