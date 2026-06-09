using UnityEngine;

public class BoatMovement : MonoBehaviour
{
	[SerializeField]
	private float m_buoyancyForce = 10f;

	[SerializeField]
	private float m_linearDragOutOfWater = 0.1f;

	[SerializeField]
	private float m_linearDragInWater = 2f;

	[SerializeField]
	private float m_submersionHeightOffset;

	[Header("Engine")]
	[SerializeField]
	private float m_engineForce = 10f;

	[Header("Velocity Tilt")]
	[SerializeField]
	private AnimationCurve m_velocityTilt;

	[Header("Components")]
	[SerializeField]
	private Collider2D m_collider;

	private Rigidbody2D m_rigidbody;

	private CharacterDirection m_direction;

	private float m_input;

	private void Awake()
	{
		m_rigidbody = GetComponent<Rigidbody2D>();
		m_direction = GetComponent<CharacterDirection>();
	}

	private float GetWaterLevel()
	{
		return 0f;
	}

	private void FixedUpdate()
	{
		ApplyBuoyancy();
		ApplyEngineForce();
		float time = Mathf.Abs(m_rigidbody.linearVelocity.x);
		float num = m_velocityTilt.Evaluate(time);
		if (m_direction.CurrentDirection == CharacterDirection.Facing.Left)
		{
			num *= -1f;
		}
		base.transform.rotation = Quaternion.Euler(0f, 0f, num);
	}

	private void ApplyBuoyancy()
	{
		float num = base.transform.position.y + m_submersionHeightOffset;
		float waterLevel = GetWaterLevel();
		float y = m_collider.bounds.size.y;
		float num2 = Mathf.Clamp(waterLevel - (num - y / 2f), 0f, y);
		if (num2 > 0f)
		{
			float y2 = m_buoyancyForce * (num2 / y);
			m_rigidbody.AddForce(new Vector2(0f, y2), ForceMode2D.Force);
		}
		m_rigidbody.linearDamping = Mathf.Lerp(m_linearDragOutOfWater, m_linearDragInWater, num2);
	}

	private void ApplyEngineForce()
	{
		m_rigidbody.AddForce(Vector2.right * m_engineForce * m_input);
	}

	public void SetInput(float input)
	{
		m_input = input;
	}
}
