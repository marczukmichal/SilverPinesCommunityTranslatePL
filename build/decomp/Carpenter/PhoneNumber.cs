using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Serialization;

[Serializable]
public class PhoneNumber
{
	public string m_id;

	[SerializeField]
	private LocalizedString m_numberNameStringReference;

	public string m_number;

	[FormerlySerializedAs("m_dialogues")]
	public List<PhoneEvent> m_events;

	[FormerlySerializedAs("m_visible")]
	public bool m_startsKnown;

	public bool m_shouldSaveGame;

	public bool m_canBeAddedToPhoneBook;

	public string PhoneName
	{
		get
		{
			if (m_numberNameStringReference == null || m_numberNameStringReference.IsEmpty)
			{
				return m_id;
			}
			return m_numberNameStringReference.GetLocalizedString();
		}
	}

	public PhoneEvent GetBestPhoneEvent(PhoneMemory memory, PhoneFlags phoneFlags)
	{
		List<PhoneEvent> list = new List<PhoneEvent>();
		List<PhoneEvent> list2 = new List<PhoneEvent>();
		List<PhoneEvent> list3 = new List<PhoneEvent>();
		PhoneEvent phoneEvent = null;
		LevelRegionSettings activePlayerRegion = null;
		LevelMetadata item = GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Item;
		if (item != null)
		{
			activePlayerRegion = item.Region;
		}
		foreach (PhoneEvent @event in m_events)
		{
			if (!@event.IsValid(activePlayerRegion, phoneFlags))
			{
				continue;
			}
			if (@event.m_isOverrideEvent)
			{
				list.Add(@event);
			}
			else if (memory.IsSavePhoneEvent(@event) && !@event.m_onlyOnce)
			{
				phoneEvent = @event;
			}
			else if (memory.HasPreviouslyDonePhoneEvent(@event))
			{
				if (!@event.m_onlyOnce)
				{
					list3.Add(@event);
				}
			}
			else
			{
				list2.Add(@event);
			}
		}
		if (list.Count > 0)
		{
			return list[UnityEngine.Random.Range(0, list.Count)];
		}
		if (list2.Count > 0)
		{
			list2.Sort((PhoneEvent a, PhoneEvent b) => a.m_priority.CompareTo(b.m_priority));
			return list2[list2.Count - 1];
		}
		if (list3.Count > 0)
		{
			return list3[UnityEngine.Random.Range(0, list3.Count)];
		}
		if (phoneEvent != null)
		{
			return phoneEvent;
		}
		return null;
	}

	public PhoneEvent GetSavePhoneEvent(PhoneMemory memory)
	{
		foreach (PhoneEvent @event in m_events)
		{
			if (memory.IsSavePhoneEvent(@event))
			{
				return @event;
			}
		}
		return null;
	}
}
