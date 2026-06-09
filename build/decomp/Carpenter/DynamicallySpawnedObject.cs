using FMODUnity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.VFX;

public class DynamicallySpawnedObject : MonoBehaviour
{
	private ObjectPool<DynamicallySpawnedObject> m_parentObjectPool;

	private PersistentDataIdentifier m_pdi;

	private PersistentDataDynamicallySpawnedObject m_persistentSpawnedObjectData;

	private ParticleSystem[] m_particleSystems;

	private VisualEffect[] m_visualEffects;

	private StudioEventEmitter[] m_fmodAudioEvents;

	private DSOEffectDisableAfterTime[] m_dsoEffectDisableAfterTimes;

	private static float s_minAliveTime = 0.2f;

	private static float s_maxAliveTime = 10f;

	private float m_aliveTimer;

	private bool m_autoCleanup;

	public bool AutoCleanup
	{
		get
		{
			return m_autoCleanup;
		}
		set
		{
			m_autoCleanup = value;
		}
	}

	private void Awake()
	{
		m_particleSystems = GetComponentsInChildren<ParticleSystem>();
		m_visualEffects = GetComponentsInChildren<VisualEffect>();
		m_fmodAudioEvents = GetComponentsInChildren<StudioEventEmitter>();
		m_dsoEffectDisableAfterTimes = GetComponentsInChildren<DSOEffectDisableAfterTime>();
	}

	public void SetObjectPool(ObjectPool<DynamicallySpawnedObject> objectPool)
	{
		m_parentObjectPool = objectPool;
	}

	private void PlayEffects()
	{
		DSOEffectDisableAfterTime[] dsoEffectDisableAfterTimes = m_dsoEffectDisableAfterTimes;
		for (int i = 0; i < dsoEffectDisableAfterTimes.Length; i++)
		{
			dsoEffectDisableAfterTimes[i].gameObject.SetActive(value: true);
		}
		VisualEffect[] visualEffects = m_visualEffects;
		for (int i = 0; i < visualEffects.Length; i++)
		{
			visualEffects[i].Play();
		}
		ParticleSystem[] particleSystems = m_particleSystems;
		for (int i = 0; i < particleSystems.Length; i++)
		{
			particleSystems[i].Play();
		}
	}

	private void OnEnable()
	{
		m_aliveTimer = 0f;
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelUnloaded.Register(OnLevelUnloaded);
		GlobalReferences.Instance.EventChannels.SaveLoad.PrepareSave.Register(OnSave);
		PlayEffects();
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelUnloaded.Unregister(OnLevelUnloaded);
		GlobalReferences.Instance.EventChannels.SaveLoad.PrepareSave.Unregister(OnSave);
	}

	private void OnSave()
	{
		UpdatePersistentData();
	}

	private void UpdatePersistentData()
	{
		if (m_persistentSpawnedObjectData != null)
		{
			m_persistentSpawnedObjectData.m_position = base.transform.localPosition;
			m_persistentSpawnedObjectData.m_rotation = base.transform.localRotation;
			m_persistentSpawnedObjectData.m_scale = base.transform.localScale;
		}
	}

	private void OnLevelUnloaded()
	{
		UpdatePersistentData();
		ClearData();
		Release();
	}

	public void Release()
	{
		m_parentObjectPool.Release(this);
	}

	public void ReleaseSafe()
	{
		if (base.gameObject.activeSelf)
		{
			if (m_autoCleanup)
			{
				m_aliveTimer = 0f;
			}
			else
			{
				Release();
			}
		}
	}

	private void ClearData()
	{
		if (m_pdi != null)
		{
			m_pdi.ClearGUID();
		}
		m_persistentSpawnedObjectData = null;
	}

	public void SpawnAsNewPersistentObject(AssetReference assetReference)
	{
		LevelMetadata item = GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Item;
		if (item != null)
		{
			if (m_pdi == null)
			{
				m_pdi = GetComponent<PersistentDataIdentifier>();
			}
			m_pdi.SetAsNewDynamicObject();
			m_persistentSpawnedObjectData = new PersistentDataDynamicallySpawnedObject(m_pdi.GUID, assetReference, item);
			GlobalReferences.Instance.DataStore.Data.AddDynamicallySpawnedObject(m_persistentSpawnedObjectData);
		}
	}

	public void ClearPersistentData()
	{
		if (m_persistentSpawnedObjectData != null)
		{
			GlobalReferences.Instance.DataStore.Data.RemoveDynamicallySpawnedObject(m_persistentSpawnedObjectData);
		}
	}

	public void SetPeristentDataObject(PersistentDataDynamicallySpawnedObject spawnedObjectData)
	{
		m_persistentSpawnedObjectData = spawnedObjectData;
		if (m_pdi == null)
		{
			m_pdi = GetComponent<PersistentDataIdentifier>();
		}
		m_pdi.SetAsExistingSpawnedDynamicObject(spawnedObjectData.GUID);
	}

	private void Update()
	{
		if (!m_autoCleanup)
		{
			return;
		}
		m_aliveTimer += Time.deltaTime;
		if (m_aliveTimer >= s_maxAliveTime)
		{
			Release();
		}
		else
		{
			if (!(m_aliveTimer >= s_minAliveTime))
			{
				return;
			}
			bool flag = false;
			ParticleSystem[] particleSystems = m_particleSystems;
			for (int i = 0; i < particleSystems.Length; i++)
			{
				if (particleSystems[i].isPlaying)
				{
					flag = true;
					break;
				}
			}
			VisualEffect[] visualEffects = m_visualEffects;
			for (int i = 0; i < visualEffects.Length; i++)
			{
				if (visualEffects[i].HasAnySystemAwake())
				{
					flag = true;
					break;
				}
			}
			StudioEventEmitter[] fmodAudioEvents = m_fmodAudioEvents;
			for (int i = 0; i < fmodAudioEvents.Length; i++)
			{
				if (fmodAudioEvents[i].IsPlaying())
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				Release();
			}
		}
	}

	public static void Spawn(DynamicallySpawnObjectEventData eventData)
	{
		GlobalReferences.Instance.EventChannels.DynamicallySpawnedObjects.Spawn.Raise(eventData);
	}

	public static void Spawn(AssetReference assetReference, bool persistent, Vector3 position, Quaternion rotation, Vector3 scale)
	{
		Spawn(new DynamicallySpawnObjectEventData(assetReference, persistent, position, rotation, scale));
	}

	public static void Spawn(AssetReference assetReference, bool persistent, Vector3 position, Quaternion rotation)
	{
		Spawn(assetReference, persistent, position, rotation, Vector3.one);
	}

	public static void Spawn(AssetReference assetReference, bool persistent, Vector3 position)
	{
		Spawn(assetReference, persistent, position, Quaternion.identity, Vector3.one);
	}
}
