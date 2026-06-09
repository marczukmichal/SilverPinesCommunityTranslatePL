using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Misc/Button Prompt Icon Settings")]
public class ButtonPromptIconSettings : ScriptableObject
{
	[Serializable]
	public struct ButtonPrompt
	{
		[FormerlySerializedAs("m_buttonString")]
		public string m_inputDisplayString;

		public InputActionReference m_inputActionReference;

		public string m_controlPathString;

		public Sprite m_buttonSprite;
	}

	[SerializeField]
	private TMP_SpriteAsset m_tmpSpriteAsset;

	[SerializeField]
	private ButtonPrompt[] m_buttonPrompts;

	private Dictionary<string, Sprite> m_buttonInputDisplayDict;

	private Dictionary<string, Sprite> m_buttonControlPathDict;

	private Dictionary<InputAction, Sprite> m_buttonActionDict;

	public TMP_SpriteAsset SpriteAsset => m_tmpSpriteAsset;

	public bool HasSpriteAsset => m_tmpSpriteAsset != null;

	private Dictionary<string, Sprite> ButtonInputDisplayDict
	{
		get
		{
			if (m_buttonInputDisplayDict == null)
			{
				BuildDictionary();
			}
			return m_buttonInputDisplayDict;
		}
	}

	private Dictionary<string, Sprite> ButtonControlPathDict
	{
		get
		{
			if (m_buttonControlPathDict == null)
			{
				BuildDictionary();
			}
			return m_buttonControlPathDict;
		}
	}

	private Dictionary<InputAction, Sprite> ButtonActionDict
	{
		get
		{
			if (m_buttonActionDict == null)
			{
				BuildDictionary();
			}
			return m_buttonActionDict;
		}
	}

	private void BuildDictionary()
	{
		m_buttonInputDisplayDict = new Dictionary<string, Sprite>();
		m_buttonControlPathDict = new Dictionary<string, Sprite>();
		m_buttonActionDict = new Dictionary<InputAction, Sprite>();
		ButtonPrompt[] buttonPrompts = m_buttonPrompts;
		for (int i = 0; i < buttonPrompts.Length; i++)
		{
			ButtonPrompt buttonPrompt = buttonPrompts[i];
			if (!string.IsNullOrEmpty(buttonPrompt.m_inputDisplayString))
			{
				m_buttonInputDisplayDict.Add(buttonPrompt.m_inputDisplayString, buttonPrompt.m_buttonSprite);
			}
			if (!string.IsNullOrEmpty(buttonPrompt.m_controlPathString))
			{
				m_buttonControlPathDict.Add(buttonPrompt.m_controlPathString, buttonPrompt.m_buttonSprite);
			}
			if (buttonPrompt.m_inputActionReference != null)
			{
				InputAction inputAction = GameInputManager.GameInputActions.FindAction(buttonPrompt.m_inputActionReference.name);
				if (inputAction != null)
				{
					m_buttonActionDict.Add(inputAction, buttonPrompt.m_buttonSprite);
				}
			}
		}
	}

	public Sprite GetButtonPrompt(InputAction inputAction, string controlPath, string buttonString)
	{
		if (ButtonActionDict.ContainsKey(inputAction))
		{
			return ButtonActionDict[inputAction];
		}
		if (!string.IsNullOrEmpty(controlPath) && ButtonControlPathDict.ContainsKey(controlPath))
		{
			return ButtonControlPathDict[controlPath];
		}
		if (!string.IsNullOrEmpty(buttonString) && ButtonInputDisplayDict.ContainsKey(buttonString))
		{
			return ButtonInputDisplayDict[buttonString];
		}
		return null;
	}
}
