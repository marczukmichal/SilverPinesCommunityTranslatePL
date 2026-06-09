using HutongGames.PlayMaker;

[ActionCategory(ActionCategory.Movement)]
public class SwimMovementAction : CharacterMovementAction
{
	private float m_submergedDepth = 1.425f;

	private float m_bouyancyScalar = 20f;

	private float m_outOfWaterGravity = -10f;

	private float m_dragCoefficient = 4f;

	protected override float Gravity
	{
		get
		{
			float num = 0f;
			GameplayWaterBounds activeWaterBound = GameplayWaterBounds.GetActiveWaterBound(base.Owner.transform.position, m_submergedDepth);
			if (activeWaterBound != null)
			{
				m_movement.SetActiveWaterBound(activeWaterBound);
				float num2 = activeWaterBound.GetWaterSurfaceHeight() - (base.Owner.transform.position.y + m_submergedDepth);
				float num3 = 0f;
				num3 = ((!(num2 > m_submergedDepth)) ? (num2 / m_submergedDepth) : 1f);
				num = num3 * m_bouyancyScalar;
			}
			else
			{
				num = m_outOfWaterGravity;
			}
			return num + m_movement.PreviousVelocity.y * (0f - m_dragCoefficient);
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		GameplayWaterBounds activeWaterBound = GameplayWaterBounds.GetActiveWaterBound(base.Owner.transform.position, m_submergedDepth);
		m_movement.SetActiveWaterBound(activeWaterBound);
	}

	public override void OnExit()
	{
		base.OnExit();
		m_movement.SetActiveWaterBound(null);
	}
}
