using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace BepInEx.ConsoleUtil;

internal class Kon
{
	private struct CONSOLE_SCREEN_BUFFER_INFO
	{
		internal COORD dwSize;

		internal COORD dwCursorPosition;

		internal short wAttributes;

		internal SMALL_RECT srWindow;

		internal COORD dwMaximumWindowSize;
	}

	private struct COORD
	{
		internal short X;

		internal short Y;
	}

	private struct SMALL_RECT
	{
		internal short Left;

		internal short Top;

		internal short Right;

		internal short Bottom;
	}

	private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

	internal static IntPtr conOut = IntPtr.Zero;

	public static ConsoleColor ForegroundColor
	{
		get
		{
			return GetConsoleColor(isBackground: false);
		}
		set
		{
			SetConsoleColor(isBackground: false, value);
		}
	}

	public static ConsoleColor BackgroundColor
	{
		get
		{
			return GetConsoleColor(isBackground: true);
		}
		set
		{
			SetConsoleColor(isBackground: true, value);
		}
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool GetConsoleScreenBufferInfo(IntPtr hConsoleOutput, out CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool SetConsoleTextAttribute(IntPtr hConsoleOutput, short attributes);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr GetStdHandle(int nStdHandle);

	private static short ConsoleColorToColorAttribute(short color, bool isBackground)
	{
		if ((color & -16) != 0)
		{
			throw new ArgumentException("Arg_InvalidConsoleColor");
		}
		if (isBackground)
		{
			color <<= 4;
		}
		return color;
	}

	private static CONSOLE_SCREEN_BUFFER_INFO GetBufferInfo(bool throwOnNoConsole, out bool succeeded)
	{
		succeeded = false;
		if (!(conOut == INVALID_HANDLE_VALUE))
		{
			if (!GetConsoleScreenBufferInfo(conOut, out var lpConsoleScreenBufferInfo))
			{
				bool consoleScreenBufferInfo = GetConsoleScreenBufferInfo(GetStdHandle(-12), out lpConsoleScreenBufferInfo);
				if (!consoleScreenBufferInfo)
				{
					consoleScreenBufferInfo = GetConsoleScreenBufferInfo(GetStdHandle(-10), out lpConsoleScreenBufferInfo);
				}
				if (!consoleScreenBufferInfo && Marshal.GetLastWin32Error() == 6 && !throwOnNoConsole)
				{
					return default(CONSOLE_SCREEN_BUFFER_INFO);
				}
			}
			succeeded = true;
			return lpConsoleScreenBufferInfo;
		}
		if (!throwOnNoConsole)
		{
			return default(CONSOLE_SCREEN_BUFFER_INFO);
		}
		throw new Exception("IO.IO_NoConsole");
	}

	private static void SetConsoleColor(bool isBackground, ConsoleColor c)
	{
		new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
		short num = ConsoleColorToColorAttribute((short)c, isBackground);
		bool succeeded;
		CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo(throwOnNoConsole: false, out succeeded);
		if (succeeded)
		{
			short wAttributes = bufferInfo.wAttributes;
			wAttributes &= (short)(isBackground ? (-241) : (-16));
			wAttributes = (short)((ushort)wAttributes | (ushort)num);
			SetConsoleTextAttribute(conOut, wAttributes);
		}
	}

	private static ConsoleColor GetConsoleColor(bool isBackground)
	{
		bool succeeded;
		CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo(throwOnNoConsole: false, out succeeded);
		if (!succeeded)
		{
			if (!isBackground)
			{
				return ConsoleColor.Gray;
			}
			return ConsoleColor.Black;
		}
		return ColorAttributeToConsoleColor((short)(bufferInfo.wAttributes & 0xF0));
	}

	private static ConsoleColor ColorAttributeToConsoleColor(short c)
	{
		if ((short)(c & 0xFF) != 0)
		{
			c >>= 4;
		}
		return (ConsoleColor)c;
	}

	public static void ResetConsoleColor()
	{
		SetConsoleColor(isBackground: true, ConsoleColor.Black);
		SetConsoleColor(isBackground: false, ConsoleColor.Gray);
	}
}
