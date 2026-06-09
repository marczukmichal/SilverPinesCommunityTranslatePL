using UnityEngine;

public class RandomWarpWobble : MonoBehaviour
{
	[SerializeField]
	private float m_updateDelay;

	[SerializeField]
	private Vector2 m_randomPositionAmount;

	private float m_updateTimer;

	private void Update()
	{
		m_updateTimer += Time.deltaTime;
		if (m_updateTimer >= m_updateDelay)
		{
			UpdateWarp();
		}
	}

	private void UpdateWarp()
	{
		Vector3 localPosition = new Vector3(Random.Range(0f - m_randomPositionAmount.x, m_randomPositionAmount.x), Random.Range(0f - m_randomPositionAmount.y, m_randomPositionAmount.y), 0f);
		base.transform.localPosition = localPosition;
	}
}
