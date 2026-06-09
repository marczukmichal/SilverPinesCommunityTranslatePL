using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class CanTargetedLeapDown : FsmStateAction
{
	protected CharacterTargetedLeap m_targetedLeap;

	public FsmEvent m_canLeapDown;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_targetedLeap = base.Owner.GetComponent<CharacterTargetedLeap>();
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (m_targetedLeap.CheckForLeapDown())
		{
			base.Fsm.Event(m_canLeapDown);
		}
	}
}
