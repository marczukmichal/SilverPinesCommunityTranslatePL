using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class EnemyMigrationManager : MonoBehaviour
{
	[Serializable]
	public struct MigrationTransition
	{
		public string m_transitionTargetName;

		public bool m_transitionActive;

		public float m_transitionTimer;

		[NonSerialized]
		public GameObject m_transitionObject;
	}

	[Serializable]
	public class MigratedEnemyStatus
	{
		public string m_guid;

		public string m_newScene;

		public string m_previousScene;

		public AssetReferenceGameObject m_assetReference;

		public MigrationTransition m_transition;
	}

	private static EnemyMigrationManager m_instance;

	[SerializeField]
	private AnimationCurve m_migrationTimeDistanceCurve;

	public static readonly float MaxMigrationDistance = 8f;

	[SerializeField]
	private List<MigratedEnemyStatus> m_migratedEnemies;

	private bool m_levelTransitionActive;

	private string m_currentScenePath;

	private List<GameObject> m_enemiesToClearUp = new List<GameObject>();

	public static EnemyMigrationManager Get()
	{
		if (m_instance != null)
		{
			m_instance = UnityEngine.Object.FindAnyObjectByType<EnemyMigrationManager>();
		}
		return m_instance;
	}

	private void Start()
	{
		m_migratedEnemies = new List<MigratedEnemyStatus>();
		OnLoadData(GlobalReferences.Instance.DataStore.Data);
	}

	private void OnEnable()
	{
		m_instance = this;
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransitionStarted.Register(LevelTransitionStarted);
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransitionCompleted.Register(OnEnterScene);
		GlobalReferences.Instance.EventChannels.Migration.MigrateEnemyRequest.Register(MigrateEnemyRequest);
		GlobalReferences.Instance.EventChannels.SaveLoad.PersistentDataPopulateForSave.Register(OnPrepareSave);
		GlobalReferences.Instance.EventChannels.SaveLoad.PersistentDataOnLoaded.Register(OnLoadData);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransitionStarted.Unregister(LevelTransitionStarted);
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransitionCompleted.Unregister(OnEnterScene);
		GlobalReferences.Instance.EventChannels.Migration.MigrateEnemyRequest.Unregister(MigrateEnemyRequest);
		GlobalReferences.Instance.EventChannels.SaveLoad.PersistentDataPopulateForSave.Register(OnPrepareSave);
		GlobalReferences.Instance.EventChannels.SaveLoad.PersistentDataOnLoaded.Register(OnLoadData);
	}

	private void OnLoadData(PersistentData persistentData)
	{
		m_migratedEnemies = new List<MigratedEnemyStatus>(persistentData.MigratedEnemies);
	}

	private void OnPrepareSave(PersistentData persistentData)
	{
		persistentData.MigratedEnemies = new List<MigratedEnemyStatus>(m_migratedEnemies);
	}

	private void LevelTransitionStarted()
	{
		m_levelTransitionActive = true;
	}

	private void MigrateEnemyRequest(MigrationRequestEventData migrationRequest)
	{
		MigratedEnemyStatus migratedEnemyStatus = null;
		foreach (MigratedEnemyStatus migratedEnemy in m_migratedEnemies)
		{
			if (migrationRequest.m_enemy.GUID.Equals(migratedEnemy.m_guid))
			{
				migratedEnemyStatus = migratedEnemy;
				break;
			}
		}
		if (migratedEnemyStatus == null)
		{
			migratedEnemyStatus = new MigratedEnemyStatus
			{
				m_guid = migrationRequest.m_enemy.GUID,
				m_assetReference = migrationRequest.m_enemy.AssetReferenceGameObject
			};
			m_migratedEnemies.Add(migratedEnemyStatus);
		}
		float transitionTimer = m_migrationTimeDistanceCurve.Evaluate(migrationRequest.m_distanceFromTransition / MaxMigrationDistance) * migrationRequest.m_enemy.MigrationTimeDelayScalar;
		if (string.IsNullOrEmpty(migrationRequest.m_transitionEvent.m_targetSceneName))
		{
			Debug.LogWarning("Tried to set an enemy migrating but the new target scene name is empty? New scene asset reference is: " + migrationRequest.m_transitionEvent.m_targetSceneAssetReference?.ToString() + " - broken level metadata?");
		}
		migratedEnemyStatus.m_newScene = migrationRequest.m_transitionEvent.m_targetSceneName;
		migratedEnemyStatus.m_transition = new MigrationTransition
		{
			m_transitionActive = true,
			m_transitionTargetName = migrationRequest.m_transitionEvent.m_targetEntryPoint,
			m_transitionTimer = transitionTimer
		};
	}

	private void OnEnterScene()
	{
		m_levelTransitionActive = false;
		m_currentScenePath = SceneManager.GetActiveScene().path;
		foreach (MigratedEnemyStatus migratedEnemy in m_migratedEnemies)
		{
			if (migratedEnemy == null)
			{
				Debug.LogWarning("Migrated enemy was null in m_migratedEnemies array. Migrated enemy data lost somehow?");
			}
			else if (string.IsNullOrEmpty(migratedEnemy.m_newScene))
			{
				Debug.LogWarning("Migrated enemy has no new scene set? Migrated enemy data lost somehow?");
			}
			else if (m_currentScenePath.Equals(migratedEnemy.m_newScene))
			{
				if (!migratedEnemy.m_transition.m_transitionActive)
				{
					SpawnEnemy(migratedEnemy, forceTargetPlayer: false);
				}
				else
				{
					migratedEnemy.m_transition.m_transitionObject = GameObject.Find(migratedEnemy.m_transition.m_transitionTargetName);
				}
			}
		}
	}

	private void SpawnEnemy(MigratedEnemyStatus enemy, bool forceTargetPlayer)
	{
		GameObject gameObject = enemy.m_transition.m_transitionObject;
		if (gameObject == null)
		{
			gameObject = GameObject.Find(enemy.m_transition.m_transitionTargetName);
		}
		Vector3 zero = Vector3.zero;
		if (gameObject != null)
		{
			zero = gameObject.transform.position;
			LevelTransitionDoor component = gameObject.GetComponent<LevelTransitionDoor>();
			LevelTransition component2 = gameObject.GetComponent<LevelTransition>();
			LevelTransition.ArrivalDirection arrivalDirection = LevelTransition.ArrivalDirection.None;
			LevelTransition.ArrivalState arrivalState = LevelTransition.ArrivalState.Normal;
			if (component2 != null)
			{
				arrivalDirection = component2.TransitionArrivalDirection;
			}
			zero = LevelManager.GetPositionForLevelTransition(gameObject, arrivalDirection, component);
			zero = LevelManager.RayCastForFloor(zero, arrivalState);
			DynamicallySpawnObjectEventData dynamicallySpawnObjectEventData = default(DynamicallySpawnObjectEventData);
			dynamicallySpawnObjectEventData.m_assetReference = enemy.m_assetReference;
			dynamicallySpawnObjectEventData.m_persistent = false;
			dynamicallySpawnObjectEventData.m_position = zero;
			dynamicallySpawnObjectEventData.m_scale = Vector3.one;
			dynamicallySpawnObjectEventData.m_guid = enemy.m_guid;
			DynamicallySpawnObjectEventData eventData = dynamicallySpawnObjectEventData;
			if (forceTargetPlayer)
			{
				ref UnityAction<GameObject> onSpawnedAction = ref eventData.m_onSpawnedAction;
				onSpawnedAction = (UnityAction<GameObject>)Delegate.Combine(onSpawnedAction, new UnityAction<GameObject>(OnEnemyMigratedToScene));
			}
			DynamicallySpawnedObject.Spawn(eventData);
		}
		else
		{
			Debug.Log("Can't spawn migrated enemy, no transition target found! Trying to migrate " + enemy.m_guid + " to scene " + enemy.m_newScene + " from scene " + enemy.m_previousScene);
		}
	}

	private void OnEnemyMigratedToScene(GameObject spawnedEnemy)
	{
		PersistentDataIdentifier component = spawnedEnemy.GetComponent<PersistentDataIdentifier>();
		bool flag = false;
		bool flag2 = false;
		foreach (MigratedEnemyStatus migratedEnemy in m_migratedEnemies)
		{
			if (!migratedEnemy.m_guid.Equals(component.GUID))
			{
				continue;
			}
			if (!migratedEnemy.m_newScene.Equals(m_currentScenePath))
			{
				migratedEnemy.m_transition.m_transitionActive = true;
				Debug.LogWarning("Enemy spawned in wrong scene: " + m_currentScenePath + " currently, expected " + migratedEnemy.m_newScene);
				m_enemiesToClearUp.Add(spawnedEnemy);
				flag2 = true;
				break;
			}
			if (migratedEnemy.m_transition.m_transitionObject != null)
			{
				LevelTransitionDoor component2 = migratedEnemy.m_transition.m_transitionObject.GetComponent<LevelTransitionDoor>();
				LevelTransition component3 = migratedEnemy.m_transition.m_transitionObject.GetComponent<LevelTransition>();
				if (component2 != null)
				{
					component2.OpenForEnemyMigration(spawnedEnemy.GetComponent<CharacterMovement>());
				}
				if (component3 != null && component2 != null && (component3.TransitionArrivalDirection == LevelTransition.ArrivalDirection.MoveLeft || component3.TransitionArrivalDirection == LevelTransition.ArrivalDirection.MoveRight))
				{
					flag = true;
				}
			}
		}
		if (flag2)
		{
			return;
		}
		AISenses component4 = spawnedEnemy.GetComponent<AISenses>();
		if (component4 != null)
		{
			component4.ForcePlayerAsTarget();
		}
		CharacterDirection component5 = spawnedEnemy.GetComponent<CharacterDirection>();
		if (component5 != null)
		{
			GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
			if (item != null)
			{
				component5.CurrentDirection = ((!(item.transform.position.x < spawnedEnemy.transform.position.x)) ? CharacterDirection.Facing.Right : CharacterDirection.Facing.Left);
			}
		}
		if (!flag)
		{
			spawnedEnemy.GetComponent<MigratableEnemy>().SceneMigrationFadeEffect();
		}
		if (flag)
		{
			PlayMakerFSM component6 = spawnedEnemy.GetComponent<PlayMakerFSM>();
			if (component6 != null)
			{
				component6.SendEvent("TransitionDoor/Enemy");
			}
		}
	}

	public bool IsInScene(MigratableEnemy enemy)
	{
		string path = enemy.gameObject.scene.path;
		foreach (MigratedEnemyStatus migratedEnemy in m_migratedEnemies)
		{
			if (migratedEnemy.m_guid.Equals(migratedEnemy.m_guid))
			{
				return migratedEnemy.m_newScene.Equals(path);
			}
		}
		return true;
	}

	private void Update()
	{
		if (m_levelTransitionActive)
		{
			return;
		}
		if (m_enemiesToClearUp.Count > 0)
		{
			foreach (GameObject item in m_enemiesToClearUp)
			{
				UnityEngine.Object.Destroy(item);
			}
			m_enemiesToClearUp.Clear();
		}
		foreach (MigratedEnemyStatus migratedEnemy in m_migratedEnemies)
		{
			if (!migratedEnemy.m_transition.m_transitionActive)
			{
				continue;
			}
			migratedEnemy.m_transition.m_transitionTimer -= Time.deltaTime;
			if (migratedEnemy.m_transition.m_transitionTimer < 0f)
			{
				migratedEnemy.m_transition.m_transitionActive = false;
				if (migratedEnemy.m_newScene.Equals(m_currentScenePath))
				{
					SpawnEnemy(migratedEnemy, forceTargetPlayer: true);
				}
			}
		}
	}

	public bool IsEnemyMigrating(LevelTransition levelTransition)
	{
		foreach (MigratedEnemyStatus migratedEnemy in m_migratedEnemies)
		{
			if (migratedEnemy.m_transition.m_transitionActive && migratedEnemy.m_newScene.Equals(levelTransition.gameObject.scene.path) && migratedEnemy.m_transition.m_transitionTargetName.Equals(levelTransition.gameObject.name))
			{
				return true;
			}
		}
		return false;
	}
}
