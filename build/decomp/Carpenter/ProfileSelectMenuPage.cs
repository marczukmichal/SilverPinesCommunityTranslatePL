using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ProfileSelectMenuPage : MenuPage
{
	[SerializeField]
	private ProfileSelectButton[] m_profileButtons;

	[SerializeField]
	private AudioEvent m_switchProfileAudio;

	[SerializeField]
	private AudioEvent m_deleteProfileAudio;

	private int m_selectedProfileIndex = -1;

	public override bool CanExitPage()
	{
		if (m_selectedProfileIndex != -1)
		{
			return false;
		}
		return base.CanExitPage();
	}

	protected override void Awake()
	{
		base.Awake();
		ProfileSelectButton[] profileButtons = m_profileButtons;
		foreach (ProfileSelectButton obj in profileButtons)
		{
			obj.OnProfileSelected = (UnityAction<ProfileSelectButton>)Delegate.Combine(obj.OnProfileSelected, new UnityAction<ProfileSelectButton>(OnProfileButtonClicked));
			obj.OnSwitchButtonPressed = (UnityAction<ProfileSelectButton>)Delegate.Combine(obj.OnSwitchButtonPressed, new UnityAction<ProfileSelectButton>(SwitchToProfile));
		}
		SelectedProfileChanged();
	}

	private void OnProfileButtonClicked(ProfileSelectButton selectedButton)
	{
		m_selectedProfileIndex = selectedButton.ProfileIndex;
		SelectedProfileChanged();
	}

	private void SelectedProfileChanged()
	{
		if (m_selectedProfileIndex != -1)
		{
			for (int i = 0; i < m_profileButtons.Length; i++)
			{
				ProfileSelectButton obj = m_profileButtons[i];
				obj.SetDimmed(m_selectedProfileIndex != i);
				obj.SetSelected(m_selectedProfileIndex == i);
			}
		}
		else
		{
			for (int j = 0; j < m_profileButtons.Length; j++)
			{
				ProfileSelectButton obj2 = m_profileButtons[j];
				obj2.SetDimmed(dimmed: false);
				obj2.SetSelected(selected: false);
			}
		}
	}

	private void PopulateProfileButtons()
	{
		int num = 0;
		ProfileSelectButton[] profileButtons = m_profileButtons;
		for (int i = 0; i < profileButtons.Length; i++)
		{
			profileButtons[i].SetProfileData(num, num == SaveDataManager.Instance.SharedData.SaveProfileIndex);
			num++;
		}
	}

	public override void Show(bool instant = false, bool onAwake = false)
	{
		base.Show(instant, onAwake);
		PopulateProfileButtons();
		DeselectProfile();
		GameInputManager.GameInputActions.UI.Submit.performed += ConfirmAction;
		GameInputManager.GameInputActions.UI.Click.performed += ConfirmAction;
		GameInputManager.GameInputActions.UI.Cancel.performed += CancelAction;
	}

	public override void Hide(bool instant = false)
	{
		base.Hide(instant);
		GameInputManager.GameInputActions.UI.Submit.performed -= ConfirmAction;
		GameInputManager.GameInputActions.UI.Click.performed -= ConfirmAction;
		GameInputManager.GameInputActions.UI.Cancel.performed -= CancelAction;
	}

	private void OnDisable()
	{
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.UI.Submit.performed -= ConfirmAction;
			GameInputManager.GameInputActions.UI.Click.performed -= ConfirmAction;
			GameInputManager.GameInputActions.UI.Cancel.performed -= CancelAction;
		}
	}

	private void ConfirmAction(InputAction.CallbackContext obj)
	{
		if (m_selectedProfileIndex != -1 && m_profileButtons[m_selectedProfileIndex].CurrentMode == ProfileSelectButton.Mode.DeleteProfileConfirmation)
		{
			if (m_deleteProfileAudio != null)
			{
				m_deleteProfileAudio.Play2D();
			}
			GlobalReferences.Instance.EventChannels.SaveLoad.DeleteProfile.Raise(m_selectedProfileIndex);
			if (m_selectedProfileIndex == SaveDataManager.Instance.SharedData.SaveProfileIndex)
			{
				GlobalReferences.Instance.GameState.ClearStateFlag(GameState.GameStateFlag.GameActive);
				SceneManager.LoadScene("Assets/Scenes/Utility/Startup.unity");
			}
			else
			{
				PopulateProfileButtons();
				DeselectProfile();
			}
		}
	}

	private void CancelAction(InputAction.CallbackContext obj)
	{
		if (m_selectedProfileIndex != -1)
		{
			if (m_profileButtons[m_selectedProfileIndex].CurrentMode == ProfileSelectButton.Mode.DeleteProfileConfirmation)
			{
				m_profileButtons[m_selectedProfileIndex].CurrentMode = ProfileSelectButton.Mode.ShowingButtons;
			}
			else
			{
				DeselectProfile();
			}
		}
	}

	private void SwitchToProfile(ProfileSelectButton profileButton)
	{
		if (Array.IndexOf(m_profileButtons, profileButton) == m_selectedProfileIndex)
		{
			Debug.Log("Switch to profile: " + m_selectedProfileIndex);
			Hide();
			SaveDataManager instance = SaveDataManager.Instance;
			instance.SharedData.SaveProfileIndex = m_selectedProfileIndex;
			instance.SaveSharedData();
			GlobalReferences.Instance.GameState.ClearStateFlag(GameState.GameStateFlag.GameActive);
			SceneManager.LoadScene("Assets/Scenes/Utility/Startup.unity");
			if (m_switchProfileAudio != null)
			{
				m_switchProfileAudio.Play2D();
			}
		}
	}

	private void DeselectProfile()
	{
		if (m_selectedProfileIndex != -1)
		{
			EventSystem.current.SetSelectedGameObject(m_profileButtons[m_selectedProfileIndex].gameObject);
		}
		m_selectedProfileIndex = -1;
		SelectedProfileChanged();
	}

	private void Update()
	{
		if (m_selectedProfileIndex == -1)
		{
			return;
		}
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		if (currentSelectedGameObject != null)
		{
			if (!(currentSelectedGameObject != m_profileButtons[m_selectedProfileIndex].gameObject))
			{
				return;
			}
			if (currentSelectedGameObject != null)
			{
				ProfileSelectButton componentInParent = currentSelectedGameObject.GetComponentInParent<ProfileSelectButton>();
				if (componentInParent != null && componentInParent.gameObject == m_profileButtons[m_selectedProfileIndex].gameObject)
				{
					return;
				}
			}
			DeselectProfile();
		}
		else
		{
			EventSystem.current.SetSelectedGameObject(m_profileButtons[m_selectedProfileIndex].gameObject);
		}
	}
}
