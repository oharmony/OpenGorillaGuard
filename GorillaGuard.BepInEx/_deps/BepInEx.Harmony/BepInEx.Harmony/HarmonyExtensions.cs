using System;
using HarmonyLib;

namespace BepInEx.Harmony;

public static class HarmonyExtensions
{

	[Obsolete("Use HarmonyLib.Harmony.PatchAll directly", true)]
	public static void PatchAll(this HarmonyLib.Harmony harmonyInstance, Type type)
	{
		HarmonyWrapper.PatchAll(type, harmonyInstance);
	}
}
