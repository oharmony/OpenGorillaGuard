using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MonoMod.Utils;

namespace HarmonyXInterop;

internal static class TranspilerInterop
{
	private static readonly Dictionary<OpCode, OpCode> AllJumpCodes = new Dictionary<OpCode, OpCode>
	{
		{
			OpCodes.Beq_S,
			OpCodes.Beq
		},
		{
			OpCodes.Bge_S,
			OpCodes.Bge
		},
		{
			OpCodes.Bge_Un_S,
			OpCodes.Bge_Un
		},
		{
			OpCodes.Bgt_S,
			OpCodes.Bgt
		},
		{
			OpCodes.Bgt_Un_S,
			OpCodes.Bgt_Un
		},
		{
			OpCodes.Ble_S,
			OpCodes.Ble
		},
		{
			OpCodes.Ble_Un_S,
			OpCodes.Ble_Un
		},
		{
			OpCodes.Blt_S,
			OpCodes.Blt
		},
		{
			OpCodes.Blt_Un_S,
			OpCodes.Blt_Un
		},
		{
			OpCodes.Bne_Un_S,
			OpCodes.Bne_Un
		},
		{
			OpCodes.Brfalse_S,
			OpCodes.Brfalse
		},
		{
			OpCodes.Brtrue_S,
			OpCodes.Brtrue
		},
		{
			OpCodes.Br_S,
			OpCodes.Br
		},
		{
			OpCodes.Leave_S,
			OpCodes.Leave
		}
	};

	private static readonly Dictionary<MethodInfo, MethodInfo> Wrappers = new Dictionary<MethodInfo, MethodInfo>();

	private static readonly MethodInfo ResolveToken = AccessTools.Method(typeof(MethodBase), "GetMethodFromHandle", new Type[1] { typeof(RuntimeMethodHandle) });

	private static readonly MethodInfo ApplyTranspilerMethod = AccessTools.Method(typeof(TranspilerInterop), "ApplyTranspiler");

	public static MethodInfo WrapInterop(MethodInfo transpiler)
	{
		lock (Wrappers)
		{
			if (Wrappers.TryGetValue(transpiler, out var value))
			{
				return value;
			}
		}
		using DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition("TranspilerWrapper<" + transpiler.GetID(null, null, withType: true, proxyMethod: false, simple: true) + ">", typeof(IEnumerable<CodeInstruction>), new Type[3]
		{
			typeof(IEnumerable<CodeInstruction>),
			typeof(ILGenerator),
			typeof(MethodBase)
		});
		ILGenerator iLGenerator = dynamicMethodDefinition.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldtoken, transpiler);
		iLGenerator.Emit(OpCodes.Call, ResolveToken);
		iLGenerator.Emit(OpCodes.Castclass, typeof(MethodInfo));
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.Emit(OpCodes.Ldarg_2);
		iLGenerator.Emit(OpCodes.Call, ApplyTranspilerMethod);
		iLGenerator.Emit(OpCodes.Ret);
		MethodInfo methodInfo = dynamicMethodDefinition.GenerateWith<DMDCecilGenerator>();
		lock (Wrappers)
		{
			Wrappers[transpiler] = methodInfo;
		}
		return methodInfo;
	}

	private static IEnumerable<CodeInstruction> ApplyTranspiler(MethodInfo transpiler, IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase method)
	{
		Dictionary<object, Dictionary<string, object>> unassignedValues;
		IEnumerable enumerable = ConvertToGeneralInstructions(transpiler, instructions, out unassignedValues);
		List<object> originalInstructions = null;
		if (unassignedValues != null)
		{
			originalInstructions = instructions.Cast<object>().ToList();
		}
		List<object> transpilerCallParameters = GetTranspilerCallParameters(generator, transpiler, method, enumerable);
		if (transpiler.Invoke(null, transpilerCallParameters.ToArray()) is IEnumerable enumerable2)
		{
			enumerable = enumerable2;
		}
		if (unassignedValues != null)
		{
			enumerable = ConvertToOurInstructions(enumerable, typeof(CodeInstruction), originalInstructions, unassignedValues);
		}
		return (enumerable as List<CodeInstruction>) ?? enumerable.Cast<CodeInstruction>().ToList();
	}

	private static OpCode ReplaceShortJumps(OpCode opcode)
	{
		using (IEnumerator<KeyValuePair<OpCode, OpCode>> enumerator = AllJumpCodes.Where((KeyValuePair<OpCode, OpCode> pair) => opcode == pair.Key).GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current.Value;
			}
		}
		return opcode;
	}

	private static object ConvertInstruction(Type type, object instruction, out Dictionary<string, object> unassigned)
	{
		Dictionary<string, object> nonExisting = new Dictionary<string, object>();
		object result = AccessTools.MakeDeepCopy(instruction, type, delegate(string namePath, Traverse trvSrc, Traverse trvDest)
		{
			object value = trvSrc.GetValue();
			if (trvDest.FieldExists())
			{
				if (!(namePath == "opcode"))
				{
					return value;
				}
				return ReplaceShortJumps((OpCode)value);
			}
			nonExisting[namePath] = value;
			return (object)null;
		});
		unassigned = nonExisting;
		return result;
	}

	private static bool ShouldAddExceptionInfo(object op, int opIndex, List<object> originalInstructions, List<object> newInstructions, Dictionary<object, Dictionary<string, object>> unassignedValues)
	{
		int num = originalInstructions.IndexOf(op);
		if (num == -1)
		{
			return false;
		}
		if (!unassignedValues.TryGetValue(op, out var unassigned))
		{
			return false;
		}
		if (!unassigned.TryGetValue("blocks", out var blocksObject))
		{
			return false;
		}
		List<ExceptionBlock> blocks = blocksObject as List<ExceptionBlock>;
		if (newInstructions.Count((object instr) => instr == op) <= 1)
		{
			return true;
		}
		ExceptionBlock exceptionBlock = blocks.FirstOrDefault((ExceptionBlock block) => block.blockType != ExceptionBlockType.EndExceptionBlock);
		ExceptionBlock exceptionBlock2 = blocks.FirstOrDefault((ExceptionBlock block) => block.blockType == ExceptionBlockType.EndExceptionBlock);
		if (exceptionBlock != null && exceptionBlock2 == null)
		{
			object obj = originalInstructions.Skip(num + 1).FirstOrDefault(delegate(object instr)
			{
				if (!unassignedValues.TryGetValue(instr, out unassigned))
				{
					return false;
				}
				if (!unassigned.TryGetValue("blocks", out blocksObject))
				{
					return false;
				}
				blocks = blocksObject as List<ExceptionBlock>;
				return blocks.Any();
			});
			if (obj != null)
			{
				int num2 = num + 1;
				int num3 = num2 + originalInstructions.Skip(num2).ToList().IndexOf(obj) - 1;
				IEnumerable<object> first = originalInstructions.GetRange(num2, num3 - num2).Intersect(newInstructions);
				obj = newInstructions.Skip(opIndex + 1).FirstOrDefault(delegate(object instr)
				{
					if (!unassignedValues.TryGetValue(instr, out unassigned))
					{
						return false;
					}
					if (!unassigned.TryGetValue("blocks", out blocksObject))
					{
						return false;
					}
					blocks = blocksObject as List<ExceptionBlock>;
					return blocks.Any();
				});
				if (obj != null)
				{
					num2 = opIndex + 1;
					num3 = num2 + newInstructions.Skip(opIndex + 1).ToList().IndexOf(obj) - 1;
					List<object> range = newInstructions.GetRange(num2, num3 - num2);
					return !first.Except(range).ToList().Any();
				}
			}
		}
		if (exceptionBlock == null && exceptionBlock2 != null)
		{
			object obj2 = originalInstructions.GetRange(0, num).LastOrDefault(delegate(object instr)
			{
				if (!unassignedValues.TryGetValue(instr, out unassigned))
				{
					return false;
				}
				if (!unassigned.TryGetValue("blocks", out blocksObject))
				{
					return false;
				}
				blocks = blocksObject as List<ExceptionBlock>;
				return blocks.Any();
			});
			if (obj2 == null)
			{
				return true;
			}
			int num4 = originalInstructions.GetRange(0, num).LastIndexOf(obj2);
			int num5 = num;
			IEnumerable<object> first2 = originalInstructions.GetRange(num4, num5 - num4).Intersect(newInstructions);
			obj2 = newInstructions.GetRange(0, opIndex).LastOrDefault(delegate(object instr)
			{
				if (!unassignedValues.TryGetValue(instr, out unassigned))
				{
					return false;
				}
				if (!unassigned.TryGetValue("blocks", out blocksObject))
				{
					return false;
				}
				blocks = blocksObject as List<ExceptionBlock>;
				return blocks.Any();
			});
			if (obj2 == null)
			{
				return true;
			}
			num4 = newInstructions.GetRange(0, opIndex).LastIndexOf(obj2);
			num5 = opIndex;
			List<object> range2 = newInstructions.GetRange(num4, num5 - num4);
			return !first2.Except(range2).Any();
		}
		return true;
	}

	private static IEnumerable ConvertInstructionsAndUnassignedValues(Type type, IEnumerable enumerable, out Dictionary<object, Dictionary<string, object>> unassignedValues)
	{
		Assembly assembly = type.GetGenericTypeDefinition().Assembly;
		Type type2 = assembly.GetType(typeof(List<>).FullName);
		Type type3 = type.GetGenericArguments()[0];
		Type type4 = type2.MakeGenericType(type3);
		object obj = Activator.CreateInstance(assembly.GetType(type4.FullName));
		MethodInfo method = obj.GetType().GetMethod("Add");
		unassignedValues = new Dictionary<object, Dictionary<string, object>>();
		foreach (object item in enumerable)
		{
			Dictionary<string, object> unassigned;
			object obj2 = ConvertInstruction(type3, item, out unassigned);
			unassignedValues.Add(obj2, unassigned);
			method.Invoke(obj, new object[1] { obj2 });
		}
		return obj as IEnumerable;
	}

	private static IEnumerable ConvertToOurInstructions(IEnumerable instructions, Type codeInstructionType, List<object> originalInstructions, Dictionary<object, Dictionary<string, object>> unassignedValues)
	{
		List<object> newInstructions = instructions.Cast<object>().ToList();
		int index = -1;
		foreach (object item in newInstructions)
		{
			index++;
			object obj = AccessTools.MakeDeepCopy(item, codeInstructionType);
			if (unassignedValues.TryGetValue(item, out var value))
			{
				bool addExceptionInfo = ShouldAddExceptionInfo(item, index, originalInstructions, newInstructions, unassignedValues);
				Traverse traverse = Traverse.Create(obj);
				foreach (KeyValuePair<string, object> item2 in value.Where((KeyValuePair<string, object> field) => addExceptionInfo || field.Key != "blocks"))
				{
					traverse.Field(item2.Key).SetValue(item2.Value);
				}
			}
			yield return obj;
		}
	}

	private static bool IsCodeInstructionsParameter(Type type)
	{
		if (type.IsGenericType)
		{
			return type.GetGenericTypeDefinition().Name.StartsWith("IEnumerable", StringComparison.Ordinal);
		}
		return false;
	}

	private static IEnumerable ConvertToGeneralInstructions(MethodInfo transpiler, IEnumerable enumerable, out Dictionary<object, Dictionary<string, object>> unassignedValues)
	{
		Type type = (from p in transpiler.GetParameters()
			select p.ParameterType).FirstOrDefault((Type t) => IsCodeInstructionsParameter(t));
		if (type == null)
		{
			unassignedValues = new Dictionary<object, Dictionary<string, object>>();
			return enumerable;
		}
		if (type != typeof(IEnumerable<CodeInstruction>))
		{
			return ConvertInstructionsAndUnassignedValues(type, enumerable, out unassignedValues);
		}
		unassignedValues = null;
		return (enumerable as IList<CodeInstruction>) ?? ((enumerable as IEnumerable<CodeInstruction>) ?? enumerable.Cast<CodeInstruction>()).ToList();
	}

	private static List<object> GetTranspilerCallParameters(ILGenerator generator, MethodInfo transpiler, MethodBase method, IEnumerable instructions)
	{
		List<object> parameter = new List<object>();
		(from param in transpiler.GetParameters()
			select param.ParameterType).Do(delegate(Type type)
		{
			if (type.IsAssignableFrom(typeof(ILGenerator)))
			{
				parameter.Add(generator);
			}
			else if (type.IsAssignableFrom(typeof(MethodBase)))
			{
				parameter.Add(method);
			}
			else if (IsCodeInstructionsParameter(type))
			{
				parameter.Add(instructions);
			}
		});
		return parameter;
	}
}
