using System.Collections;
using UnityEngine;

public class RandomPhysicsForce : MonoBehaviour
{
	[SerializeField]
	private Rigidbody2D m_rigidbody;

	[SerializeField]
	private Vector2 m_minForce;

	[SerializeField]
	private Vector2 m_maxForce;

	[SerializeField]
	private Vector2 m_refireTimeRange;

	private void Reset()
	{
		m_rigidbody = GetComponent<Rigidbody2D>();
	}

	private IEnumerator Start()
	{
		while (true)
		{
			ApplyForce();
			yield return new WaitForSeconds(m_refireTimeRange.GetRandom());
		}
	}

	private void ApplyForce()
	{
		Vector2 force = new Vector2(Random.Range(m_minForce.x, m_maxForce.x), Random.Range(m_minForce.y, m_maxForce.y));
		m_rigidbody.AddForce(force);
	}
}
