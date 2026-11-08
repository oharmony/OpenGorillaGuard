using System;
using HarmonyLib;

namespace BepInEx.Harmony;

[AttributeUsage(AttributeTargets.Method)]
[Obsolete("Use HarmonyLib.ParameterByRefAttribute directly", true)]
public class ParameterByRefAttribute : ParameterByRefAttribute
{

	public int[] ParameterIndices => ((ParameterByRefAttribute)this).ParameterIndices;

	public ParameterByRefAttribute(params int[] parameterIndices)
		: base(parameterIndices)
	{
	}
}
