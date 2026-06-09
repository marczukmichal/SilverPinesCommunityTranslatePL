using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class ComicController : MonoBehaviour
{
	[SerializeField]
	private GameObject m_container;

	[SerializeField]
	private Transform m_comicParent;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private GameMenuState m_gameMenuState;

	[SerializeField]
	private ComicMemory m_comicMemory;

	[Header("Animation")]
	[SerializeField]
	private float m_fadeInComicTime = 0.5f;

	[SerializeField]
	private float m_fadeInBackgroundTime = 0.5f;

	[SerializeField]
	private float m_fadeOutComicTime = 0.75f;

	[SerializeField]
	private float m_fadeOutBackgroundTime = 0.75f;

	[SerializeField]
	private float m_fadeOutBackgroundDelay = 0.5f;

	[Header("UI")]
	[SerializeField]
	private MenuInputPrompts m_uiPrompts;

	[SerializeField]
	private MenuInputPrompts.AvaialbleInput[] m_availableInputs;

	[SerializeField]
	private Image m_backgroundFade;

	[SerializeField]
	private Image m_foregroundFade;

	private GameObject m_activeComicInstance;

	private ComicPage[] m_pages;

	private int m_activePageIndex;

	private bool m_isShowing;

	private bool m_isComicFinished;

	private AsyncOperationHandle<GameObject> m_activeComicHandle;

	private ComicMetadata m_activeComicMetadata;

	private bool m_fromMenu;

	private int ActivePageIndex
	{
		get
		{
			return m_activePageIndex;
		}
		set
		{
			if (m_activePageIndex >= 0 && m_activePageIndex < m_pages.Length)
			{
				m_pages[m_activePageIndex].Hide();
			}
			m_activePageIndex = value;
			if (value >= 0 && value < m_pages.Length)
			{
				m_pages[value].Show();
			}
			UpdatePageNumber();
		}
	}

	public ComicPage CurrentPage
	{
		get
		{
			if (m_pages != null && m_pages.Length != 0)
			{
				return m_pages[m_activePageIndex];
			}
			return null;
		}
	}

	private void Start()
	{
		m_container.gameObject.SetActive(value: false);
		m_uiPrompts.gameObject.SetActive(value: false);
		m_uiPrompts.SetInputs(m_availableInputs);
		m_isShowing = false;
		m_canvasGroup.alpha = 0f;
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.Comic.PlayComic.Register(PlayComicEvent);
		GlobalReferences.Instance.EventChannels.Comic.PlayComicFromMenu.Register(PlayComicEventFromMenu);
	}

	private void OnDisable()
	{
		if (CurrentPage != null)
		{
			CurrentPage.KillAudio();
		}
		GlobalReferences.Instance.EventChannels.Comic.PlayComic.Unregister(PlayComicEvent);
		GlobalReferences.Instance.EventChannels.Comic.PlayComicFromMenu.Unregister(PlayComicEventFromMenu);
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.Game.Proceed.performed -= ComicProgress;
			GameInputManager.GameInputActions.UI.NextPage.performed -= NextPageInput;
			GameInputManager.GameInputActions.UI.PreviousPage.performed -= PreviousPageInput;
		}
	}

	private void PlayComicEvent(ComicMetadata comic)
	{
		m_comicMemory.RecordSeenComic(comic);
		PlayComic(comic, fromMenu: false);
	}

	private void PlayComicEventFromMenu(ComicMetadata comic)
	{
		PlayComic(comic, fromMenu: true);
	}

	private void PlayComic(ComicMetadata comic, bool fromMenu)
	{
		m_fromMenu = fromMenu;
		m_container.gameObject.SetActive(value: true);
		m_uiPrompts.gameObject.SetActive(value: false);
		StartCoroutine(ShowComic(comic));
	}

	private void UpdatePageNumber()
	{
	}

	private void InitiailiseComic()
	{
		m_activePageIndex = -1;
		m_pages = GetComponentsInChildren<ComicPage>(includeInactive: true);
		for (int i = 0; i < m_pages.Length; i++)
		{
			m_pages[i].Init();
		}
		if (m_pages.Length != 0)
		{
			ActivePageIndex = 0;
			m_pages[ActivePageIndex].ActivePanelIndex = 0;
		}
	}

	private void SetComicContainerSizeFromResolution()
	{
		float num = (float)Screen.width / (float)Screen.height;
		float num2 = 1.7777778f;
		if (num < num2)
		{
			m_comicParent.localScale = Vector3.one * (num / num2);
		}
	}

	private IEnumerator ShowComic(ComicMetadata comic)
	{
		if (!m_isShowing)
		{
			m_activeComicMetadata = comic;
			m_activeComicHandle = Addressables.LoadAssetAsync<GameObject>(comic.AssetReference);
			m_isShowing = true;
			m_isComicFinished = false;
			m_gameMenuState.SetInMenu(GameMenuState.GameMenu.Comic);
			DOTween.Kill(m_backgroundFade);
			DOTween.Kill(m_canvasGroup);
			m_foregroundFade.gameObject.SetActive(value: false);
			m_backgroundFade.color = new Color(0f, 0f, 0f, 0f);
			m_canvasGroup.alpha = 0f;
			yield return m_backgroundFade.DOFade(1f, m_fadeInBackgroundTime).SetEase(Ease.OutQuad).SetUpdate(isIndependentUpdate: true)
				.WaitForCompletion();
			yield return new WaitUntil(() => m_activeComicHandle.IsDone);
			m_activeComicInstance = UnityEngine.Object.Instantiate(m_activeComicHandle.Result, m_comicParent);
			SetComicContainerSizeFromResolution();
			InitiailiseComic();
			yield return m_canvasGroup.DOFade(1f, m_fadeInComicTime).SetUpdate(isIndependentUpdate: true).WaitForCompletion();
			GameInputManager.GameInputActions.Game.Proceed.performed += ComicProgress;
			GameInputManager.GameInputActions.UI.NextPage.performed += NextPageInput;
			GameInputManager.GameInputActions.UI.PreviousPage.performed += PreviousPageInput;
			GlobalReferences.Instance.EventChannels.Comic.ToggleComicViewActive.Raise(value: true);
		}
	}

	private void CloseComic()
	{
		if (m_isShowing)
		{
			m_isShowing = false;
			StartCoroutine(CloseComicCorutine());
			GameInputManager.GameInputActions.Game.Proceed.performed -= ComicProgress;
			GameInputManager.GameInputActions.UI.NextPage.performed -= NextPageInput;
			GameInputManager.GameInputActions.UI.PreviousPage.performed -= PreviousPageInput;
		}
	}

	private IEnumerator CloseComicCorutine()
	{
		if (!m_fromMenu && m_activeComicMetadata.StayFadedOnExit)
		{
			GlobalReferences.Instance.EventChannels.Generic.ScreenFadeOfType.Raise(FadeType.BlackInstant);
		}
		DOTween.Kill(m_canvasGroup);
		DOTween.Kill(m_backgroundFade);
		m_foregroundFade.gameObject.SetActive(value: true);
		m_foregroundFade.color = new Color(0f, 0f, 0f, 0f);
		yield return m_foregroundFade.DOFade(1f, m_fadeOutComicTime).SetUpdate(isIndependentUpdate: true).WaitForCompletion();
		if (m_fromMenu || !m_activeComicMetadata.StayFadedOnExit)
		{
			m_canvasGroup.alpha = 0f;
			yield return new WaitForSecondsRealtime(m_fadeOutBackgroundDelay);
			m_foregroundFade.gameObject.SetActive(value: false);
			yield return m_backgroundFade.DOFade(0f, m_fadeOutBackgroundTime).SetEase(Ease.InQuad).SetUpdate(isIndependentUpdate: true)
				.WaitForCompletion();
		}
		GlobalReferences.Instance.EventChannels.Comic.ToggleComicViewActive.Raise(value: false);
		UnloadComicAndResume();
	}

	private void UnloadComicAndResume()
	{
		m_gameMenuState.ClearInMenu(GameMenuState.GameMenu.Comic);
		m_container.gameObject.SetActive(value: false);
		if (m_activeComicInstance != null)
		{
			UnityEngine.Object.Destroy(m_activeComicInstance);
		}
		Addressables.ReleaseInstance(m_activeComicHandle);
	}

	private void Update()
	{
		if (!(m_activeComicInstance == null))
		{
			_ = m_isShowing;
		}
	}

	private void ProgressToNextPage()
	{
		int num = ActivePageIndex + 1;
		if (num < m_pages.Length)
		{
			ActivePageIndex = num;
			m_pages[ActivePageIndex].ActivePanelIndex = 0;
		}
		else if (!m_isComicFinished)
		{
			FinishComic();
		}
		else
		{
			CloseComic();
		}
	}

	private void FinishComic()
	{
		m_isComicFinished = true;
		m_uiPrompts.gameObject.SetActive(value: true);
	}

	public void ProgressForward()
	{
		if (CurrentPage.IsAnimating())
		{
			CurrentPage.Skip();
		}
		if (!CurrentPage.HasMorePanels())
		{
			ProgressToNextPage();
			m_activeComicInstance.GetComponent<Comic>().SkipToNextPage();
		}
		else
		{
			CurrentPage.Progress();
		}
	}

	private void ComicProgress(InputAction.CallbackContext callbackContext)
	{
		if (callbackContext.performed && callbackContext.action.WasPressedThisFrame())
		{
			ProgressForward();
		}
	}

	private void ProgressToPage(string pageName)
	{
		if (CurrentPage.name.Equals(pageName))
		{
			return;
		}
		for (int i = 0; i < m_pages.Length; i++)
		{
			if (m_pages[i].name.Equals(pageName))
			{
				ActivePageIndex = i;
				return;
			}
		}
		Debug.LogWarning("Comic Page progression timeline event could not find matching page for " + pageName + " - just proceeding to next page");
		ProgressToNextPage();
	}

	public void AudioTimelineEvent(string eventName)
	{
		Debug.Log("Comic controller timeline event: " + eventName);
		if (eventName.Contains("Next", StringComparison.OrdinalIgnoreCase))
		{
			if (!CurrentPage.HasMorePanels())
			{
				ProgressToNextPage();
			}
			else
			{
				CurrentPage.Progress();
			}
		}
		else if (eventName.Contains("Page", StringComparison.OrdinalIgnoreCase))
		{
			ProgressToPage(eventName);
		}
		else if (eventName.Contains("Panel", StringComparison.OrdinalIgnoreCase))
		{
			CurrentPage.ProgressToPanel(eventName);
		}
		else if (eventName.Contains("Finish", StringComparison.OrdinalIgnoreCase))
		{
			FinishComic();
		}
	}

	private void GoToPreviousPage()
	{
		int num = ActivePageIndex - 1;
		if (num >= 0)
		{
			ActivePageIndex = num;
			m_pages[ActivePageIndex].EnableAllPanels();
		}
	}

	private void GoToNextPage()
	{
		int num = ActivePageIndex + 1;
		if (num < m_pages.Length)
		{
			ActivePageIndex = num;
			m_pages[ActivePageIndex].EnableAllPanels();
		}
	}

	private void PreviousPageInput(InputAction.CallbackContext context)
	{
		if (m_isComicFinished)
		{
			GoToPreviousPage();
		}
	}

	private void NextPageInput(InputAction.CallbackContext context)
	{
		if (m_isComicFinished)
		{
			GoToNextPage();
		}
	}
}
