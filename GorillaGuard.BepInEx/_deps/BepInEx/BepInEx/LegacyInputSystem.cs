using System;
using System.Collections.Generic;
using UnityEngine;

namespace BepInEx;

internal class LegacyInputSystem : IInputSystem
{
	public Vector3 mousePosition => Input.mousePosition;

	public Vector2 mouseScrollDelta => Input.mouseScrollDelta;

	public bool mousePresent => Input.mousePresent;

	public bool anyKey => Input.anyKey;

	public bool anyKeyDown => Input.anyKeyDown;

	public IEnumerable<KeyCode> SupportedKeyCodes { get; } = (KeyCode[])Enum.GetValues(typeof(KeyCode));

	public bool GetKey(string name)
	{
		return Input.GetKey(name);
	}

	public bool GetKey(KeyCode key)
	{

		return Input.GetKey(key);
	}

	public bool GetKeyDown(string name)
	{
		return Input.GetKeyDown(name);
	}

	public bool GetKeyDown(KeyCode key)
	{

		return Input.GetKeyDown(key);
	}

	public bool GetKeyUp(string name)
	{
		return Input.GetKeyUp(name);
	}

	public bool GetKeyUp(KeyCode key)
	{

		return Input.GetKeyUp(key);
	}

	public bool GetMouseButton(int button)
	{
		return Input.GetMouseButton(button);
	}

	public bool GetMouseButtonDown(int button)
	{
		return Input.GetMouseButtonDown(button);
	}

	public bool GetMouseButtonUp(int button)
	{
		return Input.GetMouseButtonUp(button);
	}

	public void ResetInputAxes()
	{
		Input.ResetInputAxes();
	}
}
