using System;
using UnityEngine;
using UnityEngine.Events;

public class ShootableLootContainer : MonoBehaviour, IDamageable, IPersistentComponent
{
	private enum State
	{
		Idle,
		WaitTime,
		WaitForStopped,
		Stopped
	}

	[Serializable]
	private class PersistentData
	{
		public bool m_shot;

		public Vector3 m_restingPosition;

		public Quaternion m_restingRotation;
	}

	[SerializeField]
	private Rigidbody2D m_rigidbody2D;

	[SerializeField]
	private Interactable m_interactable;

	[SerializeField]
	private bool m_projectilesOnly;

	[Header("Disable on Shot")]
	[SerializeField]
	private GameObject m_hitBoxCollider;

	[Header("Events")]
	[SerializeField]
	private UnityEvent m_onShotEvent;

	[SerializeField]
	private UnityEvent m_onAlreadyShotReloadedEvent;

	private State m_state;

	private float m_timer;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public void ApplyDamageInstance(DamageInstance instance)
	{
		if (instance.HealthDamageAmount > 0 && (!m_projectilesOnly || instance.DamageCategory == DamageCategory.Projectile))
		{
			OnShot();
		}
	}

	private void OnShot()
	{
		if (m_interactable == null)
		{
			base.gameObject.SetActive(value: false);
		}
		else if (m_state == State.Idle)
		{
			m_state = State.WaitTime;
			m_timer = 0f;
			if (m_hitBoxCollider != null)
			{
				m_hitBoxCollider.SetActive(value: false);
			}
			m_rigidbody2D.transform.SetParent(null);
			m_rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
			m_rigidbody2D.simulated = true;
			m_onShotEvent.Invoke();
		}
	}

	private void Update()
	{
		switch (m_state)
		{
		case State.WaitTime:
			m_timer += Time.deltaTime;
			if (m_timer > 0.5f)
			{
				m_state = State.WaitForStopped;
			}
			break;
		case State.WaitForStopped:
			if (m_rigidbody2D.linearVelocity.magnitude < 0.01f && Mathf.Abs(m_rigidbody2D.angularVelocity) < 0.1f)
			{
				m_rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
				m_state = State.Stopped;
				EnableInteract();
				if (m_persistentData != null)
				{
					m_persistentData.m_shot = true;
					m_persistentData.m_restingPosition = m_rigidbody2D.transform.position;
					m_persistentData.m_restingRotation = m_rigidbody2D.transform.rotation;
				}
			}
			break;
		}
	}

	private void EnableInteract()
	{
		m_interactable.enabled = true;
		base.gameObject.layer = GameLayers.InteractableLayer;
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
			if (!m_persistentData.m_shot)
			{
				return;
			}
			if (m_interactable == null)
			{
				base.gameObject.SetActive(value: false);
				return;
			}
			EnableInteract();
			m_rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
			m_state = State.Stopped;
			if (m_hitBoxCollider != null)
			{
				m_hitBoxCollider.SetActive(value: false);
			}
			m_rigidbody2D.transform.SetParent(null);
			m_rigidbody2D.transform.position = m_persistentData.m_restingPosition;
			m_rigidbody2D.transform.rotation = m_persistentData.m_restingRotation;
			m_onAlreadyShotReloadedEvent.Invoke();
		}
		else
		{
			m_persistentData = new PersistentData();
			m_persistentDataObject.Data = m_persistentData;
		}
	}
}
