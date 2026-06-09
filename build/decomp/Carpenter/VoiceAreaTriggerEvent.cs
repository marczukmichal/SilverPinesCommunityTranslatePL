using System;
using UnityEngine;

[DisallowMultipleComponent]
public class VoiceAreaTriggerEvent : MonoBehaviour, IPersistentComponent
{
	[Serializable]
	private class PersistentData
	{
		public bool m_triggered;
	}

	[SerializeField]
	private AudioVoicedEvent m_voiceLineEvent;

	private PersistentDataObject m_persistentData;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		Transform root = collision.gameObject.transform.root;
		if (GameUtils.IsPlayer(root.gameObject))
		{
			if (m_persistentData != null)
			{
				m_persistentData.Data = new PersistentData
				{
					m_triggered = true
				};
			}
			if (m_voiceLineEvent != null)
			{
				GlobalReferences.Instance.EventChannels.Audio.PlayAudioVoiced.Raise(new PlayAudioVoicedEventData(m_voiceLineEvent, root.transform, isPlayer: true));
			}
			else
			{
				Debug.LogError("Voice line area trigger event (" + base.gameObject.name + ") has no voice line set!");
			}
			base.gameObject.SetActive(value: false);
		}
	}

	public bool RequiresPersistentData()
	{
		return true;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentData = dataEntry;
		PersistentData persistentData = ((m_persistentData.Data != null) ? (m_persistentData.Data as PersistentData) : null);
		if (persistentData != null && persistentData.m_triggered)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
