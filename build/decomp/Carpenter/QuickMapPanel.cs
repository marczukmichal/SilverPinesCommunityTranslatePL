using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuickMapPanel : MonoBehaviour
{
	[Header("UI")]
	[SerializeField]
	private UIMap3DPanel m_mapViewer;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[Header("Animation")]
	[SerializeField]
	private float m_fadeInSpeed = 5f;

	[SerializeField]
	private float m_fadeOutSpeed = 2f;

	[Header("Inputs")]
	[SerializeField]
	private MenuInputPrompts m_inputPrompts;

	[SerializeField]
	private MenuInputPrompts.AvaialbleInput[] m_availableInputs;

	private bool m_isShowing;

	private bool m_shouldShow;

	private void OnEnable()
	{
		m_inputPrompts.SetInputs(m_availableInputs);
		GlobalReferences.Instance.EventChannels.Map.ShowQuickMap.Register(SetShowShowMap);
		m_canvasGroup.alpha = 0f;
		m_canvasGroup.gameObject.SetActive(value: false);
	}

	private void OnDisable()
	{
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.Player.Interact.performed -= OpenFullMap;
		}
		GlobalReferences.Instance.EventChannels.Map.ShowQuickMap.Unregister(SetShowShowMap);
	}

	private void SetShowShowMap(bool showing)
	{
		m_shouldShow = showing;
	}

	public void Update()
	{
		if (m_isShowing != m_shouldShow)
		{
			OnQuickMapShowingStateChanged(m_shouldShow);
		}
	}

	private void OnQuickMapShowingStateChanged(bool showing)
	{
		m_isShowing = showing;
		m_mapViewer.MapViewActive = m_isShowing;
		DOTween.Kill(m_canvasGroup);
		if (m_isShowing)
		{
			m_mapViewer.PopulateMap(isQuickMap: true, focusOnPlayer: true);
			m_canvasGroup.gameObject.SetActive(value: true);
			m_canvasGroup.DOFade(1f, m_fadeInSpeed).SetSpeedBased(isSpeedBased: true);
			GameInputManager.GameInputActions.Player.Interact.performed += OpenFullMap;
		}
		else
		{
			GameInputManager.GameInputActions.Player.Interact.performed -= OpenFullMap;
			m_canvasGroup.DOFade(0f, m_fadeOutSpeed).SetSpeedBased(isSpeedBased: true).OnComplete(delegate
			{
				m_canvasGroup.gameObject.SetActive(value: false);
			});
		}
	}

	private void OpenFullMap(InputAction.CallbackContext obj)
	{
		OnQuickMapShowingStateChanged(showing: false);
		GlobalReferences.Instance.EventChannels.InGameMenu.ShowMapMenuTab.Raise();
	}
}
