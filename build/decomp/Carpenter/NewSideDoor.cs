using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Serialization;

public class NewSideDoor : BaseInteractable, IPersistentComponent, IDamageable, IObjectVisibilityListener
{
	public enum LockType
	{
		Unlocked,
		ApplyItem,
		Minigame,
		ScriptedEvent,
		LockedFromThisSide,
		UnlockFromSide,
		BossEntrance,
		PermanentlyBlocked,
		DestroyableLock
	}

	[Serializable]
	private class DoorLockSettings
	{
		[SerializeField]
		public LockType m_lockType;

		[SerializeField]
		public SideDoorLockVisuals.LockVisualsType m_lockVisuals;

		[SerializeField]
		public ItemDefinition m_requiredItem;

		[SerializeField]
		public bool m_consumeItem;

		[FormerlySerializedAs("m_interactUIPrefab")]
		[SerializeField]
		public GameObject m_overrideInteractUIPrefab;

		[SerializeField]
		public MinigameMetadata m_minigameMetadata;

		[SerializeField]
		public LocalizedString m_lockedExamineString = new LocalizedString("Examinable", "Shared_Door_Locked_Elsewhere");

		public void SetNormalDoorLockType()
		{
			m_lockType = LockType.Unlocked;
		}
	}

	public enum DoorForcedPlayerMovement
	{
		None,
		ForceSprint
	}

	public enum DoorState
	{
		Closed,
		OpenLeft,
		OpenRight
	}

	public enum DoorAnimationVariant
	{
		Normal,
		Walking,
		Sprinting,
		CloseFar,
		CloseNear
	}

	[Serializable]
	public struct DoorAnimationSettings
	{
		public float m_animationDelay;

		public float m_snapToPosition;

		public DoorVisuals.DoorAudioEvent m_audioEventType;

		public float m_audioEventDelay;

		public AnimationCurve m_animationCurve;

		public float m_animDuration;

		public bool m_triggerDamage;

		public float m_triggerDamageDelay;

		public int m_damageDoorAmount;

		public float m_doorAnimationTime;
	}

	[Serializable]
	private class PersistentData
	{
		public DoorState m_doorState;

		public bool m_unlocked;

		public bool m_hasBeenDestroyed;
	}

	[Header("Lock Settings")]
	[ShowInDesignerInspector]
	[SerializeField]
	private DoorLockSettings m_leftLockSettings;

	[ShowInDesignerInspector]
	[SerializeField]
	private DoorLockSettings m_rightLockSettings;

	[Header("Animation")]
	[SerializeField]
	private DoorForcedPlayerMovement m_forcedPlayerMovement;

	[SerializeField]
	private Transform m_doorTransform;

	[SerializeField]
	private Vector3 m_openLeftRotation = new Vector3(0f, 90f, 0f);

	[SerializeField]
	private Vector3 m_openRightRotation = new Vector3(0f, -90f, 0f);

	[SerializeField]
	private DoorAnimationSettings m_closeDoorFarAnimationSettings;

	[SerializeField]
	private DoorAnimationSettings m_closeDoorNearAnimationSettings;

	[SerializeField]
	private DoorAnimationSettings m_openStandingDoorAnimationSettings;

	[SerializeField]
	private DoorAnimationSettings m_openWalkingDoorAnimationSettings;

	[SerializeField]
	private DoorAnimationSettings m_openSprintingDoorAnimationSettings;

	[Header("On Hit Effect")]
	[SerializeField]
	private HitReactEffectSettings m_onHitEffect;

	[SerializeField]
	private int m_impactPunchVibrato = 30;

	[SerializeField]
	private float m_impactPunchDuration = 0.5f;

	[SerializeField]
	private float m_impactPunchElasticity = 1f;

	[SerializeField]
	private float m_impactPunchDistance = 0.05f;

	[Header("Health")]
	[SerializeField]
	private bool m_indestructibleWhileLocked = true;

	[SerializeField]
	private bool m_forceIndestructible;

	[Header("Door State Components")]
	[SerializeField]
	private Collider2D m_doorCollider;

	[SerializeField]
	private CameraBoundsExtender m_cameraBoundsExtender;

	[Header("Door Break")]
	[SerializeField]
	private DamageCollider m_doorBreakDamageCollider;

	[SerializeField]
	private float m_doorDestroyedDamageDelay = 0.5f;

	[Header("Strings")]
	[SerializeField]
	private LocalizedString m_openTextString = new LocalizedString("InteractPrompts", "Open");

	[SerializeField]
	private LocalizedString m_closeTextString = new LocalizedString("InteractPrompts", "Close");

	[Header("Scripting")]
	[SerializeField]
	private UnityEvent m_onDoorOpenEvent;

	[SerializeField]
	private UnityEvent m_onDoorUnlockedEvent;

	[Header("Lock Visuals Transform")]
	[SerializeField]
	private Transform m_leftLockVisualsParentTransform;

	[SerializeField]
	private Transform m_rightLockVisualsParentTransform;

	[Header("Door Visuals Settings")]
	[SerializeField]
	private Transform m_doorVisualsParentTransform;

	[SerializeField]
	private MeshRenderer[] m_doorFrameRenderers;

	private LevelMapAreaSeparator m_mapAreaSeparator;

	private bool m_interactionDone;

	private bool m_isVisible = true;

	private bool m_doorDisable;

	private int m_damageStateIndex = -1;

	private static readonly float s_updateRate = 1f / 24f;

	private Tween m_activeTween;

	private Coroutine m_coroutine;

	private int m_currentHealth;

	private Vector3 m_doorTransformStartLocalPosition;

	private bool m_hasBeenUnlocked;

	private bool m_hasBeenDestroyed;

	private bool m_doorAnimationActive;

	private DoorState m_doorState;

	private DoorVisuals m_doorVisuals;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public DoorForcedPlayerMovement ForcedPlayerMovement => m_forcedPlayerMovement;

	public Transform DoorVisualsParentTrasnform => m_doorVisualsParentTransform;

	public MeshRenderer[] DoorFrameRendererers => m_doorFrameRenderers;

	public override string InteractString
	{
		get
		{
			if (m_doorState != 0)
			{
				return m_closeTextString.GetLocalizedString();
			}
			return m_openTextString.GetLocalizedString();
		}
	}

	public bool InteractionDone => m_interactionDone;

	private int Health
	{
		get
		{
			return m_currentHealth;
		}
		set
		{
			if (m_currentHealth != value)
			{
				m_currentHealth = value;
			}
		}
	}

	public bool DoorAnimationActive => m_doorAnimationActive;

	public DoorState State => m_doorState;

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

	public bool ShouldOccludeAudio
	{
		get
		{
			DoorVisuals doorVisuals = DoorVisuals;
			if (doorVisuals != null)
			{
				return doorVisuals.ShouldOccludeAudio;
			}
			return false;
		}
	}

	public override InteractType GetInteractType()
	{
		return InteractType.SideDoor;
	}

	public void SetDoorDisabled(bool disable)
	{
		m_doorDisable = disable;
	}

	public override bool CanInteract(BaseInteractor interactor)
	{
		bool flag = base.enabled && m_isVisible && !m_hasBeenDestroyed && !m_doorDisable;
		if (flag)
		{
			CharacterDirection component = interactor.GetComponent<CharacterDirection>();
			if (component != null && !component.IsFacingPoint(base.transform.position))
			{
				flag = false;
			}
		}
		return flag;
	}

	public bool IsClosed()
	{
		return m_doorState == DoorState.Closed;
	}

	private GameObject GetApplyInteractPrefab(DoorLockSettings settings)
	{
		if (settings.m_overrideInteractUIPrefab != null)
		{
			return settings.m_overrideInteractUIPrefab;
		}
		if (DoorVisuals != null)
		{
			return DoorVisuals.ApplyInteractPrefab;
		}
		return settings.m_overrideInteractUIPrefab;
	}

	public bool AllowPassThroughProjectile()
	{
		return DoorVisuals.AllowPassthroughProjectiles;
	}

	protected override void Start()
	{
		base.Start();
		m_mapAreaSeparator = GetComponent<LevelMapAreaSeparator>();
		SurfaceType component = GetComponent<SurfaceType>();
		if (component != null)
		{
			component.Surface = DoorVisuals.SurfaceSettings;
		}
		m_promptPositionTransform = DoorVisuals.PromptPosition;
		m_doorTransformStartLocalPosition = m_doorTransform.localPosition;
		Health = ((!m_hasBeenDestroyed) ? DoorVisuals.StartingDoorHealth : 0);
		SetLockVisualsState(m_hasBeenUnlocked);
		if (m_leftLockSettings.m_lockType == LockType.DestroyableLock)
		{
			SideDoorLockVisuals componentInChildren = m_leftLockVisualsParentTransform.GetComponentInChildren<SideDoorLockVisuals>();
			if (componentInChildren != null)
			{
				componentInChildren.OnLockBroken = (UnityAction)Delegate.Combine(componentInChildren.OnLockBroken, new UnityAction(UnlockDoor));
			}
			else
			{
				Debug.LogError(base.gameObject.name + " has a destroyable lock set for the Left lock but no door visuals were found for this side!", this);
			}
		}
		if (m_rightLockSettings.m_lockType == LockType.DestroyableLock)
		{
			SideDoorLockVisuals componentInChildren2 = m_rightLockVisualsParentTransform.GetComponentInChildren<SideDoorLockVisuals>();
			if (componentInChildren2 != null)
			{
				componentInChildren2.OnLockBroken = (UnityAction)Delegate.Combine(componentInChildren2.OnLockBroken, new UnityAction(UnlockDoor));
			}
			else
			{
				Debug.LogError(base.gameObject.name + " has a destroyable lock set for the Right lock but no door visuals were found for this side!", this);
			}
		}
		for (int i = 0; i < DoorVisuals.DamageStates.Length; i++)
		{
			if (DoorVisuals.DamageStates[i].m_enabledChildObject != null)
			{
				DoorVisuals.DamageStates[i].m_enabledChildObject.SetActive(value: false);
			}
			else
			{
				Debug.LogError("Door " + base.gameObject.name + "has broken damage states");
			}
		}
		m_damageStateIndex = -1;
		UpdateHealthDamageState(null);
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

	public override void Interact(BaseInteractor interactor)
	{
		m_interactionDone = false;
		bool flag = interactor.transform.position.x > base.transform.position.x;
		if (IsLocked(flag) && !GameDebugCommands.CHEAT_MASTER_KEY)
		{
			StartCoroutine(DoLockedInteractFlow(interactor, flag));
		}
		else
		{
			StartCoroutine(InteractWithDoor(interactor, (m_doorState == DoorState.Closed) ? true : false, flag));
		}
	}

	private IEnumerator DoLockedInteractFlow(BaseInteractor interactor, bool rightSide)
	{
		InteractionResult result = new InteractionResult();
		BaseInteractableCommand command = null;
		bool canUnlock = true;
		yield return new WaitForEndOfFrame();
		PositionCharacterForDistance(interactor.gameObject, 0.75f);
		PlayMakerFSM component = interactor.GetComponent<PlayMakerFSM>();
		if (component != null)
		{
			component.SendEvent("Interact/SideDoor/Locked");
		}
		if (m_mapAreaSeparator != null)
		{
			m_mapAreaSeparator.SetMapConnectionStatus(MapConnectionStatus.Locked);
		}
		bool flag = true;
		bool playUnlockedAudio = false;
		DoorLockSettings settings = (rightSide ? m_rightLockSettings : m_leftLockSettings);
		switch (settings.m_lockType)
		{
		case LockType.ApplyItem:
			command = InteractableApplyItemCommand.CreateInstance(settings.m_requiredItem, settings.m_consumeItem, GetApplyInteractPrefab(settings));
			flag = false;
			break;
		case LockType.Minigame:
			command = InteractableMinigameCommand.CreateInstance(settings.m_minigameMetadata);
			flag = false;
			break;
		case LockType.LockedFromThisSide:
			command = InteractableVoicedAudioCommand.CreateInstance(GlobalReferences.Instance.DoorInteractSettings.m_doorLockedOtherSideVoiceEvent, waitForCompletion: false);
			canUnlock = false;
			break;
		case LockType.UnlockFromSide:
			command = InteractableExaminableCommand.CreateInstance(new LocalizedString("Examinable", "Shared_DoorOneWay_UnlockArea_1"), isChoice: true);
			playUnlockedAudio = true;
			break;
		case LockType.PermanentlyBlocked:
			command = InteractableVoicedAudioCommand.CreateInstance(GlobalReferences.Instance.DoorInteractSettings.m_doorLockedPermanentlyBlockedVoiceEvent, waitForCompletion: false);
			canUnlock = false;
			break;
		case LockType.BossEntrance:
			command = InteractableExaminableCommand.CreateInstance(new LocalizedString("Examinable", "Shared_BossDoorInteract"), isChoice: true);
			flag = false;
			break;
		case LockType.ScriptedEvent:
			command = InteractableExaminableCommand.CreateInstance(settings.m_lockedExamineString, isChoice: false);
			canUnlock = false;
			break;
		case LockType.DestroyableLock:
			if (settings.m_lockVisuals == SideDoorLockVisuals.LockVisualsType.BreakablePadlock)
			{
				command = InteractableVoicedAudioCommand.CreateInstance(GlobalReferences.Instance.DoorInteractSettings.m_doorLockedPadlockVoiceEvent, waitForCompletion: false);
			}
			else
			{
				Debug.LogError("Invalid lock visuals type for DestroyableLock on gameobject " + base.gameObject.name);
			}
			canUnlock = false;
			break;
		}
		if (flag && m_doorVisuals != null)
		{
			m_doorVisuals.PlayDoorAudioEvent(DoorVisuals.DoorAudioEvent.Locked);
		}
		yield return new WaitForSeconds(0.5f);
		command.SetupInteraction(this, interactor);
		yield return command.DoInteraction(this, interactor, result);
		command.FinishInteraction(this, interactor);
		if (!result.m_cancel && canUnlock)
		{
			if (playUnlockedAudio && m_doorVisuals != null)
			{
				m_doorVisuals.PlayDoorAudioEvent(DoorVisuals.DoorAudioEvent.Unlock);
			}
			UnlockDoor();
			yield return InteractWithDoor(interactor, activated: true, rightSide);
			if (settings.m_lockType == LockType.BossEntrance)
			{
				GlobalReferences.Instance.EventChannels.Camera.PauseCameraFollow.Raise(value: true);
				yield return new WaitForSeconds(0.2f);
				GlobalReferences.Instance.EventChannels.Generic.ScreenFadeOfType.Raise(FadeType.RedDoor);
				yield return new WaitForSeconds(1.2f);
				SetDoorState(DoorState.Closed, instant: true);
				SetDoorDisabled(disable: true);
				GlobalReferences.Instance.EventChannels.Camera.PauseCameraFollow.Raise(value: false);
				yield return new WaitForSeconds(0.1f);
				GlobalReferences.Instance.EventChannels.Camera.ForceCameraReset.Raise();
				yield return new WaitForSeconds(0.4f);
				if (base.transform.parent != null)
				{
					Transform transform = base.transform.parent.Find("RedLight/Audio");
					if (transform != null)
					{
						transform.gameObject.SetActive(value: false);
					}
				}
				GlobalReferences.Instance.EventChannels.Generic.ScreenFadeOfType.Raise(FadeType.None);
			}
		}
		m_interactionDone = true;
	}

	public void UnlockDoor()
	{
		m_hasBeenUnlocked = true;
		if (m_persistentData != null)
		{
			m_persistentData.m_unlocked = m_hasBeenUnlocked;
		}
		m_onDoorUnlockedEvent.Invoke();
		if (m_mapAreaSeparator != null)
		{
			m_mapAreaSeparator.SetMapConnectionStatus(MapConnectionStatus.Open);
		}
		SetLockVisualsState(m_hasBeenUnlocked);
	}

	public IEnumerator InteractWithDoor(BaseInteractor interactor, bool activated, bool rightSide)
	{
		yield return new WaitForEndOfFrame();
		DoorLockSettings doorLockSettings = (rightSide ? m_rightLockSettings : m_leftLockSettings);
		PlayMakerFSM component = interactor.GetComponent<PlayMakerFSM>();
		if (component != null)
		{
			if (!activated)
			{
				bool flag = true;
				if (m_doorState == DoorState.OpenLeft)
				{
					if (interactor.transform.position.x > base.transform.position.x)
					{
						flag = false;
					}
				}
				else if (interactor.transform.position.x < base.transform.position.x)
				{
					flag = false;
				}
				component.SendEvent(flag ? "Interact/SideDoor/CloseNear" : "Interact/SideDoor/CloseFar");
			}
			else if (doorLockSettings.m_lockType == LockType.BossEntrance)
			{
				component.SendEvent("Interact/SideDoor/BossDoor");
			}
			else
			{
				component.SendEvent("Interact/SideDoor/Open");
			}
		}
		if (activated)
		{
			if (m_mapAreaSeparator != null)
			{
				m_mapAreaSeparator.SetMapConnectionStatus(MapConnectionStatus.Open);
			}
			m_onDoorOpenEvent?.Invoke();
		}
		m_interactionDone = true;
	}

	private void OnDoorStateChanged(bool opened)
	{
		if (m_cameraBoundsExtender != null)
		{
			m_cameraBoundsExtender.enabled = opened;
		}
		if (m_doorCollider != null)
		{
			m_doorCollider.enabled = !opened;
		}
	}

	private DoorAnimationSettings GetDoorAnimationSettings(DoorAnimationVariant variant)
	{
		return variant switch
		{
			DoorAnimationVariant.Normal => m_openStandingDoorAnimationSettings, 
			DoorAnimationVariant.Walking => m_openWalkingDoorAnimationSettings, 
			DoorAnimationVariant.Sprinting => m_openSprintingDoorAnimationSettings, 
			DoorAnimationVariant.CloseFar => m_closeDoorFarAnimationSettings, 
			DoorAnimationVariant.CloseNear => m_closeDoorNearAnimationSettings, 
			_ => m_openStandingDoorAnimationSettings, 
		};
	}

	private void PositionCharacterForAnimation(GameObject character, DoorAnimationVariant animationVariant)
	{
		PositionCharacterForDistance(character, GetDoorAnimationSettings(animationVariant).m_snapToPosition);
	}

	private void PositionCharacterForDistance(GameObject character, float distance)
	{
		bool num = character.transform.position.x < m_doorTransform.position.x;
		Vector3 position = character.transform.position;
		if (num)
		{
			position.x = base.transform.position.x - distance;
		}
		else
		{
			position.x = base.transform.position.x + distance;
		}
		character.transform.position = position;
		CharacterMovement component = character.GetComponent<CharacterMovement>();
		if (component != null)
		{
			component.SnapToGround();
		}
	}

	public void TriggerDoorInteractFromCharacter(GameObject interactor, bool activated, DoorAnimationVariant animationVariant)
	{
		PositionCharacterForAnimation(interactor, animationVariant);
		if (activated)
		{
			bool flag = interactor.transform.position.x < m_doorTransform.position.x;
			SetDoorState((!flag) ? DoorState.OpenLeft : DoorState.OpenRight, instant: false, animationVariant);
			DoorAnimationSettings doorAnimationSettings = GetDoorAnimationSettings(animationVariant);
			if (doorAnimationSettings.m_triggerDamage)
			{
				StartCoroutine(DoBreakDamageCollider(!flag, doorAnimationSettings.m_triggerDamageDelay, interactor, doorAnimationSettings.m_damageDoorAmount));
			}
		}
		else
		{
			SetDoorState(DoorState.Closed, instant: false, animationVariant);
		}
	}

	public void OpenDoorLeft(bool instant)
	{
		UnlockDoor();
		SetDoorState(DoorState.OpenLeft, instant);
	}

	public void OpenDoorRight(bool instant)
	{
		UnlockDoor();
		SetDoorState(DoorState.OpenLeft, instant);
	}

	public void SetDoorState(DoorState newState, bool instant = false, DoorAnimationVariant animationVariant = DoorAnimationVariant.Normal)
	{
		if (m_doorState != newState)
		{
			if (newState == DoorState.Closed)
			{
				OnDoorStateChanged(opened: false);
			}
			else
			{
				OnDoorStateChanged(opened: true);
			}
			if (m_persistentData != null)
			{
				m_persistentData.m_doorState = newState;
			}
			m_doorState = newState;
			Vector3 vector = Vector3.zero;
			switch (newState)
			{
			case DoorState.OpenLeft:
				vector = m_openLeftRotation;
				break;
			case DoorState.OpenRight:
				vector = m_openRightRotation;
				break;
			}
			m_doorAnimationActive = false;
			if (m_activeTween != null)
			{
				DOTween.Kill(m_activeTween, complete: true);
			}
			if (m_coroutine != null)
			{
				StopCoroutine(m_coroutine);
			}
			if (instant)
			{
				m_doorTransform.localRotation = Quaternion.Euler(vector);
				return;
			}
			DoorAnimationSettings doorAnimationSettings = GetDoorAnimationSettings(animationVariant);
			StartCoroutine(PlayDoorAnimation(vector, doorAnimationSettings));
		}
	}

	private IEnumerator PlayDoorAnimation(Vector3 targetRotation, DoorAnimationSettings animationSettings)
	{
		m_doorAnimationActive = true;
		yield return new WaitForSeconds(animationSettings.m_animationDelay);
		StartCoroutine(FlagAnimationEndedCoroutine(animationSettings.m_doorAnimationTime));
		AnimationCurve animationCurve = animationSettings.m_animationCurve;
		m_activeTween = m_doorTransform.DOLocalRotate(targetRotation, animationSettings.m_animDuration).SetUpdate(UpdateType.Manual).SetEase(animationCurve);
		m_coroutine = StartCoroutine(UpdateCoroutine());
		if (animationSettings.m_audioEventType != 0)
		{
			DoorVisuals.PlayDoorAudioEvent(animationSettings.m_audioEventType, animationSettings.m_audioEventDelay);
		}
	}

	private IEnumerator UpdateCoroutine()
	{
		while (m_activeTween != null && m_activeTween.IsActive())
		{
			m_activeTween.ManualUpdate(s_updateRate, s_updateRate);
			yield return new WaitForSeconds(s_updateRate);
		}
		m_doorAnimationActive = false;
	}

	private IEnumerator FlagAnimationEndedCoroutine(float delay)
	{
		yield return new WaitForSeconds(delay);
		m_doorAnimationActive = false;
	}

	private bool IsLocked(bool right)
	{
		if (m_hasBeenUnlocked)
		{
			return false;
		}
		if (right)
		{
			return m_rightLockSettings.m_lockType != LockType.Unlocked;
		}
		return m_leftLockSettings.m_lockType != LockType.Unlocked;
	}

	private bool IsDestructible()
	{
		if (m_forceIndestructible)
		{
			return false;
		}
		if (m_indestructibleWhileLocked)
		{
			if (DoorVisuals.StartingDoorHealth != -1 && !IsLocked(right: true))
			{
				return !IsLocked(right: false);
			}
			return false;
		}
		return DoorVisuals.StartingDoorHealth != -1;
	}

	bool IDamageable.AllowProjectilePenetration()
	{
		if (!IsDestructible())
		{
			return DoorVisuals.AllowPassthroughProjectiles;
		}
		return true;
	}

	public bool ShouldForceMeleeRebound()
	{
		if (!IsDestructible())
		{
			return !CanBeForcedOpen();
		}
		return false;
	}

	private int GetDamageStateIndexForHealth()
	{
		int num = int.MaxValue;
		int num2 = -1;
		int result = -1;
		int num3 = -1;
		int num4 = m_currentHealth;
		if (m_currentHealth == -1)
		{
			num4 = 1000;
		}
		for (int i = 0; i < DoorVisuals.DamageStates.Length; i++)
		{
			if (num4 <= DoorVisuals.DamageStates[i].m_health && DoorVisuals.DamageStates[i].m_health < num)
			{
				num = DoorVisuals.DamageStates[i].m_health;
				num2 = i;
			}
			if (DoorVisuals.DamageStates[i].m_health > num3)
			{
				num3 = DoorVisuals.DamageStates[i].m_health;
				result = i;
			}
		}
		if (num2 != -1)
		{
			return num2;
		}
		return result;
	}

	private void UpdateHealthDamageState(DamageInstance damageInstance)
	{
		int damageStateIndexForHealth = GetDamageStateIndexForHealth();
		if (damageStateIndexForHealth == m_damageStateIndex)
		{
			return;
		}
		if (m_damageStateIndex != -1 && DoorVisuals.DamageStates[m_damageStateIndex].m_enabledChildObject != null)
		{
			DoorVisuals.DamageStates[m_damageStateIndex].m_enabledChildObject.SetActive(value: false);
		}
		m_damageStateIndex = damageStateIndexForHealth;
		if (damageStateIndexForHealth != -1)
		{
			if (DoorVisuals.DamageStates[m_damageStateIndex].m_enabledChildObject != null)
			{
				DoorVisuals.DamageStates[damageStateIndexForHealth].m_enabledChildObject.SetActive(value: true);
			}
			if (damageInstance != null)
			{
				Quaternion impactEffectRotationForDamageInstance = CharacterHitReact.GetImpactEffectRotationForDamageInstance(damageInstance);
				DynamicallySpawnedObject.Spawn(DoorVisuals.DamageStates[damageStateIndexForHealth].m_damageFX, persistent: false, damageInstance.Position, impactEffectRotationForDamageInstance);
			}
		}
	}

	private bool CanBeForcedOpen()
	{
		if (!m_forceIndestructible && !IsDestructible() && !IsLocked(right: false) && !IsLocked(right: true))
		{
			return m_doorState == DoorState.Closed;
		}
		return false;
	}

	public void ApplyDamageInstance(DamageInstance instance)
	{
		ApplyDamageInstance(instance, isFromDoorOpening: false);
	}

	private void ApplyDamageInstance(DamageInstance instance, bool isFromDoorOpening)
	{
		if (AllowPassThroughProjectile() && instance.DamageCategory == DamageCategory.Projectile)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		if (IsDestructible())
		{
			int currentHealth = m_currentHealth;
			m_currentHealth -= instance.HealthDamageAmount;
			m_currentHealth = Math.Max(m_currentHealth, 0);
			if (m_persistentData != null)
			{
				Health = m_currentHealth;
			}
			flag = Health <= 0;
			if (m_currentHealth != currentHealth)
			{
				flag2 = true;
			}
			UpdateHealthDamageState(instance);
		}
		if (flag)
		{
			OnDoorStateChanged(opened: true);
			if (!isFromDoorOpening && m_activeTween != null)
			{
				DOTween.Kill(m_activeTween, complete: true);
				m_activeTween = null;
			}
			m_hasBeenDestroyed = true;
			if (m_persistentData != null)
			{
				m_persistentData.m_hasBeenDestroyed = true;
			}
			if (m_mapAreaSeparator != null)
			{
				m_mapAreaSeparator.SetMapConnectionStatus(MapConnectionStatus.Open);
			}
			if (instance.Direction.x < 0f)
			{
				SetDoorState(DoorState.OpenLeft, instant: false, DoorAnimationVariant.Sprinting);
			}
			else
			{
				SetDoorState(DoorState.OpenRight, instant: false, DoorAnimationVariant.Sprinting);
			}
			if (DoorVisuals.BreakDoorFX.HasAsset())
			{
				Quaternion impactEffectRotationForDamageInstance = CharacterHitReact.GetImpactEffectRotationForDamageInstance(instance);
				DynamicallySpawnedObject.Spawn(DoorVisuals.BreakDoorFX, persistent: false, instance.Position, impactEffectRotationForDamageInstance);
			}
			if (m_doorBreakDamageCollider != null && !isFromDoorOpening)
			{
				StartCoroutine(DoBreakDamageCollider(instance.Direction.x < 0f, m_doorDestroyedDamageDelay, instance.DamageSource, 0));
			}
			DoorVisuals.PlayDoorAudioEvent(DoorVisuals.DoorAudioEvent.Break);
			return;
		}
		if (flag2)
		{
			PerformHitReact(instance, m_onHitEffect);
		}
		if (!isFromDoorOpening)
		{
			float num = m_impactPunchDistance;
			if (!flag2)
			{
				num *= 0.2f;
			}
			Vector3 zero = Vector3.zero;
			zero.x += instance.Direction.x * num;
			if (m_activeTween != null)
			{
				DOTween.Kill(m_activeTween, complete: true);
			}
			Vector3 euler = Vector3.zero;
			switch (m_doorState)
			{
			case DoorState.OpenLeft:
				euler = m_openLeftRotation;
				break;
			case DoorState.OpenRight:
				euler = m_openRightRotation;
				break;
			}
			m_doorTransform.localRotation = Quaternion.Euler(euler);
			m_activeTween = m_doorTransform.DOPunchPosition(zero, m_impactPunchDuration, m_impactPunchVibrato, m_impactPunchElasticity).OnComplete(ResetDoorPosition);
			if (CanBeForcedOpen())
			{
				OnDoorStateChanged(opened: true);
				if (instance.Direction.x < 0f)
				{
					SetDoorState(DoorState.OpenLeft, instant: false, DoorAnimationVariant.Sprinting);
				}
				else
				{
					SetDoorState(DoorState.OpenRight, instant: false, DoorAnimationVariant.Sprinting);
				}
				if (DoorVisuals.BreakDoorFX.HasAsset())
				{
					Quaternion impactEffectRotationForDamageInstance2 = CharacterHitReact.GetImpactEffectRotationForDamageInstance(instance);
					DynamicallySpawnedObject.Spawn(DoorVisuals.BreakDoorFX, persistent: false, instance.Position, impactEffectRotationForDamageInstance2);
				}
				if (m_doorBreakDamageCollider != null && !isFromDoorOpening)
				{
					StartCoroutine(DoBreakDamageCollider(instance.Direction.x < 0f, m_doorDestroyedDamageDelay, instance.DamageSource, 0));
				}
			}
		}
		DoorVisuals.PlayDoorAudioEvent(DoorVisuals.DoorAudioEvent.TakeDamage);
	}

	private void PerformHitReact(DamageInstance instance, HitReactEffectSettings effects)
	{
		if (effects == null || instance.HealthDamageAmount <= 0)
		{
			return;
		}
		Quaternion impactEffectRotationForDamageInstance = CharacterHitReact.GetImpactEffectRotationForDamageInstance(instance);
		HitReactEffectSettings.HitReactEffectGroup[] effectGroups = effects.m_effectGroups;
		foreach (HitReactEffectSettings.HitReactEffectGroup hitReactEffectGroup in effectGroups)
		{
			if (hitReactEffectGroup.m_supportedImpactTypes.HasFlag(instance.ImpactType))
			{
				CharacterHitReact.PlayEffectGroup(instance, base.transform, impactEffectRotationForDamageInstance, Vector3.one, hitReactEffectGroup);
			}
		}
	}

	private void ResetDoorPosition()
	{
		m_doorTransform.localPosition = m_doorTransformStartLocalPosition;
	}

	private IEnumerator DoBreakDamageCollider(bool toLeft, float delay, GameObject source, int doorDamageAmount)
	{
		float num = 0.75f;
		Vector3 localPosition = m_doorBreakDamageCollider.transform.localPosition;
		localPosition.x = (toLeft ? (0f - num) : num);
		m_doorBreakDamageCollider.transform.localPosition = localPosition;
		yield return new WaitForSeconds(delay);
		m_doorBreakDamageCollider.Source = source;
		m_doorBreakDamageCollider.gameObject.SetActive(value: true);
		m_doorBreakDamageCollider.SetDamageEnabled(enabled: true);
		if (doorDamageAmount > 0)
		{
			DamageInstance damageInstance = new DamageInstance();
			damageInstance.SetHealthDamage(doorDamageAmount);
			damageInstance.SetDirection(toLeft ? Vector2.left : Vector2.right);
			ApplyDamageInstance(damageInstance, isFromDoorOpening: true);
		}
		yield return new WaitForSeconds(0.2f);
		m_doorBreakDamageCollider.gameObject.SetActive(value: false);
		m_doorBreakDamageCollider.SetDamageEnabled(enabled: false);
	}

	public void SetObjectVisibility(bool visible)
	{
		m_isVisible = visible;
	}

	public MapConnectionType GetMapConnectionType()
	{
		if (m_leftLockSettings.m_lockType == LockType.ApplyItem)
		{
			return MapConnectionType.DoorApplyItem;
		}
		if (m_rightLockSettings.m_lockType == LockType.ApplyItem)
		{
			return MapConnectionType.DoorApplyItem;
		}
		if (m_leftLockSettings.m_lockType == LockType.DestroyableLock)
		{
			return MapConnectionType.DoorPadlock;
		}
		if (m_rightLockSettings.m_lockType == LockType.DestroyableLock)
		{
			return MapConnectionType.DoorPadlock;
		}
		return MapConnectionType.Door;
	}

	public ItemDefinition GetRequiredItem()
	{
		if (m_leftLockSettings.m_lockType == LockType.ApplyItem)
		{
			return m_leftLockSettings.m_requiredItem;
		}
		if (m_rightLockSettings.m_lockType == LockType.ApplyItem)
		{
			return m_rightLockSettings.m_requiredItem;
		}
		return null;
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
			m_hasBeenUnlocked = m_persistentData.m_unlocked;
			if (m_hasBeenUnlocked)
			{
				m_onDoorUnlockedEvent.Invoke();
			}
			m_hasBeenDestroyed = m_persistentData.m_hasBeenDestroyed;
			SetDoorState(m_persistentData.m_doorState, instant: true);
			SetLockVisualsState(m_hasBeenUnlocked);
			if (m_hasBeenDestroyed)
			{
				m_currentHealth = 0;
			}
			UpdateHealthDamageState(null);
		}
		else
		{
			m_persistentData = new PersistentData();
			m_persistentDataObject.Data = m_persistentData;
		}
	}

	private void OnDrawGizmos()
	{
		DrawGizmosForSetting(m_closeDoorFarAnimationSettings, Color.magenta, 0f);
		DrawGizmosForSetting(m_closeDoorNearAnimationSettings, Color.magenta * 0.5f, 0.05f);
		DrawGizmosForSetting(m_openSprintingDoorAnimationSettings, Color.red, 0.1f);
		DrawGizmosForSetting(m_openWalkingDoorAnimationSettings, Color.yellow, 0.15f);
		DrawGizmosForSetting(m_openStandingDoorAnimationSettings, Color.green, 0.2f);
	}

	private void DrawGizmosForSetting(DoorAnimationSettings setting, Color color, float heightOffset)
	{
		Gizmos.color = color;
		Vector3 position = base.transform.position;
		position.x -= setting.m_snapToPosition;
		position.y += heightOffset;
		Gizmos.DrawSphere(position, 0.05f);
		Gizmos.color = Color.white;
	}
}
