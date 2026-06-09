using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class SetMovementOrientationMode : FsmStateAction
{
	protected LegacyCharacterMovement m_charMovement;

	public LegacyCharacterMovement.MovementOrientation m_orientation;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_charMovement = base.Owner.GetComponent<LegacyCharacterMovement>();
		}
	}

	public override void OnEnter()
	{
		m_charMovement.MovementOrientationMode = m_orientation;
		Finish();
	}
}
