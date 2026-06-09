using DG.Tweening;
using UnityEngine;

public class CinematicBarsUI : MonoBehaviour
{
	[SerializeField]
	private GameObjectGameEventChannel m_nearSceneTransitionEventChannel;

	[SerializeField]
	private BoolGameEventChannel m_inPhoneDialogueEventChannel;

	[SerializeField]
	private BoolGameEventChannel m_isPlayingVideoCutsceneEventChannel;

	[SerializeField]
	private BoolGameEventChannel m_isPlayingInGameCutsceneEventChannel;

	[SerializeField]
	private BoolGameEventChannel m_fadeScreenEvent;

	[SerializeField]
	private RectTransform m_topBar;

	[SerializeField]
	private RectTransform m_bottomBar;

	[SerializeField]
	private float m_animTime = 0.2f;

	private bool m_isShown;

	private bool m_isNearTransition;

	private bool m_isInDialogue;

	private bool m_isPlayingVideoCutscene;

	private bool m_isPlayingInGameCutscene;

	private bool m_isFadeActive;

	private void Awake()
	{
		m_isShown = false;
		m_topBar.pivot = new Vector2(0.5f, 0f);
		m_bottomBar.pivot = new Vector2(0.5f, 1f);
	}

	private void OnEnable()
	{
		m_nearSceneTransitionEventChannel.Register(SetNearTransition);
		m_inPhoneDialogueEventChannel.Register(SetInDialogue);
		m_isPlayingVideoCutsceneEventChannel.Register(SetPlayingVideoCutscene);
		m_isPlayingInGameCutsceneEventChannel.Register(SetPlayingInGameCutscene);
		m_fadeScreenEvent.Register(SetScreenFading);
	}

	private void OnDisable()
	{
		m_nearSceneTransitionEventChannel.Unregister(SetNearTransition);
		m_inPhoneDialogueEventChannel.Unregister(SetInDialogue);
		m_isPlayingVideoCutsceneEventChannel.Unregister(SetPlayingVideoCutscene);
		m_isPlayingInGameCutsceneEventChannel.Unregister(SetPlayingInGameCutscene);
		m_fadeScreenEvent.Unregister(SetScreenFading);
	}

	private void SetInDialogue(bool inDialogue)
	{
		m_isInDialogue = inDialogue;
		SetShown(ShouldShow());
	}

	private void SetNearTransition(GameObject nearbyLevelMetataTransition)
	{
		m_isNearTransition = nearbyLevelMetataTransition != null;
		SetShown(ShouldShow());
	}

	private void SetPlayingVideoCutscene(bool isPlaying)
	{
		m_isPlayingVideoCutscene = isPlaying;
		SetShown(ShouldShow());
	}

	private void SetPlayingInGameCutscene(bool isPlaying)
	{
		m_isPlayingInGameCutscene = isPlaying;
		SetShown(ShouldShow());
	}

	private void SetScreenFading(bool isFading)
	{
		m_isFadeActive = isFading;
		SetShown(ShouldShow());
	}

	private bool ShouldShow()
	{
		if (!m_isInDialogue && !m_isNearTransition && !m_isPlayingVideoCutscene)
		{
			return m_isPlayingInGameCutscene;
		}
		return true;
	}

	private void SetShown(bool shown)
	{
		float duration = (m_isPlayingInGameCutscene ? 0f : m_animTime);
		if (m_isShown != shown && !m_isFadeActive)
		{
			m_isShown = shown;
			if (m_isShown)
			{
				m_topBar.DOPivotY(1f, duration).SetEase(Ease.OutQuad);
				m_bottomBar.DOPivotY(0f, duration).SetEase(Ease.OutQuad);
			}
			else
			{
				m_topBar.DOPivotY(0f, duration).SetEase(Ease.OutQuad);
				m_bottomBar.DOPivotY(1f, duration).SetEase(Ease.OutQuad);
			}
		}
	}
}
