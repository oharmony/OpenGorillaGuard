using System;
using System.IO;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityInjector.ConsoleUtil;

namespace BepInEx.Unix;

internal class LinuxConsoleDriver : IConsoleDriver
{
	private static readonly ConfigEntry<bool> ForceCustomTtyDriverConfig;

	public static bool UseMonoTtyDriver { get; }

	public TextWriter StandardOut { get; private set; }

	public TextWriter ConsoleOut { get; private set; }

	public bool ConsoleActive { get; private set; }

	public bool ConsoleIsExternal => false;

	public bool StdoutRedirected { get; private set; }

	public TtyInfo TtyInfo { get; private set; }

	static LinuxConsoleDriver()
	{
		ForceCustomTtyDriverConfig = ConfigFile.CoreConfig.Bind("Logging.Console", "ForceBepInExTTYDriver", defaultValue: false, "If enabled, forces to use custom BepInEx TTY driver for handling terminal output on unix.");
		UseMonoTtyDriver = false;
		if (!ForceCustomTtyDriverConfig.Value && typeof(Console).Assembly.GetType("System.ConsoleDriver") != null)
		{
			UseMonoTtyDriver = typeof(Console).Assembly.GetType("System.ParameterizedStrings") != null;
		}
	}

	public void PreventClose()
	{
	}

	public void Initialize(bool alreadyActive)
	{
		ConsoleActive = true;
		StdoutRedirected = UnixStreamHelper.isatty(1) != 1;
		Stream stream = UnixStreamHelper.CreateDuplicateStream(1);
		if (UseMonoTtyDriver && !StdoutRedirected)
		{
			TextWriter textWriter = ConsoleWriter.CreateConsoleStreamWriter(stream, Console.Out.Encoding, leaveOpen: true);
			StandardOut = TextWriter.Synchronized(textWriter);
			object value = AccessTools.Field(AccessTools.TypeByName("System.ConsoleDriver"), "driver").GetValue(null);
			AccessTools.Field(AccessTools.TypeByName("System.TermInfoDriver"), "stdout").SetValue(value, textWriter);
		}
		else
		{
			StreamWriter streamWriter = new StreamWriter(stream, Console.Out.Encoding);
			streamWriter.AutoFlush = true;
			StandardOut = TextWriter.Synchronized(streamWriter);
			TtyInfo = TtyHandler.GetTtyInfo();
		}
		ConsoleOut = StandardOut;
	}

	public void CreateConsole(uint codepage)
	{
		Logger.LogWarning("An external console currently cannot be spawned on a Unix platform.");
	}

	public void DetachConsole()
	{
		throw new PlatformNotSupportedException("Cannot detach console on a Unix platform");
	}

	public void SetConsoleColor(ConsoleColor color)
	{
		if (!StdoutRedirected)
		{
			if (UseMonoTtyDriver)
			{
				SafeConsole.ForegroundColor = color;
			}
			else
			{
				ConsoleOut.Write(TtyInfo.GetAnsiCode(color));
			}
		}
	}

	public void SetConsoleTitle(string title)
	{
		if (!StdoutRedirected)
		{
			if (UseMonoTtyDriver && SafeConsole.TitleExists)
			{
				SafeConsole.Title = title;
			}
			else
			{
				ConsoleOut.Write("\u001b]2;" + title.Replace("\\", "\\\\") + "\a");
			}
		}
	}
}
