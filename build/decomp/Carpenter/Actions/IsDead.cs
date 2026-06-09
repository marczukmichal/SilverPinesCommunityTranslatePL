using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class IsDead : FsmStateAction
{
	[Tooltip("Event to trigger if dead")]
	public FsmEvent m_onDeadEvent;

	[Tooltip("Event to trigger if not dead")]
	public FsmEvent m_onNotDeadEvent;

	private CharacterHealth m_characterHealth;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterHealth = base.Owner.GetComponent<CharacterHealth>();
		}
	}

	public override void OnEnter()
	{
		Check();
	}

	public override void OnUpdate()
	{
		Check();
	}

	private void Check()
	{
		if (m_onDeadEvent != null && m_characterHealth.IsDead)
		{
			base.Fsm.Event(m_onDeadEvent);
		}
		if (m_onNotDeadEvent != null && !m_characterHealth.IsDead)
		{
			base.Fsm.Event(m_onNotDeadEvent);
		}
	}
}
