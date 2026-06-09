using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DebugLevelSelectMenuPage : MenuPage
{
	public enum LevelSelectPageType
	{
		StartConfig,
		LevelRegion
	}

	private struct LevelSelectPageInfo
	{
		public LevelSelectPageType m_pageType;

		public LevelRegionSettings m_region;

		public LevelSelectPageInfo(LevelSelectPageType pageType)
		{
			m_pageType = pageType;
			m_region = null;
		}

		public LevelSelectPageInfo(LevelRegionSettings region)
		{
			m_pageType = LevelSelectPageType.LevelRegion;
			m_region = region;
		}
	}

	[Header("DebugLevelSelectMenuPage")]
	[SerializeField]
	private GameObject m_buttonTemplate;

	[SerializeField]
	private TextMeshProUGUI m_headerText;

	[Header("Chapter Select")]
	[SerializeField]
	private NewGameStartConfiguration[] m_startConfigurations;

	[Header("Level Select")]
	[SerializeField]
	private GameLevelMetadataInfo m_levelsMetadataInfo;

	private List<LevelSelectPageInfo> m_pages;

	private List<GameObject> m_activeButtons = new List<GameObject>();

	private int m_pageIndex;

	protected override void Awake()
	{
		base.Awake();
		m_pages = new List<LevelSelectPageInfo>
		{
			new LevelSelectPageInfo(LevelSelectPageType.StartConfig)
		};
		foreach (GameLevelMetadataInfo.RegionInfo region in m_levelsMetadataInfo.m_regions)
		{
			m_pages.Add(new LevelSelectPageInfo(region.m_region));
		}
	}

	public override void Show(bool instant = false, bool onAwake = false)
	{
		base.Show(instant, onAwake);
		GameInputManager.GameInputActions.UI.TabRight.performed += NextTabInput;
		GameInputManager.GameInputActions.UI.TabLeft.performed += PreviousTabInput;
		Populate();
	}

	public override void Hide(bool instant = false)
	{
		base.Hide(instant);
		GameInputManager.GameInputActions.UI.TabRight.performed -= NextTabInput;
		GameInputManager.GameInputActions.UI.TabLeft.performed -= PreviousTabInput;
	}

	private void NextTabInput(InputAction.CallbackContext context)
	{
		NextTab();
	}

	private void PreviousTabInput(InputAction.CallbackContext context)
	{
		PreviousTab();
	}

	private void ClearExistingButtons()
	{
		foreach (GameObject activeButton in m_activeButtons)
		{
			Object.Destroy(activeButton);
		}
		m_activeButtons.Clear();
	}

	private List<LevelMetadata> GetLevelList()
	{
		LevelSelectPageInfo levelSelectPageInfo = m_pages[m_pageIndex];
		foreach (GameLevelMetadataInfo.RegionInfo region in m_levelsMetadataInfo.m_regions)
		{
			if (region.m_region == levelSelectPageInfo.m_region)
			{
				return region.m_levels;
			}
		}
		return null;
	}

	[UsedImplicitly]
	public int GetPagesCount()
	{
		return m_pages.Count;
	}

	[UsedImplicitly]
	public string GetPageName(int pageIndex)
	{
		LevelSelectPageInfo levelSelectPageInfo = m_pages[m_pageIndex];
		if (levelSelectPageInfo.m_pageType == LevelSelectPageType.StartConfig)
		{
			return "Start Configurations";
		}
		if (levelSelectPageInfo.m_region != null)
		{
			return "Region: " + levelSelectPageInfo.m_region.DisplayName;
		}
		return m_headerText.text = "Unspecified Region";
	}

	[UsedImplicitly]
	public int GetActiveButtonsCount()
	{
		return m_activeButtons.Count;
	}

	[UsedImplicitly]
	public string GetButtonLevel(int activeButtonIndex)
	{
		return m_activeButtons[activeButtonIndex].GetComponentInChildren<TextMeshProUGUI>().text;
	}

	[UsedImplicitly]
	public void PressActiveButtonAtIndex(int activeButtonIndex)
	{
		m_activeButtons[activeButtonIndex].GetComponent<Button>().onClick?.Invoke();
	}

	private void Populate()
	{
		ClearExistingButtons();
		LevelSelectPageInfo levelSelectPageInfo = m_pages[m_pageIndex];
		if (levelSelectPageInfo.m_pageType == LevelSelectPageType.StartConfig)
		{
			m_headerText.text = "Start Configurations";
			NewGameStartConfiguration[] startConfigurations = m_startConfigurations;
			foreach (NewGameStartConfiguration config in startConfigurations)
			{
				GameObject gameObject = Object.Instantiate(m_buttonTemplate, m_buttonTemplate.transform.parent);
				gameObject.gameObject.SetActive(value: true);
				gameObject.GetComponentInChildren<TextMeshProUGUI>().text = config.name;
				gameObject.GetComponent<Button>().onClick.AddListener(delegate
				{
					StartGame(config);
				});
				m_activeButtons.Add(gameObject);
			}
		}
		else
		{
			if (levelSelectPageInfo.m_region != null)
			{
				m_headerText.text = "Region: " + levelSelectPageInfo.m_region.DisplayName;
			}
			else
			{
				m_headerText.text = "Unspecified Region";
			}
			List<LevelMetadata> levelList = GetLevelList();
			if (levelList != null)
			{
				foreach (LevelMetadata level in levelList)
				{
					GameObject gameObject2 = Object.Instantiate(m_buttonTemplate, m_buttonTemplate.transform.parent);
					gameObject2.gameObject.SetActive(value: true);
					gameObject2.GetComponentInChildren<TextMeshProUGUI>().text = level.name;
					gameObject2.GetComponent<Button>().onClick.AddListener(delegate
					{
						StartGame(level);
					});
					m_activeButtons.Add(gameObject2);
				}
			}
		}
		if (m_activeButtons.Count > 0)
		{
			EventSystem.current.SetSelectedGameObject(m_activeButtons[0]);
		}
	}

	private void StartGame(NewGameStartConfiguration startConfiguration)
	{
		GlobalReferences.Instance.EventChannels.MainMenu.ChapterSelect.Raise(startConfiguration);
	}

	private void StartGame(LevelMetadata levelMetadata)
	{
		LevelTransitionEventData value = new LevelTransitionEventData
		{
			m_levelMetadata = levelMetadata,
			m_targetSceneName = levelMetadata.SceneFileName,
			m_targetSceneAssetReference = levelMetadata.TargetSceneAssetReference,
			m_type = LevelTransitionEventType.DebugColdStartup
		};
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransition.Raise(value);
	}

	public void NextTab()
	{
		m_pageIndex++;
		m_pageIndex %= m_pages.Count;
		Populate();
	}

	public void PreviousTab()
	{
		m_pageIndex--;
		if (m_pageIndex < 0)
		{
			m_pageIndex = m_pages.Count - 1;
		}
		Populate();
	}
}
