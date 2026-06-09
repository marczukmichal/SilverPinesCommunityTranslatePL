using System;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;

public class AudioManager : MonoBehaviour
{
	private enum OcclusionType
	{
		None,
		BehindDoor,
		BehindWall,
		AboveFloor,
		BelowFloor,
		VeryFarAway,
		SameRoom
	}

	private VCA m_masterVCA;

	private VCA m_ambienceVCA;

	private VCA m_musicVCA;

	private VCA m_effectsVCA;

	private VCA m_voicesVCA;

	private PlayAudioVoicedEventData m_activeVoiceLines;

	private int m_activeVoiceLinesIndex;

	private EventInstance m_activeVoiceEventInstance;

	private float m_playNextVoiceLineTimer;

	private bool m_isVoiceLinePlaying;

	private bool m_isDialoguePlaying;

	private ClipSubtitleEntry m_activeSubtitlesEntry;

	private Queue<PlayAudioVoicedEventData> m_queuedEvents;

	private List<Bus> m_pauseBusses;

	private void Start()
	{
		m_queuedEvents = new Queue<PlayAudioVoicedEventData>();
		m_pauseBusses = new List<Bus>();
		m_masterVCA = RuntimeManager.GetVCA("vca:/Master Volume");
		m_ambienceVCA = RuntimeManager.GetVCA("vca:/Ambience Volume");
		m_musicVCA = RuntimeManager.GetVCA("vca:/Music Volume");
		m_effectsVCA = RuntimeManager.GetVCA("vca:/Sound Effects Volume");
		m_voicesVCA = RuntimeManager.GetVCA("vca:/VO");
		m_pauseBusses.Add(RuntimeManager.GetBus("bus:/PlayerVo"));
		m_pauseBusses.Add(RuntimeManager.GetBus("bus:/Sound Effects"));
		m_pauseBusses.Add(RuntimeManager.GetBus("bus:/VO"));
		SetBussesPaused(paused: false);
		SetVCAVolumeLevels();
	}

	private void SetVCAVolumeLevels()
	{
		UserPreferences userPreferences = GlobalReferences.Instance.UserPreferences;
		m_masterVCA.setVolume(userPreferences.MasterVolume);
		m_ambienceVCA.setVolume(userPreferences.AmbientVolume);
		m_musicVCA.setVolume(userPreferences.MusicVolume);
		m_effectsVCA.setVolume(userPreferences.EffectsVolume);
		m_voicesVCA.setVolume(userPreferences.VoiceVolume);
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.Audio.PlayAudio.Register(PlayAudioEvent);
		GlobalReferences.Instance.EventChannels.Audio.PlayAudioVoiced.Register(PlayVoiceLineEvent);
		GlobalReferences.Instance.EventChannels.Audio.UpdateAudioVolumeLevelsFromUserPreferences.Register(SetVCAVolumeLevels);
		GlobalReferences.Instance.EventChannels.Audio.StopAllVoicedEvents.Register(StopActiveVoiceOver);
		GlobalReferences.Instance.EventChannels.Dialogue.Dialogue.Register(PlayDialogueEvent);
		GlobalReferences.Instance.EventChannels.Comic.ToggleComicViewActive.Register(OnComicViewChanged);
		GlobalReferences.Instance.EventChannels.Audio.PauseGameAudioBusses.Register(SetBussesPaused);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Audio.PlayAudio.Unregister(PlayAudioEvent);
		GlobalReferences.Instance.EventChannels.Audio.PlayAudioVoiced.Unregister(PlayVoiceLineEvent);
		GlobalReferences.Instance.EventChannels.Audio.UpdateAudioVolumeLevelsFromUserPreferences.Unregister(SetVCAVolumeLevels);
		GlobalReferences.Instance.EventChannels.Audio.StopAllVoicedEvents.Unregister(StopActiveVoiceOver);
		GlobalReferences.Instance.EventChannels.Dialogue.Dialogue.Unregister(PlayDialogueEvent);
		GlobalReferences.Instance.EventChannels.Comic.ToggleComicViewActive.Unregister(OnComicViewChanged);
		GlobalReferences.Instance.EventChannels.Audio.PauseGameAudioBusses.Unregister(SetBussesPaused);
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.Game.Proceed.performed -= OnProceedInputPerformed;
		}
	}

	private void SetBussesPaused(bool paused)
	{
		foreach (Bus pauseBuss in m_pauseBusses)
		{
			pauseBuss.setPaused(paused);
		}
	}

	private void OnComicViewChanged(bool comicActive)
	{
		if (comicActive)
		{
			StopActiveVoiceOver();
		}
	}

	private void PlayAudioEvent(PlayAudioEventData playAudioEvent)
	{
		EventInstance eventInstance = PlayAudioEvent(playAudioEvent.m_audioEvent, playAudioEvent.m_position, playAudioEvent.m_volume, playAudioEvent.m_useOcclusion, playAudioEvent.m_parameterName, playAudioEvent.m_parameterLabel, playAudioEvent.m_parameterValue);
		if (eventInstance.isValid())
		{
			eventInstance.release();
		}
	}

	private static string GetOcclusionParameterString(OcclusionType type)
	{
		return type switch
		{
			OcclusionType.BehindDoor => "BehindDoor", 
			OcclusionType.BehindWall => "BehindWall", 
			OcclusionType.AboveFloor => "AboveFloor", 
			OcclusionType.BelowFloor => "BelowFloor", 
			OcclusionType.VeryFarAway => "VeryFarAway", 
			OcclusionType.SameRoom => "SameRoom", 
			_ => "None", 
		};
	}

	public static EventInstance PlayAudioEvent(AudioEvent audioEvent, Vector3 position, float volume, bool useOcclusion, string parameterName = null, string parameterLabel = null, float parameterValue = 0f)
	{
		if (!audioEvent.FMODEvent.IsNull)
		{
			if (!Application.isPlaying)
			{
				return default(EventInstance);
			}
			EventInstance result = RuntimeManager.CreateInstance(audioEvent.FMODEvent);
			result.set3DAttributes(position.To3DAttributes());
			result.setVolume(volume);
			if (parameterName != null)
			{
				if (!string.IsNullOrEmpty(parameterLabel))
				{
					result.setParameterByNameWithLabel(parameterName, parameterLabel);
				}
				else
				{
					result.setParameterByName(parameterName, parameterValue);
				}
			}
			if (useOcclusion)
			{
				GameObject item = GlobalReferences.Instance.Anchors.Generic.AudioListenerAnchor.Item;
				if (item != null)
				{
					Vector2 vector = item.transform.position;
					Vector2 vector2 = position;
					float num = Vector2.Distance(vector, vector2);
					OcclusionType type = OcclusionType.SameRoom;
					if (num > 2f)
					{
						Vector2 normalized = (vector2 - vector).normalized;
						RaycastHit2D[] array = Physics2D.RaycastAll(vector, normalized, num - 2f, GameLayers.EnvironmentMask);
						for (int i = 0; i < array.Length; i++)
						{
							RaycastHit2D raycastHit2D = array[i];
							OcclusionType occlusionType = OcclusionType.None;
							if (raycastHit2D.collider != null)
							{
								float num2 = vector2.y - vector.y;
								NewSideDoor componentInParent = raycastHit2D.collider.gameObject.GetComponentInParent<NewSideDoor>();
								if (!(componentInParent != null))
								{
									occlusionType = ((num2 > 4f) ? OcclusionType.AboveFloor : ((!(num2 < -4f)) ? OcclusionType.BehindWall : OcclusionType.BelowFloor));
								}
								else
								{
									if (!componentInParent.ShouldOccludeAudio)
									{
										continue;
									}
									occlusionType = OcclusionType.BehindDoor;
								}
							}
							if (occlusionType != 0)
							{
								type = occlusionType;
								break;
							}
						}
					}
					else
					{
						type = OcclusionType.SameRoom;
					}
					result.setParameterByNameWithLabel("Occlusion", GetOcclusionParameterString(type));
				}
			}
			result.start();
			return result;
		}
		return default(EventInstance);
	}

	private void PlayVoiceLineEvent(PlayAudioVoicedEventData playVoiceLineEvent)
	{
		if (playVoiceLineEvent.m_voiceLineEvent == null)
		{
			Debug.LogWarning("Tried to play a null voice line!");
		}
		else
		{
			PlayVoiceLineEvent(playVoiceLineEvent, fromQueue: false);
		}
	}

	private bool AlreadyInQueue(AudioVoicedEvent audioEvent)
	{
		foreach (PlayAudioVoicedEventData queuedEvent in m_queuedEvents)
		{
			if (queuedEvent.m_voiceLineEvent == audioEvent)
			{
				return true;
			}
		}
		return false;
	}

	private void PlayVoiceLineEvent(PlayAudioVoicedEventData playVoiceLineEvent, bool fromQueue)
	{
		if (playVoiceLineEvent.m_voiceLineEvent == null)
		{
			Debug.LogWarning("Tried to play a null voice line!");
			return;
		}
		if ((m_activeVoiceLines != null && m_activeVoiceLines.m_voiceLineEvent == playVoiceLineEvent.m_voiceLineEvent) || AlreadyInQueue(playVoiceLineEvent.m_voiceLineEvent))
		{
			Debug.Log("Skip adding voice line to queue as it is already being played or in the queue", this);
			return;
		}
		if (playVoiceLineEvent.m_delay > 0f && !fromQueue)
		{
			if (m_activeVoiceLines == null)
			{
				m_playNextVoiceLineTimer = playVoiceLineEvent.m_delay;
			}
			m_queuedEvents.Enqueue(playVoiceLineEvent);
			return;
		}
		if (m_activeVoiceLines != null || m_isDialoguePlaying)
		{
			Debug.Log("Had to queue a voice over line existing voice line " + playVoiceLineEvent.m_voiceLineEvent?.ToString() + " while line " + m_activeVoiceLines.m_voiceLineEvent.name + " is active.", this);
			m_queuedEvents.Enqueue(playVoiceLineEvent);
			return;
		}
		m_activeVoiceLines = playVoiceLineEvent;
		m_activeVoiceLinesIndex = 0;
		Transform transform = playVoiceLineEvent.m_transform;
		if (transform == null && playVoiceLineEvent.m_isPlayer)
		{
			transform = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item.transform;
		}
		PlayVoiceLineEvent(playVoiceLineEvent.m_voiceLineEvent.VoiceLines[m_activeVoiceLinesIndex], playVoiceLineEvent.m_speakerSettings, transform);
		if (playVoiceLineEvent.m_skippable)
		{
			GameInputManager.GameInputActions.Game.Proceed.performed += OnProceedInputPerformed;
			GlobalReferences.Instance.GameMenuState.SetInMenu(GameMenuState.GameMenu.Dialogue);
		}
	}

	private void PlayVoiceLineEvent(AudioVoicedEvent.VoiceLine voiceLine, CharacterSpeakerSettings speakerSettings, Transform speakerTransform)
	{
		if (!voiceLine.FMODEvent.IsNull)
		{
			m_activeVoiceEventInstance = RuntimeManager.CreateInstance(voiceLine.FMODEvent);
			if (speakerTransform != null)
			{
				RuntimeManager.AttachInstanceToGameObject(m_activeVoiceEventInstance, speakerTransform.gameObject);
			}
			m_activeVoiceEventInstance.start();
			m_isVoiceLinePlaying = true;
		}
		else
		{
			Debug.LogWarning("Missing fmod audio event for voice line " + voiceLine.Subtitles);
			m_playNextVoiceLineTimer = 3f;
		}
	}

	private void PlayDialogueEvent(DialogueEventData dialogue)
	{
		if (dialogue.m_show)
		{
			if (dialogue.m_dialogueLine != null && !dialogue.m_dialogueLine.VoiceLineClip.IsNull)
			{
				m_isDialoguePlaying = true;
				StopActiveVoiceOver();
				m_activeVoiceEventInstance = RuntimeManager.CreateInstance(dialogue.m_dialogueLine.VoiceLineClip);
				if (dialogue.m_speakerGameObject != null)
				{
					RuntimeManager.AttachInstanceToGameObject(m_activeVoiceEventInstance, dialogue.m_speakerGameObject);
				}
				m_activeVoiceEventInstance.start();
			}
		}
		else
		{
			m_isDialoguePlaying = false;
			StopActiveVoiceOver();
		}
	}

	private void StopActiveVoiceOver()
	{
		if (m_activeVoiceEventInstance.isValid())
		{
			m_activeVoiceEventInstance.getPlaybackState(out var state);
			if (state == PLAYBACK_STATE.PLAYING)
			{
				m_activeVoiceEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
				m_activeVoiceEventInstance.release();
			}
		}
		if (m_isVoiceLinePlaying)
		{
			m_isVoiceLinePlaying = false;
		}
		OnAudioVoicedEventFinished();
		m_queuedEvents.Clear();
	}

	private void OnAudioVoicedEventFinished()
	{
		if (m_activeVoiceLines != null && m_activeVoiceLines.m_skippable)
		{
			GameInputManager.GameInputActions.Game.Proceed.performed -= OnProceedInputPerformed;
			GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.Dialogue);
		}
		m_activeVoiceLines = null;
		GlobalReferences.Instance.EventChannels.Audio.ShowVoiceSubtitles.Raise(new SubtitlesEventData
		{
			m_enable = false
		});
		GlobalReferences.Instance.EventChannels.Audio.OnAudioVoicedEventFinished.Raise();
	}

	private void Update()
	{
		if (m_isDialoguePlaying)
		{
			return;
		}
		if (m_activeVoiceLines != null && m_isVoiceLinePlaying && m_activeVoiceEventInstance.isValid())
		{
			m_activeVoiceEventInstance.getPlaybackState(out var state);
			if (state == PLAYBACK_STATE.STOPPED)
			{
				m_isVoiceLinePlaying = false;
				m_playNextVoiceLineTimer = 0.25f;
			}
			m_activeVoiceEventInstance.getTimelinePosition(out var position);
			double totalSeconds = TimeSpan.FromMilliseconds(position).TotalSeconds;
			ClipSubtitleEntry entryForTimestamp = m_activeVoiceLines.m_voiceLineEvent.VoiceLines[m_activeVoiceLinesIndex].Subtitles.GetEntryForTimestamp((float)totalSeconds);
			if (m_activeSubtitlesEntry != entryForTimestamp)
			{
				if (entryForTimestamp != null)
				{
					GlobalReferences.Instance.EventChannels.Audio.ShowVoiceSubtitles.Raise(new SubtitlesEventData
					{
						m_enable = true,
						m_speakerSettings = entryForTimestamp.m_speaker,
						m_text = entryForTimestamp.SubtitlesText
					});
				}
				else
				{
					GlobalReferences.Instance.EventChannels.Audio.ShowVoiceSubtitles.Raise(new SubtitlesEventData
					{
						m_enable = false
					});
				}
			}
			m_activeSubtitlesEntry = entryForTimestamp;
		}
		if (m_activeVoiceLines != null)
		{
			if (m_isVoiceLinePlaying)
			{
				return;
			}
			m_playNextVoiceLineTimer -= Time.unscaledDeltaTime;
			if (!(m_playNextVoiceLineTimer <= 0f))
			{
				return;
			}
			if (m_activeVoiceLinesIndex + 1 < m_activeVoiceLines.m_voiceLineEvent.VoiceLines.Length)
			{
				PlayVoiceLineEvent(m_activeVoiceLines.m_voiceLineEvent.VoiceLines[++m_activeVoiceLinesIndex], m_activeVoiceLines.m_speakerSettings, m_activeVoiceLines.m_transform);
				return;
			}
			OnAudioVoicedEventFinished();
			if (m_queuedEvents.Count > 0)
			{
				PlayVoiceLineEvent(m_queuedEvents.Dequeue(), fromQueue: true);
			}
		}
		else if (m_queuedEvents.Count > 0)
		{
			m_playNextVoiceLineTimer -= Time.unscaledDeltaTime;
			if (m_playNextVoiceLineTimer <= 0f)
			{
				PlayVoiceLineEvent(m_queuedEvents.Dequeue(), fromQueue: true);
			}
		}
	}

	private void SkipActiveVoiceLine()
	{
		if (m_isVoiceLinePlaying)
		{
			if (m_activeVoiceEventInstance.isValid())
			{
				m_activeVoiceEventInstance.getPlaybackState(out var state);
				if (state == PLAYBACK_STATE.PLAYING)
				{
					m_activeVoiceEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
					m_activeVoiceEventInstance.release();
				}
			}
			m_isVoiceLinePlaying = false;
		}
		m_playNextVoiceLineTimer = 0f;
	}

	private void OnProceedInputPerformed(InputAction.CallbackContext context)
	{
		SkipActiveVoiceLine();
	}
}
