using UnityEngine;
using UnityEngine.EventSystems;

public class OptionsMenuPage : MenuPage
{
	[SerializeField]
	private OptionsMenuTab[] m_tabs;

	[SerializeField]
	private OptionsMenuTabButton[] m_tabButtons;

	[SerializeField]
	private CanvasGroup m_tabsCanvasGroup;

	[SerializeField]
	private RetunToMainMenuDialog m_retunToMainMenuDialog;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_onChangeTabAudioEvent;

	[SerializeField]
	private bool m_pauseAudio;

	[Header("UI Elements")]
	[SerializeField]
	private GameObject m_returnToMenuButton;

	private OptionsMenuTab m_activeTab;

	private void Start()
	{
		SetOpenTab(null);
		UpdateTabButtons();
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.UserPreferences.SaveUserPreferences.Raise();
		GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.OptionsPage);
		if (m_pauseAudio)
		{
			GlobalReferences.Instance.EventChannels.Audio.PauseGameAudioBusses.Raise(value: false);
		}
	}

	public void SetOpenTab(OptionsMenuTab tab)
	{
		if (m_activeTab == tab)
		{
			return;
		}
		OptionsMenuTabButton optionsMenuTabButton = null;
		if (m_activeTab != null)
		{
			OptionsMenuTabButton[] tabButtons = m_tabButtons;
			foreach (OptionsMenuTabButton optionsMenuTabButton2 in tabButtons)
			{
				if (optionsMenuTabButton2.TabGameObject == m_activeTab.gameObject)
				{
					optionsMenuTabButton = optionsMenuTabButton2;
					break;
				}
			}
			m_activeTab.gameObject.SetActive(value: false);
		}
		m_activeTab = tab;
		if (m_activeTab != null)
		{
			m_activeTab.gameObject.SetActive(value: true);
			SelectEventSystemGameObjectForTab(m_activeTab);
		}
		else if (optionsMenuTabButton != null)
		{
			EventSystem.current.SetSelectedGameObject(optionsMenuTabButton.gameObject);
		}
		else
		{
			EventSystem.current.SetSelectedGameObject(m_tabButtons[0].gameObject);
		}
		UpdateTabButtons();
		if (m_onChangeTabAudioEvent != null)
		{
			m_onChangeTabAudioEvent.Play2D();
		}
	}

	private void UpdateTabButtons()
	{
		GameObject gameObject = ((m_activeTab != null) ? m_activeTab.gameObject : null);
		OptionsMenuTabButton[] tabButtons = m_tabButtons;
		foreach (OptionsMenuTabButton optionsMenuTabButton in tabButtons)
		{
			optionsMenuTabButton.SetTabActive(gameObject == optionsMenuTabButton.TabGameObject);
		}
		bool flag = m_activeTab != null && GlobalReferences.Instance.InputState.InputMode == InputState.Mode.Gamepad;
		m_tabsCanvasGroup.interactable = !flag;
	}

	public override void Show(bool instant = false, bool onAwake = false)
	{
		base.Show(instant, onAwake);
		SetOpenTab(null);
		UpdateTabButtons();
		EventSystem.current.SetSelectedGameObject(m_tabButtons[0].gameObject);
		GlobalReferences.Instance.GameMenuState.SetInMenu(GameMenuState.GameMenu.OptionsPage);
		if (m_pauseAudio)
		{
			GlobalReferences.Instance.EventChannels.Audio.PauseGameAudioBusses.Raise(value: true);
		}
	}

	public override void Hide(bool instant = false)
	{
		base.Hide(instant);
		SetOpenTab(null);
		GlobalReferences.Instance.EventChannels.UserPreferences.SaveUserPreferences.Raise();
		GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.OptionsPage);
		if (m_pauseAudio)
		{
			GlobalReferences.Instance.EventChannels.Audio.PauseGameAudioBusses.Raise(value: false);
		}
	}

	private void SelectEventSystemGameObjectForTab(OptionsMenuTab tab)
	{
		if (!(tab == null) && !(tab.SelectGameObject == null) && base.IsShowing && EventSystem.current != null)
		{
			EventSystem.current.SetSelectedGameObject(tab.SelectGameObject);
		}
	}

	public override bool CanExitPage()
	{
		if (m_activeTab != null)
		{
			return m_activeTab.CanExitPage();
		}
		if (m_retunToMainMenuDialog != null && m_retunToMainMenuDialog.IsExiting)
		{
			return false;
		}
		return base.CanExitPage();
	}

	public override bool CanChangeTabs()
	{
		if (m_retunToMainMenuDialog != null && m_retunToMainMenuDialog.IsShowing())
		{
			return false;
		}
		return base.CanChangeTabs();
	}

	public override bool OnBackInput()
	{
		if (m_activeTab != null)
		{
			SetOpenTab(null);
			return true;
		}
		if (m_retunToMainMenuDialog != null && m_retunToMainMenuDialog.IsExiting)
		{
			return true;
		}
		if (m_retunToMainMenuDialog != null && m_retunToMainMenuDialog.IsShowing())
		{
			m_retunToMainMenuDialog.Close();
			EventSystem.current.SetSelectedGameObject(m_returnToMenuButton);
			return true;
		}
		return base.OnBackInput();
	}

	public void ReturnToMainMenu()
	{
		SetOpenTab(null);
		EventSystem.current.SetSelectedGameObject(null);
		m_retunToMainMenuDialog.Show();
	}
}
