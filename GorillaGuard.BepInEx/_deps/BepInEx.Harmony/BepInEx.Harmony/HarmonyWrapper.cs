using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace BepInEx.Harmony;

public class HarmonyWrapper
{

	[Obsolete("Use HarmonyLib.Harmony.CreateAndPatchAll or HarmonyLib.Harmony.PatchAll", true)]
	public static HarmonyLib.Harmony PatchAll(Type type, HarmonyLib.Harmony harmonyInstance = null)
	{
		if (harmonyInstance == null)
		{
			return HarmonyLib.Harmony.CreateAndPatchAll(type);
		}
		harmonyInstance.PatchAll(type);
		return harmonyInstance;
	}

	[Obsolete("Use HarmonyLib.Harmony.CreateAndPatchAll", true)]
	public static HarmonyLib.Harmony PatchAll(Type type, string harmonyInstanceId)
	{
		return HarmonyLib.Harmony.CreateAndPatchAll(type, harmonyInstanceId);
	}

	[Obsolete("Use HarmonyLib.Harmony.CreateAndPatchAll or HarmonyLib.Harmony.PatchAll", true)]
	public static HarmonyLib.Harmony PatchAll(Assembly assembly, HarmonyLib.Harmony harmonyInstance = null)
	{
		return assembly.GetTypes().Aggregate(harmonyInstance, (HarmonyLib.Harmony current, Type type) => PatchAll(type, current));
	}

	[Obsolete("Use HarmonyLib.Harmony.CreateAndPatchAll", true)]
	public static HarmonyLib.Harmony PatchAll(Assembly assembly, string harmonyInstanceId)
	{
		return HarmonyLib.Harmony.CreateAndPatchAll(assembly, harmonyInstanceId);
	}

	[Obsolete("Use HarmonyLib.Harmony.PatchAll with no arguments", true)]
	public static HarmonyLib.Harmony PatchAll(HarmonyLib.Harmony harmonyInstance = null)
	{
		return PatchAll(Assembly.GetCallingAssembly(), harmonyInstance);
	}

	[Obsolete("Use HarmonyLib.Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), ...)", true)]
	public static HarmonyLib.Harmony PatchAll(string harmonyInstanceId)
	{
		return PatchAll(Assembly.GetCallingAssembly(), harmonyInstanceId);
	}

	[Obsolete("Use HarmonyLib.Transpilers.EmitDelegate instead", true)]
	public static CodeInstruction EmitDelegate<T>(T action) where T : Delegate
	{
		return Transpilers.EmitDelegate(action);
	}
}
