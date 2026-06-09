using System;
using System.Collections;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[ShowInDesignerInspector]
public class LevelTransition : MonoBehaviour
{
	public enum ArrivalDirection
	{
		None,
		MoveLeft,
		MoveRight,
		AwayFromCameraToLeft,
		AwayFromCameraToRight,
		TowardsCameraToLeft,
		TowardsCameraToRight,
		TowardsCameraFromDoorToLeft,
		TowardsCameraFromDoorToRight,
		LadderDown,
		LadderUp
	}

	public enum ArrivalState
	{
		Normal,
		Crouching,
		Falling,
		LadderAttach,
		Ledge
	}

	public enum LeaveDirection
	{
		None,
		ToLeft,
		ToRight,
		AwayFromCamera,
		TowardsCamera,
		DisableCharacter,
		LadderUp,
		LadderDown
	}

	[SerializeField]
	private MapConnection m_mapConnection;

	[SerializeField]
	private LevelMetadata m_targetLevelMetadata;

	[FormerlySerializedAs("m_targetDoorName")]
	[SerializeField]
	private string m_targetEntryPoint;

	[SerializeField]
	private bool m_triggerOnTouch;

	[SerializeField]
	private ArrivalDirection m_arrivalDirection;

	[SerializeField]
	private ArrivalState m_arrivalState;

	[SerializeField]
	private float m_arriveForceMoveTimeOverride = -1f;

	[SerializeField]
	private UnityEvent m_onArriveEvent;

	[SerializeField]
	private bool m_showNearbyPrompt;

	[SerializeField]
	public Bounds m_nearbyBounds;

	[FormerlySerializedAs("m_transitionDirection")]
	[SerializeField]
	private LeaveDirection m_leaveDirection;

	[SerializeField]
	private AudioEvent m_leaveAudioEvent;

	[SerializeField]
	private EventReference m_transitionFMODEvent;

	[SerializeField]
	private AudioEvent m_arriveAudioEvent;

	[Header("Ladder Transition")]
	[SerializeField]
	private Ladder m_ladder;

	private bool m_triggered;

	private bool m_canBeTriggered;

	private bool m_isPlayerNear;

	private Collider2D m_collider;

	[Header("Enemy Migration")]
	[SerializeField]
	private bool m_allowEnemyMigration;

	[Header("Fade Type")]
	[SerializeField]
	private LevelTransitionFadeType m_fadeType;

	[Header("Map")]
	[SerializeField]
	private MapConnectionType m_connectionType;

	[SerializeField]
	private SimpleHealth m_associatedBarrier;

	public MapConnection MapConnection
	{
		get
		{
			return m_mapConnection;
		}
		set
		{
			m_mapConnection = value;
		}
	}

	public LevelMetadata TargetLevel => m_targetLevelMetadata;

	public string TargetEntryPoint => m_targetEntryPoint;

	public ArrivalDirection TransitionArrivalDirection => m_arrivalDirection;

	public float ArriveForceMoveTimeOverride => m_arriveForceMoveTimeOverride;

	public ArrivalState TransitionArrivalState => m_arrivalState;

	public bool ShowNearbyPrompt => m_showNearbyPrompt;

	public LeaveDirection Direction => m_leaveDirection;

	public AudioEvent ArriveAudioEvent => m_arriveAudioEvent;

	public Ladder Ladder => m_ladder;

	public MapConnectionType MapConnectionType => m_connectionType;

	public void SetFadeType(LevelTransitionFadeType fadeType)
	{
		m_fadeType = fadeType;
	}

	public void SetRedFade()
	{
		m_fadeType = LevelTransitionFadeType.Red;
	}

	private void Start()
	{
		m_collider = GetComponent<Collider2D>();
		if (m_associatedBarrier != null && !m_associatedBarrier.IsDead && m_connectionType != 0)
		{
			MarkAsLockedOnMap();
			SimpleHealth associatedBarrier = m_associatedBarrier;
			associatedBarrier.OnDeadEvent = (UnityAction)Delegate.Combine(associatedBarrier.OnDeadEvent, new UnityAction(OnAssociatedBarrierDestroyed));
		}
	}

	private void OnAssociatedBarrierDestroyed()
	{
		MarkAsOpenOnMap();
	}

	private void OnEnable()
	{
		m_isPlayerNear = false;
		GlobalReferences.Instance.EventChannels.LevelTransition.EnableLevelTransitionTriggers.Register(EnableTrigger);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.LevelTransition.EnableLevelTransitionTriggers.Unregister(EnableTrigger);
	}

	private void EnableTrigger()
	{
		m_canBeTriggered = true;
	}

	public void DoTransition()
	{
		DoTransition(forceSprinting: false);
	}

	public void DoTransition(bool forceSprinting = false)
	{
		if (m_triggered)
		{
			return;
		}
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		bool isSprinting = forceSprinting;
		if (item != null)
		{
			CharacterHealth component = item.GetComponent<CharacterHealth>();
			if (component != null && component.IsDead)
			{
				Debug.Log("Ignoring level transition request because the player is dead!");
				return;
			}
			DamageBlock component2 = item.GetComponent<DamageBlock>();
			if (component2 != null)
			{
				component2.ActivateDamageBlock(touchOnly: false);
			}
			if (!forceSprinting)
			{
				BaseCharacterInput component3 = item.GetComponent<BaseCharacterInput>();
				if (component3 != null)
				{
					isSprinting = component3.IsSprinting;
				}
			}
		}
		GlobalReferences.Instance.EventChannels.Camera.PauseCameraFollow.Raise(value: true);
		m_triggered = true;
		LevelTransitionEventData value = new LevelTransitionEventData
		{
			m_levelMetadata = m_targetLevelMetadata,
			m_levelTransitionComponent = this,
			m_targetSceneName = m_targetLevelMetadata.SceneFileName,
			m_targetSceneAssetReference = m_targetLevelMetadata.TargetSceneAssetReference,
			m_targetEntryPoint = m_targetEntryPoint,
			m_type = LevelTransitionEventType.Transition,
			m_arrivalDirection = m_arrivalDirection,
			m_leaveDirection = m_leaveDirection,
			m_leavingTransitionAudioEvent = m_leaveAudioEvent,
			m_transitionAudioEvent = m_transitionFMODEvent,
			m_canEnemiesMigrate = m_allowEnemyMigration,
			m_transitionFromPosition = base.transform.position,
			m_isSprinting = isSprinting,
			m_fadeType = m_fadeType
		};
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransition.Raise(value);
		GlobalReferences.Instance.MapDynamicData.SetConnectionStatus(m_mapConnection.ConnectionID, MapConnectionStatus.Open);
		StartCoroutine(ReenableCoroutine());
	}

	private IEnumerator ReenableCoroutine()
	{
		yield return new WaitForSeconds(1f);
		m_triggered = false;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!m_triggerOnTouch || !m_canBeTriggered)
		{
			return;
		}
		CharacterIdentifier componentInParent = collision.GetComponentInParent<CharacterIdentifier>();
		if (!(componentInParent != null) || !GameUtils.IsPlayer(componentInParent.gameObject))
		{
			return;
		}
		if (m_leaveDirection == LeaveDirection.LadderUp)
		{
			CharacterTraversalUtils component = componentInParent.gameObject.GetComponent<CharacterTraversalUtils>();
			if (component != null && component.GetLadder() == null)
			{
				return;
			}
		}
		DoTransition();
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (!m_triggerOnTouch || !m_canBeTriggered || (m_leaveDirection != LeaveDirection.ToRight && m_leaveDirection != LeaveDirection.ToLeft))
		{
			return;
		}
		CharacterIdentifier componentInParent = collision.GetComponentInParent<CharacterIdentifier>();
		if (componentInParent != null && GameUtils.IsPlayer(componentInParent.gameObject))
		{
			Vector3 position = collision.transform.position;
			Vector3 center = m_collider.bounds.center;
			if (position.x > center.x && m_leaveDirection == LeaveDirection.ToRight)
			{
				DoTransition();
			}
			else if (position.x < center.x && m_leaveDirection == LeaveDirection.ToLeft)
			{
				DoTransition();
			}
		}
	}

	private void Update()
	{
		bool flag = false;
		if (m_showNearbyPrompt && (m_collider == null || m_collider.enabled) && GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor != null)
		{
			GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
			if (item != null)
			{
				Vector2 vector = item.transform.position;
				Vector2 vector2 = m_nearbyBounds.center;
				vector2.x += base.transform.position.x;
				vector2.y += base.transform.position.y;
				Vector3 size = m_nearbyBounds.size;
				size.x += 0.5f;
				size.y += 1.5f;
				size.z = 10f;
				vector2.y -= 0.75f;
				if (new Bounds(vector2, size).Contains(vector))
				{
					CharacterDirection component = item.GetComponent<CharacterDirection>();
					if (m_leaveDirection == LeaveDirection.None || m_leaveDirection == LeaveDirection.AwayFromCamera || m_leaveDirection == LeaveDirection.TowardsCamera || (component.CurrentDirection == CharacterDirection.Facing.Right && m_leaveDirection == LeaveDirection.ToRight) || (component.CurrentDirection == CharacterDirection.Facing.Left && m_leaveDirection == LeaveDirection.ToLeft))
					{
						flag = true;
					}
				}
			}
		}
		if (m_ladder != null)
		{
			GameObject item2 = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
			if (item2 != null)
			{
				CharacterTraversalUtils component2 = item2.GetComponent<CharacterTraversalUtils>();
				CharacterInputPlayer component3 = item2.GetComponent<CharacterInputPlayer>();
				if (component2 != null && component3 != null && component2.m_ladderData.m_attachedLadder == m_ladder)
				{
					if (m_leaveDirection == LeaveDirection.LadderUp)
					{
						if (item2.transform.position.y > m_collider.bounds.max.y - 1f && component3.MovementInput.y > 0f)
						{
							DoTransition();
						}
					}
					else if (m_leaveDirection == LeaveDirection.LadderDown && component3.MovementInput.y < 0f && item2.transform.position.y < m_collider.bounds.min.y)
					{
						DoTransition();
					}
				}
			}
		}
		if (m_isPlayerNear != flag)
		{
			SetPlayerNearTransition(flag);
			m_isPlayerNear = flag;
		}
	}

	private void SetPlayerNearTransition(bool isNear)
	{
		GlobalReferences.Instance.EventChannels.LevelTransition.NearbyLevelTransitionObject.Raise(isNear ? base.gameObject : null);
	}

	public void DoArriveEvent()
	{
		m_onArriveEvent?.Invoke();
	}

	public void MarkAsLockedOnMap()
	{
		SetMapConnectionStatus(MapConnectionStatus.Locked);
	}

	public void MarkAsOpenOnMap()
	{
		SetMapConnectionStatus(MapConnectionStatus.Open);
	}

	public void SetMapConnectionStatus(MapConnectionStatus status)
	{
		if (MapConnection.ConnectionID != 0)
		{
			GlobalReferences.Instance.MapDynamicData.SetConnectionStatus(MapConnection.ConnectionID, status);
		}
	}

	private static LevelTransition FindMatchingLevelTransitionFromList(LevelTransition[] transitions, string targetEntryPointName)
	{
		foreach (LevelTransition levelTransition in transitions)
		{
			if (levelTransition.gameObject.name.Equals(targetEntryPointName))
			{
				return levelTransition;
			}
			if (levelTransition.gameObject.name.Equals("Old_" + targetEntryPointName))
			{
				return levelTransition;
			}
		}
		return null;
	}
}
