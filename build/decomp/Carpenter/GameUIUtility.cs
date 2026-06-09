using UnityEngine.InputSystem;

public static class GameUIUtility
{
	public static string CheckForNumberInput()
	{
		if (Keyboard.current == null)
		{
			return "";
		}
		Keyboard current = Keyboard.current;
		if (current.digit0Key.wasPressedThisFrame || current.numpad0Key.wasPressedThisFrame)
		{
			return "0";
		}
		if (current.digit1Key.wasPressedThisFrame || current.numpad1Key.wasPressedThisFrame)
		{
			return "1";
		}
		if (current.digit2Key.wasPressedThisFrame || current.numpad2Key.wasPressedThisFrame)
		{
			return "2";
		}
		if (current.digit3Key.wasPressedThisFrame || current.numpad3Key.wasPressedThisFrame)
		{
			return "3";
		}
		if (current.digit4Key.wasPressedThisFrame || current.numpad4Key.wasPressedThisFrame)
		{
			return "4";
		}
		if (current.digit5Key.wasPressedThisFrame || current.numpad5Key.wasPressedThisFrame)
		{
			return "5";
		}
		if (current.digit6Key.wasPressedThisFrame || current.numpad6Key.wasPressedThisFrame)
		{
			return "6";
		}
		if (current.digit7Key.wasPressedThisFrame || current.numpad7Key.wasPressedThisFrame)
		{
			return "7";
		}
		if (current.digit8Key.wasPressedThisFrame || current.numpad8Key.wasPressedThisFrame)
		{
			return "8";
		}
		if (current.digit9Key.wasPressedThisFrame || current.numpad9Key.wasPressedThisFrame)
		{
			return "9";
		}
		return "";
	}

	public static int CheckForNumberInputInteger()
	{
		if (Keyboard.current == null)
		{
			return int.MaxValue;
		}
		Keyboard current = Keyboard.current;
		if (current.digit0Key.wasPressedThisFrame || current.numpad0Key.wasPressedThisFrame)
		{
			return 0;
		}
		if (current.digit1Key.wasPressedThisFrame || current.numpad1Key.wasPressedThisFrame)
		{
			return 1;
		}
		if (current.digit2Key.wasPressedThisFrame || current.numpad2Key.wasPressedThisFrame)
		{
			return 2;
		}
		if (current.digit3Key.wasPressedThisFrame || current.numpad3Key.wasPressedThisFrame)
		{
			return 3;
		}
		if (current.digit4Key.wasPressedThisFrame || current.numpad4Key.wasPressedThisFrame)
		{
			return 4;
		}
		if (current.digit5Key.wasPressedThisFrame || current.numpad5Key.wasPressedThisFrame)
		{
			return 5;
		}
		if (current.digit6Key.wasPressedThisFrame || current.numpad6Key.wasPressedThisFrame)
		{
			return 6;
		}
		if (current.digit7Key.wasPressedThisFrame || current.numpad7Key.wasPressedThisFrame)
		{
			return 7;
		}
		if (current.digit8Key.wasPressedThisFrame || current.numpad8Key.wasPressedThisFrame)
		{
			return 8;
		}
		if (current.digit9Key.wasPressedThisFrame || current.numpad9Key.wasPressedThisFrame)
		{
			return 9;
		}
		return int.MaxValue;
	}

	public static string WrapAroundSubstring(string input, int startIndex, int length)
	{
		if (string.IsNullOrEmpty(input) || length <= 0)
		{
			return string.Empty;
		}
		startIndex = (startIndex % input.Length + input.Length) % input.Length;
		string text = "";
		for (int i = 0; i < length; i++)
		{
			int index = (startIndex + i) % input.Length;
			text += input[index];
		}
		return text;
	}
}
