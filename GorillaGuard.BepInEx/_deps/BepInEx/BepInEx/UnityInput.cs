using System;
using BepInEx.Logging;
using UnityEngine;

namespace BepInEx;

public static class UnityInput
{
	private static IInputSystem _current;

	public static IInputSystem Current
	{
		get
		{
			if (_current != null)
			{
				return _current;
			}
			try
			{
				try
				{
					Input.GetKeyDown((KeyCode)97);
					_current = new LegacyInputSystem();
					Logger.LogDebug("[UnityInput] Using LegacyInputSystem");
				}
				catch (InvalidOperationException)
				{
					_current = new NewInputSystem();
					Logger.LogDebug("[UnityInput] Using NewInputSystem");
				}
			}
			catch (Exception arg)
			{
				Logger.LogWarning($"[UnityInput] Failed to detect available input systems - {arg}");
				_current = new NullInputSystem();
			}
			return _current;
		}
	}

	public static bool LegacyInputSystemAvailable => Current is LegacyInputSystem;
}
