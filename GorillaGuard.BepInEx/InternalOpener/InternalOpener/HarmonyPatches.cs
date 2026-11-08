using System.Reflection;
using HarmonyLib;

namespace InternalOpener;

public class HarmonyPatches
{
	private static Harmony instance;

	public const string InstanceId = "com.jacksonjonjr.gorillatag.internalopener";

	public static bool IsPatched { get; private set; }

	internal static void ApplyHarmonyPatches()
	{
		if (!IsPatched)
		{
			if (instance == null)
			{
				instance = new Harmony("com.jacksonjonjr.gorillatag.internalopener");
			}
			instance.PatchAll(Assembly.GetExecutingAssembly());
			IsPatched = true;
		}
	}

	internal static void RemoveHarmonyPatches()
	{
		if (instance != null && IsPatched)
		{
			instance.UnpatchSelf();
			IsPatched = false;
		}
	}
}
