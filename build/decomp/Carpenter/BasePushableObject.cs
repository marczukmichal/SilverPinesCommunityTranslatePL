using System;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public abstract class BasePushableObject : MonoBehaviour, IPersistentComponent
{
	[Serializable]
	private class PersistentData
	{
		public Vector3 m_position;

		public Quaternion m_rotation;

		public bool m_lockedInPlace;
	}

	[SerializeField]
	private EventReference m_pushingAudioFMODEvent;

	[SerializeField]
	private AnimationCurve m_pushVolumeSpeedCurve;

	private EventInstance m_activeFmodEvent;

	private List<Collider2D> m_colliders;

	protected bool m_lockedInPlace;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public bool LockedInPlace => m_lockedInPlace;

	public void LockInPlace(Transform targetTransform)
	{
		LockInPlace(targetTransform.position, targetTransform.rotation);
	}

	public void LockInPlace(Vector3 position, Quaternion rotation)
	{
		m_lockedInPlace = true;
		base.transform.position = position;
		base.transform.rotation = rotation;
		if (m_persistentData != null)
		{
			m_persistentData.m_position = position;
			m_persistentData.m_rotation = rotation;
		}
		SetMoveSpeed(0f);
	}

	protected virtual void Awake()
	{
		m_colliders = new List<Collider2D>(GetComponentsInChildren<Collider2D>());
	}

	public virtual bool CanBePushed()
	{
		if (m_lockedInPlace)
		{
			return false;
		}
		return true;
	}

	public bool ShouldIgnoreCollider(Collider2D collider)
	{
		if (collider.GetComponent<PlatformEffector2D>() != null)
		{
			return true;
		}
		return m_colliders.Contains(collider);
	}

	public abstract void SetMoveSpeed(float speed);

	public abstract void TrySlowDown(float basePushSpeed);

	public abstract void DisableMovement();

	public abstract float GetCurrentMoveSpeed();

	public virtual bool CheckForForcedExit(float maxAllowedVelocity)
	{
		return false;
	}

	private void Update()
	{
		if (m_persistentData != null)
		{
			m_persistentData.m_position = base.transform.position;
			m_persistentData.m_rotation = base.transform.rotation;
		}
		if (!m_pushingAudioFMODEvent.IsNull)
		{
			float time = Mathf.Abs(GetCurrentMoveSpeed());
			float num = m_pushVolumeSpeedCurve.Evaluate(time);
			bool flag = m_activeFmodEvent.isValid();
			if (!flag && num > 0.01f)
			{
				m_activeFmodEvent = RuntimeManager.CreateInstance(m_pushingAudioFMODEvent);
				m_activeFmodEvent.set3DAttributes(base.transform.position.To3DAttributes());
				m_activeFmodEvent.start();
			}
			else if (flag && num <= 0.01f)
			{
				m_activeFmodEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
				m_activeFmodEvent.release();
			}
			if (m_activeFmodEvent.isValid())
			{
				m_activeFmodEvent.set3DAttributes(base.transform.position.To3DAttributes());
				m_activeFmodEvent.setVolume(num);
			}
		}
	}

	public bool RequiresPersistentData()
	{
		return false;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData != null)
		{
			base.transform.position = m_persistentData.m_position;
			base.transform.rotation = m_persistentData.m_rotation;
			if (m_persistentData.m_lockedInPlace)
			{
				LockInPlace(m_persistentData.m_position, m_persistentData.m_rotation);
			}
		}
		else
		{
			m_persistentData = new PersistentData();
			m_persistentDataObject.Data = m_persistentData;
			m_persistentData.m_position = base.transform.position;
			m_persistentData.m_rotation = base.transform.rotation;
		}
	}
}
