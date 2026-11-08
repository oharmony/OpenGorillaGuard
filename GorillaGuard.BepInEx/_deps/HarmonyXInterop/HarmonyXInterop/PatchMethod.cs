using System.Reflection;
using HarmonyLib;

namespace HarmonyXInterop;

public class PatchMethod
{
	public string[] after;

	public string[] before;

	public MethodInfo method;

	public string owner;

	public int priority = -1;

	public HarmonyMethod ToHarmonyMethod()
	{
		return new HarmonyMethod
		{
			after = after,
			before = before,
			method = method,
			priority = priority
		};
	}
}
