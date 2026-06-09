using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class SetMovementBlockingType : FsmStateAction
{
	public CharacterMovement.MovementBlockingType m_blockingType;

	public bool m_resetOnExit;

	protected CharacterMovement m_charMovement;

	private CharacterMovement.MovementBlockingType m_previousBlockingType;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_charMovement = base.Owner.GetComponent<CharacterMovement>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_previousBlockingType = m_charMovement.BlockingType;
		m_charMovement.BlockingType = m_blockingType;
		Finish();
	}

	public override void OnExit()
	{
		base.OnExit();
		if (m_resetOnExit)
		{
			m_charMovement.BlockingType = m_previousBlockingType;
		}
	}
}
