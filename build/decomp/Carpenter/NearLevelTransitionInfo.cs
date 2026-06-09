using DG.Tweening;
using TMPro;
using UnityEngine;

public class NearLevelTransitionInfo : MonoBehaviour
{
	public enum NearLevelTransitionInfoMode
	{
		Horizontal,
		Up,
		Down
	}

	[SerializeField]
	private GameObjectGameEventChannel m_nearbyTransitionObjectEventChannel;

	[SerializeField]
	private TextMeshProUGUI m_text;

	[SerializeField]
	private float m_animTime;

	[SerializeField]
	private float m_showDelayTime;

	[SerializeField]
	private NearLevelTransitionInfoMode m_mode;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	private LevelMetadata m_targetLevel;

	private Tween m_activeTween;

	private void Start()
	{
		m_canvasGroup.gameObject.SetActive(value: false);
	}

	private void OnEnable()
	{
		m_nearbyTransitionObjectEventChannel.Register(NearbyTransitionEvent);
	}

	private void OnDisable()
	{
		m_nearbyTransitionObjectEventChannel.Unregister(NearbyTransitionEvent);
	}

	private bool IsValidForTransitionDirection(LevelTransition.LeaveDirection direction)
	{
		switch (direction)
		{
		case LevelTransition.LeaveDirection.ToLeft:
		case LevelTransition.LeaveDirection.ToRight:
			return m_mode == NearLevelTransitionInfoMode.Horizontal;
		case LevelTransition.LeaveDirection.AwayFromCamera:
			return m_mode == NearLevelTransitionInfoMode.Up;
		case LevelTransition.LeaveDirection.TowardsCamera:
			return m_mode == NearLevelTransitionInfoMode.Down;
		default:
			return false;
		}
	}

	private void NearbyTransitionEvent(GameObject transitionObject)
	{
		LevelTransition levelTransition = (transitionObject ? transitionObject.GetComponent<LevelTransition>() : null);
		if (levelTransition == null || levelTransition.TargetLevel == null || !IsValidForTransitionDirection(levelTransition.Direction))
		{
			Hide();
			return;
		}
		m_targetLevel = levelTransition.TargetLevel;
		Show();
	}

	private void Show()
	{
		if (m_activeTween != null)
		{
			m_activeTween.Kill();
		}
		m_canvasGroup.gameObject.SetActive(value: true);
		m_activeTween = m_canvasGroup.DOFade(1f, m_animTime).SetDelay(m_showDelayTime);
		m_text.text = m_targetLevel.DisplayName;
	}

	private void Hide()
	{
		if (m_activeTween != null)
		{
			m_activeTween.Kill();
		}
		m_activeTween = m_canvasGroup.DOFade(0f, m_animTime).OnComplete(delegate
		{
			m_canvasGroup.gameObject.SetActive(value: false);
		});
	}
}
