using System;
using UnityEngine;

[DisallowMultipleComponent]
[ShowInDesignerInspector]
public class PhoneMinigameData : MonoBehaviour, IPersistentComponent
{
	[Serializable]
	private class PersistentData
	{
		public int m_coins;

		public bool m_takenCoins;

		public float m_lastCoinTakenTime;
	}

	[SerializeField]
	private int m_initialCoinSlotCoins;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public int CoinSlotCoins
	{
		get
		{
			if (m_persistentData == null)
			{
				return 0;
			}
			return m_persistentData.m_coins;
		}
	}

	public void RemoveCoins()
	{
		if (m_persistentData != null)
		{
			m_persistentData.m_coins = 0;
			m_persistentData.m_takenCoins = true;
			m_persistentData.m_lastCoinTakenTime = GlobalReferences.Instance.DataStore.Data.PlayTime;
		}
	}

	public bool HasTakenCoins()
	{
		if (m_persistentData != null)
		{
			return m_persistentData.m_takenCoins;
		}
		return false;
	}

	public bool HasRecentlyTakenCoin()
	{
		if (m_persistentData != null)
		{
			if (m_persistentData.m_lastCoinTakenTime <= 0f)
			{
				return false;
			}
			return GlobalReferences.Instance.DataStore.Data.PlayTime - m_persistentData.m_lastCoinTakenTime <= 300f;
		}
		return true;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData == null)
		{
			m_persistentData = new PersistentData();
			m_persistentDataObject.Data = m_persistentData;
			m_persistentData.m_coins = m_initialCoinSlotCoins;
		}
	}

	public bool RequiresPersistentData()
	{
		return true;
	}
}
