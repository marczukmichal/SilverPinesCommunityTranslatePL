using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class HasMovementCollided : FsmStateAction
{
	[Tooltip("Event to trigger on collided with wall")]
	public FsmEvent m_onCollidedWithWall;

	private CharacterMovement m_movement;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_movement = base.Owner.GetComponent<CharacterMovement>();
		}
	}

	public override void OnUpdate()
	{
		Check();
	}

	private void Check()
	{
		if (m_movement.IsGrounded && (double)m_movement.PreviousVelocity.magnitude <= 0.1 && (m_movement.m_collisions.m_right || m_movement.m_collisions.m_left))
		{
			base.Fsm.Event(m_onCollidedWithWall);
		}
	}
}
