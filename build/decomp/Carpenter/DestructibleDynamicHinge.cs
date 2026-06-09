using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class DestructibleDynamicHinge : MonoBehaviour, IDamageable, IPersistentComponent
{
	private enum DestructibleHingeState
	{
		Normal,
		Hanging,
		Destroyed
	}

	[Serializable]
	private class PersistentData
	{
		public DestructibleHingeState m_hingeState;

		public int m_hingeIndex;
	}

	[SerializeField]
	private HingeJoint2D m_hingeJoint;

	[SerializeField]
	private Rigidbody2D m_rigidbody;

	[SerializeField]
	private SimpleHealth m_simpleHealth;

	[SerializeField]
	private Vector2[] m_hingePositions;

	[SerializeField]
	private float m_minimumTimeToHang = 1f;

	[Header("Spawned Effect")]
	[SerializeField]
	private AssetReferenceGameObject m_breakHingeEffectAsset;

	[SerializeField]
	private AssetReferenceGameObject m_detachEffectAsset;

	private float m_startHangTime;

	private DestructibleHingeState m_state;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	private void Reset()
	{
		m_hingeJoint = GetComponentInChildren<HingeJoint2D>();
		m_rigidbody = GetComponentInChildren<Rigidbody2D>();
		m_simpleHealth = GetComponent<SimpleHealth>();
	}

	public void ApplyDamageInstance(DamageInstance instance)
	{
		switch (m_state)
		{
		case DestructibleHingeState.Normal:
			GoToHangingHingeFromHit(instance.Position);
			break;
		case DestructibleHingeState.Hanging:
			if (Time.time > m_startHangTime + m_minimumTimeToHang && m_simpleHealth.IsDead)
			{
				Detach();
			}
			break;
		}
	}

	private int GetFurthestHingeIndex(Vector2 position)
	{
		int result = -1;
		float num = 0f;
		for (int i = 0; i < m_hingePositions.Length; i++)
		{
			float num2 = Vector2.Distance(position, m_hingePositions[i]);
			if (num2 > num)
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	private void GoToHangingHingeFromHit(Vector2 hitPosition)
	{
		m_state = DestructibleHingeState.Hanging;
		int furthestHingeIndex = GetFurthestHingeIndex(base.transform.InverseTransformPoint(hitPosition));
		m_rigidbody.bodyType = RigidbodyType2D.Dynamic;
		m_hingeJoint.anchor = m_hingePositions[furthestHingeIndex];
		m_startHangTime = Time.time;
		if (m_persistentData != null)
		{
			m_persistentData.m_hingeState = m_state;
			m_persistentData.m_hingeIndex = furthestHingeIndex;
		}
		if (m_breakHingeEffectAsset != null && m_breakHingeEffectAsset.HasAsset())
		{
			Vector3 position = base.transform.TransformPoint(m_hingeJoint.anchor);
			position.z = m_hingeJoint.transform.position.z;
			DynamicallySpawnedObject.Spawn(m_breakHingeEffectAsset, persistent: false, position);
		}
	}

	private void Detach()
	{
		m_state = DestructibleHingeState.Destroyed;
		m_hingeJoint.enabled = false;
		if (m_persistentData != null)
		{
			m_persistentData.m_hingeState = m_state;
		}
		if (m_detachEffectAsset != null && m_detachEffectAsset.HasAsset())
		{
			Vector3 position = base.transform.TransformPoint(m_hingeJoint.anchor);
			position.z = m_hingeJoint.transform.position.z;
			DynamicallySpawnedObject.Spawn(m_detachEffectAsset, persistent: false, position);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		if (m_hingePositions != null)
		{
			Vector2[] hingePositions = m_hingePositions;
			foreach (Vector2 vector in hingePositions)
			{
				Gizmos.DrawSphere(base.transform.TransformPoint(vector), 0.1f);
			}
		}
		Gizmos.color = Color.white;
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
			switch (m_persistentData.m_hingeState)
			{
			case DestructibleHingeState.Hanging:
				m_state = DestructibleHingeState.Hanging;
				m_rigidbody.bodyType = RigidbodyType2D.Dynamic;
				m_hingeJoint.anchor = m_hingePositions[m_persistentData.m_hingeIndex];
				break;
			case DestructibleHingeState.Destroyed:
				base.gameObject.SetActive(value: false);
				break;
			}
		}
		else
		{
			m_persistentData = new PersistentData();
			m_persistentDataObject.Data = m_persistentData;
		}
	}
}
