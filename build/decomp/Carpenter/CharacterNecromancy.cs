using System;
using UnityEngine;
using UnityEngine.Events;

[ShowInDesignerInspector]
public class CharacterNecromancy : MonoBehaviour, IPersistentComponent
{
	public enum ReviveTimeScale
	{
		Never,
		Instant,
		Short,
		Medium,
		Long,
		VeryLong
	}

	[Serializable]
	private class PersistentData
	{
		public bool m_reviveTimerActive;

		public float m_reviveTimeStamp;

		public int m_reviveCount;

		public ReviveTimeScale m_reviveTimeScale;
	}

	[SerializeField]
	private ReviveTimeScale m_reviveTime;

	public static Vector2[] ReviveTimeSpansInMinutes = new Vector2[6]
	{
		Vector2.zero,
		new Vector2(0.1f, 0.2f),
		new Vector2(5f, 10f),
		new Vector2(10f, 30f),
		new Vector2(30f, 120f),
		new Vector2(120f, 360f)
	};

	[SerializeField]
	private int m_maxReviveCount = 1;

	private bool m_canRevive;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public ReviveTimeScale ReviveTime
	{
		get
		{
			if (m_persistentData != null)
			{
				return m_persistentData.m_reviveTimeScale;
			}
			return m_reviveTime;
		}
	}

	public bool CanRevive => m_canRevive;

	public bool ReviveTimerActive
	{
		get
		{
			if (m_persistentData != null)
			{
				return m_persistentData.m_reviveTimerActive;
			}
			return false;
		}
	}

	private void OnCharacterDying(DamageInstance damageInstance)
	{
		if (m_persistentData.m_reviveCount < m_maxReviveCount && m_persistentData != null)
		{
			m_persistentData.m_reviveTimerActive = true;
			m_persistentData.m_reviveTimeStamp = GetNewReviveTimeStamp();
		}
	}

	private float GetNewReviveTimeStamp()
	{
		int reviveTime = (int)ReviveTime;
		float num = UnityEngine.Random.Range(ReviveTimeSpansInMinutes[reviveTime].x, ReviveTimeSpansInMinutes[reviveTime].y) * 60f;
		return GetCurrentTime() + num;
	}

	private float GetCurrentTime()
	{
		return GlobalReferences.Instance.DataStore.Data.PlayTime;
	}

	public string GetDebugString()
	{
		if (m_persistentData == null)
		{
			return "";
		}
		if (m_canRevive)
		{
			return "Revive Allowed";
		}
		float num = m_persistentData.m_reviveTimeStamp - GetCurrentTime();
		if (num < 60f)
		{
			return num.ToString("F0") + " seconds";
		}
		return (num / 60f).ToString("F0") + " minutes";
	}

	private float GetDistanceFromPlayer()
	{
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (item != null)
		{
			return Vector2.Distance(base.transform.position, item.transform.position);
		}
		return float.MaxValue;
	}

	public void OnRevive()
	{
		m_canRevive = false;
		m_persistentData.m_reviveTimerActive = false;
		m_persistentData.m_reviveTimeStamp = 0f;
		m_persistentData.m_reviveCount++;
	}

	public bool RequiresPersistentData()
	{
		return true;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData != null)
		{
			if (m_persistentData.m_reviveTimerActive && GetCurrentTime() > m_persistentData.m_reviveTimeStamp)
			{
				m_canRevive = true;
			}
		}
		else
		{
			m_persistentData = new PersistentData();
			m_persistentData.m_reviveTimeScale = m_reviveTime;
			m_persistentDataObject.Data = m_persistentData;
		}
		if (ReviveTime != 0 && m_persistentData.m_reviveCount < m_maxReviveCount)
		{
			CharacterHealth component = GetComponent<CharacterHealth>();
			component.OnDying = (UnityAction<DamageInstance>)Delegate.Combine(component.OnDying, new UnityAction<DamageInstance>(OnCharacterDying));
		}
	}
}
