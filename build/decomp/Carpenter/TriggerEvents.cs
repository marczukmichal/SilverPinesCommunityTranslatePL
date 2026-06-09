using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
[ShowInDesignerInspector]
[AddComponentMenu("Gameplay/Trigger Events (Collider 2D)")]
public class TriggerEvents : MonoBehaviour, IPersistentComponent
{
	private enum TargetType
	{
		Player,
		SpecificTransform
	}

	[Serializable]
	private class PersistentData
	{
		public bool m_triggered;
	}

	[SerializeField]
	private UnityEvent m_onTriggerEnter;

	[SerializeField]
	private UnityEvent m_onTriggerExit;

	[Header("Persistent Options")]
	[Tooltip("If true then this trigger will only fire when first triggered")]
	[SerializeField]
	private bool m_onlyOnce;

	[Header("If using 'Only Once' this event will retrigger when re-entering the room, should be used for all permanent effects")]
	[SerializeField]
	private UnityEvent m_onTriggerPersistent;

	[SerializeField]
	private float m_triggerDelayEnter;

	[SerializeField]
	private float m_triggerDelayExit = 0.1f;

	[SerializeField]
	private TargetType m_triggerTarget;

	[SerializeField]
	private Transform m_triggerTargetTransform;

	private float m_exitTimer;

	private GameObject m_trackedGameObject;

	private bool m_delayActive;

	private bool m_isTriggered;

	private Collider2D m_triggerCollider;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	private void Start()
	{
		m_triggerCollider = GetComponent<Collider2D>();
	}

	private void OnDisable()
	{
		if (m_isTriggered)
		{
			TriggerExit();
		}
	}

	private bool IsTargetTransform(Transform collidedTransform)
	{
		if (m_triggerTarget == TargetType.Player)
		{
			return GameUtils.IsPlayer(collidedTransform.root.gameObject);
		}
		return collidedTransform == m_triggerTargetTransform;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if ((bool)collision.GetComponent<DamageCollider>() || (m_onlyOnce && m_persistentData != null && m_persistentData.m_triggered) || m_delayActive || (bool)collision.gameObject.GetComponent<DetectableLightBeam>() || !IsTargetTransform(collision.gameObject.transform))
		{
			return;
		}
		m_exitTimer = 0f;
		if (!m_trackedGameObject)
		{
			if (m_triggerDelayEnter <= 0f)
			{
				Trigger();
			}
			else
			{
				StartCoroutine(DelayedTrigger());
			}
			if (m_triggerTarget == TargetType.Player)
			{
				m_trackedGameObject = collision.transform.root.gameObject;
			}
			else
			{
				m_trackedGameObject = collision.gameObject;
			}
		}
	}

	private IEnumerator DelayedTrigger()
	{
		m_delayActive = true;
		yield return new WaitForSeconds(m_triggerDelayEnter);
		Trigger();
		m_delayActive = false;
	}

	private void Trigger()
	{
		m_onTriggerEnter?.Invoke();
		if (m_onlyOnce)
		{
			if (m_persistentData != null)
			{
				m_persistentData.m_triggered = true;
			}
			else
			{
				Debug.LogError("Triggerevent " + base.name + " is marked as only once but has no PersistenDataIdentifier component, so this state cannot be saved! Please fix!", this);
			}
			m_onTriggerPersistent?.Invoke();
		}
		m_isTriggered = true;
	}

	private void TriggerExit()
	{
		m_exitTimer = 0f;
		m_trackedGameObject = null;
		m_onTriggerExit?.Invoke();
		m_isTriggered = false;
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (!collision.GetComponent<DamageCollider>() && IsTargetTransform(collision.transform))
		{
			m_exitTimer = Mathf.Max(m_triggerDelayExit, 0.01f);
		}
	}

	private void Update()
	{
		if (!(m_exitTimer > 0f))
		{
			return;
		}
		bool flag = true;
		if (m_trackedGameObject != null)
		{
			CharacterStance component = m_trackedGameObject.GetComponent<CharacterStance>();
			if (component != null && component.CurrentStance == CharacterStance.Stance.NoCollision)
			{
				Bounds standingBounds = component.GetStandingBounds();
				Vector3 center = standingBounds.center;
				center.y = standingBounds.max.y;
				Vector3 center2 = standingBounds.center;
				center2.y = standingBounds.min.x;
				if (m_triggerCollider.OverlapPoint(center) || m_triggerCollider.OverlapPoint(center2))
				{
					flag = false;
				}
			}
		}
		if (flag)
		{
			m_exitTimer -= Time.deltaTime;
			if (m_exitTimer <= 0f)
			{
				TriggerExit();
			}
		}
	}

	public bool RequiresPersistentData()
	{
		return m_onlyOnce;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData != null)
		{
			if (m_persistentData.m_triggered && m_onlyOnce)
			{
				m_onTriggerPersistent.Invoke();
			}
		}
		else
		{
			m_persistentData = new PersistentData();
			m_persistentDataObject.Data = m_persistentData;
		}
	}
}
