using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SubtitlesDisplay : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_text;

	[SerializeField]
	private TextMeshProUGUI m_speakerText;

	[SerializeField]
	private CharacterSpeakerSettings m_playerSpeakerSettings;

	[SerializeField]
	private RectTransform m_container;

	[SerializeField]
	private Color m_defaultColor;

	[Header("Positions")]
	[SerializeField]
	private RectTransform m_topPosition;

	[SerializeField]
	private RectTransform m_bottomPosition;

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.Audio.ShowVoiceSubtitles.Register(SubtitlesEvent);
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Combine(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Audio.ShowVoiceSubtitles.Unregister(SubtitlesEvent);
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Remove(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
	}

	private void OnGameMenuChanged(GameMenuState.GameMenu gameMenu)
	{
		RectTransform parent = ((gameMenu.HasFlag(GameMenuState.GameMenu.Examinable) || gameMenu.HasFlag(GameMenuState.GameMenu.ExaminableWithImage)) ? m_topPosition : m_bottomPosition);
		m_container.SetParent(parent, worldPositionStays: false);
	}

	private void Start()
	{
		m_container.gameObject.SetActive(value: false);
	}

	private void SubtitlesEvent(SubtitlesEventData subtitlesEvent)
	{
		if (subtitlesEvent.m_enable && GlobalReferences.Instance.UserPreferences.VOSubtitles)
		{
			m_text.text = subtitlesEvent.m_text;
			bool active = false;
			m_speakerText.gameObject.SetActive(active);
			m_text.color = m_defaultColor;
			m_container.gameObject.SetActive(value: true);
		}
		else
		{
			m_container.gameObject.SetActive(value: false);
		}
	}
}
