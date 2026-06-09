using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class InteractableVoicedAudioCommand : BaseInteractableCommand, IPersistentComponent
{
	[Serializable]
	private class PersistentData
	{
		public bool m_playedAudio;
	}

	[SerializeField]
	private AudioVoicedEvent m_voiceEvent;

	[SerializeField]
	private bool m_waitForCompletion = true;

	[SerializeField]
	private bool m_onlyOnce;

	[SerializeField]
	private float m_delay;

	private bool m_finishedVoice;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public static InteractableVoicedAudioCommand CreateInstance(AudioVoicedEvent audioEvent, bool waitForCompletion)
	{
		return new InteractableVoicedAudioCommand
		{
			m_voiceEvent = audioEvent,
			m_onlyOnce = false,
			m_waitForCompletion = waitForCompletion
		};
	}

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		if (m_voiceEvent == null)
		{
			Debug.LogWarning("InteractableVoicedAudioCommand is missing a voiced event, don't do anything");
			yield break;
		}
		bool flag = false;
		GameObject gameObject = interactor.gameObject;
		if (interactor is MinigameInteractor)
		{
			gameObject = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item.gameObject;
		}
		if (m_onlyOnce && m_persistentData != null && m_persistentData.m_playedAudio)
		{
			flag = true;
		}
		if (flag)
		{
			yield break;
		}
		gameObject.GetComponent<CharacterIdentifier>();
		bool isPlayer = GameUtils.IsPlayer(gameObject);
		GlobalReferences.Instance.EventChannels.Audio.PlayAudioVoiced.Raise(new PlayAudioVoicedEventData(m_voiceEvent, gameObject.transform, isPlayer, skippable: false, m_delay));
		if (m_onlyOnce)
		{
			if (m_persistentData != null)
			{
				m_persistentData.m_playedAudio = true;
			}
			else
			{
				Debug.LogWarning("InteractableVoicedAudioCommand is set to only once, but there is no persistent data component on parent object " + interactable.gameObject.name);
			}
		}
		if (m_waitForCompletion)
		{
			m_finishedVoice = false;
			GlobalReferences.Instance.EventChannels.Audio.OnAudioVoicedEventFinished.Register(OnVoiceEventDone);
			yield return new WaitUntil(IsFinishedVoiceLines);
		}
		OnExit();
	}

	private bool IsFinishedVoiceLines()
	{
		return m_finishedVoice;
	}

	private void OnVoiceEventDone()
	{
		m_finishedVoice = true;
	}

	private void OnExit()
	{
		GlobalReferences.Instance.EventChannels.Audio.OnAudioVoicedEventFinished.Unregister(OnVoiceEventDone);
	}

	public override void Cancel()
	{
		base.Cancel();
		OnExit();
	}

	public override bool RequiresPersistentData()
	{
		return m_onlyOnce;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData == null)
		{
			m_persistentData = new PersistentData();
			m_persistentDataObject.Data = m_persistentData;
		}
	}
}
