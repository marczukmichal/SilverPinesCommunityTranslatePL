using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Combat")]
public class ActivateDamageBlock : FsmStateAction
{
	[SerializeField]
	public bool m_onlyTouchDamage;

	[SerializeField]
	public bool m_deactivateOnly;

	protected DamageBlock m_damageBlock;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_damageBlock = base.Owner.GetComponent<DamageBlock>();
		}
	}

	public override void OnEnter()
	{
		if (!m_deactivateOnly)
		{
			m_damageBlock.ActivateDamageBlock(m_onlyTouchDamage);
		}
	}

	public override void OnExit()
	{
		m_damageBlock.DeactivateDamageBlock(m_onlyTouchDamage);
	}
}
