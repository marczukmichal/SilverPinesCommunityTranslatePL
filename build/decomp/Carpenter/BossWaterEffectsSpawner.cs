using UnityEngine;
using UnityEngine.AddressableAssets;

public class BossWaterEffectsSpawner : MonoBehaviour
{
	[SerializeField]
	private Transform m_waterTransform;

	[SerializeField]
	private GameObject m_waterRipplesPrefab;

	[Header("Little Splashes")]
	[SerializeField]
	private AssetReference[] m_waterSplashEffects;

	[SerializeField]
	private float m_moveDistanceToSpawn;

	[SerializeField]
	private Vector2 m_refireTime;

	[SerializeField]
	private float m_positionRandomVariance;

	[Header("Big Splash")]
	[SerializeField]
	private AssetReference[] m_waterSplashLarge;

	private Vector3 m_previousSpawnPosition;

	private GameObject m_waterRipples;

	private float m_timer;

	private void Awake()
	{
		m_waterRipples = Object.Instantiate(m_waterRipplesPrefab);
	}

	private void OnEnable()
	{
		m_previousSpawnPosition = base.transform.position;
	}

	private void SpawnEffect(AssetReference[] effects)
	{
		Vector3 position = base.transform.position;
		position.x += Random.Range(m_positionRandomVariance * -0.5f, m_positionRandomVariance * 0.5f);
		position.y = m_waterTransform.transform.position.y;
		position.z += Random.Range(0f, m_positionRandomVariance);
		DynamicallySpawnedObject.Spawn(effects[Random.Range(0, effects.Length)], persistent: false, position);
	}

	public void SpawnSplashLarge()
	{
		SpawnEffect(m_waterSplashLarge);
	}

	private void Update()
	{
		m_timer -= Time.deltaTime;
		if (Vector3.Distance(base.transform.position, m_previousSpawnPosition) > m_moveDistanceToSpawn || m_timer <= 0f)
		{
			SpawnEffect(m_waterSplashEffects);
			m_timer = Random.Range(m_refireTime.x, m_refireTime.y);
			m_previousSpawnPosition = base.transform.position;
		}
		Vector3 position = base.transform.position;
		position.y = m_waterTransform.transform.position.y;
		m_waterRipples.transform.position = position;
	}
}
