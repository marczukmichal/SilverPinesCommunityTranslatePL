using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Combat")]
public class TakeDamage : FsmStateAction
{
	[SerializeField]
	public int m_damageAmount;

	private IDamageable[] m_damageables;

	private DamageBlock m_damageBlock;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_damageables = base.Owner.GetComponentsInChildren<IDamageable>();
			m_damageBlock = base.Owner.GetComponent<DamageBlock>();
		}
	}

	public override void OnEnter()
	{
		DamageInstance damageInstance = new DamageInstance().SetHealthDamage(m_damageAmount).SetDamageSource(base.Owner).SetImpactType(ImpactType.Medium)
			.SetPosition(base.Owner.transform.position);
		if (!m_damageBlock || m_damageBlock.CanTakeDamage(damageInstance))
		{
			IDamageable[] damageables = m_damageables;
			for (int i = 0; i < damageables.Length; i++)
			{
				damageables[i].ApplyDamageInstance(damageInstance);
			}
			Finish();
		}
	}
}
