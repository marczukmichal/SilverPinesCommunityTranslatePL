using UnityEngine;
using UnityEngine.AddressableAssets;

public class GibletPiece : MonoBehaviour
{
	[SerializeField]
	private AssetReference[] m_bloodSmearsPrefabs;

	[SerializeField]
	private AssetReference[] m_bloodCeilingDripPrefab;

	[SerializeField]
	private int m_maxBloodSmears;

	[SerializeField]
	private float m_minTimeBetweenBloodSmears = 0.4f;

	private int m_spawnedBloodSmearCount;

	private float m_lastBloodSmearTime;

	private Vector2 m_startingPosition;

	private void Start()
	{
		m_spawnedBloodSmearCount = 0;
		m_lastBloodSmearTime = -10f;
		m_startingPosition = base.transform.position;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (!(Time.time - m_lastBloodSmearTime < m_minTimeBetweenBloodSmears) && m_spawnedBloodSmearCount < m_maxBloodSmears && (collision.gameObject.layer == GameLayers.EnvironmentLayer || collision.gameObject.layer == 0))
		{
			SpawnBloodSmear(collision);
		}
	}

	private void SpawnBloodSmear(Collision2D collision)
	{
		if (m_bloodCeilingDripPrefab.Length != 0)
		{
			m_lastBloodSmearTime = Time.time;
			ContactPoint2D contact = collision.GetContact(0);
			Vector3 position = contact.point;
			position.z = base.transform.position.z;
			Vector3 vector = contact.normal;
			Quaternion rotation = Quaternion.FromToRotation(Vector3.up, vector);
			DynamicallySpawnedObject.Spawn(m_bloodSmearsPrefabs[Random.Range(0, m_bloodSmearsPrefabs.Length)], persistent: true, position, rotation);
			bool flag = true;
			if (Vector2.Dot(vector, Vector2.down) < 0.8f)
			{
				flag = false;
			}
			if (position.y < m_startingPosition.y)
			{
				flag = false;
			}
			if (position.y < base.transform.position.y)
			{
				flag = false;
			}
			if (collision.relativeVelocity.y < 0f)
			{
				flag = false;
			}
			if (flag)
			{
				DynamicallySpawnedObject.Spawn(m_bloodCeilingDripPrefab[Random.Range(0, m_bloodCeilingDripPrefab.Length)], persistent: false, position, Quaternion.identity);
			}
		}
	}
}
