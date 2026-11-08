using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx.ConsoleUtil;
using HarmonyLib;
using Microsoft.Win32.SafeHandles;
using UnityInjector.ConsoleUtil;

namespace BepInEx;

internal class WindowsConsoleDriver : IConsoleDriver
{
	private static readonly ConstructorInfo FileStreamCtor = new ConstructorInfo[2]
	{
		AccessTools.Constructor(typeof(FileStream), new Type[2]
		{
			typeof(SafeFileHandle),
			typeof(FileAccess)
		}),
		AccessTools.Constructor(typeof(FileStream), new Type[2]
		{
			typeof(IntPtr),
			typeof(FileAccess)
		})
	}.FirstOrDefault((ConstructorInfo m) => m != null);

	public TextWriter StandardOut { get; private set; }

	public TextWriter ConsoleOut { get; private set; }

	public bool ConsoleActive { get; private set; }

	public bool ConsoleIsExternal => true;

	public void PreventClose()
	{
		ConsoleWindow.PreventClose();
	}

	public void Initialize(bool alreadyActive)
	{
		ConsoleActive = alreadyActive;
		StandardOut = Console.Out;
	}

	private static FileStream OpenFileStream(IntPtr handle)
	{
		SafeFileHandle safeFileHandle = new SafeFileHandle(handle, ownsHandle: false);
		object[] args = AccessTools.ActualParameters(FileStreamCtor, new object[3]
		{
			safeFileHandle,
			safeFileHandle.DangerousGetHandle(),
			FileAccess.Write
		});
		return (FileStream)Activator.CreateInstance(typeof(FileStream), args);
	}

	public void CreateConsole(uint codepage)
	{
		ConsoleWindow.Attach();
		ConsoleEncoding.ConsoleCodePage = codepage;
		IntPtr outHandle = GetOutHandle();
		if (outHandle == IntPtr.Zero)
		{
			StandardOut = TextWriter.Null;
			ConsoleOut = TextWriter.Null;
			return;
		}
		FileStream stream = OpenFileStream(outHandle);
		StandardOut = new StreamWriter(stream, Utility.UTF8NoBom)
		{
			AutoFlush = true
		};
		FileStream stream2 = OpenFileStream(ConsoleWindow.ConsoleOutHandle);
		ConsoleOut = new StreamWriter(stream2, ConsoleEncoding.OutputEncoding)
		{
			AutoFlush = true
		};
		ConsoleActive = true;
	}

	private IntPtr GetOutHandle()
	{
		switch (ConsoleManager.ConfigConsoleOutRedirectType.Value)
		{
		case ConsoleManager.ConsoleOutRedirectType.ConsoleOut:
			return ConsoleWindow.ConsoleOutHandle;
		case ConsoleManager.ConsoleOutRedirectType.StandardOut:
			return ConsoleWindow.OriginalStdoutHandle;
		default:
			if (!(ConsoleWindow.OriginalStdoutHandle != IntPtr.Zero))
			{
				return ConsoleWindow.ConsoleOutHandle;
			}
			return ConsoleWindow.OriginalStdoutHandle;
		}
	}

	public void DetachConsole()
	{
		ConsoleWindow.Detach();
		ConsoleOut.Close();
		ConsoleOut = null;
		ConsoleActive = false;
	}

	public void SetConsoleColor(ConsoleColor color)
	{
		SafeConsole.ForegroundColor = color;
		Kon.ForegroundColor = color;
	}

	public void SetConsoleTitle(string title)
	{
		ConsoleWindow.Title = title;
	}
}
