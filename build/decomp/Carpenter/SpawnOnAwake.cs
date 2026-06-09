using UnityEngine;

public class SpawnOnAwake : MonoBehaviour
{
	[SerializeField]
	private GameObject m_prefab;

	private void Awake()
	{
		Debug.LogWarning("Spawn on awake is deprecated " + base.gameObject.name + " using prefab instantiation! Use SpawnOnEnable instead!", this);
		Object.Instantiate(m_prefab, base.transform.position, base.transform.rotation);
	}
}
