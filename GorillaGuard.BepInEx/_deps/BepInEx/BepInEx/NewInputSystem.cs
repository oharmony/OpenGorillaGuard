using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx.Logging;
using HarmonyLib;
using MonoMod.Utils;
using UnityEngine;

namespace BepInEx;

internal class NewInputSystem : IInputSystem
{
	private delegate bool GetButtonDelegate(KeyCode key);

	private delegate Vector2 GetMouseVectorDelegate();

	private delegate bool GetMouseStateDelegate();

	private static bool initialized;

	private static readonly Dictionary<KeyCode, int> keyCodeToIndex = new Dictionary<KeyCode, int>();

	private static readonly Dictionary<string, string> keyToKeyCodeNameRemap = new Dictionary<string, string>
	{
		["LeftCtrl"] = "LeftControl",
		["RightCtrl"] = "RightControl",
		["LeftMeta"] = "LeftApple",
		["RightMeta"] = "RightApple",
		["ContextMenu"] = "Menu",
		["PrintScreen"] = "Print",
		["Enter"] = "Return"
	};

	private static GetButtonDelegate _getKey;

	private static GetButtonDelegate _getKeyDown;

	private static GetButtonDelegate _getKeyUp;

	private static GetButtonDelegate _getMouseButton;

	private static GetButtonDelegate _getMouseButtonDown;

	private static GetButtonDelegate _getMouseButtonUp;

	private static GetMouseVectorDelegate _getMousePosition;

	private static GetMouseVectorDelegate _getMouseScrollDelta;

	private static GetMouseStateDelegate _getMousePresent;

	private static GetMouseStateDelegate _getKeyboardAnyKeyIsPressed;

	private static GetMouseStateDelegate _getKeyboardAnyKeyWasPressedThisFrame;

	public Vector3 mousePosition => Vector2.op_Implicit(_getMousePosition());

	public Vector2 mouseScrollDelta => _getMouseScrollDelta();

	public bool mousePresent => _getMousePresent();

	public bool anyKey
	{
		get
		{
			if (_getKeyboardAnyKeyIsPressed())
			{
				return true;
			}
			if (!GetMouseButton(0) && !GetMouseButton(1) && !GetMouseButton(2) && !GetMouseButton(3))
			{
				return GetMouseButton(4);
			}
			return true;
		}
	}

	public bool anyKeyDown
	{
		get
		{
			if (_getKeyboardAnyKeyWasPressedThisFrame())
			{
				return true;
			}
			if (!GetMouseButtonDown(0) && !GetMouseButtonDown(1) && !GetMouseButtonDown(2) && !GetMouseButtonDown(3))
			{
				return GetMouseButtonDown(4);
			}
			return true;
		}
	}

	public IEnumerable<KeyCode> SupportedKeyCodes => keyCodeToIndex.Keys.ToArray();

	public NewInputSystem()
	{
		Init();
	}

	private static void Init()
	{

		if (initialized)
		{
			return;
		}
		Assembly assembly = Assembly.Load("Unity.InputSystem");
		Type type = assembly.GetType("UnityEngine.InputSystem.Key");
		foreach (object value3 in Enum.GetValues(type))
		{
			string text = value3.ToString();
			int value = (int)value3;
			string value2;
			if (text.StartsWith("Numpad"))
			{
				text = text.Replace("Numpad", "Keypad");
			}
			else if (text.StartsWith("Digit"))
			{
				text = text.Replace("Digit", "Alpha");
			}
			else if (keyToKeyCodeNameRemap.TryGetValue(text, out value2))
			{
				text = value2;
			}
			if (TryEnumParse<KeyCode>(text, out var val))
			{
				keyCodeToIndex[val] = value;
			}
			else
			{
				Logger.LogDebug("[UnityInput] Unknown key name: " + text + ", skipping remapping");
			}
		}
		FieldInfo keyCodeToIndexField = AccessTools.Field(typeof(NewInputSystem), "keyCodeToIndex");
		MethodInfo tryGetValueMethod = AccessTools.Method(typeof(Dictionary<KeyCode, int>), "TryGetValue");
		Type type2 = assembly.GetType("UnityEngine.InputSystem.Keyboard");
		MethodInfo currentProperty = AccessTools.PropertyGetter(type2, "current");
		MethodInfo indexer = AccessTools.Method(type2, "get_Item", new Type[1] { type });
		Type buttonControlType = assembly.GetType("UnityEngine.InputSystem.Controls.ButtonControl");
		_getKey = GenerateKeyGetter("InputSystemGetKey", "isPressed");
		_getKeyDown = GenerateKeyGetter("InputSystemGetKeyDown", "wasPressedThisFrame");
		_getKeyUp = GenerateKeyGetter("InputSystemGetKeyUp", "wasReleasedThisFrame");
		Type mouseType = assembly.GetType("UnityEngine.InputSystem.Mouse");
		MethodInfo mouseCurrentProperty = AccessTools.PropertyGetter(mouseType, "current");
		string[] source = new string[5] { "leftButton", "rightButton", "middleButton", "backButton", "forwardButton" };
		MethodInfo[] buttonPropertyGetters = source.Select((string p) => AccessTools.PropertyGetter(mouseType, p)).ToArray();
		_getMouseButton = GenerateMouseGetter("InputSystemGetMouseButton", "isPressed");
		_getMouseButtonDown = GenerateMouseGetter("InputSystemGetMouseButtonDown", "wasPressedThisFrame");
		_getMouseButtonUp = GenerateMouseGetter("InputSystemGetMouseButtonUp", "wasReleasedThisFrame");
		_getMousePosition = GetPositionDelegate("InputSystemGetMousePosition", "position");
		_getMouseScrollDelta = GetPositionDelegate("InputSystemGetMouseScrollDelta", "scroll");
		DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition("InputSystemGetMouseEnabled", typeof(bool), Type.EmptyTypes);
		ILGenerator iLGenerator = dynamicMethodDefinition.GetILGenerator();
		iLGenerator.Emit(OpCodes.Call, mouseCurrentProperty);
		iLGenerator.Emit(OpCodes.Callvirt, AccessTools.PropertyGetter(mouseType, "enabled"));
		iLGenerator.Emit(OpCodes.Ret);
		_getMousePresent = dynamicMethodDefinition.Generate().CreateDelegate<GetMouseStateDelegate>();
		MethodInfo anyKeyProperty = AccessTools.PropertyGetter(type2, "anyKey");
		_getKeyboardAnyKeyIsPressed = GetAnyKeyDelegate("InputSystemGetKeyboardAnyKeyIsPressed", "isPressed");
		_getKeyboardAnyKeyWasPressedThisFrame = GetAnyKeyDelegate("InputSystemGetKeyboardAnyKeyWasPressedThisFrame", "wasPressedThisFrame");
		initialized = true;
		GetButtonDelegate GenerateKeyGetter(string name, string property)
		{
			MethodInfo meth = AccessTools.PropertyGetter(buttonControlType, property);
			DynamicMethodDefinition dynamicMethodDefinition2 = new DynamicMethodDefinition(name, typeof(bool), new Type[1] { typeof(KeyCode) });
			ILGenerator iLGenerator2 = dynamicMethodDefinition2.GetILGenerator();
			LocalBuilder local = iLGenerator2.DeclareLocal(typeof(int));
			Label label = iLGenerator2.DefineLabel();
			iLGenerator2.Emit(OpCodes.Ldsfld, keyCodeToIndexField);
			iLGenerator2.Emit(OpCodes.Ldarg_0);
			iLGenerator2.Emit(OpCodes.Ldloca, local);
			iLGenerator2.Emit(OpCodes.Callvirt, tryGetValueMethod);
			iLGenerator2.Emit(OpCodes.Brfalse, label);
			iLGenerator2.Emit(OpCodes.Call, currentProperty);
			iLGenerator2.Emit(OpCodes.Ldloc, local);
			iLGenerator2.Emit(OpCodes.Callvirt, indexer);
			iLGenerator2.Emit(OpCodes.Callvirt, meth);
			iLGenerator2.Emit(OpCodes.Ret);
			iLGenerator2.MarkLabel(label);
			iLGenerator2.Emit(OpCodes.Ldc_I4_0);
			iLGenerator2.Emit(OpCodes.Ret);
			return dynamicMethodDefinition2.Generate().CreateDelegate<GetButtonDelegate>();
		}
		GetButtonDelegate GenerateMouseGetter(string name, string property)
		{
			MethodInfo meth2 = AccessTools.PropertyGetter(buttonControlType, property);
			DynamicMethodDefinition dynamicMethodDefinition3 = new DynamicMethodDefinition(name, typeof(bool), new Type[1] { typeof(KeyCode) });
			ILGenerator il = dynamicMethodDefinition3.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldc_I4, 323);
			il.Emit(OpCodes.Sub);
			Label[] array = buttonPropertyGetters.Select((MethodInfo _) => il.DefineLabel()).ToArray();
			Label label2 = il.DefineLabel();
			il.Emit(OpCodes.Switch, array);
			il.Emit(OpCodes.Br, label2);
			for (int i = 0; i < array.Length; i++)
			{
				il.MarkLabel(array[i]);
				il.Emit(OpCodes.Call, mouseCurrentProperty);
				il.Emit(OpCodes.Callvirt, buttonPropertyGetters[i]);
				il.Emit(OpCodes.Callvirt, meth2);
				il.Emit(OpCodes.Ret);
			}
			il.MarkLabel(label2);
			il.Emit(OpCodes.Ldc_I4_0);
			il.Emit(OpCodes.Ret);
			return dynamicMethodDefinition3.Generate().CreateDelegate<GetButtonDelegate>();
		}
		GetMouseStateDelegate GetAnyKeyDelegate(string name, string property)
		{
			MethodInfo meth4 = AccessTools.PropertyGetter(buttonControlType, property);
			DynamicMethodDefinition dynamicMethodDefinition5 = new DynamicMethodDefinition(name, typeof(bool), Type.EmptyTypes);
			ILGenerator iLGenerator4 = dynamicMethodDefinition5.GetILGenerator();
			iLGenerator4.Emit(OpCodes.Call, currentProperty);
			iLGenerator4.Emit(OpCodes.Callvirt, anyKeyProperty);
			iLGenerator4.Emit(OpCodes.Callvirt, meth4);
			iLGenerator4.Emit(OpCodes.Ret);
			return dynamicMethodDefinition5.Generate().CreateDelegate<GetMouseStateDelegate>();
		}
		GetMouseVectorDelegate GetPositionDelegate(string name, string property)
		{
			MethodInfo methodInfo = AccessTools.PropertyGetter(mouseType, property);
			MethodInfo meth3 = AccessTools.Method(methodInfo.ReturnType, "ReadValue");
			DynamicMethodDefinition dynamicMethodDefinition4 = new DynamicMethodDefinition(name, typeof(Vector2), Type.EmptyTypes);
			ILGenerator iLGenerator3 = dynamicMethodDefinition4.GetILGenerator();
			iLGenerator3.Emit(OpCodes.Call, mouseCurrentProperty);
			iLGenerator3.Emit(OpCodes.Callvirt, methodInfo);
			iLGenerator3.Emit(OpCodes.Callvirt, meth3);
			iLGenerator3.Emit(OpCodes.Ret);
			return dynamicMethodDefinition4.Generate().CreateDelegate<GetMouseVectorDelegate>();
		}
	}

	public bool GetKey(string name)
	{

		return _getKey(GetKeyCode(name));
	}

	public bool GetKey(KeyCode key)
	{

		return _getKey(key);
	}

	public bool GetKeyDown(string name)
	{

		return _getKeyDown(GetKeyCode(name));
	}

	public bool GetKeyDown(KeyCode key)
	{

		return _getKeyDown(key);
	}

	public bool GetKeyUp(string name)
	{

		return _getKeyUp(GetKeyCode(name));
	}

	public bool GetKeyUp(KeyCode key)
	{

		return _getKeyUp(key);
	}

	public bool GetMouseButton(int button)
	{
		return _getMouseButton((KeyCode)(323 + button));
	}

	public bool GetMouseButtonDown(int button)
	{
		return _getMouseButtonDown((KeyCode)(323 + button));
	}

	public bool GetMouseButtonUp(int button)
	{
		return _getMouseButtonUp((KeyCode)(323 + button));
	}

	public void ResetInputAxes()
	{
	}

	private static KeyCode GetKeyCode(string name)
	{

		return (KeyCode)Enum.Parse(typeof(KeyCode), name, ignoreCase: true);
	}

	private static bool TryEnumParse<T>(string name, out T val)
	{
		try
		{
			val = (T)Enum.Parse(typeof(T), name, ignoreCase: true);
			return true;
		}
		catch (Exception)
		{
			val = default(T);
			return false;
		}
	}
}
