using HutongGames.PlayMaker;

[ActionCategory(ActionCategory.Character)]
public class GetMovementHealthState : FsmStateAction
{
	public bool m_everyFrame;

	private CharacterHealth m_characterHealth;

	private StatusEffectReceiver m_statusEffectReceiver;

	public FsmEvent m_normalEvent;

	public FsmEvent m_injuredEvent;

	public FsmEvent m_criticalEvent;

	private static readonly float m_injuredHealthLevel = 0.525f;

	private static readonly float m_criticalHealthLevel = 0.275f;

	public override void Awake()
	{
		base.Awake();
		if (base.Owner != null)
		{
			m_characterHealth = base.Owner.GetComponent<CharacterHealth>();
			m_statusEffectReceiver = base.Owner.GetComponent<StatusEffectReceiver>();
		}
	}

	public override void OnEnter()
	{
		CheckValues();
		if (!m_everyFrame)
		{
			Finish();
		}
	}

	public override void OnUpdate()
	{
		if (m_everyFrame)
		{
			CheckValues();
		}
	}

	private void CheckValues()
	{
		float healthPercentage = m_characterHealth.HealthPercentage;
		if (healthPercentage <= m_criticalHealthLevel && (!(m_statusEffectReceiver != null) || !m_statusEffectReceiver.IsPreventingCriticalHealthMovement()))
		{
			base.Fsm.Event(m_criticalEvent);
		}
		else if (healthPercentage <= m_injuredHealthLevel)
		{
			base.Fsm.Event(m_injuredEvent);
		}
		else
		{
			base.Fsm.Event(m_normalEvent);
		}
	}
}
