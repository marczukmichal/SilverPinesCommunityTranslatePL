using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class InGameMenu : MonoBehaviour, IInputBackHandler
{
	private enum InGameTargetPage
	{
		None,
		Inventory,
		Artifacts,
		Map,
		Notes,
		Options
	}

	[SerializeField]
	private MenuPage m_activePage;

	[SerializeField]
	private GameObject m_container;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[Header("Pages")]
	[SerializeField]
	private InventoryPage m_inventoryPage;

	[SerializeField]
	private MapPage m_mapPage;

	[SerializeField]
	private NotesPage m_notesPage;

	[SerializeField]
	private OptionsMenuPage m_optionsPage;

	[SerializeField]
	private ArtifactsPage m_artifactsPage;

	[Header("Tab Buttons")]
	[SerializeField]
	private GameObject m_tabButtonsContainer;

	[SerializeField]
	private MenuTabButton m_inventoryTabButton;

	[SerializeField]
	private MenuTabButton m_mapTabButton;

	[SerializeField]
	private MenuTabButton m_notesTabButton;

	[SerializeField]
	private MenuTabButton m_optionsTabButton;

	[SerializeField]
	private MenuTabButton m_artifactsTabButton;

	[SerializeField]
	private LocalizedString m_inventoryTabNameString;

	[SerializeField]
	private LocalizedString m_mapTabNameString;

	[SerializeField]
	private LocalizedString m_notesTabNameString;

	[SerializeField]
	private LocalizedString m_optionsTabNameString;

	[SerializeField]
	private LocalizedString m_artifactsTabNameString;

	[SerializeField]
	private TextMeshProUGUI m_tabText;

	[Header("Game Menu State")]
	[SerializeField]
	private GameMenuState m_gameMenuState;

	[Header("Audio")]
	[FormerlySerializedAs("m_audioOpenPauseMenu")]
	[SerializeField]
	private AudioEvent m_audioOpenInGameMenu;

	[FormerlySerializedAs("m_audioClosePauseMenu")]
	[SerializeField]
	private AudioEvent m_audioCloseInGameMenu;

	[Header("Input Prompts")]
	[SerializeField]
	private MenuInputPrompts m_menuInputPrompts;

	[Header("Game Logic")]
	[SerializeField]
	private ProgressionVariable m_inventoryDisabledVariable;

	[SerializeField]
	private ProgressionVariable m_mapDisabledVariable;

	[Header("Status Container")]
	[SerializeField]
	private CanvasGroup m_statusContainerCanvasGroup;

	[Header("Background")]
	[SerializeField]
	private GameObject m_backgroundToggle;

	[Header("Hints")]
	[SerializeField]
	private MenuHintPanel m_hintPanel;

	private List<MenuHintInfo> m_queuedHints = new List<MenuHintInfo>();

	private bool m_skipAnimation;

	private float m_menuOpenTime;

	private bool m_isMenuOpen;

	private float m_animWaitTime;

	private bool m_isPlayerDead;

	private MenuPage m_queuedPage;

	private Coroutine m_activeTabTransitionCoroutine;

	private bool m_inputHeld;

	private bool m_isClosing;

	private float m_lastCloseMenuTime;

	public MenuPage ActivePage
	{
		get
		{
			return m_activePage;
		}
		private set
		{
			if (m_activePage != value)
			{
				m_activePage = value;
				m_menuInputPrompts.SetInputs(m_activePage.AvailableInputs);
				UpdateTabButtons();
			}
		}
	}

	private bool IsInventoryTabEnabled()
	{
		return !m_inventoryDisabledVariable.Value;
	}

	private bool IsArtifactsTabEnabled()
	{
		return GlobalReferences.Instance.MainInventory.ArtifactItems.Count > 0;
	}

	private bool IsMapTabEnabled()
	{
		return !m_mapDisabledVariable.Value;
	}

	private bool IsNotesTabEnabled()
	{
		return GlobalReferences.Instance.LoreInventory.CollectedLore.Count > 0;
	}

	private bool IsOptionsTabEnabled()
	{
		return true;
	}

	private bool CanOpenMenu()
	{
		if (!IsOptionsTabEnabled() && !IsInventoryTabEnabled() && !IsMapTabEnabled() && !IsNotesTabEnabled())
		{
			return IsArtifactsTabEnabled();
		}
		return true;
	}

	private void UpdateTabButtons()
	{
		m_inventoryTabButton.gameObject.SetActive(IsInventoryTabEnabled());
		m_inventoryTabButton.SetActive(m_activePage == m_inventoryPage);
		m_mapTabButton.gameObject.SetActive(IsMapTabEnabled());
		m_mapTabButton.SetActive(m_activePage == m_mapPage);
		m_notesTabButton.gameObject.SetActive(IsNotesTabEnabled());
		m_notesTabButton.SetActive(m_activePage == m_notesPage);
		m_optionsTabButton.gameObject.SetActive(IsOptionsTabEnabled());
		m_optionsTabButton.SetActive(m_activePage == m_optionsPage);
		m_artifactsTabButton.gameObject.SetActive(IsArtifactsTabEnabled());
		m_artifactsTabButton.SetActive(m_activePage == m_artifactsPage);
		m_tabText.transform.SetAsLastSibling();
		MenuTabButton activeTabButton = null;
		string newLabelName = "";
		if (m_activePage == m_inventoryPage)
		{
			newLabelName = m_inventoryTabNameString.GetLocalizedString();
			activeTabButton = m_inventoryTabButton;
		}
		else if (m_activePage == m_artifactsPage)
		{
			newLabelName = m_artifactsTabNameString.GetLocalizedString();
			activeTabButton = m_artifactsTabButton;
		}
		else if (m_activePage == m_mapPage)
		{
			newLabelName = m_mapTabNameString.GetLocalizedString();
			activeTabButton = m_mapTabButton;
		}
		else if (m_activePage == m_notesPage)
		{
			newLabelName = m_notesTabNameString.GetLocalizedString();
			activeTabButton = m_notesTabButton;
		}
		else if (m_activePage == m_optionsPage)
		{
			newLabelName = m_optionsTabNameString.GetLocalizedString();
			activeTabButton = m_optionsTabButton;
		}
		if (m_activeTabTransitionCoroutine != null)
		{
			StopCoroutine(m_activeTabTransitionCoroutine);
		}
		m_activeTabTransitionCoroutine = StartCoroutine(DoTabTextTransitionAnimation(newLabelName, activeTabButton));
	}

	private IEnumerator DoTabTextTransitionAnimation(string newLabelName, MenuTabButton activeTabButton)
	{
		DOTween.Kill(m_tabText);
		yield return m_tabText.DOFade(0f, 0.05f).WaitForCompletion();
		m_tabText.text = newLabelName;
		yield return new WaitForEndOfFrame();
		List<MenuTabButton> list = new List<MenuTabButton>();
		if (IsInventoryTabEnabled())
		{
			list.Add(m_inventoryTabButton);
		}
		if (IsArtifactsTabEnabled())
		{
			list.Add(m_artifactsTabButton);
		}
		if (IsMapTabEnabled())
		{
			list.Add(m_mapTabButton);
		}
		if (IsNotesTabEnabled())
		{
			list.Add(m_notesTabButton);
		}
		if (IsOptionsTabEnabled())
		{
			list.Add(m_optionsTabButton);
		}
		float num = (float)list.Count * -40f;
		foreach (MenuTabButton item in list)
		{
			RectTransform component = item.GetComponent<RectTransform>();
			DOTween.Kill(component);
			component.DOAnchorPosX(num, 0.2f).SetUpdate(isIndependentUpdate: true);
			if (item == activeTabButton)
			{
				num += 32f;
				m_tabText.GetComponent<RectTransform>().anchoredPosition = new Vector2(num, 0f);
				num += m_tabText.preferredWidth + 32f;
			}
			else
			{
				num += 64f;
			}
		}
		yield return new WaitForSecondsRealtime(0.1f);
		m_tabText.DOFade(1f, 0.1f).SetUpdate(isIndependentUpdate: true).WaitForCompletion();
		m_tabText.maxVisibleCharacters = 0;
		int charCount = 0;
		while (charCount < m_tabText.text.Length)
		{
			charCount = (m_tabText.maxVisibleCharacters = charCount + 1);
			yield return new WaitForSecondsRealtime(0.12f / (float)m_tabText.text.Length);
		}
	}

	private void Awake()
	{
		m_container.SetActive(value: false);
		m_statusContainerCanvasGroup.alpha = 0f;
		if (m_hintPanel != null)
		{
			MenuHintPanel hintPanel = m_hintPanel;
			hintPanel.OnClosedHint = (UnityAction)Delegate.Combine(hintPanel.OnClosedHint, new UnityAction(OnMenuHintClosed));
		}
	}

	private void OnMenuHintClosed()
	{
		if (m_activePage != null)
		{
			m_activePage.SelectDefaultSelectable();
		}
	}

	private void OnEnable()
	{
		LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
		GameMenuState gameMenuState = m_gameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Combine(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
		GlobalReferences.Instance.EventChannels.InGameMenu.ShowMapMenuTab.Register(ShowMapPageInstant);
		GlobalReferences.Instance.EventChannels.InGameMenu.ShowInventoryMenuTab.Register(ShowInventoryPageInstant);
		GlobalReferences.Instance.EventChannels.InGameMenu.ShowOptionsMenuTab.Register(ShowOptionsPageInstant);
		GameInputManager.GameInputActions.MenuToggles.SystemMenu.performed += SystemMenuInput;
		GameInputManager.GameInputActions.MenuToggles.Inventory.performed += InventoryMenuInput;
		GameInputManager.GameInputActions.MenuToggles.Artifacts.performed += ArtifactsMenuInput;
		GameInputManager.GameInputActions.MenuToggles.Map.performed += MapMenuInput;
		GameInputManager.GameInputActions.MenuToggles.Notes.performed += NotesMenuInput;
		GameInputManager.GameInputActions.MenuToggles.SystemMenu.canceled += ReleaseMenuInput;
		GameInputManager.GameInputActions.MenuToggles.Inventory.canceled += ReleaseMenuInput;
		GameInputManager.GameInputActions.MenuToggles.Artifacts.canceled += ReleaseMenuInput;
		GameInputManager.GameInputActions.MenuToggles.Map.canceled += ReleaseMenuInput;
		GameInputManager.GameInputActions.MenuToggles.Notes.canceled += ReleaseMenuInput;
		GlobalReferences.Instance.EventChannels.Player.PlayerDead.Register(OnPlayerDeadEvent);
		GlobalReferences.Instance.EventChannels.Player.PlayerTakeDamage.Register(OnPlayerTakeDamageEvent);
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelUnloaded.Register(ClearPlayerDeadFlag);
		GlobalReferences.Instance.EventChannels.SaveLoad.OnGameReloadedEvent.Register(ClearPlayerDeadFlag);
		GlobalReferences.Instance.EventChannels.Lore.ShowLoreEntryInNotesPage.Register(OnShowLoreInNotesPage);
		GlobalReferences.Instance.EventChannels.Lore.ShowLoreEntryOnMapPage.Register(OnShowLoreOnMapPage);
		GlobalReferences.Instance.EventChannels.Inventory.ArtifactItemCollected.Register(OnArtifactItemCollected);
		GlobalReferences.Instance.EventChannels.Hints.ShowMenuHintInfo.Register(OnShowMenuHint);
	}

	private void OnDisable()
	{
		LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
		m_gameMenuState.ClearInMenu(GameMenuState.GameMenu.InGameMenu);
		GameMenuState gameMenuState = m_gameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Remove(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
		GlobalReferences.Instance.EventChannels.InGameMenu.InGameMenuRequestPage.Unregister(OnRequestPage);
		GlobalReferences.Instance.EventChannels.InGameMenu.ShowMapMenuTab.Unregister(ShowMapPageInstant);
		GlobalReferences.Instance.EventChannels.InGameMenu.ShowInventoryMenuTab.Unregister(ShowInventoryPageInstant);
		GlobalReferences.Instance.EventChannels.InGameMenu.ShowOptionsMenuTab.Unregister(ShowOptionsPageInstant);
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.MenuToggles.SystemMenu.performed -= SystemMenuInput;
			GameInputManager.GameInputActions.MenuToggles.Inventory.performed -= InventoryMenuInput;
			GameInputManager.GameInputActions.MenuToggles.Artifacts.performed -= ArtifactsMenuInput;
			GameInputManager.GameInputActions.MenuToggles.Map.performed -= MapMenuInput;
			GameInputManager.GameInputActions.MenuToggles.Notes.performed -= NotesMenuInput;
			GameInputManager.GameInputActions.MenuToggles.SystemMenu.canceled -= ReleaseMenuInput;
			GameInputManager.GameInputActions.MenuToggles.Inventory.canceled -= ReleaseMenuInput;
			GameInputManager.GameInputActions.MenuToggles.Artifacts.canceled -= ReleaseMenuInput;
			GameInputManager.GameInputActions.MenuToggles.Map.canceled -= ReleaseMenuInput;
			GameInputManager.GameInputActions.MenuToggles.Notes.canceled -= ReleaseMenuInput;
		}
		GlobalReferences.Instance.EventChannels.Player.PlayerDead.Unregister(OnPlayerDeadEvent);
		GlobalReferences.Instance.EventChannels.Player.PlayerTakeDamage.Unregister(OnPlayerTakeDamageEvent);
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelUnloaded.Unregister(ClearPlayerDeadFlag);
		GlobalReferences.Instance.EventChannels.SaveLoad.OnGameReloadedEvent.Unregister(ClearPlayerDeadFlag);
		GlobalReferences.Instance.EventChannels.Lore.ShowLoreEntryInNotesPage.Unregister(OnShowLoreInNotesPage);
		GlobalReferences.Instance.EventChannels.Lore.ShowLoreEntryOnMapPage.Unregister(OnShowLoreOnMapPage);
		GlobalReferences.Instance.EventChannels.Inventory.ArtifactItemCollected.Unregister(OnArtifactItemCollected);
		GlobalReferences.Instance.EventChannels.Hints.ShowMenuHintInfo.Unregister(OnShowMenuHint);
	}

	private void OnDestroy()
	{
		if (m_gameMenuState != null)
		{
			GameMenuState gameMenuState = m_gameMenuState;
			gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Remove(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
		}
		GameInputManager.RemoveBackInputHandler(this);
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.UI.TabLeft.performed -= OnPreviousTabCallback;
			GameInputManager.GameInputActions.UI.TabRight.performed -= OnNextTabCallback;
			GameInputManager.GameInputActions.MenuToggles.SystemMenu.performed -= SystemMenuInput;
			GameInputManager.GameInputActions.MenuToggles.Inventory.performed -= InventoryMenuInput;
			GameInputManager.GameInputActions.MenuToggles.Artifacts.performed -= ArtifactsMenuInput;
			GameInputManager.GameInputActions.MenuToggles.Map.performed -= MapMenuInput;
			GameInputManager.GameInputActions.MenuToggles.Notes.performed -= NotesMenuInput;
			GameInputManager.GameInputActions.MenuToggles.SystemMenu.canceled -= ReleaseMenuInput;
			GameInputManager.GameInputActions.MenuToggles.Inventory.canceled -= ReleaseMenuInput;
			GameInputManager.GameInputActions.MenuToggles.Artifacts.canceled -= ReleaseMenuInput;
			GameInputManager.GameInputActions.MenuToggles.Map.canceled -= ReleaseMenuInput;
			GameInputManager.GameInputActions.MenuToggles.Notes.canceled -= ReleaseMenuInput;
		}
	}

	private void OnGameMenuChanged(GameMenuState.GameMenu menu)
	{
		if (menu.HasFlag(GameMenuState.GameMenu.InGameMenu))
		{
			ShowInGameMenu();
		}
		else
		{
			CloseInGameMenu();
		}
	}

	private void SystemMenuInput(InputAction.CallbackContext context)
	{
		if (context.performed && !m_inputHeld)
		{
			DoShowMenuInput(InGameTargetPage.Options);
		}
	}

	private void InventoryMenuInput(InputAction.CallbackContext context)
	{
		if (context.performed && !m_inputHeld)
		{
			DoShowMenuInput(InGameTargetPage.Inventory);
		}
	}

	private void ArtifactsMenuInput(InputAction.CallbackContext context)
	{
		if (context.performed && !m_inputHeld)
		{
			DoShowMenuInput(InGameTargetPage.Artifacts);
		}
	}

	private void MapMenuInput(InputAction.CallbackContext context)
	{
		if (context.performed && !m_inputHeld)
		{
			DoShowMenuInput(InGameTargetPage.Map);
		}
	}

	private void NotesMenuInput(InputAction.CallbackContext context)
	{
		if (context.performed && !m_inputHeld)
		{
			DoShowMenuInput(InGameTargetPage.Notes);
		}
	}

	private void ReleaseMenuInput(InputAction.CallbackContext context)
	{
		m_inputHeld = false;
	}

	private void DoShowMenuInput(InGameTargetPage targetPage)
	{
		bool flag = false;
		m_inputHeld = true;
		if (targetPage == InGameTargetPage.Inventory && !IsInventoryTabEnabled())
		{
			targetPage = InGameTargetPage.Options;
		}
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (item != null)
		{
			CharacterInputPlayer component = item.GetComponent<CharacterInputPlayer>();
			if (component != null && component.IsUsingQuickMap)
			{
				component.ClearQuickMapInput();
				return;
			}
		}
		if (m_isMenuOpen)
		{
			switch (targetPage)
			{
			case InGameTargetPage.Inventory:
				if (m_activePage == m_inventoryPage)
				{
					flag = true;
				}
				break;
			case InGameTargetPage.Map:
				if (m_activePage == m_mapPage)
				{
					flag = true;
				}
				break;
			case InGameTargetPage.Notes:
				if (m_activePage == m_notesPage)
				{
					flag = true;
				}
				break;
			case InGameTargetPage.Options:
				if (m_activePage == m_optionsPage)
				{
					flag = true;
				}
				break;
			}
		}
		if (flag)
		{
			TryClose();
		}
		else
		{
			if (m_isPlayerDead || (!m_gameMenuState.IsInMenuExclusively(GameMenuState.GameMenu.Minigame) && m_gameMenuState.IsInAnyMenu() && !m_gameMenuState.IsInMenuExclusively(GameMenuState.GameMenu.InGameMenu)) || LevelManager.LevelTransitionActive || GameCutsceneManager.CutsceneActive || !CanOpenMenu())
			{
				return;
			}
			LevelMetadataReference levelMetadataReference = UnityEngine.Object.FindAnyObjectByType<LevelMetadataReference>();
			if (levelMetadataReference != null && levelMetadataReference.Metadata.IsCutscene)
			{
				return;
			}
			if (m_isMenuOpen)
			{
				switch (targetPage)
				{
				case InGameTargetPage.Inventory:
					if (IsInventoryTabEnabled())
					{
						OnRequestPage(m_inventoryPage);
					}
					break;
				case InGameTargetPage.Map:
					if (IsMapTabEnabled())
					{
						OnRequestPage(m_mapPage);
					}
					break;
				case InGameTargetPage.Notes:
					if (IsNotesTabEnabled())
					{
						OnRequestPage(m_notesPage);
					}
					break;
				case InGameTargetPage.Options:
					if (IsOptionsTabEnabled())
					{
						OnRequestPage(m_optionsPage);
					}
					break;
				case InGameTargetPage.Artifacts:
					break;
				}
				return;
			}
			switch (targetPage)
			{
			case InGameTargetPage.Inventory:
				if (IsInventoryTabEnabled())
				{
					ShowInventoryPageInstant();
				}
				break;
			case InGameTargetPage.Map:
				if (IsMapTabEnabled())
				{
					ShowMapPageInstant();
				}
				break;
			case InGameTargetPage.Notes:
				if (IsNotesTabEnabled())
				{
					ShowPageInstant(m_notesPage);
				}
				break;
			case InGameTargetPage.Options:
				if (IsOptionsTabEnabled())
				{
					ShowPageInstant(m_optionsPage);
				}
				break;
			default:
				m_gameMenuState.SetInMenu(GameMenuState.GameMenu.InGameMenu);
				break;
			}
		}
	}

	private void ShowMapPageInstant()
	{
		ShowPageInstant(m_mapPage);
	}

	private void ShowInventoryPageInstant()
	{
		ShowPageInstant(m_inventoryPage);
	}

	private void ShowOptionsPageInstant()
	{
		ShowPageInstant(m_optionsPage);
	}

	private void ShowPageInstant(MenuPage page)
	{
		OnRequestPage(page, instant: true);
		ShowInGameMenu();
	}

	public void ShowInGameMenu()
	{
		if (!m_isMenuOpen)
		{
			m_isClosing = false;
			m_isMenuOpen = true;
			DOTween.Kill(m_canvasGroup);
			m_canvasGroup.alpha = 0f;
			m_canvasGroup.DOFade(1f, 0.2f).SetUpdate(isIndependentUpdate: true);
			GlobalReferences.Instance.EventChannels.InGameMenu.InGameMenuRequestPage.Register(OnRequestPage);
			UpdateTabButtons();
			Animate(animateIn: true);
			GameInputManager.PushBackInputHandler(this);
			GameInputManager.GameInputActions.MenuToggles.SystemMenu.performed += OnCloseMenuCallback;
			bool flag = GlobalReferences.Instance.Anchors.Inventory.ItemPickupInteractAnchor.Item != null || GlobalReferences.Instance.Anchors.Inventory.ApplyItemInteractableAnchor.Item != null || GlobalReferences.Instance.Anchors.Inventory.ItemBoxInteractableAnchor.Item != null;
			bool flag2 = GlobalReferences.Instance.Anchors.Inventory.ItemBoxInteractableAnchor.Item != null;
			bool flag3 = flag;
			if (!flag3)
			{
				GameInputManager.GameInputActions.UI.TabLeft.performed += OnPreviousTabCallback;
				GameInputManager.GameInputActions.UI.TabRight.performed += OnNextTabCallback;
			}
			m_backgroundToggle.gameObject.SetActive(!flag3);
			m_tabButtonsContainer.SetActive(!flag3);
			m_inventoryPage.SetQuickInventoryMode(flag);
			m_gameMenuState.SetInMenu(GameMenuState.GameMenu.InGameMenu);
			m_menuOpenTime = Time.unscaledTime;
			m_activePage.Show(instant: true);
			m_queuedPage = null;
			m_menuInputPrompts.SetInputs(m_activePage.AvailableInputs);
			m_mapPage.SetMapShouldReposition();
			AudioEvent.Play2D(m_audioOpenInGameMenu);
			if (!flag || flag2)
			{
				CheckForHintToShow();
			}
		}
	}

	public void CloseInGameMenu()
	{
		if (!m_isClosing && m_isMenuOpen)
		{
			m_isClosing = true;
			m_inputHeld = false;
			GlobalReferences.Instance.EventChannels.InGameMenu.InGameMenuRequestPage.Unregister(OnRequestPage);
			GlobalReferences.Instance.EventChannels.Camera.SetMainCameraEnabled.Raise(value: true);
			GameInputManager.RemoveBackInputHandler(this);
			if (GameInputManager.GameInputActions != null)
			{
				GameInputManager.GameInputActions.UI.TabLeft.performed -= OnPreviousTabCallback;
				GameInputManager.GameInputActions.UI.TabRight.performed -= OnNextTabCallback;
				GameInputManager.GameInputActions.UI.Cancel.performed -= OnCloseMenuCallback;
				GameInputManager.GameInputActions.MenuToggles.SystemMenu.performed -= OnCloseMenuCallback;
			}
			StartCoroutine(CloseAnimation());
			AudioEvent.Play2D(m_audioCloseInGameMenu);
		}
	}

	private IEnumerator CloseAnimation()
	{
		yield return m_canvasGroup.DOFade(0f, 0.2f).SetUpdate(isIndependentUpdate: true).WaitForCompletion();
		m_gameMenuState.ClearInMenu(GameMenuState.GameMenu.InGameMenu);
		Animate(animateIn: false);
		m_isClosing = false;
		m_isMenuOpen = false;
	}

	private void Animate(bool animateIn)
	{
		if (animateIn)
		{
			m_menuInputPrompts.Show(m_skipAnimation);
		}
		else
		{
			m_menuInputPrompts.Hide();
		}
		if (m_skipAnimation)
		{
			m_container.SetActive(animateIn);
			return;
		}
		if (animateIn)
		{
			m_container.SetActive(value: true);
		}
		m_container.SetActive(animateIn);
	}

	private void OnRequestPage(MenuPage newPage)
	{
		OnRequestPage(newPage, instant: false);
	}

	private void OnRequestPage(MenuPage newPage, bool instant)
	{
		if (!(newPage == m_activePage) && m_activePage.CanExitPage())
		{
			bool flag = newPage == m_inventoryPage || newPage == m_artifactsPage;
			DOTween.Kill(m_statusContainerCanvasGroup);
			if (instant)
			{
				m_statusContainerCanvasGroup.alpha = (flag ? 1f : 0f);
			}
			else
			{
				m_statusContainerCanvasGroup.DOFade(flag ? 1f : 0f, 0.5f).SetUpdate(isIndependentUpdate: true);
			}
			MenuPage activePage = m_activePage;
			activePage.Hide(instant);
			ActivePage = newPage;
			CheckForHintToShow();
			if (instant)
			{
				newPage.Show(instant: true);
				return;
			}
			m_animWaitTime = activePage.AnimTime;
			m_queuedPage = newPage;
		}
	}

	public void ReturnToGameButton()
	{
		CloseInGameMenu();
	}

	public void ReturnToMainMenu()
	{
		SceneManager.LoadScene("Assets/Scenes/Utility/Startup.unity");
	}

	private void OnPreviousTabCallback(InputAction.CallbackContext inputCallback)
	{
		TrySwitchTab(forward: false);
	}

	private void OnNextTabCallback(InputAction.CallbackContext inputCallback)
	{
		TrySwitchTab(forward: true);
	}

	private MenuPage GetNextPage(bool forward)
	{
		List<MenuPage> list = new List<MenuPage>();
		if (IsInventoryTabEnabled())
		{
			list.Add(m_inventoryPage);
		}
		if (IsArtifactsTabEnabled())
		{
			list.Add(m_artifactsPage);
		}
		if (IsMapTabEnabled())
		{
			list.Add(m_mapPage);
		}
		if (IsNotesTabEnabled())
		{
			list.Add(m_notesPage);
		}
		if (IsOptionsTabEnabled())
		{
			list.Add(m_optionsPage);
		}
		int num = list.IndexOf(m_activePage);
		if (forward)
		{
			num++;
			num %= list.Count;
		}
		else
		{
			num--;
			if (num < 0)
			{
				num = list.Count - 1;
			}
		}
		return list[num];
	}

	private void TrySwitchTab(bool forward)
	{
		if (m_activePage.CanExitPage() && m_activePage.CanChangeTabs() && !m_hintPanel.IsShowing)
		{
			MenuPage nextPage = GetNextPage(forward);
			OnRequestPage(nextPage);
		}
	}

	private void OnCloseMenuCallback(InputAction.CallbackContext inputCallback)
	{
		if (inputCallback.performed)
		{
			TryClose();
		}
	}

	public bool OnInputBack()
	{
		TryClose(isBackInput: true);
		return false;
	}

	private void TryClose(bool isBackInput = false)
	{
		if (m_hintPanel.IsShowing)
		{
			m_hintPanel.TryHide();
		}
		else
		{
			if (!m_activePage.CanExitPage() || (isBackInput && m_activePage.OnBackInput()) || Time.unscaledTime - m_lastCloseMenuTime < 0.1f)
			{
				return;
			}
			m_lastCloseMenuTime = Time.unscaledTime;
			if (Time.unscaledTime - m_menuOpenTime <= 0.1f)
			{
				return;
			}
			if (m_activePage is InventoryPage)
			{
				bool flag = true;
				InventoryPanel[] componentsInChildren = m_activePage.GetComponentsInChildren<InventoryPanel>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					if (!componentsInChildren[i].ShouldExitOnCancel())
					{
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					return;
				}
			}
			CloseInGameMenu();
		}
	}

	private void Update()
	{
		if (!m_container.activeSelf || m_isClosing)
		{
			return;
		}
		m_menuInputPrompts.SetInputs(m_activePage.AvailableInputs);
		if (m_queuedPage != null)
		{
			m_animWaitTime -= Time.unscaledDeltaTime;
			if (m_animWaitTime <= 0f)
			{
				m_queuedPage.Show();
				m_queuedPage = null;
			}
		}
	}

	private void OnLoreReaderActiveStateChanged(bool active)
	{
		m_tabButtonsContainer.SetActive(!active);
	}

	private void OnPlayerDeadEvent()
	{
		m_gameMenuState.ClearInMenu(GameMenuState.GameMenu.InGameMenu);
		m_isPlayerDead = true;
	}

	private void OnPlayerTakeDamageEvent()
	{
		if (m_activePage != m_inventoryPage)
		{
			m_gameMenuState.ClearInMenu(GameMenuState.GameMenu.InGameMenu);
		}
	}

	private void ClearPlayerDeadFlag()
	{
		m_isPlayerDead = false;
	}

	private void OnShowLoreInNotesPage(LoreEntry loreEntry)
	{
		m_notesPage.JumpToLoreEntry(loreEntry);
		OnRequestPage(m_notesPage);
	}

	private void OnShowLoreOnMapPage(LoreEntry loreEntry)
	{
		m_mapPage.LocateLoreOnMap(loreEntry);
		OnRequestPage(m_mapPage);
	}

	private void OnArtifactItemCollected(ItemInstance artifact)
	{
		StartCoroutine(ArtifactItemCollectedFlow(artifact));
	}

	private void OnShowMenuHint(MenuHintInfo menuHint)
	{
		if (!(menuHint == null))
		{
			m_queuedHints.Add(menuHint);
			if (m_isMenuOpen && !m_hintPanel.IsShowing)
			{
				CheckForHintToShow();
			}
		}
	}

	private bool CanShowHint(MenuHintInfo info)
	{
		return info.MenuPage switch
		{
			MenuHintInfo.AssociatedMenuPage.Inventory => m_activePage == m_inventoryPage, 
			MenuHintInfo.AssociatedMenuPage.Artifacts => m_activePage == m_artifactsPage, 
			MenuHintInfo.AssociatedMenuPage.Map => m_activePage == m_mapPage, 
			_ => false, 
		};
	}

	private void CheckForHintToShow()
	{
		if (GlobalReferences.Instance.Anchors.Inventory.ItemPickupInteractAnchor.Item != null || GlobalReferences.Instance.Anchors.Inventory.ApplyItemInteractableAnchor.Item != null)
		{
			return;
		}
		MenuHintInfo menuHintInfo = null;
		foreach (MenuHintInfo queuedHint in m_queuedHints)
		{
			if (CanShowHint(queuedHint))
			{
				menuHintInfo = queuedHint;
				break;
			}
		}
		if (menuHintInfo != null)
		{
			m_hintPanel.Show(menuHintInfo);
			m_queuedHints.Remove(menuHintInfo);
		}
	}

	private IEnumerator ArtifactItemCollectedFlow(ItemInstance artifact)
	{
		yield return new WaitForSeconds(2f);
		if (m_isMenuOpen)
		{
			yield return new WaitUntil(() => m_activePage.CanExitPage());
		}
		ShowInGameMenu();
		OnRequestPage(m_artifactsPage);
		m_artifactsPage.HighlightArtfact(artifact);
	}

	private void OnLocaleChanged(UnityEngine.Localization.Locale locale)
	{
		UpdateTabButtons();
	}
}
