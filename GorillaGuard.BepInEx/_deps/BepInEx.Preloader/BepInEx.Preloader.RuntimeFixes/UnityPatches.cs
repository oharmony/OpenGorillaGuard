using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace BepInEx.Preloader.RuntimeFixes;

internal static class UnityPatches
{
	private static Harmony HarmonyInstance { get; set; }

	public static Dictionary<string, string> AssemblyLocations { get; } = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

	public static void Apply()
	{
		HarmonyInstance = Harmony.CreateAndPatchAll(typeof(UnityPatches));
		try
		{
			TraceFix.ApplyFix();
		}
		catch
		{
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Assembly), "Location", MethodType.Getter)]
	public static void GetLocation(ref string __result, Assembly __instance)
	{
		if (AssemblyLocations.TryGetValue(__instance.FullName, out var value))
		{
			__result = value;
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Assembly), "CodeBase", MethodType.Getter)]
	public static void GetCodeBase(ref string __result, Assembly __instance)
	{
		if (AssemblyLocations.TryGetValue(__instance.FullName, out var value))
		{
			__result = "file://" + value.Replace('\\', '/');
		}
	}
}
