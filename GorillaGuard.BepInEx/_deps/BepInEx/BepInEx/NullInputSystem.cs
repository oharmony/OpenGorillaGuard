using System.Collections.Generic;
using UnityEngine;

namespace BepInEx;

internal class NullInputSystem : IInputSystem
{
	private static readonly KeyCode[] NoKeys = (KeyCode[])(object)new KeyCode[0];

	public Vector3 mousePosition => Vector3.zero;

	public Vector2 mouseScrollDelta => Vector2.zero;

	public bool mousePresent => false;

	public bool anyKey => false;

	public bool anyKeyDown => false;

	public IEnumerable<KeyCode> SupportedKeyCodes => NoKeys;

	public bool GetKey(string name)
	{
		return false;
	}

	public bool GetKey(KeyCode key)
	{
		return false;
	}

	public bool GetKeyDown(string name)
	{
		return false;
	}

	public bool GetKeyDown(KeyCode key)
	{
		return false;
	}

	public bool GetKeyUp(string name)
	{
		return false;
	}

	public bool GetKeyUp(KeyCode key)
	{
		return false;
	}

	public bool GetMouseButton(int button)
	{
		return false;
	}

	public bool GetMouseButtonDown(int button)
	{
		return false;
	}

	public bool GetMouseButtonUp(int button)
	{
		return false;
	}

	public void ResetInputAxes()
	{
	}
}
