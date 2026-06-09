using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Serialization;

[RequireComponent(typeof(LevelTransition))]
public class LevelTransitionDoor : BaseInteractable, IPersistentComponent, IObjectVisibilityListener, IDamageable
{
	public enum LockType
	{
		Unlocked,
		ApplyItem,
		Minigame,
		ScriptedEvent,
		UnlockFromThisSide,
		UnlockFromOtherSide,
		PermanentlyBlocked,
		DestroyableLock
	}

	public enum LevelTransitionDoorStringType
	{
		Enter,
		Exit
	}

	[Serializable]
	private class PersistentData
	{
		public bool m_unlocked;
	}

	[Header("Lock Settings")]
	[SerializeField]
	private LockType m_lockType;

	[SerializeField]
	private ItemDefinition m_requiredItem;

	[SerializeField]
	private bool m_consumeItem;

	[FormerlySerializedAs("m_interactUIPrefab")]
	[SerializeField]
	private GameObject m_overrideInteractUIPrefab;

	[SerializeField]
	private LocalizedString m_lockedExamineString = new LocalizedString("Examinable", "Shared_Door_Locked_Elsewhere");

	[SerializeField]
	private MinigameMetadata m_minigameMetadata;

	[SerializeField]
	public SideDoorLockVisuals.LockVisualsType m_lockVisuals;

	[Header("Door Visuals Settings")]
	[SerializeField]
	private Transform[] m_doorVisualsParentTransforms;

	[SerializeField]
	private MeshRenderer[] m_doorFrameRenderers;

	[Header("Variable")]
	[SerializeField]
	private ProgressionVariable m_doorUnlockedVariable;

	[Header("Animations")]
	[SerializeField]
	private PlayMakerFSM m_doorFSM;

	[SerializeField]
	private bool m_canSprintOpen;

	[Header("Strings")]
	[SerializeField]
	protected LevelTransitionDoorStringType m_doorStringType;

	[Header("Collider")]
	[SerializeField]
	private Collider2D m_collider;

	[Header("Lock Visuals Transform")]
	[SerializeField]
	private Transform m_lockVisualsParentTransform;

	[Header("Damage")]
	[SerializeField]
	private DamageCollider m_doorBashDamageCollider;

	[Header("Event")]
	[SerializeField]
	private UnityEvent m_onDoorOpenEvent;

	private LevelTransition m_levelTransition;

	private bool m_isVisible = true;

	private bool m_hasBeenUnlocked;

	private BaseInteractableCommand m_activeCommand;

	private Coroutine m_lockedCoroutine;

	private bool m_interactionDone;

	private DoorVisuals m_doorVisuals;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public Transform[] DoorVisualsParentTrasnforms => m_doorVisualsParentTransforms;

	public MeshRenderer[] DoorFrameRendererers => m_doorFrameRenderers;

	public override string InteractString
	{
		get
		{
			if (m_levelTransition != null && m_levelTransition.TargetLevel != null)
			{
				switch (m_doorStringType)
				{
				case LevelTransitionDoorStringType.Enter:
					return new LocalizedString("InteractPrompts", "Enter_Basic").GetLocalizedString();
				case LevelTransitionDoorStringType.Exit:
					return new LocalizedString("InteractPrompts", "Exit_Basic").GetLocalizedString();
				}
			}
			return new LocalizedString("InteractPrompts", "Enter_Basic").GetLocalizedString();
		}
	}

	public bool InteractionDone => m_interactionDone;

	private DoorVisuals DoorVisuals
	{
		get
		{
			if (m_doorVisuals == null)
			{
				m_doorVisuals = GetComponentInChildren<DoorVisuals>();
			}
			return m_doorVisuals;
		}
	}

	public override InteractType GetInteractType()
	{
		return InteractType.LevelTransitionDoor;
	}

	private GameObject GetApplyInteractPrefab()
	{
		if (m_overrideInteractUIPrefab != null)
		{
			return m_overrideInteractUIPrefab;
		}
		if (DoorVisuals != null)
		{
			return DoorVisuals.ApplyInteractPrefab;
		}
		return m_overrideInteractUIPrefab;
	}

	public bool AllowPassThroughProjectile()
	{
		return DoorVisuals.AllowPassthroughProjectiles;
	}

	public bool ShouldForceMeleeRebound()
	{
		return true;
	}

	private void Awake()
	{
		m_levelTransition = GetComponent<LevelTransition>();
	}

	protected override void Start()
	{
		base.Start();
		SurfaceType component = GetComponent<SurfaceType>();
		if (component != null)
		{
			component.Surface = DoorVisuals.SurfaceSettings;
		}
		if (DoorVisuals != null)
		{
			m_promptPositionTransform = DoorVisuals.PromptPosition;
		}
		if (m_doorUnlockedVariable != null && !m_hasBeenUnlocked)
		{
			m_hasBeenUnlocked = m_doorUnlockedVariable.Value;
		}
		SetLockVisualsState(!IsLocked());
		if (m_lockType == LockType.DestroyableLock)
		{
			SideDoorLockVisuals componentInChildren = m_lockVisualsParentTransform.GetComponentInChildren<SideDoorLockVisuals>();
			if (componentInChildren != null)
			{
				componentInChildren.OnLockBroken = (UnityAction)Delegate.Combine(componentInChildren.OnLockBroken, new UnityAction(UnlockDoor));
			}
			else
			{
				Debug.LogError(base.gameObject.name + " has a destroyable lock set for the lock but no door visuals were found for this side!", this);
			}
		}
	}

	public override void Interact(BaseInteractor interactor)
	{
		m_interactionDone = false;
		EnemyMigrationManager enemyMigrationManager = EnemyMigrationManager.Get();
		if (enemyMigrationManager != null && enemyMigrationManager.IsEnemyMigrating(m_levelTransition))
		{
			StartCoroutine(DoorBlockedByEnemyFlow(interactor));
		}
		else if (IsLocked() && !GameDebugCommands.CHEAT_MASTER_KEY)
		{
			m_lockedCoroutine = StartCoroutine(DoLockedInteractFlow(interactor));
		}
		else
		{
			StartCoroutine(InteractWithDoor(interactor));
		}
	}

	private void PlayDoorLockedAudio()
	{
		if (m_doorVisuals != null)
		{
			m_doorVisuals.PlayDoorAudioEvent(DoorVisuals.DoorAudioEvent.Locked);
		}
	}

	private IEnumerator DoorBlockedByEnemyFlow(BaseInteractor interactor)
	{
		InteractionResult result = new InteractionResult();
		m_activeCommand = null;
		yield return new WaitForEndOfFrame();
		PlayMakerFSM component = interactor.GetComponent<PlayMakerFSM>();
		if (component != null)
		{
			if (m_levelTransition.Direction == LevelTransition.LeaveDirection.ToLeft || m_levelTransition.Direction == LevelTransition.LeaveDirection.ToRight)
			{
				component.SendEvent("Interact/LevelTransitionDoor/LockedSideways");
			}
			else if (interactor.transform.position.z > base.transform.position.z)
			{
				component.SendEvent("Interact/LevelTransitionDoor/LockedTowardsCamera");
			}
			else
			{
				component.SendEvent("Interact/LevelTransitionDoor/Locked");
			}
		}
		PlayDoorLockedAudio();
		m_activeCommand = InteractableVoicedAudioCommand.CreateInstance(GlobalReferences.Instance.DoorInteractSettings.m_doorEnemyBlockingVoiceEvent, waitForCompletion: false);
		m_activeCommand.SetupInteraction(this, interactor);
		yield return m_activeCommand.DoInteraction(this, interactor, result);
		m_activeCommand.FinishInteraction(this, interactor);
		m_activeCommand = null;
		m_interactionDone = true;
	}

	private IEnumerator DoLockedInteractFlow(BaseInteractor interactor)
	{
		InteractionResult result = new InteractionResult();
		m_activeCommand = null;
		bool canUnlock = true;
		yield return new WaitForEndOfFrame();
		PlayMakerFSM fsm = interactor.GetComponent<PlayMakerFSM>();
		if (fsm != null)
		{
			if (m_levelTransition.Direction == LevelTransition.LeaveDirection.ToLeft || m_levelTransition.Direction == LevelTransition.LeaveDirection.ToRight)
			{
				fsm.SendEvent("Interact/LevelTransitionDoor/LockedSideways");
			}
			else if (interactor.transform.position.z > base.transform.position.z)
			{
				fsm.SendEvent("Interact/LevelTransitionDoor/LockedTowardsCamera");
			}
			else
			{
				fsm.SendEvent("Interact/LevelTransitionDoor/Locked");
			}
		}
		if (m_levelTransition != null)
		{
			m_levelTransition.MarkAsLockedOnMap();
		}
		bool flag = true;
		bool playUnlockedAudio = false;
		switch (m_lockType)
		{
		case LockType.ApplyItem:
			m_activeCommand = InteractableApplyItemCommand.CreateInstance(m_requiredItem, m_consumeItem, GetApplyInteractPrefab());
			flag = false;
			break;
		case LockType.Minigame:
			m_activeCommand = InteractableMinigameCommand.CreateInstance(m_minigameMetadata);
			flag = false;
			break;
		case LockType.UnlockFromThisSide:
			m_activeCommand = InteractableExaminableCommand.CreateInstance(new LocalizedString("Examinable", "Shared_DoorOneWay_UnlockArea_1"), isChoice: true);
			playUnlockedAudio = true;
			break;
		case LockType.UnlockFromOtherSide:
			m_activeCommand = InteractableVoicedAudioCommand.CreateInstance(GlobalReferences.Instance.DoorInteractSettings.m_doorLockedOtherSideVoiceEvent, waitForCompletion: false);
			canUnlock = false;
			break;
		case LockType.ScriptedEvent:
			m_activeCommand = InteractableExaminableCommand.CreateInstance(m_lockedExamineString, isChoice: false);
			canUnlock = false;
			break;
		case LockType.PermanentlyBlocked:
			m_activeCommand = InteractableVoicedAudioCommand.CreateInstance(GlobalReferences.Instance.DoorInteractSettings.m_doorLockedPermanentlyBlockedVoiceEvent, waitForCompletion: false);
			canUnlock = false;
			break;
		case LockType.DestroyableLock:
			if (m_lockVisuals == SideDoorLockVisuals.LockVisualsType.BreakablePadlock)
			{
				m_activeCommand = InteractableVoicedAudioCommand.CreateInstance(GlobalReferences.Instance.DoorInteractSettings.m_doorLockedPadlockVoiceEvent, waitForCompletion: false);
			}
			else
			{
				Debug.LogError("Invalid lock visuals type for DestroyableLock on gameobject " + base.gameObject.name);
			}
			canUnlock = false;
			break;
		}
		if (flag)
		{
			PlayDoorLockedAudio();
		}
		m_activeCommand.SetupInteraction(this, interactor);
		yield return m_activeCommand.DoInteraction(this, interactor, result);
		m_activeCommand.FinishInteraction(this, interactor);
		m_activeCommand = null;
		if (!result.m_cancel && canUnlock)
		{
			UnlockDoor();
			if (playUnlockedAudio && m_doorVisuals != null)
			{
				m_doorVisuals.PlayDoorAudioEvent(DoorVisuals.DoorAudioEvent.Unlock);
			}
			yield return InteractWithDoor(interactor);
		}
		else if (m_lockType == LockType.UnlockFromOtherSide)
		{
			fsm.SendEvent("Interact/LevelTransitionDoor/Locked");
			yield return new WaitForSeconds(1f);
		}
		m_interactionDone = true;
	}

	private void SetLockVisualsState(bool unlocked)
	{
		SideDoorLockVisuals[] componentsInChildren = GetComponentsInChildren<SideDoorLockVisuals>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SetUnlockedState(unlocked);
		}
		if (DoorVisuals != null)
		{
			DoorVisuals.SetUnlockedState(unlocked);
		}
	}

	public IEnumerator InteractWithDoor(BaseInteractor interactor)
	{
		yield return new WaitForEndOfFrame();
		DamageBlock component = interactor.GetComponent<DamageBlock>();
		if (component != null)
		{
			component.ActivateDamageBlock(touchOnly: false);
		}
		GlobalReferences.Instance.EventChannels.Camera.PauseCameraFollow.Raise(value: true);
		bool isSprinting = false;
		if (m_canSprintOpen)
		{
			BaseCharacterInput component2 = interactor.GetComponent<BaseCharacterInput>();
			if (component2 != null && component2.IsSprinting)
			{
				isSprinting = true;
			}
		}
		if (m_doorFSM != null)
		{
			m_doorFSM.SendEvent(isSprinting ? "TransitionDoor/SprintOpen" : "TransitionDoor/Open");
		}
		m_onDoorOpenEvent.Invoke();
		TempDisableCollision(1f);
		float seconds = (isSprinting ? 0.4f : 1f);
		PlayMakerFSM component3 = interactor.GetComponent<PlayMakerFSM>();
		if (component3 != null)
		{
			switch (m_levelTransition.Direction)
			{
			case LevelTransition.LeaveDirection.TowardsCamera:
				component3.SendEvent("Interact/LevelTransitionDoor/TowardsCamera");
				seconds = 0f;
				break;
			case LevelTransition.LeaveDirection.AwayFromCamera:
				component3.SendEvent("Interact/LevelTransitionDoor/OpenAwayFromCamera");
				break;
			case LevelTransition.LeaveDirection.ToLeft:
			case LevelTransition.LeaveDirection.ToRight:
				component3.SendEvent("Interact/LevelTransitionDoor/TravelSidewaysDoor");
				break;
			}
		}
		yield return new WaitForSeconds(seconds);
		m_levelTransition.DoTransition(isSprinting);
		yield return new WaitForSeconds(3f);
		m_interactionDone = true;
	}

	private bool IsLocked()
	{
		if (m_lockType == LockType.Unlocked)
		{
			return false;
		}
		if (m_doorUnlockedVariable != null && m_doorUnlockedVariable.Value)
		{
			return false;
		}
		return !m_hasBeenUnlocked;
	}

	public override bool CanInteract(BaseInteractor interactor)
	{
		if (base.enabled)
		{
			return m_isVisible;
		}
		return false;
	}

	public void SetObjectVisibility(bool visible)
	{
		m_isVisible = visible;
	}

	public void UnlockDoor()
	{
		if (m_levelTransition != null)
		{
			m_levelTransition.MarkAsOpenOnMap();
		}
		m_hasBeenUnlocked = true;
		if (m_doorUnlockedVariable != null)
		{
			m_doorUnlockedVariable.SetValue(value: true);
		}
		if (m_persistentData != null)
		{
			m_persistentData.m_unlocked = true;
		}
		SetLockVisualsState(!IsLocked());
	}

	public void OpenForEnemyMigration(CharacterMovement movement)
	{
		if (m_doorFSM != null)
		{
			m_doorFSM.SendEvent("TransitionDoor/Enemy");
		}
		GameObject source = null;
		if (movement != null)
		{
			if (m_collider != null)
			{
				movement.TempIgnoreCollider(m_collider, 2f);
			}
			source = movement.gameObject;
		}
		if (m_doorBashDamageCollider != null)
		{
			StartCoroutine(DoBreakDamageCollider(0.1f, source));
		}
	}

	private IEnumerator DoBreakDamageCollider(float delay, GameObject source)
	{
		yield return new WaitForSeconds(delay);
		m_doorBashDamageCollider.Source = source;
		m_doorBashDamageCollider.gameObject.SetActive(value: true);
		m_doorBashDamageCollider.SetDamageEnabled(enabled: true);
		yield return new WaitForSeconds(0.5f);
		m_doorBashDamageCollider.gameObject.SetActive(value: false);
		m_doorBashDamageCollider.SetDamageEnabled(enabled: false);
	}

	public void TempDisableCollision(float time)
	{
		if (m_collider != null)
		{
			m_collider.enabled = false;
		}
		StartCoroutine(ReenableCollision(time));
	}

	private IEnumerator ReenableCollision(float time)
	{
		yield return new WaitForSeconds(time);
		if (m_collider != null)
		{
			m_collider.enabled = true;
		}
	}

	public void ApplyDamageInstance(DamageInstance instance)
	{
	}

	public MapConnectionType GetMapConnectionType()
	{
		if (m_lockType == LockType.ApplyItem)
		{
			return MapConnectionType.DoorApplyItem;
		}
		return MapConnectionType.Door;
	}

	public ItemDefinition GetRequiredItem()
	{
		if (m_lockType == LockType.ApplyItem)
		{
			return m_requiredItem;
		}
		return null;
	}

	public override void FinishedInteract()
	{
		base.FinishedInteract();
		if (m_activeCommand != null)
		{
			m_activeCommand.Cancel();
			m_activeCommand = null;
		}
		if (m_lockedCoroutine != null)
		{
			StopCoroutine(m_lockedCoroutine);
		}
		m_interactionDone = true;
	}

	public bool RequiresPersistentData()
	{
		if (m_lockType != 0)
		{
			return m_doorUnlockedVariable == null;
		}
		return false;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData != null)
		{
			m_hasBeenUnlocked = m_persistentData.m_unlocked;
			SetLockVisualsState(!IsLocked());
		}
		else
		{
			m_persistentData = new PersistentData();
			m_persistentDataObject.Data = m_persistentData;
		}
	}
}
