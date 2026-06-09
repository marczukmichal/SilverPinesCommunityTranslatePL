using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

public class SaveSelectPanel : MonoBehaviour
{
	public enum SaveSelectionMode
	{
		Save,
		Load
	}

	[SerializeField]
	private SaveSelectButton m_saveSelectButtonPrefab;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private Transform m_parentTransform;

	[SerializeField]
	private GameObject m_confirmationDialogue;

	[SerializeField]
	private UnityEvent<int> m_onSaveSelectedEvent;

	[Header("Text")]
	[SerializeField]
	private TextMeshProUGUI m_promptText;

	[SerializeField]
	private LocalizedString m_saveConfirmCheckString;

	[SerializeField]
	private LocalizedString m_saveSuccessfulString;

	[SerializeField]
	private GameObject m_buttonPrompts;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_saveSelectedAudio;

	private SaveSelectButton[] m_saveSelectButtons;

	private SaveSelectButton m_selectedButton;

	[SerializeField]
	private SaveSelectionMode m_selectionMode;

	private bool m_inConfirmationState;

	protected void Awake()
	{
		GenerateButtons();
	}

	public void Show()
	{
		m_selectedButton = null;
		PopulateButtons();
		SelectedSaveChanged();
		GameInputManager.GameInputActions.UI.Submit.performed += ConfirmAction;
		GameInputManager.GameInputActions.UI.Click.performed += ConfirmAction;
		GlobalReferences.Instance.EventChannels.Generic.SetGamepadCursorAllowed.Raise(value: false);
		EventSystem.current.SetSelectedGameObject(m_saveSelectButtons[0].gameObject);
		m_inConfirmationState = false;
		m_promptText.text = m_saveConfirmCheckString.GetLocalizedString();
		m_buttonPrompts.gameObject.SetActive(value: true);
	}

	private void GenerateButtons()
	{
		if (m_saveSelectButtons == null)
		{
			m_saveSelectButtons = new SaveSelectButton[GameUtils.Constants.s_saveGameSlots];
			for (int i = 0; i < GameUtils.Constants.s_saveGameSlots; i++)
			{
				m_saveSelectButtons[i] = UnityEngine.Object.Instantiate(m_saveSelectButtonPrefab, m_parentTransform);
				SaveSelectButton obj = m_saveSelectButtons[i];
				obj.OnSaveSelected = (UnityAction<SaveSelectButton>)Delegate.Combine(obj.OnSaveSelected, new UnityAction<SaveSelectButton>(OnSaveSelected));
			}
		}
	}

	public void Hide()
	{
		GameInputManager.GameInputActions.UI.Submit.performed -= ConfirmAction;
		GameInputManager.GameInputActions.UI.Click.performed -= ConfirmAction;
		GlobalReferences.Instance.EventChannels.Generic.SetGamepadCursorAllowed.Raise(value: true);
	}

	private void OnDisable()
	{
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.UI.Submit.performed -= ConfirmAction;
			GameInputManager.GameInputActions.UI.Click.performed -= ConfirmAction;
			GlobalReferences.Instance.EventChannels.Generic.SetGamepadCursorAllowed.Raise(value: true);
		}
	}

	private void ConfirmAction(InputAction.CallbackContext obj)
	{
		ConfirmSelectedSave();
	}

	private void ConfirmSelectedSave()
	{
		if (!(m_selectedButton == null))
		{
			if (m_saveSelectedAudio != null)
			{
				m_saveSelectedAudio.Play2D();
			}
			m_confirmationDialogue.SetActive(value: false);
			Hide();
			m_onSaveSelectedEvent.Invoke(m_selectedButton.SaveIndex);
		}
	}

	public bool IsConfirmationDialogueActive()
	{
		return m_confirmationDialogue.gameObject.activeInHierarchy;
	}

	public void CancelAction()
	{
		if (!(m_selectedButton == null))
		{
			DeselectSave();
		}
	}

	private void DeselectSave()
	{
		m_selectedButton = null;
		SelectedSaveChanged();
	}

	private void OnSaveSelected(SaveSelectButton button)
	{
		if (!m_inConfirmationState && (button.HasSave || m_selectionMode != SaveSelectionMode.Load))
		{
			m_selectedButton = button;
			if (m_selectionMode == SaveSelectionMode.Save && !m_selectedButton.HasSave)
			{
				ConfirmSelectedSave();
			}
			else
			{
				SelectedSaveChanged();
			}
		}
	}

	private void SelectedSaveChanged()
	{
		if (m_selectedButton != null)
		{
			SaveSelectButton[] saveSelectButtons = m_saveSelectButtons;
			foreach (SaveSelectButton obj in saveSelectButtons)
			{
				obj.SetSelected(obj == m_selectedButton);
			}
			m_canvasGroup.interactable = false;
			m_promptText.text = m_saveConfirmCheckString.GetLocalizedString();
			m_confirmationDialogue.SetActive(value: true);
		}
		else
		{
			SaveSelectButton[] saveSelectButtons = m_saveSelectButtons;
			for (int i = 0; i < saveSelectButtons.Length; i++)
			{
				saveSelectButtons[i].SetSelected(selected: false);
			}
			m_canvasGroup.interactable = true;
			m_confirmationDialogue.SetActive(value: false);
		}
	}

	private void PopulateButtons()
	{
		for (int i = 0; i < GameUtils.Constants.s_saveGameSlots; i++)
		{
			m_saveSelectButtons[i].SetSaveData(i, m_selectionMode == SaveSelectionMode.Save);
		}
	}

	public void RefreshButton(int index)
	{
		m_saveSelectButtons[index].SetSaveData(index, m_selectionMode == SaveSelectionMode.Save);
	}

	public void SetSaveDoneMode()
	{
		m_inConfirmationState = true;
		m_confirmationDialogue.SetActive(value: false);
		m_buttonPrompts.gameObject.SetActive(value: false);
	}
}
