using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Misc/Phone Memory")]
public class PhoneMemory : ScriptableObject
{
	[SerializeField]
	private List<string> m_knownPhoneNumberIds = new List<string>();

	[SerializeField]
	private List<string> m_calledNumberIDs = new List<string>();

	[SerializeField]
	private List<string> m_previouslyDonePhoneEvents = new List<string>();

	[SerializeField]
	private string m_savePhoneEvent;

	[SerializeField]
	private PhoneBook m_phoneBook;

	public List<string> KnownPhoneNumbers
	{
		get
		{
			return m_knownPhoneNumberIds;
		}
		set
		{
			m_knownPhoneNumberIds = value;
		}
	}

	public List<string> CalledNumberIDs
	{
		get
		{
			return m_calledNumberIDs;
		}
		set
		{
			m_calledNumberIDs = value;
		}
	}

	public List<string> PreviouslyDonePhoneEvents
	{
		get
		{
			return m_previouslyDonePhoneEvents;
		}
		set
		{
			m_previouslyDonePhoneEvents = value;
		}
	}

	public string SavePhoneEvent
	{
		get
		{
			return m_savePhoneEvent;
		}
		set
		{
			m_savePhoneEvent = value;
		}
	}

	public void AddPhoneNumber(string phoneID)
	{
		if (!m_knownPhoneNumberIds.Contains(phoneID))
		{
			PhoneNumber numberDataFromID = m_phoneBook.GetNumberDataFromID(phoneID);
			if (numberDataFromID != null)
			{
				m_knownPhoneNumberIds.Add(phoneID);
				GlobalReferences.Instance.EventChannels.Phone.PhoneNumberAdded.Raise(numberDataFromID);
			}
			else
			{
				Debug.LogError("Tried to record a phone number for an invalid ID: " + phoneID);
			}
		}
	}

	public bool HasPhoneNumber(string phoneID)
	{
		return m_knownPhoneNumberIds.Contains(phoneID);
	}

	public bool HasCalledNumber(string phoneID)
	{
		return m_calledNumberIDs.Contains(phoneID);
	}

	public void RecordCalledNumber(string phoneID)
	{
		if (!m_calledNumberIDs.Contains(phoneID))
		{
			m_calledNumberIDs.Add(phoneID);
		}
	}

	public void RecordPreviouslyDonePhoneEvent(string name)
	{
		if (!m_previouslyDonePhoneEvents.Contains(name))
		{
			m_previouslyDonePhoneEvents.Add(name);
		}
	}

	public void SetSavePhoneEvent(string name)
	{
		m_savePhoneEvent = name;
	}

	public bool HasPreviouslyDonePhoneEvent(PhoneEvent phoneEvent)
	{
		return m_previouslyDonePhoneEvents.Contains(phoneEvent.name);
	}

	public bool IsSavePhoneEvent(PhoneEvent phoneEvent)
	{
		return phoneEvent.name == m_savePhoneEvent;
	}

	public void Clear()
	{
		m_savePhoneEvent = null;
		m_knownPhoneNumberIds.Clear();
		m_calledNumberIDs.Clear();
		m_previouslyDonePhoneEvents.Clear();
	}
}
