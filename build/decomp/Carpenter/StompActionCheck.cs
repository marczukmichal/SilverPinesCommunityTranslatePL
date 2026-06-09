using System.Collections.Generic;
using UnityEngine;

public class StompActionCheck : MonoBehaviour
{
	[SerializeField]
	private Collider2D m_collider;

	private bool m_stompEnabled;

	private bool m_hasStompTarget;

	private bool m_forceAllowStomp;

	private Vector3 m_promptPosition;

	public bool StompEnabled
	{
		get
		{
			if (!m_stompEnabled)
			{
				return m_forceAllowStomp;
			}
			return true;
		}
		set
		{
			m_stompEnabled = value;
		}
	}

	public bool HasStompTarget => m_hasStompTarget;

	public bool CanStomp => true;

	public bool ForceAllowStomp
	{
		get
		{
			return m_forceAllowStomp;
		}
		set
		{
			m_forceAllowStomp = value;
		}
	}

	public Vector3 PromptPosition => m_promptPosition;

	private void FixedUpdate()
	{
		if (ForceAllowStomp)
		{
			m_promptPosition = base.transform.position;
			m_promptPosition.y += 1f;
			return;
		}
		bool hasStompTarget = false;
		ContactFilter2D contactFilter = default(ContactFilter2D);
		contactFilter.SetLayerMask(GameLayers.ProjectileMask);
		List<Collider2D> list = new List<Collider2D>();
		int num = m_collider.Overlap(contactFilter, list);
		for (int i = 0; i < num; i++)
		{
			Collider2D collider2D = list[i];
			if (GameUtils.IsPlayer(collider2D.gameObject))
			{
				continue;
			}
			CharacterIdentifier componentInParent = collider2D.GetComponentInParent<CharacterIdentifier>();
			if (!(componentInParent != null))
			{
				continue;
			}
			bool flag = false;
			CharacterHealth component = componentInParent.GetComponent<CharacterHealth>();
			if (component != null)
			{
				flag = component.IsDead;
			}
			if (!flag)
			{
				StatusEffectReceiver component2 = componentInParent.GetComponent<StatusEffectReceiver>();
				if (component2 != null && component2.IsStatusEffectActive(GlobalReferences.Instance.StatusEffects.Generic.Knockdown))
				{
					hasStompTarget = true;
					Vector3 position = componentInParent.transform.position;
					position.x = base.transform.position.x;
					position.y -= 0.25f;
					m_promptPosition = position;
					break;
				}
			}
		}
		m_hasStompTarget = hasStompTarget;
	}
}
