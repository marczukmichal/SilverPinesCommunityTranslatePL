using UnityEngine;

public class ResiduePuddle : MonoBehaviour
{
	[SerializeField]
	private float m_lifeTime;

	private float m_timer;

	private void OnEnable()
	{
		m_timer = 0f;
	}

	public void Update()
	{
		m_timer += Time.deltaTime;
		if (m_timer > m_lifeTime)
		{
			GetComponent<DynamicallySpawnedObject>().ReleaseSafe();
		}
	}
}
