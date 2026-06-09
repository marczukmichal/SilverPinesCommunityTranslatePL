using System;
using TMPro;
using Team17;
using Team17.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.UI;

public class SaveSelectButton : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_saveDateText;

	[SerializeField]
	private TextMeshProUGUI m_playtimeText;

	[SerializeField]
	private TextMeshProUGUI m_locationText;

	[SerializeField]
	private TextMeshProUGUI m_saveCountText;

	[SerializeField]
	private TextMeshProUGUI m_fileIDText;

	[SerializeField]
	private TextMeshProUGUI m_difficultyText;

	[SerializeField]
	private RawImage m_screenshotImage;

	[Header("Groups")]
	[SerializeField]
	private GameObject m_loadingOverlay;

	[SerializeField]
	private GameObject m_contentContainer;

	[Header("Strings")]
	[SerializeField]
	private LocalizedString m_fileNameString;

	[SerializeField]
	private LocalizedString m_noDataString;

	private int m_index;

	public UnityAction<SaveSelectButton> OnSaveSelected;

	private bool m_hasSave;

	private Texture2D m_thumbnailTexture;

	public int SaveIndex => m_index;

	public bool HasSave => m_hasSave;

	private void SetLoading(bool loading)
	{
		m_loadingOverlay.SetActive(loading);
		m_contentContainer.SetActive(!loading);
	}

	public void SetSaveData(int index, bool canSelectEmptySlots)
	{
		SetLoading(loading: true);
		m_fileIDText.text = m_fileNameString.GetLocalizedString(index + 1);
		m_index = index;
		m_hasSave = false;
		if (m_thumbnailTexture == null)
		{
			m_thumbnailTexture = new Texture2D(GameUtils.Constants.s_saveGameThumbnailWidth, GameUtils.Constants.s_saveGameThumbnailHeight, TextureFormat.RGB24, mipChain: false);
		}
		SaveDataManager.Instance.RequestSaveDataForActiveProfile(index, m_thumbnailTexture, LoadedCallback);
	}

	private void OnDestroy()
	{
		if (m_thumbnailTexture != null)
		{
			UnityEngine.Object.Destroy(m_thumbnailTexture);
		}
	}

	private void LoadedCallback(DataLoadedCallbackEvent<PersistentData> result)
	{
		switch (result.m_resultCode)
		{
		case ResultCode.Success:
		{
			PersistentData loadedObject = result.m_loadedObject;
			if (loadedObject != null)
			{
				m_hasSave = true;
				m_saveDateText.text = new DateTime(loadedObject.TimeStamp, DateTimeKind.Utc).ToLocalTime().ToString();
				m_playtimeText.text = new TimeSpan(0, 0, 0, (int)loadedObject.PlayTime).ToString();
				m_locationText.text = loadedObject.ActiveLevelMetadata.DisplayName;
				m_saveCountText.text = loadedObject.SaveCount.ToString();
				m_screenshotImage.gameObject.SetActive(value: true);
				DifficultySettings difficultyModeConfiguration = GlobalReferences.Instance.GameDifficultySettings.GetDifficultyModeConfiguration(loadedObject.CurrentDifficulty);
				m_difficultyText.text = difficultyModeConfiguration.DifficultyNameString.GetLocalizedString();
			}
			m_screenshotImage.texture = m_thumbnailTexture;
			break;
		}
		case ResultCode.FileDoesNotExist:
			m_saveDateText.text = "";
			m_playtimeText.text = "";
			m_locationText.text = m_noDataString.GetLocalizedString();
			m_saveCountText.text = "";
			m_difficultyText.text = "";
			m_screenshotImage.gameObject.SetActive(value: false);
			break;
		default:
			m_saveDateText.text = "";
			m_playtimeText.text = "";
			m_locationText.text = Services.Get<Team17DialogLocalizationService>().LoadSaveErrorTitle.GetLocalizedString();
			m_saveCountText.text = "";
			m_difficultyText.text = "";
			m_screenshotImage.gameObject.SetActive(value: false);
			break;
		}
		SetLoading(loading: false);
	}

	public void OnClicked()
	{
		OnSaveSelected?.Invoke(this);
	}

	public void SetSelected(bool selected)
	{
		GetComponent<ButtonExtended>().SetForceSelected(selected);
	}
}
