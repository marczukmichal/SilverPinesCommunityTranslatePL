using UnityEngine;

public class PassiveRandomRotator : MonoBehaviour
{
	private class Rotator
	{
		private Vector3 m_rotation;

		private float m_timer;

		private float m_randomValue;

		private float m_randomTimeScalar;

		private float m_speed;

		private AnimationCurve m_speedCurve;

		public Rotator(Vector3 rotation, float baseSpeed, float speedVariance, AnimationCurve curve)
		{
			m_rotation = rotation;
			m_speedCurve = curve;
			m_randomValue = Random.Range(0f, 1f);
			m_timer = Random.Range(0f, 10f);
			m_speed = Random.Range(baseSpeed - speedVariance * 0.5f, baseSpeed + speedVariance * 0.5f);
		}

		public void Update(ref Vector3 rotation)
		{
			m_timer += Time.deltaTime * m_speed * m_speedCurve.Evaluate(Time.time + m_randomValue);
			rotation += m_rotation * Mathf.Sin(m_timer);
		}
	}

	[SerializeField]
	private Vector3 m_rotationAmount;

	[SerializeField]
	private float m_baseSpeed;

	[SerializeField]
	private float m_speedVariance;

	[SerializeField]
	private AnimationCurve m_speedCurve;

	private Rotator m_xRotator;

	private Rotator m_yRotator;

	private Rotator m_zRotator;

	private void Start()
	{
		m_xRotator = new Rotator(new Vector3(m_rotationAmount.x, 0f, 0f), m_baseSpeed, m_speedVariance, m_speedCurve);
		m_yRotator = new Rotator(new Vector3(0f, m_rotationAmount.y, 0f), m_baseSpeed, m_speedVariance, m_speedCurve);
		m_zRotator = new Rotator(new Vector3(0f, 0f, m_rotationAmount.z), m_baseSpeed, m_speedVariance, m_speedCurve);
	}

	private void Update()
	{
		Vector3 rotation = Vector3.zero;
		m_xRotator.Update(ref rotation);
		m_yRotator.Update(ref rotation);
		m_zRotator.Update(ref rotation);
		base.transform.localRotation = Quaternion.Euler(rotation);
	}
}
