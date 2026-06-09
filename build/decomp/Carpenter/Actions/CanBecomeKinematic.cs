using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class CanBecomeKinematic : FsmStateAction
{
	protected LegacyCharacterMovement m_charMovement;

	public bool m_canBecomeKinematic;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_charMovement = base.Owner.GetComponent<LegacyCharacterMovement>();
		}
	}

	public override void OnEnter()
	{
	}

	public override void OnExit()
	{
	}
}
