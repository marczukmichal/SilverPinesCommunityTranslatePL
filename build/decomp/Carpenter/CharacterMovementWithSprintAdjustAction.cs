using HutongGames.PlayMaker;
using PowerTools;

[ActionCategory(ActionCategory.Movement)]
public class CharacterMovementWithSprintAdjustAction : CharacterMovementAction
{
	public CharacterMovementSettings m_sprintMoveSpeed;

	public float m_sprintAnimSpeed = 1f;

	private SpriteAnim m_spriteAnim;

	public override void Awake()
	{
		base.Awake();
		if (base.Owner != null)
		{
			m_spriteAnim = base.Owner.GetComponent<SpriteAnim>();
		}
	}

	protected override float GetMoveSpeed()
	{
		if (m_input.IsSprinting)
		{
			return m_sprintMoveSpeed.MoveSpeed;
		}
		return base.GetMoveSpeed();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		m_spriteAnim.Speed = (m_input.IsSprinting ? m_sprintAnimSpeed : 1f);
	}
}
