using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.UI;

public class ProfileSelectButton : MonoBehaviour
{
	public enum Mode
	{
		Idle,
		ShowingButtons,
		DeleteProfileConfirmation
	}

	[SerializeField]
	private TextMeshProUGUI m_profileNameText;

	[SerializeField]
	private TextMeshProUGUI m_saveDateText;

	[SerializeField]
	private TextMeshProUGUI m_playtimeText;

	[SerializeField]
	private TextMeshProUGUI m_locationText;

	[SerializeField]
	private TextMeshProUGUI m_difficultyText;

	[SerializeField]
	private Image m_image;

	[SerializeField]
	private GameObject m_buttonsPanel;

	[SerializeField]
	private GameObject m_deletePrompt;

	[SerializeField]
	private Button m_switchButton;

	[SerializeField]
	private Button m_deleteButton;

	[SerializeField]
	private GameObject m_loadingOverlay;

	[Header("Localization")]
	[SerializeField]
	private LocalizedString m_activeString;

	[SerializeField]
	private LocalizedString m_inactiveString;

	[SerializeField]
	private LocalizedString m_profileIDString;

	[Header("Active")]
	[SerializeField]
	private TextMeshProUGUI m_activeStatusText;

	[SerializeField]
	private Color m_activeTextColor;

	[SerializeField]
	private Color m_inactiveTextColor;

	private int m_index;

	public UnityAction<ProfileSelectButton> OnProfileSelected;

	public UnityAction<ProfileSelectButton> OnSwitchButtonPressed;

	private bool m_isLoading;

	private Mode m_mode;

	public int ProfileIndex => m_index;

	public bool IsLoading => m_isLoading;

	public Mode CurrentMode
	{
		get
		{
			return m_mode;
		}
		set
		{
			if (m_mode == value)
			{
				return;
			}
			switch (m_mode)
			{
			case Mode.ShowingButtons:
				m_buttonsPanel.SetActive(value: false);
				break;
			case Mode.DeleteProfileConfirmation:
				m_deletePrompt.SetActive(value: false);
				break;
			}
			m_mode = value;
			switch (m_mode)
			{
			case Mode.ShowingButtons:
				m_buttonsPanel.SetActive(value: true);
				if (m_switchButton.gameObject.activeInHierarchy)
				{
					EventSystem.current.SetSelectedGameObject(m_switchButton.gameObject);
				}
				else if (m_deleteButton.gameObject.activeInHierarchy)
				{
					EventSystem.current.SetSelectedGameObject(m_deleteButton.gameObject);
				}
				else
				{
					EventSystem.current.SetSelectedGameObject(base.gameObject);
				}
				break;
			case Mode.DeleteProfileConfirmation:
				m_deletePrompt.SetActive(value: true);
				EventSystem.current.SetSelectedGameObject(base.gameObject);
				break;
			case Mode.Idle:
				break;
			}
		}
	}

	private void Start()
	{
		m_buttonsPanel.SetActive(value: false);
		m_switchButton.onClick.AddListener(SwitchButtonPressed);
		m_deleteButton.onClick.AddListener(DeleteButtonPressed);
	}

	private void SwitchButtonPressed()
	{
		if (!m_isLoading)
		{
			OnSwitchButtonPressed?.Invoke(this);
		}
	}

	private void DeleteButtonPressed()
	{
		if (!m_isLoading)
		{
			CurrentMode = Mode.DeleteProfileConfirmation;
		}
	}

	public void SetProfileData(int index, bool isActiveProfile)
	{
		m_index = index;
		LoadData(m_index);
		m_profileNameText.text = m_profileIDString.GetLocalizedString(index + 1);
		m_activeStatusText.text = (isActiveProfile ? m_activeString.GetLocalizedString() : m_inactiveString.GetLocalizedString());
		m_activeStatusText.color = (isActiveProfile ? m_activeTextColor : m_inactiveTextColor);
		m_switchButton.gameObject.SetActive(!isActiveProfile);
	}

	public void OnClicked()
	{
		if (!m_isLoading)
		{
			OnProfileSelected?.Invoke(this);
		}
	}

	public void SetDimmed(bool dimmed)
	{
		m_image.color = (dimmed ? Color.gray : Color.white);
	}

	public void SetSelected(bool selected)
	{
		CurrentMode = (selected ? Mode.ShowingButtons : Mode.Idle);
	}

	private void LoadData(int profileIndex)
	{
		m_isLoading = true;
		m_loadingOverlay.SetActive(value: true);
		ClearData();
		SaveDataManager.Instance.RequestMainSaveDataForProfile(profileIndex, SaveDataCallback);
	}

	private void SaveDataCallback(DataLoadedCallbackEvent<PersistentData> callback)
	{
		bool flag = false;
		if (callback.m_resultCode == ResultCode.Success)
		{
			PersistentData loadedObject = callback.m_loadedObject;
			if (loadedObject != null)
			{
				m_saveDateText.text = new DateTime(loadedObject.TimeStamp, DateTimeKind.Utc).ToLocalTime().ToString();
				m_playtimeText.text = new TimeSpan(0, 0, 0, (int)loadedObject.PlayTime).ToString();
				m_locationText.text = loadedObject.ActiveLevelMetadata.DisplayName;
				m_difficultyText.text = GlobalReferences.Instance.GameDifficultySettings.GetDifficultyModeConfiguration(loadedObject.CurrentDifficulty).DifficultyNameString.GetLocalizedString();
				m_deleteButton.gameObject.SetActive(value: true);
				flag = true;
			}
		}
		if (!flag)
		{
			ClearData();
		}
		m_isLoading = false;
		m_loadingOverlay.SetActive(value: false);
	}

	private void ClearData()
	{
		m_saveDateText.text = "-";
		m_playtimeText.text = "-";
		m_locationText.text = "-";
		m_difficultyText.text = "-";
		m_deleteButton.gameObject.SetActive(value: false);
	}
}
