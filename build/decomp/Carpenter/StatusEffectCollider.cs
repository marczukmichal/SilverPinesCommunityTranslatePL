using System.Collections.Generic;
using UnityEngine;

public class StatusEffectCollider : MonoBehaviour
{
	public enum StatusEffectApplyType
	{
		Toggle,
		Ticks
	}

	[SerializeField]
	private StatusEffectApplyType m_applyType;

	[SerializeField]
	private StatusEffectDefinition m_effectDefinition;

	[Range(0.01f, 1f)]
	[SerializeField]
	private float m_timePerTick;

	[SerializeField]
	private int m_amount;

	private Dictionary<Collider2D, StatusEffectReceiver> m_presentStatusEffectReceivers;

	private float m_tickTimer;

	private List<Collider2D> m_removeColliders;

	private void Awake()
	{
		m_presentStatusEffectReceivers = new Dictionary<Collider2D, StatusEffectReceiver>();
		m_removeColliders = new List<Collider2D>();
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		StatusEffectReceiver componentInParent = collider.gameObject.GetComponentInParent<StatusEffectReceiver>();
		if (componentInParent != null)
		{
			if (m_applyType == StatusEffectApplyType.Toggle)
			{
				componentInParent.ApplyStatusEffect(m_effectDefinition, m_amount);
			}
			m_presentStatusEffectReceivers.Add(collider, componentInParent);
		}
	}

	private void OnDisable()
	{
		foreach (KeyValuePair<Collider2D, StatusEffectReceiver> presentStatusEffectReceiver in m_presentStatusEffectReceivers)
		{
			if (m_applyType == StatusEffectApplyType.Toggle)
			{
				presentStatusEffectReceiver.Value.ApplyStatusEffect(m_effectDefinition, -m_amount);
			}
		}
		m_presentStatusEffectReceivers.Clear();
	}

	private void OnTriggerExit2D(Collider2D collider)
	{
		if (m_presentStatusEffectReceivers.ContainsKey(collider))
		{
			RemoveCollider(collider);
		}
	}

	private void RemoveCollider(Collider2D collider)
	{
		StatusEffectReceiver statusEffectReceiver = m_presentStatusEffectReceivers[collider];
		if (m_applyType == StatusEffectApplyType.Toggle)
		{
			statusEffectReceiver.ApplyStatusEffect(m_effectDefinition, -m_amount);
		}
		m_presentStatusEffectReceivers.Remove(collider);
	}

	private void Update()
	{
		if (m_applyType == StatusEffectApplyType.Ticks)
		{
			m_tickTimer += Time.deltaTime;
			while (m_tickTimer > m_timePerTick)
			{
				m_tickTimer -= m_timePerTick;
				foreach (KeyValuePair<Collider2D, StatusEffectReceiver> presentStatusEffectReceiver in m_presentStatusEffectReceivers)
				{
					presentStatusEffectReceiver.Value.ApplyStatusEffect(m_effectDefinition, m_amount);
				}
			}
		}
		ValidateStillOverlapping();
	}

	private void ValidateStillOverlapping()
	{
		m_removeColliders.Clear();
		foreach (KeyValuePair<Collider2D, StatusEffectReceiver> presentStatusEffectReceiver in m_presentStatusEffectReceivers)
		{
			if (!presentStatusEffectReceiver.Key.isActiveAndEnabled)
			{
				m_removeColliders.Add(presentStatusEffectReceiver.Key);
			}
		}
		foreach (Collider2D removeCollider in m_removeColliders)
		{
			RemoveCollider(removeCollider);
		}
	}
}
