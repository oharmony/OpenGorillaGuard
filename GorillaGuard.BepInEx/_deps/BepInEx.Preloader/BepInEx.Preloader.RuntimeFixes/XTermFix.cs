using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.Platforms;
using MonoMod.Utils;

namespace BepInEx.Preloader.RuntimeFixes;

internal static class XTermFix
{
	public static int intOffset;

	public static void Apply()
	{
		if (!PlatformHelper.Is(Platform.Windows) && typeof(Console).Assembly.GetType("System.ConsoleDriver") != null && AccessTools.Method("System.TermInfoReader:DetermineVersion") == null)
		{
			DetourHelper.Native = new DetourNativeX86Platform();
			Harmony harmony = new Harmony("com.bepinex.xtermfix");
			harmony.Patch(AccessTools.Method("System.TermInfoReader:ReadHeader"), new HarmonyMethod(typeof(XTermFix), "ReadHeaderPrefix"));
			harmony.Patch(AccessTools.Method("System.TermInfoReader:Get", new Type[1] { AccessTools.TypeByName("System.TermInfoNumbers") }), null, null, new HarmonyMethod(typeof(XTermFix), "GetTermInfoNumbersTranspiler"));
			harmony.Patch(AccessTools.Method("System.TermInfoReader:Get", new Type[1] { AccessTools.TypeByName("System.TermInfoStrings") }), null, null, new HarmonyMethod(typeof(XTermFix), "GetTermInfoStringsTranspiler"));
			harmony.Patch(AccessTools.Method("System.TermInfoReader:GetStringBytes", new Type[1] { AccessTools.TypeByName("System.TermInfoStrings") }), null, null, new HarmonyMethod(typeof(XTermFix), "GetTermInfoStringsTranspiler"));
			DetourHelper.Native = null;
		}
	}

	public static int GetInt32(byte[] buffer, int offset)
	{
		byte num = buffer[offset];
		int num2 = buffer[offset + 1];
		int num3 = buffer[offset + 2];
		int num4 = buffer[offset + 3];
		return num | (num2 << 8) | (num3 << 16) | (num4 << 24);
	}

	public static short GetInt16(byte[] buffer, int offset)
	{
		byte num = buffer[offset];
		int num2 = buffer[offset + 1];
		return (short)(num | (num2 << 8));
	}

	public static int GetInteger(byte[] buffer, int offset)
	{
		if (intOffset != 2)
		{
			return GetInt32(buffer, offset);
		}
		return GetInt16(buffer, offset);
	}

	public static void DetermineVersion(short magic)
	{
		switch (magic)
		{
		case 282:
			intOffset = 2;
			break;
		case 542:
			intOffset = 4;
			break;
		default:
			throw new Exception($"Unknown xterm header format: {magic}");
		}
	}

	public static bool ReadHeaderPrefix(byte[] buffer, ref int position, ref short ___boolSize, ref short ___numSize, ref short ___strOffsets)
	{
		short @int = GetInt16(buffer, position);
		position += 2;
		DetermineVersion(@int);
		position += 2;
		___boolSize = GetInt16(buffer, position);
		position += 2;
		___numSize = GetInt16(buffer, position);
		position += 2;
		___strOffsets = GetInt16(buffer, position);
		position += 2;
		position += 2;
		return false;
	}

	public static IEnumerable<CodeInstruction> GetTermInfoNumbersTranspiler(IEnumerable<CodeInstruction> instructions)
	{
		List<CodeInstruction> list = instructions.ToList();
		list[31] = new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(XTermFix), "intOffset"));
		list[36] = new CodeInstruction(OpCodes.Nop);
		list[39] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(XTermFix), "GetInteger"));
		return list;
	}

	public static IEnumerable<CodeInstruction> GetTermInfoStringsTranspiler(IEnumerable<CodeInstruction> instructions)
	{
		List<CodeInstruction> list = instructions.ToList();
		list[32] = new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(XTermFix), "intOffset"));
		return list;
	}
}
