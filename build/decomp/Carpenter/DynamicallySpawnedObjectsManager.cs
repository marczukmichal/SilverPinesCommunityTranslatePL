using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DynamicallySpawnedObjectsManager : MonoBehaviour
{
	[Serializable]
	public struct PreloadedAssetSettings
	{
		public AssetReference m_assetReference;

		public bool m_persistentObject;
	}

	private class DynamicallySpawnedObjectPool
	{
		private AsyncOperationHandle<GameObject> m_assetHandle;

		private ObjectPool<DynamicallySpawnedObject> m_pool;

		private bool m_loaded;

		private bool m_isPersistent;

		private List<DynamicallySpawnObjectEventData> m_queuedEvents = new List<DynamicallySpawnObjectEventData>();

		private List<PersistentDataDynamicallySpawnedObject> m_queuedExistingObjects = new List<PersistentDataDynamicallySpawnedObject>();

		public DynamicallySpawnedObjectPool(AssetReference assetReference, bool isPersistent)
		{
			m_pool = new ObjectPool<DynamicallySpawnedObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject);
			AsyncOperationHandle<GameObject> assetHandle = Addressables.LoadAssetAsync<GameObject>(assetReference);
			assetHandle.Completed += OnAssetLoaded;
			m_assetHandle = assetHandle;
			m_isPersistent = isPersistent;
		}

		private void OnAssetLoaded(AsyncOperationHandle<GameObject> handle)
		{
			m_loaded = true;
			foreach (DynamicallySpawnObjectEventData queuedEvent in m_queuedEvents)
			{
				SpawnEvent(queuedEvent);
			}
			foreach (PersistentDataDynamicallySpawnedObject queuedExistingObject in m_queuedExistingObjects)
			{
				SpawnFromPersistentObject(queuedExistingObject);
			}
			m_queuedEvents.Clear();
			m_queuedExistingObjects.Clear();
		}

		public void SpawnEvent(DynamicallySpawnObjectEventData spawnEvent)
		{
			if (!m_loaded)
			{
				m_queuedEvents.Add(spawnEvent);
				return;
			}
			if (spawnEvent.m_persistent != m_isPersistent)
			{
				Debug.LogError("Spawning " + spawnEvent.m_assetReference?.ToString() + " with persistent mismatch. Triying to spawn with persistent: " + spawnEvent.m_persistent + " which does not match the object pool!");
			}
			DynamicallySpawnedObject dynamicallySpawnedObject = m_pool.Get();
			dynamicallySpawnedObject.transform.SetPositionAndRotation(spawnEvent.m_position, spawnEvent.m_rotation);
			Vector3 localScale = ((spawnEvent.m_scale == Vector3.one) ? m_assetHandle.Result.transform.localScale : spawnEvent.m_scale);
			dynamicallySpawnedObject.transform.localScale = localScale;
			if (m_isPersistent)
			{
				dynamicallySpawnedObject.SpawnAsNewPersistentObject(spawnEvent.m_assetReference);
			}
			MigratableEnemy component = dynamicallySpawnedObject.GetComponent<MigratableEnemy>();
			if (component != null)
			{
				component.SetIsSpawnedInstance();
			}
			if (!string.IsNullOrEmpty(spawnEvent.m_guid))
			{
				dynamicallySpawnedObject.GetComponent<PersistentDataIdentifier>().SetAsExistingSpawnedDynamicObject(spawnEvent.m_guid);
			}
			dynamicallySpawnedObject.gameObject.SetActive(value: true);
			IDynamicallySpawnedEventHandler[] components = dynamicallySpawnedObject.GetComponents<IDynamicallySpawnedEventHandler>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].OnInitialSpawn();
			}
			spawnEvent.m_onSpawnedAction?.Invoke(dynamicallySpawnedObject.gameObject);
		}

		public void SpawnFromPersistentObject(PersistentDataDynamicallySpawnedObject persistentObject)
		{
			if (!m_loaded)
			{
				m_queuedExistingObjects.Add(persistentObject);
				return;
			}
			DynamicallySpawnedObject dynamicallySpawnedObject = m_pool.Get();
			dynamicallySpawnedObject.transform.SetPositionAndRotation(persistentObject.m_position, persistentObject.m_rotation);
			dynamicallySpawnedObject.transform.localScale = persistentObject.m_scale;
			dynamicallySpawnedObject.SetPeristentDataObject(persistentObject);
			dynamicallySpawnedObject.gameObject.SetActive(value: true);
			IDynamicallySpawnedEventHandler[] components = dynamicallySpawnedObject.GetComponents<IDynamicallySpawnedEventHandler>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].OnReloadSpawn();
			}
		}

		private DynamicallySpawnedObject CreatePooledItem()
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_assetHandle.Result);
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			DynamicallySpawnedObject dynamicallySpawnedObject = gameObject.AddComponent<DynamicallySpawnedObject>();
			dynamicallySpawnedObject.SetObjectPool(m_pool);
			if (m_isPersistent && !dynamicallySpawnedObject.GetComponent<PersistentDataIdentifier>())
			{
				dynamicallySpawnedObject.gameObject.AddComponent<PersistentDataIdentifier>();
			}
			bool flag = dynamicallySpawnedObject.gameObject.GetComponent<Projectile>();
			bool flag2 = dynamicallySpawnedObject.gameObject.GetComponent<ResiduePuddle>();
			bool flag3 = dynamicallySpawnedObject.gameObject.GetComponent<CharacterIdentifier>();
			bool flag4 = dynamicallySpawnedObject.gameObject.GetComponent<StatusEffectVisuals>();
			dynamicallySpawnedObject.AutoCleanup = !m_isPersistent && !flag && !flag2 && !flag3 && !flag4;
			return dynamicallySpawnedObject;
		}

		private void OnReturnedToPool(DynamicallySpawnedObject poolObject)
		{
			poolObject.gameObject.SetActive(value: false);
		}

		private void OnTakeFromPool(DynamicallySpawnedObject poolObject)
		{
		}

		private void OnDestroyPoolObject(DynamicallySpawnedObject poolObject)
		{
			if (poolObject != null)
			{
				UnityEngine.Object.Destroy(poolObject.gameObject);
			}
		}

		public void ClearPool()
		{
			m_pool.Clear();
		}
	}

	[SerializeField]
	private PreloadedAssetSettings[] m_preloadAssets;

	private Dictionary<string, DynamicallySpawnedObjectPool> m_poolsMap = new Dictionary<string, DynamicallySpawnedObjectPool>();

	private void Awake()
	{
		PreloadedAssetSettings[] preloadAssets = m_preloadAssets;
		for (int i = 0; i < preloadAssets.Length; i++)
		{
			PreloadedAssetSettings preloadedAssetSettings = preloadAssets[i];
			CreateNewObjectPool(preloadedAssetSettings.m_assetReference, preloadedAssetSettings.m_persistentObject);
		}
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.DynamicallySpawnedObjects.Spawn.Register(SpawnObjectEvent);
		GlobalReferences.Instance.EventChannels.LevelTransition.SetupLevel.Register(OnLevelLoaded);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.DynamicallySpawnedObjects.Spawn.Unregister(SpawnObjectEvent);
		GlobalReferences.Instance.EventChannels.LevelTransition.SetupLevel.Unregister(OnLevelLoaded);
	}

	private DynamicallySpawnedObjectPool CreateNewObjectPool(AssetReference assetReference, bool persistent)
	{
		DynamicallySpawnedObjectPool dynamicallySpawnedObjectPool = new DynamicallySpawnedObjectPool(assetReference, persistent);
		m_poolsMap.Add(assetReference.AssetGUID, dynamicallySpawnedObjectPool);
		return dynamicallySpawnedObjectPool;
	}

	private void SpawnObjectEvent(DynamicallySpawnObjectEventData spawnEvent)
	{
		if (m_poolsMap.ContainsKey(spawnEvent.m_assetReference.AssetGUID))
		{
			m_poolsMap[spawnEvent.m_assetReference.AssetGUID].SpawnEvent(spawnEvent);
		}
		else
		{
			CreateNewObjectPool(spawnEvent.m_assetReference, spawnEvent.m_persistent).SpawnEvent(spawnEvent);
		}
	}

	private void OnLevelLoaded(LevelMetadata level)
	{
		foreach (DynamicallySpawnedObjectPool value in m_poolsMap.Values)
		{
			value.ClearPool();
		}
		SpawnPersistentObjectsInLevel(level);
	}

	private void SpawnPersistentObjectsInLevel(LevelMetadata level)
	{
		foreach (PersistentDataDynamicallySpawnedObject dynamicallySpawnedObject in GlobalReferences.Instance.DataStore.Data.GetDynamicallySpawnedObjects(level))
		{
			if (m_poolsMap.ContainsKey(dynamicallySpawnedObject.AssetReference.AssetGUID))
			{
				m_poolsMap[dynamicallySpawnedObject.AssetReference.AssetGUID].SpawnFromPersistentObject(dynamicallySpawnedObject);
			}
			else
			{
				CreateNewObjectPool(dynamicallySpawnedObject.AssetReference, persistent: true).SpawnFromPersistentObject(dynamicallySpawnedObject);
			}
		}
	}
}
