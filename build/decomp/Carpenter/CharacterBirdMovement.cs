using UnityEngine;

public class CharacterBirdMovement : MonoBehaviour
{
	private enum BirdState
	{
		None,
		HoverLeft,
		HoverRight,
		AttackPrepare,
		AttackStart,
		AttackActive,
		AttackExit,
		Dissipate
	}

	[SerializeField]
	private Transform m_target;

	[Header("Speed")]
	[SerializeField]
	private float m_normalSpeed = 5f;

	[SerializeField]
	private float m_attackStartSpeed = 3f;

	[SerializeField]
	private float m_attackSpeed = 5f;

	[Header("Boid")]
	[SerializeField]
	private float m_avoidRadius = 2f;

	[SerializeField]
	private float m_avoidanceForceScalar = 1f;

	[SerializeField]
	private float m_targetAttractionStrength = 1f;

	[SerializeField]
	private float m_goalDistance = 0.2f;

	[Header("Circling Behavior")]
	[SerializeField]
	private float m_circleRadius = 6f;

	[SerializeField]
	private float m_circleHeightOffset = 2f;

	[SerializeField]
	private float m_circleHeightVariance = 0.75f;

	[Header("Avoidance")]
	[SerializeField]
	private float m_blockedDistance = 0.5f;

	[SerializeField]
	private float m_avoidanceDistance = 2f;

	[SerializeField]
	private float m_avoidanceCheckHeight = 0.25f;

	[SerializeField]
	private AnimationCurve m_avoidanceCurve;

	[Header("Near Loop Detection")]
	[SerializeField]
	private float m_nearDistance = 1f;

	[SerializeField]
	private float m_nearEarlyOutTime = 0.5f;

	[Header("Attack")]
	[SerializeField]
	private float m_attackPrepareDuration = 0.5f;

	[SerializeField]
	private AudioEvent m_audioAttack;

	[SerializeField]
	private AudioEvent m_audioSquawk;

	[Header("PlaymakerFSM")]
	[SerializeField]
	private PlayMakerFSM m_fsm;

	[Header("Collision Mask")]
	[SerializeField]
	private CharacterMovement.MovementCollisionMask m_collisionMaskMode;

	private Rigidbody2D m_rigidbody;

	private CharacterDirection m_direction;

	private Vector2 m_attackTargetPosition;

	private Vector2 m_attackStartPosition;

	private Vector2 m_attackExitPosition;

	private Vector2 m_retreatPosition;

	private bool m_shouldAttack;

	private float m_nearGoalTimer;

	private float m_prepareTimer;

	private float m_circleHeight;

	private float m_timeInState;

	private BirdState m_birdState;

	private Vector2 m_randomPositionOffset;

	private BirdState State
	{
		get
		{
			return m_birdState;
		}
		set
		{
			if (m_birdState == value)
			{
				return;
			}
			m_birdState = value;
			switch (value)
			{
			case BirdState.HoverLeft:
			case BirdState.HoverRight:
				if (m_fsm != null)
				{
					m_fsm.SendEvent("Done");
				}
				m_circleHeight = m_circleHeightOffset + Random.Range(0f - m_circleHeightVariance, m_circleHeightVariance);
				m_randomPositionOffset = new Vector2(Random.Range(-1f, 1f), Random.Range(-0.25f, 0.25f));
				break;
			case BirdState.AttackStart:
				if (m_fsm != null)
				{
					m_fsm.SendEvent("Trigger");
				}
				CalculateAttackPositions();
				m_audioAttack.Play(base.transform.position);
				break;
			case BirdState.AttackPrepare:
				m_prepareTimer = 0f;
				m_audioSquawk.Play(base.transform.position);
				break;
			}
			m_timeInState = 0f;
		}
	}

	public void SetAttackFlag()
	{
		m_shouldAttack = true;
	}

	public void SetDissipate()
	{
		State = BirdState.Dissipate;
		m_retreatPosition = m_target.transform.position;
		m_retreatPosition.y += 20f;
		float num = m_attackExitPosition.x - base.transform.position.x;
		num = ((num < 0f) ? (-1f) : 1f);
		m_retreatPosition.x += 20f * num;
		m_shouldAttack = false;
	}

	public bool IsAttacking()
	{
		if (m_shouldAttack)
		{
			return true;
		}
		BirdState birdState = m_birdState;
		if ((uint)(birdState - 3) <= 3u)
		{
			return true;
		}
		return false;
	}

	private void CalculateAttackPositions()
	{
		float x = m_direction.GetForwardVector().x;
		m_attackStartPosition = base.transform.position;
		m_attackStartPosition.x += x * 2.5f;
		m_attackStartPosition.y = m_target.transform.position.y + 0.25f;
		m_attackTargetPosition = m_attackStartPosition;
		m_attackTargetPosition.y = m_target.transform.position.y;
		m_attackTargetPosition.x += x * 15f;
		m_attackExitPosition = m_attackTargetPosition;
		m_attackExitPosition.x += x * m_circleRadius;
		m_attackExitPosition.y += m_circleHeightOffset;
	}

	private Vector2 GetGoalPosition()
	{
		switch (m_birdState)
		{
		case BirdState.HoverLeft:
		{
			Vector2 vector2 = m_target.transform.position;
			vector2.x -= m_circleRadius;
			vector2.y += m_circleHeight;
			return vector2 + m_randomPositionOffset;
		}
		case BirdState.HoverRight:
		{
			Vector2 vector = m_target.transform.position;
			vector.x += m_circleRadius;
			vector.y += m_circleHeight;
			return vector + m_randomPositionOffset;
		}
		case BirdState.AttackPrepare:
			return base.transform.position;
		case BirdState.AttackStart:
			return m_attackStartPosition;
		case BirdState.AttackActive:
			return m_attackTargetPosition;
		case BirdState.AttackExit:
			return m_attackExitPosition;
		case BirdState.Dissipate:
			return m_retreatPosition;
		default:
			return m_target.transform.position;
		}
	}

	private void Start()
	{
		m_direction = GetComponent<CharacterDirection>();
		m_rigidbody = GetComponent<Rigidbody2D>();
		State = ((Random.Range(0f, 1f) > 0.5f) ? BirdState.HoverLeft : BirdState.HoverRight);
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.Sets.Generic.BirdMovementSet.Add(this);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Sets.Generic.BirdMovementSet.Remove(this);
	}

	private void Update()
	{
		m_timeInState += Time.deltaTime;
		if (m_birdState == BirdState.AttackPrepare)
		{
			m_prepareTimer += Time.deltaTime;
			m_rigidbody.linearVelocity = Vector2.MoveTowards(m_rigidbody.linearVelocity, Vector2.zero, Time.deltaTime * 10f);
			if (m_target.transform.position.x > base.transform.position.x)
			{
				m_direction.DesiredDirection = CharacterDirection.Facing.Right;
			}
			else
			{
				m_direction.DesiredDirection = CharacterDirection.Facing.Left;
			}
			if (m_prepareTimer > m_attackPrepareDuration)
			{
				State = BirdState.AttackStart;
			}
			return;
		}
		if (m_birdState != 0)
		{
			MoveInDepth();
		}
		AvoidOtherBoids();
		SeekTarget();
		Vector2 movement = AvoidOtherBoids() + SeekTarget();
		bool num = ObstacleAvoidance(ref movement);
		bool flag = false;
		if (num && m_timeInState >= 0.2f)
		{
			flag = true;
		}
		ApplyMovement(movement);
		float num2 = Vector2.Distance(GetGoalPosition(), base.transform.position);
		if (num2 < m_nearDistance)
		{
			m_nearGoalTimer += Time.deltaTime;
		}
		else
		{
			m_nearGoalTimer = 0f;
		}
		if (num2 < m_goalDistance || m_nearGoalTimer >= m_nearEarlyOutTime)
		{
			flag = true;
		}
		if (m_timeInState >= 5f)
		{
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		if (m_shouldAttack)
		{
			State = BirdState.AttackPrepare;
			m_shouldAttack = false;
			return;
		}
		switch (State)
		{
		case BirdState.HoverLeft:
			State = BirdState.HoverRight;
			break;
		case BirdState.HoverRight:
			State = BirdState.HoverLeft;
			break;
		case BirdState.AttackPrepare:
			State = BirdState.AttackStart;
			break;
		case BirdState.AttackStart:
			State = BirdState.AttackActive;
			break;
		case BirdState.AttackActive:
			State = BirdState.AttackExit;
			break;
		case BirdState.AttackExit:
			State = ((Random.Range(0f, 1f) > 0.5f) ? BirdState.HoverLeft : BirdState.HoverRight);
			break;
		case BirdState.Dissipate:
			base.gameObject.SetActive(value: false);
			break;
		}
	}

	private void MoveInDepth()
	{
		Vector3 position = base.transform.position;
		position.z = Mathf.MoveTowards(position.z, m_target.position.z, Time.deltaTime);
		base.transform.position = position;
	}

	private Vector2 AvoidOtherBoids()
	{
		Vector2 zero = Vector2.zero;
		int num = 0;
		foreach (CharacterBirdMovement item in GlobalReferences.Instance.Sets.Generic.BirdMovementSet)
		{
			if (!(item == this))
			{
				float num2 = Vector2.Distance(item.transform.position, base.transform.position);
				if (num2 < m_avoidRadius)
				{
					Vector2 vector = base.transform.position - item.transform.position;
					zero += vector / (num2 * num2) * m_avoidanceForceScalar;
					num++;
				}
			}
		}
		if (num > 0)
		{
			zero /= (float)num;
		}
		return zero;
	}

	private Vector2 SeekTarget()
	{
		return (GetGoalPosition() - (Vector2)base.transform.position).normalized * m_targetAttractionStrength;
	}

	private bool ObstacleAvoidance(ref Vector2 movement)
	{
		Vector2 normalized = movement.normalized;
		Vector2 position = base.transform.position;
		position.y += m_avoidanceCheckHeight * 0.5f;
		float raycastResult = GetRaycastResult(position, normalized);
		Vector2 position2 = base.transform.position;
		position2.y -= m_avoidanceCheckHeight * 0.5f;
		float raycastResult2 = GetRaycastResult(position2, normalized);
		if (raycastResult < m_blockedDistance && raycastResult2 < m_blockedDistance)
		{
			return true;
		}
		if (raycastResult < raycastResult2)
		{
			float time = 1f - raycastResult / m_avoidanceDistance;
			movement.y = 0f - m_avoidanceCurve.Evaluate(time);
		}
		else if (raycastResult2 < raycastResult)
		{
			float time2 = 1f - raycastResult2 / m_avoidanceDistance;
			movement.y = m_avoidanceCurve.Evaluate(time2);
		}
		return false;
	}

	private float GetRaycastResult(Vector2 position, Vector2 direction)
	{
		Vector2 size = new Vector2(0.1f, m_avoidanceCheckHeight);
		int layerMask = GameLayers.CharacterNavigationMask;
		if (m_collisionMaskMode == CharacterMovement.MovementCollisionMask.EnemySpecial)
		{
			layerMask = GameLayers.EnemySpecialNavigationMask;
		}
		RaycastHit2D raycastHit2D = Physics2D.BoxCast(position, size, 0f, direction, m_avoidanceDistance, layerMask);
		if (raycastHit2D.collider != null)
		{
			return raycastHit2D.distance;
		}
		return float.MaxValue;
	}

	private void ApplyMovement(Vector2 force)
	{
		if (force.x > 0.1f)
		{
			m_direction.DesiredDirection = CharacterDirection.Facing.Right;
		}
		else if (force.x < -0.1f)
		{
			m_direction.DesiredDirection = CharacterDirection.Facing.Left;
		}
		m_rigidbody.linearVelocity += force;
		float num = m_normalSpeed;
		switch (m_birdState)
		{
		case BirdState.AttackPrepare:
		case BirdState.AttackStart:
			num = m_attackStartSpeed;
			break;
		case BirdState.AttackActive:
			num = m_attackSpeed;
			break;
		}
		if (m_rigidbody.linearVelocity.magnitude > num)
		{
			m_rigidbody.linearVelocity = m_rigidbody.linearVelocity.normalized * num;
		}
	}
}
