using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

public class PauseHintInfoUI : MonoBehaviour
{
	[Header("UI")]
	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private InlineButtonPromptsText m_text;

	[SerializeField]
	private TextMeshProUGUI m_titleText;

	[SerializeField]
	private Image m_image;

	[SerializeField]
	private GameObject m_continuePrompt;

	[SerializeField]
	private GameObject m_buttonPromptContainer;

	[SerializeField]
	private InputPrompt m_buttonInputPrompt;

	[SerializeField]
	private TextMeshProUGUI m_buttonPromptLabelText;

	[SerializeField]
	private RectTransform m_textWithButtonContainer;

	[SerializeField]
	private RectTransform m_textWithNoButtonContaienr;

	[Header("Video")]
	[SerializeField]
	private VideoPlayer m_videoPlayer;

	[SerializeField]
	private RawImage m_videoPlayerRawImaage;

	[Header("Animation")]
	[SerializeField]
	private float m_showAnimTime = 0.5f;

	[Header("Game Menu State")]
	[SerializeField]
	private GameMenuState m_gameMenuState;

	[Header("Enable Input Delay")]
	[SerializeField]
	private float m_enabledInputDelay = 0.5f;

	[Header("Special Settings")]
	[SerializeField]
	private bool m_isSystemMenu;

	private bool m_isShowing;

	private RenderTexture m_videoRenderTexture;

	private void OnEnable()
	{
		m_canvasGroup.alpha = 0f;
		m_canvasGroup.gameObject.SetActive(value: false);
		if (!m_isSystemMenu)
		{
			GlobalReferences.Instance.EventChannels.Hints.ShowPauseHintInfo.Register(OnShowHint);
		}
	}

	private void OnDisable()
	{
		if (!m_isSystemMenu)
		{
			GlobalReferences.Instance.EventChannels.Hints.ShowPauseHintInfo.Unregister(OnShowHint);
			if (GameInputManager.GameInputActions != null)
			{
				GameInputManager.GameInputActions.UI.Submit.performed -= OnDoneInput;
			}
		}
	}

	private void OnDestroy()
	{
		if (m_isShowing)
		{
			PauntHintClosed();
		}
	}

	private void OnShowHint(PauseHintInfo hint)
	{
		if (GlobalReferences.Instance.UserPreferences.ShowHints)
		{
			m_gameMenuState.SetInMenu(GameMenuState.GameMenu.PauseHint);
			SetHintData(hint);
		}
	}

	public void SetHintData(PauseHintInfo hint)
	{
		m_isShowing = true;
		m_text.SetText(hint.HintText, hint.InlineTextInputActions);
		m_titleText.text = hint.TitleText;
		m_image.sprite = hint.Image;
		m_image.gameObject.SetActive(hint.Image != null);
		m_videoPlayer.gameObject.SetActive(hint.VideoClip != null);
		m_buttonPromptContainer.SetActive(hint.InputAction != null);
		RectTransform component = m_text.GetComponent<RectTransform>();
		if (hint.InputAction != null)
		{
			m_buttonInputPrompt.SetInputAction(hint.InputAction);
			m_buttonPromptLabelText.text = hint.InputLabel;
			component.SetParent(m_textWithButtonContainer);
		}
		else
		{
			component.SetParent(m_textWithNoButtonContaienr);
		}
		component.anchoredPosition = Vector2.zero;
		component.sizeDelta = Vector2.zero;
		m_canvasGroup.gameObject.SetActive(value: true);
		m_canvasGroup.DOFade(1f, m_showAnimTime);
		if (!m_isSystemMenu)
		{
			m_continuePrompt.gameObject.SetActive(value: false);
			StartCoroutine(InputDelay());
		}
		if (hint.VideoClip != null)
		{
			PlayVideo(hint.VideoClip);
		}
	}

	private void PlayVideo(VideoClip videoClip)
	{
		if (m_videoRenderTexture != null)
		{
			Object.DestroyImmediate(m_videoRenderTexture);
		}
		m_videoRenderTexture = new RenderTexture((int)videoClip.width, (int)videoClip.height, 16);
		m_videoPlayerRawImaage.texture = m_videoRenderTexture;
		m_videoPlayer.clip = videoClip;
		m_videoPlayer.renderMode = VideoRenderMode.RenderTexture;
		m_videoPlayer.targetTexture = m_videoRenderTexture;
		m_videoPlayer.Play();
	}

	private IEnumerator InputDelay()
	{
		yield return new WaitForSecondsRealtime(m_enabledInputDelay);
		m_continuePrompt.gameObject.SetActive(value: true);
		GameInputManager.GameInputActions.UI.Submit.performed += OnDoneInput;
		GameInputManager.GameInputActions.UI.Click.performed += OnDoneInput;
	}

	private void OnDoneInput(InputAction.CallbackContext obj)
	{
		CloseHint();
	}

	public void CloseHint()
	{
		if (m_isShowing)
		{
			m_isShowing = false;
			m_gameMenuState.ClearInMenu(GameMenuState.GameMenu.PauseHint);
			m_canvasGroup.DOFade(0f, m_showAnimTime).OnComplete(PauntHintClosed);
			if (!m_isSystemMenu)
			{
				GameInputManager.GameInputActions.UI.Submit.performed -= OnDoneInput;
				GameInputManager.GameInputActions.UI.Click.performed -= OnDoneInput;
			}
		}
	}

	private void PauntHintClosed()
	{
		m_canvasGroup.gameObject.SetActive(value: false);
		if (m_videoPlayer.isPlaying)
		{
			m_videoPlayer.Stop();
		}
		if (m_videoRenderTexture != null)
		{
			Object.DestroyImmediate(m_videoRenderTexture);
			m_videoRenderTexture = null;
		}
	}

	public void OnCloseButtonPressed()
	{
		CloseHint();
	}
}
