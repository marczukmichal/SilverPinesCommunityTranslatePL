using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Combat")]
public class SetDamageReceivedMulitplier : FsmStateAction
{
	public float m_multiplier;

	private CharacterHealth m_health;

	public override void Awake()
	{
		base.Awake();
		if (base.Owner != null)
		{
			m_health = base.Owner.GetComponent<CharacterHealth>();
		}
	}

	public override void OnEnter()
	{
		m_health.SetDamageReceivedScalar(m_multiplier);
	}

	public override void OnExit()
	{
		m_health.SetDamageReceivedScalar(1f);
	}
}
