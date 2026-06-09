using UnityEngine;

[RequireComponent(typeof(AISenses))]
public class AISimpleAttackStrategy : BaseAIStrategy
{
	private Vector2 m_moveDirection;

	private bool m_shouldAttack;

	[SerializeField]
	private float m_predictionLookAheadTime = 0.6f;

	[SerializeField]
	private AISenses m_senses;

	[SerializeField]
	private float m_attackDistance;

	[SerializeField]
	private bool m_moveWhileAttacking;

	private CharacterDirection m_characterDirection;

	public override Vector2 MoveDirection => m_moveDirection;

	public override bool ShouldAttack => m_shouldAttack;

	public override bool ShouldSprint => true;

	private void Reset()
	{
		m_senses = GetComponent<AISenses>();
	}

	private void Start()
	{
		m_characterDirection = GetComponent<CharacterDirection>();
	}

	private bool IsFacingTarget()
	{
		if (m_characterDirection == null)
		{
			return true;
		}
		Vector2 vector = m_senses.CurrentTarget.transform.position;
		Vector2 vector2 = base.transform.position;
		if (m_characterDirection.CurrentDirection == CharacterDirection.Facing.Right)
		{
			return vector2.x < vector.x;
		}
		return vector2.x > vector.x;
	}

	public override void UpdateStrategy()
	{
		if (!m_senses)
		{
			return;
		}
		m_moveDirection = Vector2.zero;
		m_shouldAttack = false;
		Detectable currentTarget = m_senses.CurrentTarget;
		if (!(currentTarget != null))
		{
			return;
		}
		Vector2 a;
		Vector2 vector = (a = currentTarget.transform.position);
		Rigidbody2D component = currentTarget.GetComponent<Rigidbody2D>();
		if (component != null)
		{
			a += component.linearVelocity * m_predictionLookAheadTime;
		}
		Vector2 vector2 = base.transform.position;
		Vector2 vector3 = vector - vector2;
		if (Vector2.Distance(a, vector2) >= m_attackDistance || !IsFacingTarget())
		{
			m_moveDirection = vector3.normalized;
			return;
		}
		m_shouldAttack = true;
		if (m_moveWhileAttacking)
		{
			m_moveDirection = vector3.normalized;
		}
	}
}
