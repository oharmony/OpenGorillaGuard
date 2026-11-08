using System.Collections.Generic;
using UnityEngine;

namespace BepInEx;

public interface IInputSystem
{

	Vector3 mousePosition { get; }

	Vector2 mouseScrollDelta { get; }

	bool mousePresent { get; }

	bool anyKey { get; }

	bool anyKeyDown { get; }

	IEnumerable<KeyCode> SupportedKeyCodes { get; }

	bool GetKey(string name);

	bool GetKey(KeyCode key);

	bool GetKeyDown(string name);

	bool GetKeyDown(KeyCode key);

	bool GetKeyUp(string name);

	bool GetKeyUp(KeyCode key);

	bool GetMouseButton(int button);

	bool GetMouseButtonDown(int button);

	bool GetMouseButtonUp(int button);

	void ResetInputAxes();
}
