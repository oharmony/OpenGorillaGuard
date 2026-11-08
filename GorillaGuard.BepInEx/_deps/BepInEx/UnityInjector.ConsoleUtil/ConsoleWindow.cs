using System;
using System.Runtime.InteropServices;
using BepInEx;
using BepInEx.ConsoleUtil;
using MonoMod.Utils;

namespace UnityInjector.ConsoleUtil;

internal class ConsoleWindow
{
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private delegate bool SetForegroundWindowDelegate(IntPtr hWnd);

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	private delegate IntPtr GetForegroundWindowDelegate();

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	private delegate IntPtr GetSystemMenuDelegate(IntPtr hwnd, bool bRevert);

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	private delegate bool DeleteMenuDelegate(IntPtr hMenu, uint uPosition, uint uFlags);

	private const uint SC_CLOSE = 61536u;

	private const uint MF_BYCOMMAND = 0u;

	private const uint LOAD_LIBRARY_SEARCH_SYSTEM32 = 2048u;

	public static IntPtr ConsoleOutHandle;

	public static IntPtr OriginalStdoutHandle;

	private static bool methodsInited;

	private static SetForegroundWindowDelegate setForeground;

	private static GetForegroundWindowDelegate getForeground;

	private static GetSystemMenuDelegate getSystemMenu;

	private static DeleteMenuDelegate deleteMenu;

	public static bool IsAttached { get; private set; }

	public static string Title
	{
		set
		{
			if (IsAttached)
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				if (value.Length > 24500)
				{
					throw new InvalidOperationException("Console title too long");
				}
				if (!SetConsoleTitle(value))
				{
					throw new InvalidOperationException("Console title invalid");
				}
			}
		}
	}

	public static void Attach()
	{
		if (!IsAttached)
		{
			Initialize();
			if (OriginalStdoutHandle == IntPtr.Zero)
			{
				OriginalStdoutHandle = GetStdHandle(-11);
			}
			IntPtr hWnd = getForeground();
			GetConsoleWindow();
			if (GetConsoleWindow() == IntPtr.Zero && !AllocConsole())
			{
				throw new Exception("AllocConsole() failed");
			}
			setForeground(hWnd);
			ConsoleOutHandle = CreateFile("CONOUT$", 3221225472u, 2, IntPtr.Zero, 3, 0, IntPtr.Zero);
			Kon.conOut = ConsoleOutHandle;
			if (!SetStdHandle(-11, ConsoleOutHandle))
			{
				throw new Exception("SetStdHandle() failed");
			}
			if (OriginalStdoutHandle != IntPtr.Zero && ConsoleManager.ConfigConsoleOutRedirectType.Value == ConsoleManager.ConsoleOutRedirectType.ConsoleOut)
			{
				CloseHandle(OriginalStdoutHandle);
			}
			IsAttached = true;
		}
	}

	public static void PreventClose()
	{
		if (IsAttached)
		{
			Initialize();
			IntPtr consoleWindow = GetConsoleWindow();
			IntPtr intPtr = getSystemMenu(consoleWindow, bRevert: false);
			if (intPtr != IntPtr.Zero)
			{
				deleteMenu(intPtr, 61536u, 0u);
			}
		}
	}

	public static void Detach()
	{
		if (IsAttached)
		{
			if (!CloseHandle(ConsoleOutHandle))
			{
				throw new Exception("CloseHandle() failed");
			}
			ConsoleOutHandle = IntPtr.Zero;
			if (!FreeConsole())
			{
				throw new Exception("FreeConsole() failed");
			}
			if (!SetStdHandle(-11, OriginalStdoutHandle))
			{
				throw new Exception("SetStdHandle() failed");
			}
			IsAttached = false;
		}
	}

	private static void Initialize()
	{
		if (!methodsInited)
		{
			methodsInited = true;
			IntPtr hModule = LoadLibraryEx("user32.dll", IntPtr.Zero, 2048u);
			setForeground = GetProcAddress(hModule, "SetForegroundWindow").AsDelegate<SetForegroundWindowDelegate>();
			getForeground = GetProcAddress(hModule, "GetForegroundWindow").AsDelegate<GetForegroundWindowDelegate>();
			getSystemMenu = GetProcAddress(hModule, "GetSystemMenu").AsDelegate<GetSystemMenuDelegate>();
			deleteMenu = GetProcAddress(hModule, "DeleteMenu").AsDelegate<DeleteMenuDelegate>();
		}
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool AllocConsole();

	[DllImport("kernel32.dll")]
	private static extern IntPtr GetConsoleWindow();

	[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
	private static extern bool CloseHandle(IntPtr handle);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr CreateFile(string fileName, uint desiredAccess, int shareMode, IntPtr securityAttributes, int creationDisposition, int flagsAndAttributes, IntPtr templateFile);

	[DllImport("kernel32.dll")]
	private static extern bool FreeConsole();

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr GetStdHandle(int nStdHandle);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool SetStdHandle(int nStdHandle, IntPtr hConsoleOutput);

	[DllImport("kernel32.dll", BestFitMapping = true, CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool SetConsoleTitle(string title);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr LoadLibraryEx(string lpLibFileName, IntPtr hFile, uint dwFlags);
}
