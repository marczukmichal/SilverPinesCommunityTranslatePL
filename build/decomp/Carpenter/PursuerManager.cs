using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class PursuerManager : MonoBehaviour
{
	[SerializeField]
	private ProgressionVariable m_pursuitSystemActiveVariable;

	[SerializeField]
	private IntVariable m_pursuerHealth;

	[SerializeField]
	private AssetReference m_pursuerAssetReference;

	[SerializeField]
	private float m_minimumTimeBetweenPursuits = 600f;

	[SerializeField]
	private MusicSettings m_pursuitMusic;

	[SerializeField]
	private float m_pursuitDuration;

	[SerializeField]
	private float m_damageToDurationReductionScalar = 1f;

	[SerializeField]
	private AnimationCurve m_chanceToPursueCurve;

	[SerializeField]
	private int m_sceneDistanceToEndPursuit = 3;

	[SerializeField]
	private float m_timeScalarWhilePursuerCantFollow = 0.2f;

	[SerializeField]
	private AssetReference m_pursuerDeadDroppedItem;

	private AsyncOperationHandle<GameObject> m_loadedAssetHandle;

	private bool m_pursuerActive;

	private bool m_musicActive;

	private float m_pursuitTimer;

	private float m_timeSinceLastPursuit;

	private GameObject m_activePursuerGameObject;

	private string m_activePursuerScene;

	private int m_savedHealth;

	[DebugCommand("pursuer_debug", "Show pursuer debug", "pursuer_debug <true/false>", typeof(bool), false)]
	private static bool s_showPursuerDebug;

	[DebugCommand("always_pursuer", "All pursuer triggers always trigger", "always_pursuer <true/false>", typeof(bool), false)]
	private static bool s_alwaysPursuer;

	private HashSet<LevelMetadata> m_unvisitableSceneList = new HashSet<LevelMetadata>();

	public bool PursuerActive
	{
		get
		{
			return m_pursuerActive;
		}
		private set
		{
			if (m_pursuerActive != value)
			{
				m_pursuerActive = value;
				MusicActive = m_pursuerActive;
			}
		}
	}

	private bool MusicActive
	{
		get
		{
			return m_musicActive;
		}
		set
		{
			if (m_musicActive != value)
			{
				m_musicActive = value;
				if (m_musicActive)
				{
					GlobalReferences.Instance.EventChannels.Audio.PlayMusicOverride.Raise(m_pursuitMusic);
				}
				else
				{
					GlobalReferences.Instance.EventChannels.Audio.PlayMusicOverride.Raise(null);
				}
			}
		}
	}

	private void OnEnable()
	{
		m_loadedAssetHandle = Addressables.LoadAssetAsync<GameObject>(m_pursuerAssetReference);
		GlobalReferences.Instance.EventChannels.Gameplay.PursuerSpawnTrigger.Register(OnPursuerTriggerHit);
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransition.Register(OnLevelTransitionRequest);
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransitionCompleted.Register(OnLevelTransitionCompleted);
		GlobalReferences.Instance.EventChannels.SaveLoad.OnGameReloadedEvent.Register(OnGameReloadedEvent);
		m_timeSinceLastPursuit = m_minimumTimeBetweenPursuits;
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Gameplay.PursuerSpawnTrigger.Unregister(OnPursuerTriggerHit);
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransition.Unregister(OnLevelTransitionRequest);
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransitionCompleted.Unregister(OnLevelTransitionCompleted);
		GlobalReferences.Instance.EventChannels.SaveLoad.OnGameReloadedEvent.Unregister(OnGameReloadedEvent);
		Addressables.Release(m_loadedAssetHandle);
	}

	private void OnGameReloadedEvent()
	{
		PursuerActive = false;
		m_timeSinceLastPursuit = m_minimumTimeBetweenPursuits;
	}

	private void OnPursuerTriggerHit(PursuerSpawnTrigger trigger)
	{
		if (!PursuerActive && m_pursuerHealth.Value > 0 && (s_alwaysPursuer || (m_pursuitSystemActiveVariable.Value && (trigger.IgnoreTimeBetweenPursuits || !(m_timeSinceLastPursuit < m_minimumTimeBetweenPursuits)) && !(UnityEngine.Random.Range(0f, 1f) > trigger.ChanceToTrigger))))
		{
			TriggerPursuit(trigger);
			trigger.OnTriggered();
		}
	}

	private void TriggerPursuit(PursuerSpawnTrigger trigger)
	{
		m_savedHealth = -1;
		m_pursuitTimer = m_pursuitDuration;
		TriggerPursuitSpawnEntity(PursuerSpawnPosition.GetSpawnPosition(trigger));
	}

	private void TriggerPursuitSpawnEntity(Vector3 spawnPosition)
	{
		PursuerActive = true;
		MusicActive = true;
		if (!m_loadedAssetHandle.IsValid())
		{
			Debug.LogError("Tried to spawn puruser but pursuser asset handle isn't valid!");
			return;
		}
		spawnPosition = SnapSpawnPositionToGround(spawnPosition);
		m_activePursuerGameObject = UnityEngine.Object.Instantiate(m_loadedAssetHandle.Result, spawnPosition, Quaternion.identity);
		CharacterHealth component = m_activePursuerGameObject.GetComponent<CharacterHealth>();
		if (component != null)
		{
			component.OnTakenDamage = (UnityAction<int>)Delegate.Combine(component.OnTakenDamage, new UnityAction<int>(OnPursuerTakenDamage));
			component.OnDead.AddListener(OnPursuerDead);
			component.OnRevive = (UnityAction)Delegate.Combine(component.OnRevive, new UnityAction(OnPursuerRevive));
			if (m_savedHealth > 0)
			{
				component.Health = m_savedHealth;
			}
		}
		CharacterDirection component2 = m_activePursuerGameObject.GetComponent<CharacterDirection>();
		if (component2 != null)
		{
			GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
			if (item != null)
			{
				if (item.transform.position.x < spawnPosition.x)
				{
					component2.CurrentDirection = CharacterDirection.Facing.Left;
				}
				else
				{
					component2.CurrentDirection = CharacterDirection.Facing.Right;
				}
			}
		}
		m_activePursuerScene = SceneManager.GetActiveScene().name;
		m_unvisitableSceneList.Clear();
	}

	private Vector3 SnapSpawnPositionToGround(Vector3 position)
	{
		RaycastHit2D raycastHit2D = Physics2D.Raycast(position, Vector2.down, 1f, GameLayers.CharacterNavigationMask);
		if ((bool)raycastHit2D.collider)
		{
			position.y = raycastHit2D.point.y;
		}
		return position;
	}

	private void OnPursuerDead()
	{
		PursuerActive = false;
	}

	private void OnPursuerRevive()
	{
		PursuerActive = true;
		m_pursuitTimer = Mathf.Max(m_pursuitTimer, m_pursuitDuration * 0.25f);
	}

	private void OnPursuerTakenDamage(int damage)
	{
		if (m_pursuerHealth.Value <= 0)
		{
			return;
		}
		m_pursuerHealth.Value -= damage;
		UpdatePursuitTimer((float)damage * m_damageToDurationReductionScalar);
		if (m_pursuerHealth.Value > 0)
		{
			return;
		}
		Debug.Log("Pursuer Dead");
		m_activePursuerGameObject.GetComponent<PlayMakerFSM>().SendEvent("Pursuer/FullyDead");
		Vector3 position = m_activePursuerGameObject.transform.position;
		position.y += 0.05f;
		position.z += 0.05f;
		DynamicallySpawnObjectEventData eventData = new DynamicallySpawnObjectEventData(m_pursuerDeadDroppedItem, persistent: true, position);
		ref UnityAction<GameObject> onSpawnedAction = ref eventData.m_onSpawnedAction;
		onSpawnedAction = (UnityAction<GameObject>)Delegate.Combine(onSpawnedAction, (UnityAction<GameObject>)delegate(GameObject spawnedObject)
		{
			ItemPickup component = spawnedObject.GetComponent<ItemPickup>();
			if (component != null)
			{
				component.SetItemAmount(1);
			}
		});
		DynamicallySpawnedObject.Spawn(eventData);
	}

	private void OnLevelTransitionRequest(LevelTransitionEventData transitionEvent)
	{
		if (PursuerActive && m_activePursuerGameObject != null)
		{
			CharacterHealth component = m_activePursuerGameObject.GetComponent<CharacterHealth>();
			m_savedHealth = component.Health;
		}
	}

	private float GetChanceToPursue()
	{
		return m_chanceToPursueCurve.Evaluate(m_pursuitTimer / m_pursuitDuration);
	}

	private void OnLevelTransitionCompleted()
	{
		if (!PursuerActive)
		{
			return;
		}
		if (m_pursuitTimer > 0f)
		{
			PursuerSpawnPosition pursuerSpawnPosition = null;
			if (m_activePursuerScene == SceneManager.GetActiveScene().name)
			{
				GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
				PursuerSpawnPosition[] array = UnityEngine.Object.FindObjectsByType<PursuerSpawnPosition>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
				float num = float.MaxValue;
				PursuerSpawnPosition[] array2 = array;
				foreach (PursuerSpawnPosition pursuerSpawnPosition2 in array2)
				{
					float num2 = Vector2.Distance(item.transform.position, pursuerSpawnPosition2.transform.position);
					if (num2 < num)
					{
						num = num2;
						pursuerSpawnPosition = pursuerSpawnPosition2;
					}
				}
			}
			if (pursuerSpawnPosition != null)
			{
				float chanceToPursue = GetChanceToPursue();
				if (UnityEngine.Random.Range(0f, 1f) > chanceToPursue)
				{
					PursuerActive = false;
				}
				else
				{
					TriggerPursuitSpawnEntity(pursuerSpawnPosition.transform.position);
				}
				return;
			}
			MusicActive = false;
			m_unvisitableSceneList.Add(GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Item);
			if (m_unvisitableSceneList.Count >= m_sceneDistanceToEndPursuit)
			{
				PursuerActive = false;
			}
		}
		else
		{
			PursuerActive = false;
		}
	}

	private void OnGUI()
	{
		if (s_showPursuerDebug)
		{
			GUILayout.Label("Pursuer Info");
			GUILayout.Label("System Enabled: " + m_pursuitSystemActiveVariable.Value);
			GUILayout.Label("Pursuer Active: " + PursuerActive);
			GUILayout.Label("Pursuer Total HP: " + m_pursuerHealth.Value);
			if (PursuerActive)
			{
				GUILayout.Label("Pursuer Timer: " + m_pursuitTimer);
				GUILayout.Label("Chance to pursue: " + GetChanceToPursue());
			}
			GUILayout.Label("Music Active: " + MusicActive);
			if (PursuerActive)
			{
				GUILayout.Label("Active Scene: " + m_activePursuerScene);
				GUILayout.Label("Scene Distance Count: " + m_unvisitableSceneList.Count);
			}
			else
			{
				GUILayout.Label("Time Since Last Pursuit: " + m_timeSinceLastPursuit);
			}
		}
	}

	private void UpdatePursuitTimer(float delta)
	{
		float pursuitTimer = m_pursuitTimer;
		m_pursuitTimer -= delta;
		if (m_pursuitTimer < 0f && pursuitTimer >= 0f && m_activePursuerGameObject != null)
		{
			PlayMakerFSM[] components = m_activePursuerGameObject.GetComponents<PlayMakerFSM>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].SendEvent("Retreat");
			}
		}
	}

	private void Update()
	{
		if (!m_pursuitSystemActiveVariable.Value)
		{
			return;
		}
		if (PursuerActive)
		{
			m_timeSinceLastPursuit = 0f;
			float num = ((m_activePursuerGameObject == null) ? m_timeScalarWhilePursuerCantFollow : 1f);
			UpdatePursuitTimer(Time.deltaTime * num);
			if (m_pursuitTimer < 0f && m_activePursuerGameObject == null)
			{
				PursuerActive = false;
			}
		}
		else
		{
			m_timeSinceLastPursuit += Time.deltaTime;
		}
	}
}
