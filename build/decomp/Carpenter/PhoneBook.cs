using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Misc/Phonebook")]
public class PhoneBook : ScriptableObject
{
	[SerializeField]
	private List<PhoneNumber> m_phoneNumbers;

	[SerializeField]
	private Dialogue m_invalidPhoneCallDialogue;

	[SerializeField]
	private PhoneMemory m_phoneMemory;

	public Dialogue InvalidCallDialogue => m_invalidPhoneCallDialogue;

	public PhoneNumber GetNumberDataFromNumber(string number)
	{
		foreach (PhoneNumber phoneNumber in m_phoneNumbers)
		{
			if (phoneNumber.m_number.Equals(number))
			{
				return phoneNumber;
			}
		}
		return null;
	}

	public PhoneNumber GetNumberDataFromID(string id)
	{
		foreach (PhoneNumber phoneNumber in m_phoneNumbers)
		{
			if (phoneNumber.m_id.Equals(id))
			{
				return phoneNumber;
			}
		}
		return null;
	}

	public bool IsValidNumber(string number)
	{
		return GetNumberDataFromNumber(number) != null;
	}

	public List<PhoneNumber> GetKnownNumbers()
	{
		List<PhoneNumber> list = new List<PhoneNumber>();
		foreach (PhoneNumber phoneNumber in m_phoneNumbers)
		{
			if (phoneNumber.m_startsKnown || m_phoneMemory.HasPhoneNumber(phoneNumber.m_id))
			{
				list.Add(phoneNumber);
			}
		}
		return list;
	}

	public PhoneNumber GetSaveNumber()
	{
		foreach (PhoneNumber phoneNumber in m_phoneNumbers)
		{
			if (phoneNumber.m_shouldSaveGame)
			{
				return phoneNumber;
			}
		}
		return null;
	}

	public PhoneEvent GetSavePhoneEvent(PhoneNumber number)
	{
		return number.GetSavePhoneEvent(m_phoneMemory);
	}

	public void RecordCalledNumber(PhoneNumber number)
	{
		if (number.m_canBeAddedToPhoneBook)
		{
			m_phoneMemory.AddPhoneNumber(number.m_id);
		}
		m_phoneMemory.RecordCalledNumber(number.m_id);
	}

	public bool HasCalledPhoneNumber(PhoneNumber number)
	{
		if (m_phoneMemory.HasCalledNumber(number.m_id))
		{
			return true;
		}
		foreach (PhoneEvent @event in number.m_events)
		{
			if (m_phoneMemory.HasPreviouslyDonePhoneEvent(@event))
			{
				return true;
			}
		}
		return false;
	}

	public void RecordHeardPhoneEvent(PhoneEvent phoneEvent)
	{
		m_phoneMemory.RecordPreviouslyDonePhoneEvent(phoneEvent.name);
	}

	public void SetPhoneSaveEvent(PhoneEvent phoneEvent)
	{
		m_phoneMemory.SetSavePhoneEvent(phoneEvent.name);
	}

	public PhoneEvent GetBestPhoneEvent(PhoneNumber number, PhoneFlags phoneFlags)
	{
		return number.GetBestPhoneEvent(m_phoneMemory, phoneFlags);
	}
}
