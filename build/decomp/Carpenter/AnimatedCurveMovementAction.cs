using HutongGames.PlayMaker;
using UnityEngine;

[ActionCategory(ActionCategory.Movement)]
public class AnimatedCurveMovementAction : BaseCharacterHorizontalMovementAction
{
	public FsmAnimationCurve m_xInputCurve;

	public bool m_readInput;

	private float m_timer;

	public override void OnEnter()
	{
		base.OnEnter();
		m_timer = 0f;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		m_timer += Time.deltaTime;
	}

	protected override float CalculateXVelocity()
	{
		Vector2 vector = new Vector2(m_xInputCurve.curve.Evaluate(m_timer), 0f);
		if (m_direction.CurrentDirection == CharacterDirection.Facing.Left)
		{
			vector.x *= -1f;
		}
		if (m_readInput)
		{
			float num = m_input.MovementInput.x;
			if (m_direction.CurrentDirection == CharacterDirection.Facing.Left)
			{
				num = Mathf.Clamp(num, -1f, 0f);
				num *= -1f;
			}
			else if (m_direction.CurrentDirection == CharacterDirection.Facing.Right)
			{
				num = Mathf.Clamp(num, 0f, 1f);
			}
			vector.x *= num;
		}
		return (vector * GetMoveSpeed()).x;
	}
}
