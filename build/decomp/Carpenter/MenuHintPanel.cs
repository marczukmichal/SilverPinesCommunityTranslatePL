using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

public class MenuHintPanel : MonoBehaviour
{
	[SerializeField]
	private GameObject m_container;

	[SerializeField]
	private Button m_button;

	[Header("UI")]
	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private TextMeshProUGUI m_text;

	[SerializeField]
	private TextMeshProUGUI m_titleText;

	[SerializeField]
	private Image m_image;

	[SerializeField]
	private GameObject m_continuePrompt;

	[SerializeField]
	private RectTransform m_textWithNoMediaContainer;

	[SerializeField]
	private RectTransform m_textWithMediaContainer;

	[SerializeField]
	private GameObject m_mediaContainer;

	[Header("Video")]
	[SerializeField]
	private VideoPlayer m_videoPlayer;

	[SerializeField]
	private RawImage m_videoPlayerRawImaage;

	[Header("Enable Input Delay")]
	[SerializeField]
	private float m_enabledInputDelay = 0.5f;

	[Header("Animation")]
	[SerializeField]
	private float m_animInTime = 0.5f;

	[SerializeField]
	private float m_animOutTime = 0.5f;

	public UnityAction OnClosedHint;

	private bool m_closeInputActive;

	private RenderTexture m_videoRenderTexture;

	public bool IsShowing => m_container.activeSelf;

	private void OnEnable()
	{
		if (IsShowing)
		{
			StartCoroutine(InputDelay());
		}
	}

	private void OnDisable()
	{
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.UI.Submit.performed -= OnDoneInput;
			GameInputManager.GameInputActions.UI.Click.performed -= OnDoneInput;
		}
	}

	public void Show(MenuHintInfo info)
	{
		DOTween.Kill(m_canvasGroup);
		m_container.SetActive(value: true);
		m_canvasGroup.alpha = 0f;
		m_text.SetText(info.HintText);
		m_titleText.text = info.TitleText;
		m_image.sprite = info.Image;
		m_image.gameObject.SetActive(info.Image != null);
		m_videoPlayer.gameObject.SetActive(info.VideoClip != null);
		RectTransform component = m_text.GetComponent<RectTransform>();
		if (info.Image == null && info.VideoClip == null)
		{
			component.SetParent(m_textWithNoMediaContainer);
			m_mediaContainer.SetActive(value: false);
		}
		else
		{
			component.SetParent(m_textWithMediaContainer);
			m_mediaContainer.SetActive(value: true);
		}
		component.anchoredPosition = Vector2.zero;
		component.sizeDelta = Vector2.zero;
		EventSystem.current.SetSelectedGameObject(m_button.gameObject);
		GameInputManager.GameInputActions.UI.Submit.performed += OnDoneInput;
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(InputDelay());
		}
		GlobalReferences.Instance.GameMenuState.SetInMenu(GameMenuState.GameMenu.MenuHint);
		if (info.VideoClip != null)
		{
			PlayVideo(info.VideoClip);
		}
		m_canvasGroup.DOFade(1f, m_animInTime);
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

	private void StopVideo()
	{
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

	private IEnumerator InputDelay()
	{
		m_closeInputActive = false;
		m_continuePrompt.gameObject.SetActive(value: false);
		yield return new WaitForSecondsRealtime(m_enabledInputDelay);
		m_continuePrompt.gameObject.SetActive(value: true);
		m_closeInputActive = true;
		GameInputManager.GameInputActions.UI.Submit.performed += OnDoneInput;
		GameInputManager.GameInputActions.UI.Click.performed += OnDoneInput;
	}

	private void Update()
	{
		if (base.isActiveAndEnabled && m_closeInputActive && IsShowing)
		{
			EventSystem.current.SetSelectedGameObject(m_button.gameObject);
		}
	}

	private void OnDoneInput(InputAction.CallbackContext obj)
	{
		TryHide();
	}

	public void TryHide()
	{
		if (m_closeInputActive)
		{
			Hide();
		}
	}

	public void Hide()
	{
		m_closeInputActive = false;
		StopVideo();
		GameInputManager.GameInputActions.UI.Submit.performed -= OnDoneInput;
		GameInputManager.GameInputActions.UI.Click.performed -= OnDoneInput;
		GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.MenuHint);
		m_canvasGroup.DOFade(0f, m_animOutTime).OnComplete(OnHintClosed);
	}

	private void OnHintClosed()
	{
		m_container.gameObject.SetActive(value: false);
		OnClosedHint?.Invoke();
	}
}
