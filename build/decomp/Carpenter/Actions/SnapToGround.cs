using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class SnapToGround : FsmStateAction
{
	private CharacterMovement m_movement;

	public override void Awake()
	{
		base.Awake();
		if (!(base.Owner == null))
		{
			m_movement = base.Owner.GetComponent<CharacterMovement>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_movement.SnapToGround();
		Finish();
	}
}
