using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KeypadMinigame : MonoBehaviour, IMinigameComponent
{
	public enum Mode
	{
		PasswordEntry,
		Child
	}

	[SerializeField]
	private TextMeshProUGUI m_codeDisplay;

	[SerializeField]
	private Mode m_mode;

	[SerializeField]
	private string m_targetCode;

	[SerializeField]
	private int m_inputCodeLength = 2;

	[SerializeField]
	private UnityEvent m_onKeypadCorrect;

	[SerializeField]
	private float m_animateTime = 0.1f;

	[Header("UI")]
	[SerializeField]
	private Image m_lockedLight;

	[SerializeField]
	private Image m_lockedLightGlow;

	[SerializeField]
	private Image m_unlockedLight;

	[SerializeField]
	private Image m_unlockedLightGlow;

	[SerializeField]
	private Color m_lockedLightOnColor;

	[SerializeField]
	private Color m_lockedLightOffColor;

	[SerializeField]
	private Color m_unlockedLightOnColor;

	[SerializeField]
	private Color m_unlockedLightOffColor;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_onCorrectAudio;

	[SerializeField]
	private AudioEvent m_keypadInputAudioEvent;

	[SerializeField]
	private AudioEvent m_keypadResetAudioEvent;

	[SerializeField]
	private AudioEvent m_keypadDeleteAudioEvent;

	[Header("Input")]
	[SerializeField]
	private bool m_supportKeyboardInput = true;

	public UnityAction<string> m_codeUpdated;

	private bool m_blockInput;

	private string m_currentCode = "";

	private int CodeLength => Mathf.Max(m_targetCode.Length, m_inputCodeLength);

	public string CurrentCode
	{
		get
		{
			return m_currentCode;
		}
		set
		{
			if (m_currentCode != value)
			{
				m_currentCode = value;
				m_codeUpdated?.Invoke(value);
				UpdateDisplay();
			}
		}
	}

	private void Start()
	{
		SetLockedLight(on: true);
		SetUnlockedLight(on: false);
	}

	private void CheckForCorrectness()
	{
		if (CurrentCode.Equals(m_targetCode))
		{
			StartCoroutine(SuccessCoroutine());
		}
		else
		{
			StartCoroutine(FailCoroutine());
		}
	}

	public void AddInput(string input)
	{
		if (!m_blockInput && CurrentCode.Length < CodeLength)
		{
			CurrentCode += input;
			m_keypadInputAudioEvent.Play2D();
			if (m_mode == Mode.PasswordEntry && CurrentCode.Length == CodeLength)
			{
				CheckForCorrectness();
			}
		}
	}

	public void Delete()
	{
		if (!m_blockInput && CurrentCode.Length > 0)
		{
			m_keypadDeleteAudioEvent.Play2D();
			string currentCode = CurrentCode;
			CurrentCode = currentCode.Substring(0, currentCode.Length - 1);
		}
	}

	public void Clear()
	{
		if (!m_blockInput)
		{
			m_keypadResetAudioEvent.Play2D();
			CurrentCode = "";
		}
	}

	private void UpdateDisplay()
	{
		if (m_codeDisplay != null)
		{
			m_codeDisplay.text = m_currentCode;
		}
	}

	private void SetLockedLight(bool on)
	{
		if (m_lockedLight != null)
		{
			m_lockedLight.color = (on ? m_lockedLightOnColor : m_lockedLightOffColor);
		}
		if (m_lockedLightGlow != null)
		{
			m_lockedLightGlow.gameObject.SetActive(on);
		}
	}

	private void SetUnlockedLight(bool on)
	{
		if (m_unlockedLight != null)
		{
			m_unlockedLight.color = (on ? m_unlockedLightOnColor : m_unlockedLightOffColor);
		}
		if (m_unlockedLightGlow != null)
		{
			m_unlockedLightGlow.gameObject.SetActive(on);
		}
	}

	private IEnumerator FailCoroutine()
	{
		m_blockInput = true;
		SetUnlockedLight(on: false);
		SetLockedLight(on: false);
		for (int i = 0; i < 3; i++)
		{
			yield return new WaitForSecondsRealtime(m_animateTime);
			SetLockedLight(on: true);
			yield return new WaitForSecondsRealtime(m_animateTime);
			SetLockedLight(on: false);
		}
		yield return new WaitForSecondsRealtime(m_animateTime);
		SetLockedLight(on: true);
		m_blockInput = false;
		Clear();
	}

	private IEnumerator SuccessCoroutine()
	{
		m_blockInput = true;
		SetUnlockedLight(on: false);
		SetLockedLight(on: false);
		m_onCorrectAudio.Play2D();
		for (int i = 0; i < 3; i++)
		{
			yield return new WaitForSecondsRealtime(m_animateTime);
			SetUnlockedLight(on: true);
			yield return new WaitForSecondsRealtime(m_animateTime);
			SetUnlockedLight(on: false);
		}
		SetUnlockedLight(on: true);
		m_onKeypadCorrect?.Invoke();
	}

	private void Update()
	{
		if (!m_supportKeyboardInput)
		{
			return;
		}
		string text = GameUIUtility.CheckForNumberInput();
		if (!string.IsNullOrEmpty(text))
		{
			AddInput(text);
		}
		if (Keyboard.current != null)
		{
			Keyboard current = Keyboard.current;
			if (current.deleteKey.wasPressedThisFrame || current.backspaceKey.wasReleasedThisFrame || current.numpadPeriodKey.wasPressedThisFrame)
			{
				Delete();
			}
		}
	}

	public void Setup(GameObject parent)
	{
		KeypadMinigameData component = parent.GetComponent<KeypadMinigameData>();
		if (component != null)
		{
			m_targetCode = component.Code;
			m_inputCodeLength = m_targetCode.Length;
		}
	}
}
