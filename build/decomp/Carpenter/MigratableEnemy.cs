using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

[RequireComponent(typeof(PersistentDataIdentifier))]
public class MigratableEnemy : MonoBehaviour, IPersistentComponent
{
	[Serializable]
	private class PersistentData
	{
		public bool m_hasMigrated;
	}

	[SerializeField]
	private AssetReferenceGameObject m_assetReference;

	[SerializeField]
	private float m_migrationTimeDelayScalar = 1f;

	[SerializeField]
	private SpriteRenderer m_spriteRenderer;

	[SerializeField]
	private float m_migrationChanceClose = 0.8f;

	[SerializeField]
	private float m_migrationChanceFar = 0.1f;

	private PersistentDataIdentifier m_persistentDataIdentifier;

	private CharacterHealth m_characterHealth;

	private bool m_isSpawnedInstance;

	private AIBrain m_aiBrain;

	private bool m_forceMigration;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public float MigrationTimeDelayScalar => m_migrationTimeDelayScalar;

	public AssetReferenceGameObject AssetReferenceGameObject => m_assetReference;

	public string GUID => m_persistentDataIdentifier.GUID;

	public bool ForceMigration
	{
		get
		{
			return m_forceMigration;
		}
		set
		{
			m_forceMigration = value;
		}
	}

	public void SetIsSpawnedInstance()
	{
		m_isSpawnedInstance = true;
	}

	private void Start()
	{
		m_persistentDataIdentifier = GetComponent<PersistentDataIdentifier>();
		m_characterHealth = GetComponent<CharacterHealth>();
		m_aiBrain = GetComponent<AIBrain>();
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransition.Register(OnLevelTransition);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransition.Unregister(OnLevelTransition);
	}

	private void OnLevelTransition(LevelTransitionEventData transition)
	{
		if (!transition.m_canEnemiesMigrate || m_characterHealth.IsDead)
		{
			return;
		}
		float num = Vector2.Distance(base.transform.position, transition.m_transitionFromPosition);
		if (!m_forceMigration)
		{
			if (num > EnemyMigrationManager.MaxMigrationDistance || (m_aiBrain != null && m_aiBrain.CurrentAlertState != AIBrain.AlertState.Combat))
			{
				return;
			}
			float num2 = Mathf.Lerp(m_migrationChanceClose, m_migrationChanceFar, Mathf.Clamp01(Mathf.InverseLerp(0f, EnemyMigrationManager.MaxMigrationDistance, num)));
			if (num2 < 1f && UnityEngine.Random.Range(0f, 1f) > num2)
			{
				return;
			}
			float num3 = Mathf.Abs(base.transform.position.x - transition.m_transitionFromPosition.x);
			float num4 = Mathf.Abs(base.transform.position.y - transition.m_transitionFromPosition.y);
			if (num4 > 2f || num4 > num3)
			{
				return;
			}
		}
		GlobalReferences.Instance.EventChannels.Migration.MigrateEnemyRequest.Raise(new MigrationRequestEventData
		{
			m_enemy = this,
			m_transitionEvent = transition,
			m_distanceFromTransition = num
		});
		m_persistentData.m_hasMigrated = true;
		CharacterMovement component = GetComponent<CharacterMovement>();
		if (component != null)
		{
			component.StartMigration();
		}
	}

	public void SceneMigrationFadeEffect()
	{
		StartCoroutine(SceneMigrationFadeEffectCoroutine());
	}

	private IEnumerator SceneMigrationFadeEffectCoroutine()
	{
		WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
		float fadeValue2 = 0f;
		while (fadeValue2 < 1f)
		{
			m_spriteRenderer.material.SetFloat("_Visibility", fadeValue2);
			fadeValue2 += Time.deltaTime * 0.25f;
			fadeValue2 = Mathf.Min(fadeValue2, 1f);
			yield return waitForEndOfFrame;
		}
		m_spriteRenderer.material.SetFloat("_Visibility", 1f);
	}

	public bool RequiresPersistentData()
	{
		return true;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData != null)
		{
			if (m_persistentData.m_hasMigrated && !m_isSpawnedInstance)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		else
		{
			m_persistentData = new PersistentData();
			m_persistentDataObject.Data = m_persistentData;
		}
	}
}
