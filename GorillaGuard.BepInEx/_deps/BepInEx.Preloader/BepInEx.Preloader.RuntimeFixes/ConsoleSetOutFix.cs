using System;
using System.IO;
using BepInEx.Logging;
using HarmonyLib;

namespace BepInEx.Preloader.RuntimeFixes;

internal static class ConsoleSetOutFix
{
	private static LoggedTextWriter loggedTextWriter;

	internal static ManualLogSource ConsoleLogSource = Logger.CreateLogSource("Console");

	public static void Apply()
	{
		loggedTextWriter = new LoggedTextWriter
		{
			Parent = Console.Out
		};
		Console.SetOut(loggedTextWriter);
		Harmony.CreateAndPatchAll(typeof(ConsoleSetOutFix));
	}

	[HarmonyPatch(typeof(Console), "SetOut")]
	[HarmonyPrefix]
	private static bool OnSetOut(TextWriter newOut)
	{
		loggedTextWriter.Parent = newOut;
		return false;
	}
}
