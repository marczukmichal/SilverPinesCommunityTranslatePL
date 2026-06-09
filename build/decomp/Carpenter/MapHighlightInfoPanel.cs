using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

public class MapHighlightInfoPanel : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_titleText;

	[SerializeField]
	private TextMeshProUGUI m_descriptionText;

	[SerializeField]
	private GameObject m_container;

	[Header("Button Prompts")]
	[SerializeField]
	private GameObject m_buttonPromptsContainer;

	[SerializeField]
	private GameObject m_selectPrompt;

	[SerializeField]
	private LocalizeStringEvent m_selectPromptLabelString;

	[SerializeField]
	private GameObject m_locatePrompt;

	[SerializeField]
	private bool m_showLocatePromptForLore;

	[Header("Group Info")]
	[SerializeField]
	private GameObject m_groupInfoContainer;

	[SerializeField]
	private TextMeshProUGUI m_groupInfoText;

	[Header("Audio Logs")]
	[SerializeField]
	private GameObject m_audioLogContainer;

	[SerializeField]
	private LocalizeStringEvent m_playButtonTextLocalize;

	[SerializeField]
	private LocalizedString m_playButtonString;

	[SerializeField]
	private LocalizedString m_stopButtonString;

	private Map3DIcon m_activeMapIcon;

	private IMapRaycastTooltipElement m_raycastTooltipElement;

	[Header("UI")]
	[SerializeField]
	private Vector2 m_defaultPosition;

	private RectTransform m_rectTransform;

	private void Awake()
	{
		m_rectTransform = GetComponent<RectTransform>();
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.Lore.AudioLogPlaybackStatusChanged.Register(UpdateAudioLogPlayText);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Lore.AudioLogPlaybackStatusChanged.Unregister(UpdateAudioLogPlayText);
	}

	public void PopulateForLoreEntry(LoreEntry loreEntry)
	{
		m_titleText.text = loreEntry.Title;
		m_descriptionText.text = "";
		m_audioLogContainer.SetActive(loreEntry.IsAudioLog);
		m_buttonPromptsContainer.SetActive(value: true);
		m_selectPrompt.gameObject.SetActive(value: true);
		m_selectPromptLabelString.StringReference = new LocalizedString("UICommon", "Read");
		m_locatePrompt.gameObject.SetActive(m_showLocatePromptForLore);
		if (loreEntry.IsAudioLog)
		{
			UpdateAudioLogPlayText(GlobalReferences.Instance.Anchors.Lore.AudioLogPlayerAnchor.Item.IsPlaying);
		}
	}

	private void PopulateForMapIcon(Map3DIcon icon)
	{
		m_titleText.text = icon.GetHoverName();
		m_descriptionText.text = "";
	}

	private void PopulateForUserMarker(MapUserMarkerIcon icon)
	{
		m_titleText.text = icon.GetHoverName();
		m_descriptionText.text = "";
		m_buttonPromptsContainer.SetActive(value: true);
		m_selectPrompt.gameObject.SetActive(value: true);
		m_selectPromptLabelString.StringReference = new LocalizedString("UICommon", "Select");
		m_locatePrompt.gameObject.SetActive(value: false);
	}

	private void PopulateForPhotoIcon(MapPhoto icon)
	{
		m_titleText.text = icon.GetHoverName();
		m_descriptionText.text = "";
		m_buttonPromptsContainer.SetActive(value: true);
		m_selectPrompt.gameObject.SetActive(value: true);
		m_selectPromptLabelString.StringReference = new LocalizedString("UICommon", "Select");
		m_locatePrompt.gameObject.SetActive(value: false);
	}

	private void PopulateForMapRegionIcon(MapRegionAreaIcon regionAreaIcon)
	{
		m_titleText.text = "";
		m_descriptionText.text = "";
		m_buttonPromptsContainer.SetActive(value: true);
		m_selectPrompt.gameObject.SetActive(value: true);
		m_selectPromptLabelString.StringReference = new LocalizedString("UICommon", "View");
		m_locatePrompt.gameObject.SetActive(value: false);
	}

	private void PopulateForMapRaycastTooltipElement(IMapRaycastTooltipElement element)
	{
		m_titleText.text = element.TooltipString;
		m_descriptionText.text = "";
	}

	public void SetMap3DIcon(Map3DIcon mapIcon)
	{
		if (m_activeMapIcon == mapIcon)
		{
			return;
		}
		m_activeMapIcon = mapIcon;
		m_raycastTooltipElement = null;
		if (m_activeMapIcon != null)
		{
			m_container.SetActive(value: true);
			m_audioLogContainer.SetActive(value: false);
			m_buttonPromptsContainer.SetActive(value: false);
			if (m_activeMapIcon is MapLoreEntryIcon)
			{
				PopulateForLoreEntry((m_activeMapIcon as MapLoreEntryIcon).LoreEntry);
			}
			else if (m_activeMapIcon is MapRegionAreaIcon)
			{
				PopulateForMapRegionIcon(m_activeMapIcon as MapRegionAreaIcon);
			}
			else if (m_activeMapIcon is MapPhoto)
			{
				PopulateForPhotoIcon(m_activeMapIcon as MapPhoto);
			}
			else if (m_activeMapIcon is MapUserMarkerIcon)
			{
				PopulateForUserMarker(m_activeMapIcon as MapUserMarkerIcon);
			}
			else
			{
				PopulateForMapIcon(mapIcon);
			}
		}
		else
		{
			m_container.SetActive(value: false);
		}
	}

	public void SetGroupInfo(int groupSelectionCount, int currentIndex)
	{
		m_groupInfoContainer.SetActive(groupSelectionCount > 1);
		if (groupSelectionCount > 1)
		{
			m_groupInfoText.text = currentIndex + 1 + "/" + groupSelectionCount;
		}
	}

	public void Clear()
	{
		m_activeMapIcon = null;
		m_raycastTooltipElement = null;
		m_container.SetActive(value: false);
	}

	public void SetMapRaycastTooltipElement(IMapRaycastTooltipElement element)
	{
		if (m_raycastTooltipElement != element)
		{
			m_activeMapIcon = null;
			m_raycastTooltipElement = element;
			if (m_raycastTooltipElement != null)
			{
				m_container.SetActive(value: true);
				m_audioLogContainer.SetActive(value: false);
				m_buttonPromptsContainer.SetActive(value: false);
				PopulateForMapRaycastTooltipElement(element);
				m_groupInfoContainer.SetActive(value: false);
			}
			else
			{
				m_container.SetActive(value: false);
			}
		}
	}

	public void ShowButtonPrompts(bool show)
	{
		m_buttonPromptsContainer.SetActive(show);
	}

	public void PositionAroundMapIcon(Map3DIcon mapIcon)
	{
		Vector3 position = mapIcon.transform.position;
		position += mapIcon.InfoPanelOffset;
		base.transform.position = position;
	}

	public void PositionAroundRaycastableTooltipElement(IMapRaycastTooltipElement element, Camera mapCamera)
	{
		Vector3 position = mapCamera.WorldToScreenPoint(element.TooltipWorldPosition);
		position.y -= 16f;
		base.transform.position = position;
	}

	public void PositionInNormalUI()
	{
		m_rectTransform.anchoredPosition = m_defaultPosition;
	}

	public void PositionCenter()
	{
		base.transform.localPosition = Vector3.zero;
	}

	private void UpdateAudioLogPlayText(bool isPlaying)
	{
		m_playButtonTextLocalize.StringReference = (isPlaying ? m_stopButtonString : m_playButtonString);
	}
}
