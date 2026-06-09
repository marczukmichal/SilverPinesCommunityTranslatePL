using System.Collections;
using UnityEngine;

public class RigidbodyMoveTowardsTarget : MonoBehaviour
{
	[SerializeField]
	private Transform m_target;

	[SerializeField]
	private GameObjectAnchor m_anchor;

	[SerializeField]
	private float m_forceMultiplier;

	[SerializeField]
	private float m_maxDistance;

	[SerializeField]
	private float m_flipTime;

	[SerializeField]
	private float m_forceMultiplierFlipped;

	private Rigidbody2D m_rigidbody;

	private bool m_flip;

	private void Start()
	{
		m_rigidbody = GetComponent<Rigidbody2D>();
	}

	private void FixedUpdate()
	{
		Vector2 vector = Vector3.zero;
		bool flag = false;
		if (m_anchor != null)
		{
			if (m_anchor.Item != null)
			{
				vector = m_anchor.Item.transform.position;
				flag = true;
			}
		}
		else if ((bool)m_target)
		{
			vector = m_target.position;
			flag = true;
		}
		if (flag)
		{
			Vector3 vector2 = ((!m_flip) ? ((Vector3)(vector - m_rigidbody.position)) : ((Vector3)(m_rigidbody.position - vector)));
			if (Vector2.Distance(vector, m_rigidbody.position) < m_maxDistance)
			{
				Vector2 force = vector2 * (m_flip ? m_forceMultiplierFlipped : m_forceMultiplier);
				m_rigidbody.AddForce(force);
			}
		}
	}

	public void OnHit()
	{
		StopAllCoroutines();
		StartCoroutine(TemporaryFlip());
	}

	public IEnumerator TemporaryFlip()
	{
		m_flip = true;
		yield return new WaitForSeconds(m_flipTime);
		m_flip = false;
	}
}
