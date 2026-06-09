using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InteractPrompt : MonoBehaviour
{
	[SerializeField]
	private InteractableAnchor m_nearbyInteractableAnchor;

	[SerializeField]
	private GameObject m_toggleContainer;

	[SerializeField]
	private BoolGameEventChannel m_interactionEnabledEventChannel;

	[SerializeField]
	private BoolGameEventChannel m_shouldHideInteractPromptEventChannel;

	[SerializeField]
	private GameObjectAnchor m_playerAnchor;

	[SerializeField]
	private float m_screenGeneralHeightOffset;

	[SerializeField]
	private float m_animTime = 0.1f;

	[Header("Input Prompt")]
	[SerializeField]
	private InputPrompt m_inputPrompt;

	[SerializeField]
	private InputActionReference m_inputActionInteract;

	[SerializeField]
	private InputActionReference m_inputActionTransitionUp;

	[SerializeField]
	private InputActionReference m_inputActionTransitionDown;

	[Header("UI")]
	[SerializeField]
	private RectTransform m_parentRect;

	[SerializeField]
	private RectTransform m_rectTransform;

	[SerializeField]
	private CanvasGroup m_toggleCanvasGroup;

	[SerializeField]
	private TextMeshProUGUI m_promptText;

	[SerializeField]
	private Image m_interactIconImage;

	[SerializeField]
	private Canvas m_parentCanvas;

	[SerializeField]
	private CanvasGroup m_interactEnabledCanvasGroup;

	[Header("Interact Icons")]
	[SerializeField]
	private Sprite m_investigateIcon;

	[SerializeField]
	private Sprite m_transitionAwayFromCameraIcon;

	[SerializeField]
	private Sprite m_transitionTowardsCameraIcon;

	[Header("Animation")]
	[SerializeField]
	private float m_showTextStartTime = 1f;

	[SerializeField]
	private float m_showTextEndTime = 1.5f;

	private bool m_canShow;

	private BaseInteractable m_currentInteractable;

	private BaseInteractable m_previousInteractable;

	private Tween m_activeTween;

	private float m_inWorldPromptAlpha;

	private float m_timeOnPrompt;

	private void OnEnable()
	{
		m_nearbyInteractableAnchor.Register(OnUpdatePrompt);
		m_interactionEnabledEventChannel.Register(OnInteractionEnabledChanged);
		m_shouldHideInteractPromptEventChannel.Register(OnSetShowInteractPrompt);
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Combine(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
		GameState gameState = GlobalReferences.Instance.GameState;
		gameState.OnGameStateFlagChanged = (UnityAction<GameState.GameStateFlag>)Delegate.Combine(gameState.OnGameStateFlagChanged, new UnityAction<GameState.GameStateFlag>(OnGameStateChanged));
		m_canShow = GlobalReferences.Instance.GameState.IsInState(GameState.GameStateFlag.GameActive);
		m_toggleContainer.SetActive(value: false);
		OnUpdatePrompt(m_nearbyInteractableAnchor.Item);
	}

	private void OnDisable()
	{
		m_nearbyInteractableAnchor.Unregister(OnUpdatePrompt);
		m_interactionEnabledEventChannel.Unregister(OnInteractionEnabledChanged);
		m_shouldHideInteractPromptEventChannel.Unregister(OnSetShowInteractPrompt);
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Remove(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
		GameState gameState = GlobalReferences.Instance.GameState;
		gameState.OnGameStateFlagChanged = (UnityAction<GameState.GameStateFlag>)Delegate.Remove(gameState.OnGameStateFlagChanged, new UnityAction<GameState.GameStateFlag>(OnGameStateChanged));
	}

	private void OnInteractionEnabledChanged(bool enabled)
	{
		m_interactEnabledCanvasGroup.alpha = (enabled ? 1f : 0.4f);
	}

	private void Update()
	{
		if (!(Camera.main == null) && m_currentInteractable != null)
		{
			if (m_canShow)
			{
				UpdatePosition(m_currentInteractable);
			}
			m_timeOnPrompt += Time.deltaTime;
			float alpha = ((m_timeOnPrompt > m_showTextEndTime) ? 1f : ((!(m_timeOnPrompt > m_showTextStartTime)) ? 0f : Mathf.InverseLerp(m_showTextStartTime, m_showTextEndTime, m_timeOnPrompt)));
			m_promptText.alpha = alpha;
		}
	}

	private void UpdatePosition(BaseInteractable interactable)
	{
		if (!(Camera.main == null))
		{
			Vector3 interactPromptPosition = interactable.InteractPromptPosition;
			Vector3 vector = Camera.main.WorldToScreenPoint(interactPromptPosition);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(m_parentRect, vector, m_parentCanvas.worldCamera, out var localPoint);
			m_rectTransform.anchoredPosition = localPoint;
		}
	}

	private void OnUpdatePrompt(BaseInteractable interactable)
	{
		m_previousInteractable = m_currentInteractable;
		m_currentInteractable = interactable;
		RefreshPromptState();
		m_timeOnPrompt = 0f;
		m_promptText.alpha = 0f;
	}

	private bool CanShowInteractPrompt()
	{
		CharacterInteractor interactor = null;
		if (m_playerAnchor.Item != null)
		{
			interactor = m_playerAnchor.Item.GetComponent<CharacterInteractor>();
		}
		if (m_canShow && (bool)m_nearbyInteractableAnchor.Item && !m_nearbyInteractableAnchor.Item.ShouldAutoInteract(interactor) && m_nearbyInteractableAnchor.Item.ShouldShowInteractPrompt())
		{
			return !GlobalReferences.Instance.GameMenuState.IsInAnyMenu();
		}
		return false;
	}

	private void RefreshPromptState()
	{
		if (CanShowInteractPrompt())
		{
			if (m_currentInteractable != null)
			{
				switch (m_currentInteractable.GetInteractButonType())
				{
				case InteractButtonType.Normal:
					m_inputPrompt.SetInputAction(m_inputActionInteract);
					break;
				case InteractButtonType.TransitionDown:
					m_inputPrompt.SetInputAction(m_inputActionTransitionDown);
					break;
				case InteractButtonType.TransitionUp:
					m_inputPrompt.SetInputAction(m_inputActionTransitionUp);
					break;
				}
				Sprite sprite = null;
				switch (m_currentInteractable.GetInteractPromptIconType())
				{
				case InteractPromptIconType.Investigate:
					sprite = m_investigateIcon;
					break;
				case InteractPromptIconType.TransitionTowardsCamera:
					sprite = m_transitionTowardsCameraIcon;
					break;
				case InteractPromptIconType.TransitionAwayFromCamera:
					sprite = m_transitionAwayFromCameraIcon;
					break;
				}
				m_interactIconImage.sprite = sprite;
				m_interactIconImage.gameObject.SetActive(sprite != null);
				m_promptText.text = m_currentInteractable.InteractString;
				UpdatePosition(m_currentInteractable);
			}
			if (!m_toggleContainer.activeSelf || (m_activeTween != null && m_activeTween.IsActive()))
			{
				m_toggleContainer.SetActive(value: true);
				if (m_activeTween != null)
				{
					m_activeTween.Kill();
				}
				m_activeTween = m_toggleCanvasGroup.DOFade(1f, m_animTime);
			}
		}
		else if (m_toggleContainer.activeSelf)
		{
			if (m_activeTween != null)
			{
				m_activeTween.Kill();
			}
			m_activeTween = m_toggleCanvasGroup.DOFade(0f, m_animTime).OnComplete(delegate
			{
				m_toggleContainer.SetActive(value: false);
			});
		}
	}

	private void OnSetShowInteractPrompt(bool showPrompt)
	{
		m_canShow = showPrompt;
		RefreshPromptState();
	}

	private void OnGameMenuChanged(GameMenuState.GameMenu gameMenu)
	{
		RefreshPromptState();
	}

	private void OnGameStateChanged(GameState.GameStateFlag gameStateFlag)
	{
		m_canShow = gameStateFlag == GameState.GameStateFlag.GameActive;
	}
}
