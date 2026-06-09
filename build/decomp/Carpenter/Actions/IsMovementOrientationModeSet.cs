using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class IsMovementOrientationModeSet : FsmStateAction
{
	protected LegacyCharacterMovement m_charMovement;

	public LegacyCharacterMovement.MovementOrientation m_orientation;

	public FsmEvent m_isSetEvent;

	public FsmEvent m_notSetEvent;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_charMovement = base.Owner.GetComponent<LegacyCharacterMovement>();
		}
	}

	public override void OnEnter()
	{
		if (m_charMovement.MovementOrientationMode == m_orientation)
		{
			base.Fsm.Event(m_isSetEvent);
		}
		else
		{
			base.Fsm.Event(m_notSetEvent);
		}
		Finish();
	}
}
