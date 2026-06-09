using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Status Effects")]
public class StatusEffectApply : FsmStateAction
{
	public StatusEffectDefinition m_definition;

	public int m_amount;

	public bool m_onUpdate;

	public float m_applicationRate = 0.2f;

	protected StatusEffectReceiver m_statusEffectReceiver;

	private float m_timer;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_statusEffectReceiver = base.Owner.GetComponent<StatusEffectReceiver>();
		}
	}

	public override void OnEnter()
	{
		m_timer = 0f;
		ApplyStatusEffect();
		if (!m_onUpdate)
		{
			Finish();
		}
	}

	private void ApplyStatusEffect()
	{
		m_statusEffectReceiver.ApplyStatusEffect(m_definition, m_amount);
	}

	public override void OnUpdate()
	{
		if (m_onUpdate)
		{
			m_timer += Time.deltaTime;
			if (m_timer >= m_applicationRate)
			{
				ApplyStatusEffect();
				m_timer = 0f;
			}
		}
	}
}
