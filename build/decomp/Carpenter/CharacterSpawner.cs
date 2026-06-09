using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
	[SerializeField]
	private Vector2 m_timesBetweenSpawn;

	[Tooltip("How many spawns can be active at once. -1 means no limit")]
	[SerializeField]
	private int m_maxActiveSpawns = -1;

	[Tooltip("How many spawns can we generate in total (lifetime?) -1 means infinite")]
	[SerializeField]
	private int m_maxTotalSpawns = -1;

	[SerializeField]
	private GameObject[] m_prefabsToSpawn;

	[SerializeField]
	private bool m_startActive;

	public Vector3 m_spawnArea;

	private bool m_active;

	private Coroutine m_spawnCoroutine;

	private List<GameObject> m_spawnedObjects;

	public bool Active
	{
		get
		{
			return m_active;
		}
		set
		{
			if (m_active != value)
			{
				m_active = value;
				if (m_active)
				{
					m_spawnCoroutine = StartCoroutine(SpawnCoroutine());
				}
				else if (m_spawnCoroutine != null)
				{
					StopCoroutine(m_spawnCoroutine);
				}
			}
		}
	}

	private void OnEnable()
	{
		if (m_spawnedObjects == null)
		{
			m_spawnedObjects = new List<GameObject>();
		}
		if (m_startActive)
		{
			Active = true;
		}
	}

	private void OnDisable()
	{
		Active = false;
	}

	private int GetTotalSpawnCount()
	{
		return m_spawnedObjects.Count;
	}

	private int GetActiveSpawnCount()
	{
		int num = 0;
		foreach (GameObject spawnedObject in m_spawnedObjects)
		{
			if (!(spawnedObject == null))
			{
				CharacterHealth component = spawnedObject.GetComponent<CharacterHealth>();
				if (!(component != null) || !component.IsDead)
				{
					num++;
				}
			}
		}
		return num;
	}

	private bool IsAtMaxTotalSpawnCount()
	{
		if (m_maxTotalSpawns == -1)
		{
			return false;
		}
		return GetTotalSpawnCount() >= m_maxTotalSpawns;
	}

	private IEnumerator SpawnCoroutine()
	{
		while (!IsAtMaxTotalSpawnCount())
		{
			yield return new WaitForSeconds(m_timesBetweenSpawn.GetRandom());
			int activeSpawnCount = GetActiveSpawnCount();
			if (m_maxActiveSpawns == -1 || activeSpawnCount < m_maxActiveSpawns)
			{
				Spawn();
			}
		}
	}

	private void Spawn()
	{
		GameObject original = m_prefabsToSpawn[Random.Range(0, m_prefabsToSpawn.Length)];
		Vector3 position = base.transform.position + new Vector3(Random.Range((0f - m_spawnArea.x) * 0.5f, m_spawnArea.x * 0.5f), Random.Range((0f - m_spawnArea.y) * 0.5f, m_spawnArea.y * 0.5f), Random.Range((0f - m_spawnArea.z) * 0.5f, m_spawnArea.z * 0.5f));
		GameObject item = Object.Instantiate(original, position, Quaternion.identity);
		m_spawnedObjects.Add(item);
	}

	public void ForceSpawnLoop(int spawnCount, float timeBetweenSpawns)
	{
		StartCoroutine(ForceSpawnCoroutine(spawnCount, timeBetweenSpawns));
	}

	private IEnumerator ForceSpawnCoroutine(int spawnCount, float timeBetweenSpawns)
	{
		for (int i = 0; i < spawnCount; i++)
		{
			Spawn();
			yield return new WaitForSeconds(timeBetweenSpawns);
		}
	}

	public void RemoveAllActiveSpawns()
	{
		if (m_spawnedObjects == null)
		{
			return;
		}
		foreach (GameObject spawnedObject in m_spawnedObjects)
		{
			if (!(spawnedObject == null))
			{
				Object.Destroy(spawnedObject);
			}
		}
	}

	public void KillAllActiveSpawns()
	{
		if (m_spawnedObjects == null)
		{
			return;
		}
		foreach (GameObject spawnedObject in m_spawnedObjects)
		{
			if (!(spawnedObject == null))
			{
				CharacterHealth component = spawnedObject.GetComponent<CharacterHealth>();
				if (component != null && !component.IsDead)
				{
					DamageInstance damageInstance = new DamageInstance().SetHealthDamage(int.MaxValue);
					DamageUtilities.ApplyDamage(spawnedObject.GetComponentsInChildren<IDamageable>(), damageInstance);
				}
			}
		}
	}
}
