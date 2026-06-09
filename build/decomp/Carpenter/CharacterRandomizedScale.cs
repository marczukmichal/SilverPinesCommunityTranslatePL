using System;
using UnityEngine;

public class CharacterRandomizedScale : MonoBehaviour, IPersistentComponent
{
	[Serializable]
	private class PersistentData
	{
		public float m_scale;
	}

	[SerializeField]
	private float m_minScale = 1f;

	[SerializeField]
	private float m_maxScale = 1f;

	private float m_scale = -1f;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	private void Start()
	{
		if (m_scale <= 0f)
		{
			GenerateRandomScale();
		}
	}

	private void GenerateRandomScale()
	{
		ApplyScale(UnityEngine.Random.Range(m_minScale, m_maxScale));
	}

	private void ApplyScale(float scale)
	{
		m_scale = scale;
		base.transform.localScale = Vector3.one * m_scale;
		if (m_persistentData != null)
		{
			m_persistentData.m_scale = scale;
		}
	}

	public bool RequiresPersistentData()
	{
		return true;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		if (base.enabled)
		{
			m_persistentDataObject = dataEntry;
			m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
			if (m_persistentData != null)
			{
				ApplyScale(m_persistentData.m_scale);
				return;
			}
			m_persistentData = new PersistentData();
			m_persistentDataObject.Data = m_persistentData;
			GenerateRandomScale();
			m_persistentData.m_scale = m_scale;
		}
	}
}
