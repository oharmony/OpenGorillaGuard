using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using BepInEx.Configuration;
using BepInEx.Unix;
using MonoMod.Utils;

namespace BepInEx;

internal static class ConsoleManager
{
	public enum ConsoleOutRedirectType
	{
		[Description("Auto")]
		Auto,
		[Description("Console Out")]
		ConsoleOut,
		[Description("Standard Out")]
		StandardOut
	}

	private const uint SHIFT_JIS_CP = 932u;

	private static readonly bool? EnableConsoleArgOverride;

	private const string ENABLE_CONSOLE_ARG = "--enable-console";

	public static readonly ConfigEntry<bool> ConfigConsoleEnabled;

	public static readonly ConfigEntry<bool> ConfigPreventClose;

	public static readonly ConfigEntry<bool> ConfigConsoleShiftJis;

	public static readonly ConfigEntry<ConsoleOutRedirectType> ConfigConsoleOutRedirectType;

	internal static IConsoleDriver Driver { get; set; }

	public static bool ConsoleActive => Driver?.ConsoleActive ?? false;

	public static TextWriter StandardOutStream => Driver?.StandardOut;

	public static TextWriter ConsoleStream => Driver?.ConsoleOut;

	public static bool ConsoleEnabled => EnableConsoleArgOverride ?? ConfigConsoleEnabled.Value;

	static ConsoleManager()
	{
		ConfigConsoleEnabled = ConfigFile.CoreConfig.Bind("Logging.Console", "Enabled", defaultValue: false, "Enables showing a console for log output.");
		ConfigPreventClose = ConfigFile.CoreConfig.Bind("Logging.Console", "PreventClose", defaultValue: false, "If enabled, will prevent closing the console (either by deleting the close button or in other platform-specific way).");
		ConfigConsoleShiftJis = ConfigFile.CoreConfig.Bind("Logging.Console", "ShiftJisEncoding", defaultValue: false, "If true, console is set to the Shift-JIS encoding, otherwise UTF-8 encoding.");
		ConfigConsoleOutRedirectType = ConfigFile.CoreConfig.Bind("Logging.Console", "StandardOutType", ConsoleOutRedirectType.Auto, new StringBuilder().AppendLine("Hints console manager on what handle to assign as StandardOut. Possible values:").AppendLine("Auto - lets BepInEx decide how to redirect console output").AppendLine("ConsoleOut - prefer redirecting to console output; if possible, closes original standard output")
			.AppendLine("StandardOut - prefer redirecting to standard output; if possible, closes console out")
			.ToString());
		try
		{
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			for (int i = 0; i < commandLineArgs.Length; i++)
			{
				if (commandLineArgs[i] == "--enable-console" && i + 1 < commandLineArgs.Length && bool.TryParse(commandLineArgs[i + 1], out var result))
				{
					EnableConsoleArgOverride = result;
				}
			}
		}
		catch (Exception)
		{
		}
	}

	public static void Initialize(bool alreadyActive)
	{
		if (PlatformHelper.Is(Platform.Unix))
		{
			Driver = new LinuxConsoleDriver();
		}
		else if (PlatformHelper.Is(Platform.Windows))
		{
			Driver = new WindowsConsoleDriver();
		}
		Driver.Initialize(alreadyActive);
	}

	private static void DriverCheck()
	{
		if (Driver == null)
		{
			throw new InvalidOperationException("Driver has not been initialized");
		}
	}

	public static void CreateConsole()
	{
		if (!ConsoleActive)
		{
			DriverCheck();
			uint codepage = (ConfigConsoleShiftJis.Value ? 932u : ((uint)Encoding.UTF8.CodePage));
			Driver.CreateConsole(codepage);
			if (ConfigPreventClose.Value)
			{
				Driver.PreventClose();
			}
		}
	}

	public static void DetachConsole()
	{
		if (ConsoleActive)
		{
			DriverCheck();
			Driver.DetachConsole();
		}
	}

	public static void SetConsoleTitle(string title)
	{
		DriverCheck();
		Driver.SetConsoleTitle(title);
	}

	public static void SetConsoleColor(ConsoleColor color)
	{
		DriverCheck();
		Driver.SetConsoleColor(color);
	}
}
