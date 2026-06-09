using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class InteractableAudioEventCommand : BaseInteractableCommand, IPersistentComponent
{
	[Serializable]
	private class PersistentData
	{
		public bool m_playedAudio;
	}

	[SerializeField]
	private AudioEvent m_audioEvent;

	[SerializeField]
	private bool m_onlyOnce;

	private bool m_finishedVoice;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		if (m_audioEvent == null)
		{
			Debug.LogWarning("InteractableAudioEventCommand is missing an audio event, don't do anything");
			yield return null;
		}
		bool flag = false;
		if (m_onlyOnce && m_persistentData != null && m_persistentData.m_playedAudio)
		{
			flag = true;
		}
		if (!flag)
		{
			m_audioEvent.Play(interactable.transform.position);
			if (m_onlyOnce)
			{
				if (m_persistentData != null)
				{
					m_persistentData.m_playedAudio = true;
				}
				else
				{
					Debug.LogWarning("InteractableAudioEventCommand is set to only once, but there is no persistent data component on parent object " + interactable.gameObject.name);
				}
			}
		}
		yield return null;
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
