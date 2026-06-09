using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterInteractor : BaseInteractor
{
	public enum InteractableCommandFlag
	{
		None,
		RequestEvent,
		EventDone
	}

	[SerializeField]
	private BoolGameEventChannel m_interactorEnabledEventChannel;

	[SerializeField]
	private BoolGameEventChannel m_allowInteractionPrompt;

	private List<BaseInteractable> m_nearbyInteracatables;

	[SerializeField]
	private InteractableAnchor m_onNearbyInteractableAnchor;

	private bool m_isInteracting;

	private bool m_previousInteractEnabled;

	private bool m_interactEnabled;

	private BaseInteractable m_bestInteractable;

	private BaseInteractable m_activeInteractable;

	private BaseInteractable m_previousInteractable;

	private BaseInteractable m_queuedInteractable;

	public UnityAction OnStopInteract;

	private CharacterInputPlayer m_input;

	private CharacterStance m_stance;

	private CharacterDirection m_direction;

	private CharacterMovement m_movement;

	private string m_interactableCommandEventToSend;

	private InteractableCommandFlag m_interactableCommandRequestFlag;

	private static readonly float m_targetDistanceFromWalkToPos = 0.05f;

	private static readonly float m_maxDisableTurnDistance = 0.4f;

	private List<BaseInteractable> NearbyInteractables
	{
		get
		{
			if (m_nearbyInteracatables == null)
			{
				m_nearbyInteracatables = new List<BaseInteractable>();
			}
			return m_nearbyInteracatables;
		}
	}

	public bool IsInteracting => m_isInteracting;

	public bool InteractEnabled
	{
		get
		{
			return m_interactEnabled;
		}
		set
		{
			if (m_interactEnabled != value)
			{
				m_interactEnabled = value;
			}
		}
	}

	public BaseInteractable BestInteractable => m_bestInteractable;

	public BaseInteractable ActiveInteractable => m_activeInteractable;

	public bool HasQueuedInteractable => m_queuedInteractable != null;

	public string InteractableCommandEventToSend
	{
		get
		{
			return m_interactableCommandEventToSend;
		}
		set
		{
			m_interactableCommandEventToSend = value;
		}
	}

	public InteractableCommandFlag InteractableCommandRequestFlag
	{
		get
		{
			return m_interactableCommandRequestFlag;
		}
		set
		{
			m_interactableCommandRequestFlag = value;
		}
	}

	private void Start()
	{
		m_input = GetComponent<CharacterInputPlayer>();
		m_direction = GetComponent<CharacterDirection>();
		m_movement = GetComponent<CharacterMovement>();
		m_onNearbyInteractableAnchor.Set(null);
		m_allowInteractionPrompt.Raise(value: true);
		m_interactorEnabledEventChannel.Raise(m_interactEnabled);
	}

	private void OnEnable()
	{
		m_stance = GetComponent<CharacterStance>();
		if (m_stance != null)
		{
			CharacterStance stance = m_stance;
			stance.m_onStanceChanged = (UnityAction<CharacterStance.Stance>)Delegate.Combine(stance.m_onStanceChanged, new UnityAction<CharacterStance.Stance>(OnStanceChanged));
		}
	}

	private void OnDisable()
	{
		if (m_stance != null)
		{
			CharacterStance stance = m_stance;
			stance.m_onStanceChanged = (UnityAction<CharacterStance.Stance>)Delegate.Remove(stance.m_onStanceChanged, new UnityAction<CharacterStance.Stance>(OnStanceChanged));
		}
	}

	private void OnStanceChanged(CharacterStance.Stance stance)
	{
		if (stance == CharacterStance.Stance.NoCollision)
		{
			NearbyInteractables.Clear();
		}
	}

	public bool AttemptInteractNearby()
	{
		if (m_bestInteractable != null && m_bestInteractable.GetInteractButonType() == InteractButtonType.Normal)
		{
			InteractWith(m_bestInteractable);
			return true;
		}
		return false;
	}

	public void AttemptInteractUpNearby()
	{
		if (m_bestInteractable != null && m_bestInteractable.GetInteractButonType() == InteractButtonType.TransitionUp)
		{
			InteractWith(m_bestInteractable);
		}
	}

	public void AttemptInteractDownNearby()
	{
		if (m_bestInteractable != null && m_bestInteractable.GetInteractButonType() == InteractButtonType.TransitionDown)
		{
			InteractWith(m_bestInteractable);
		}
	}

	private void InteractWith(BaseInteractable interactable, bool isQueued = false)
	{
		if (!(interactable == null))
		{
			StartCoroutine(InteractionAction(interactable, isQueued));
		}
	}

	private IEnumerator InteractionAction(BaseInteractable interactable, bool isQueued)
	{
		m_allowInteractionPrompt.Raise(value: false);
		m_previousInteractable = m_activeInteractable;
		m_activeInteractable = interactable;
		m_isInteracting = true;
		CharacterDirection.Facing desiredDirection = CharacterDirection.Facing.None;
		if (interactable.AnimationSettings.m_lookDirection == BaseInteractable.LookDirection.LookAtOrigin)
		{
			desiredDirection = ((base.transform.position.x < interactable.transform.position.x) ? CharacterDirection.Facing.Right : CharacterDirection.Facing.Left);
		}
		else if (interactable.AnimationSettings.m_lookDirection == BaseInteractable.LookDirection.ForceRight)
		{
			desiredDirection = CharacterDirection.Facing.Right;
		}
		else if (interactable.AnimationSettings.m_lookDirection == BaseInteractable.LookDirection.ForceLeft)
		{
			desiredDirection = CharacterDirection.Facing.Left;
		}
		if (interactable.UseInteractPosition && !isQueued)
		{
			float moveToTime = Time.unscaledTime + 1.5f;
			Vector2 interactPosition = interactable.GetWalkToInteractPosition();
			bool flag = interactPosition.x - base.transform.position.x > 0f;
			bool disableTurning = false;
			float num = Mathf.Abs(interactPosition.x - base.transform.position.x);
			if (num < m_maxDisableTurnDistance)
			{
				if (!flag && desiredDirection == CharacterDirection.Facing.Right && m_direction.CurrentDirection == CharacterDirection.Facing.Right)
				{
					disableTurning = true;
				}
				else if (flag && desiredDirection == CharacterDirection.Facing.Left && m_direction.CurrentDirection == CharacterDirection.Facing.Left)
				{
					disableTurning = true;
				}
			}
			PlayMakerFSM component = GetComponent<PlayMakerFSM>();
			if (component != null && component.ActiveStateName.Contains("Sprint"))
			{
				component.SendEvent("Reset");
			}
			while (num > m_targetDistanceFromWalkToPos || m_direction.CurrentDirection != m_direction.DesiredDirection)
			{
				Vector2 vector = new Vector2(interactPosition.x - base.transform.position.x, 0f);
				m_input.SetTurnDisabled(disableTurning);
				if (vector.x > 0f)
				{
					m_input.InputOverride = PlayerInputOverride.WalkRight;
				}
				else
				{
					m_input.InputOverride = PlayerInputOverride.WalkLeft;
				}
				yield return new WaitForEndOfFrame();
				num = Mathf.Abs(interactPosition.x - base.transform.position.x);
				if (Time.unscaledTime >= moveToTime)
				{
					Debug.LogWarning("Failed to reach interact position for interactable: " + interactable.gameObject, interactable.gameObject);
					break;
				}
			}
			m_input.SetTurnDisabled(disabled: false);
			Vector3 position = base.transform.position;
			position.x = interactPosition.x;
			base.transform.position = position;
			if (m_movement != null)
			{
				m_movement.SnapToGround();
			}
		}
		m_input.InputOverride = PlayerInputOverride.Static;
		if (interactable.AnimationSettings.m_lookDirection == BaseInteractable.LookDirection.LookAtOrigin)
		{
			desiredDirection = ((base.transform.position.x < interactable.transform.position.x) ? CharacterDirection.Facing.Right : CharacterDirection.Facing.Left);
		}
		if (desiredDirection != 0)
		{
			yield return new WaitUntil(() => m_direction.CurrentDirection == m_direction.DesiredDirection);
		}
		if (desiredDirection != 0 && m_direction.CurrentDirection != desiredDirection)
		{
			CharacterDirection.Facing facing3 = (m_direction.CurrentDirection = (m_direction.DesiredDirection = desiredDirection));
		}
		m_input.InputOverride = PlayerInputOverride.None;
		if (m_activeInteractable.UseFSMInteractFlow)
		{
			PlayMakerFSM component2 = GetComponent<PlayMakerFSM>();
			if (!component2.ActiveStateName.Equals("InteractFSM") && component2 != null)
			{
				component2.SendEvent("Interact/Start");
			}
		}
		else
		{
			m_activeInteractable.Interact(this);
			StartCoroutine(WaitForInteractionCompletion());
		}
	}

	public void DoInteractFromFSM()
	{
		m_activeInteractable.Interact(this);
		StartCoroutine(WaitForInteractionCompletion());
	}

	public string GetFSMEventStringForInteract()
	{
		return "Interact/" + m_activeInteractable.GetInteractType();
	}

	public string GetFSMEventForInteractAnimation()
	{
		if (m_activeInteractable.AnimationSettings.m_playerAnimation == InteractAnimationType.None)
		{
			return "";
		}
		InteractAnimationType interactAnimationType = m_activeInteractable.AnimationSettings.m_playerAnimation;
		if (interactAnimationType == InteractAnimationType.ItemPickupSelectBest)
		{
			Vector3 vector = m_activeInteractable.InteractPromptPosition - base.transform.position;
			interactAnimationType = ((vector.y < 0.2f) ? InteractAnimationType.PickupGround : ((Mathf.Abs(vector.x) > 0.5f) ? InteractAnimationType.UseSideways : ((!(vector.z < -0.25f)) ? InteractAnimationType.UseAwayFromCamera : InteractAnimationType.UseTowardsCamera)));
		}
		return interactAnimationType.ToString();
	}

	private IEnumerator WaitForInteractionCompletion()
	{
		yield return new WaitForEndOfFrame();
		float postInteractWaitTime = 0.05f;
		InteractType interactType = m_activeInteractable.GetInteractType();
		bool ignoreTimeScale = false;
		if (interactType == InteractType.Interactable)
		{
			BaseInteractable activeInteractable = m_activeInteractable;
			Interactable newInteractable = activeInteractable as Interactable;
			if ((object)newInteractable != null)
			{
				yield return new WaitUntil(() => m_activeInteractable == null || newInteractable.InteractionDone);
				goto IL_01c4;
			}
		}
		if (interactType == InteractType.Pushable && m_activeInteractable is PushableInteract)
		{
			yield return new WaitUntil(() => m_activeInteractable == null);
		}
		else
		{
			if (interactType == InteractType.SideDoor)
			{
				BaseInteractable activeInteractable = m_activeInteractable;
				NewSideDoor sideDoor = activeInteractable as NewSideDoor;
				if ((object)sideDoor != null)
				{
					yield return new WaitUntil(() => m_activeInteractable == null || sideDoor.InteractionDone);
					goto IL_01c4;
				}
			}
			if (interactType == InteractType.LevelTransitionDoor)
			{
				BaseInteractable activeInteractable = m_activeInteractable;
				LevelTransitionDoor levelTransitionDoor = activeInteractable as LevelTransitionDoor;
				if ((object)levelTransitionDoor != null)
				{
					yield return new WaitUntil(() => m_activeInteractable == null || levelTransitionDoor.InteractionDone);
				}
			}
		}
		goto IL_01c4;
		IL_01c4:
		if (ignoreTimeScale)
		{
			yield return new WaitForSecondsRealtime(postInteractWaitTime);
		}
		else
		{
			yield return new WaitForSeconds(postInteractWaitTime);
		}
		StopInteracting();
	}

	public void StopInteracting(bool stopCoroutines = true)
	{
		if (m_activeInteractable != null)
		{
			if (stopCoroutines)
			{
				StopAllCoroutines();
			}
			if (m_activeInteractable != null)
			{
				m_activeInteractable.FinishedInteract();
			}
			m_isInteracting = false;
			m_previousInteractable = m_activeInteractable;
			m_activeInteractable = null;
			m_allowInteractionPrompt.Raise(value: true);
			OnStopInteract?.Invoke();
		}
	}

	private void RegisterNearbyInteractable(GameObject interactable)
	{
		BaseInteractable[] components = interactable.GetComponents<BaseInteractable>();
		foreach (BaseInteractable item in components)
		{
			if (!NearbyInteractables.Contains(item))
			{
				NearbyInteractables.Add(item);
			}
		}
	}

	private void UnregisterInteractable(GameObject interactable)
	{
		BaseInteractable[] components = interactable.GetComponents<BaseInteractable>();
		foreach (BaseInteractable item in components)
		{
			if (NearbyInteractables.Contains(item))
			{
				NearbyInteractables.Remove(item);
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if ((bool)collision.gameObject.GetComponent<BaseInteractable>())
		{
			RegisterNearbyInteractable(collision.gameObject);
			BaseInteractable component = collision.gameObject.GetComponent<BaseInteractable>();
			if ((object)component != null && component.ShouldAutoInteract(this))
			{
				DoForceInteract(component);
			}
		}
	}

	public void DoForceInteract(BaseInteractable baseInteractable)
	{
		if (!m_isInteracting)
		{
			PlayMakerFSM component = GetComponent<PlayMakerFSM>();
			if (component != null)
			{
				component.SendEvent("Interact/ForcedInteract");
			}
			baseInteractable.SetAutoInteractOnce(enabled: false);
			InteractWith(baseInteractable);
		}
		else if (m_queuedInteractable != null)
		{
			Debug.LogWarning("Tried to do auto interact for interact " + baseInteractable.gameObject.name + " but an interact was already active with " + m_activeInteractable.gameObject.name);
		}
		else
		{
			m_queuedInteractable = baseInteractable;
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if ((bool)collision.gameObject.GetComponent<BaseInteractable>())
		{
			UnregisterInteractable(collision.gameObject);
		}
	}

	private BaseInteractable GetBestInteractable()
	{
		if (NearbyInteractables.Count == 0)
		{
			return null;
		}
		if (NearbyInteractables.Count == 1)
		{
			if (!NearbyInteractables[0].CanInteract(this))
			{
				return null;
			}
			return NearbyInteractables[0];
		}
		float num = float.MaxValue;
		BaseInteractable result = null;
		GameObject gameObject = null;
		int num2 = 0;
		Vector3 position = base.transform.position;
		for (int i = 0; i < NearbyInteractables.Count; i++)
		{
			if (!NearbyInteractables[i].CanInteract(this))
			{
				continue;
			}
			if (gameObject == NearbyInteractables[i].gameObject)
			{
				if (num2 < NearbyInteractables[i].InteractPriority)
				{
					result = NearbyInteractables[i];
					num2 = NearbyInteractables[i].InteractPriority;
				}
				continue;
			}
			float num3 = Mathf.Abs(position.x - NearbyInteractables[i].ColliderCenterPosition.x);
			if (num3 < num)
			{
				num = num3;
				if (NearbyInteractables[i].InteractPriority >= num2)
				{
					result = NearbyInteractables[i];
					gameObject = NearbyInteractables[i].gameObject;
					num2 = NearbyInteractables[i].InteractPriority;
				}
			}
		}
		return result;
	}

	public void StartQueuedInteraction()
	{
		InteractWith(m_queuedInteractable, isQueued: true);
		m_queuedInteractable = null;
	}

	private void Update()
	{
		if (m_interactEnabled && (bool)m_queuedInteractable && !m_isInteracting)
		{
			if (m_queuedInteractable.enabled)
			{
				StartQueuedInteraction();
			}
			else
			{
				Debug.LogWarning("Tried to queue interact an an interactable that is disabled somehow. Ignoring.");
				m_queuedInteractable = null;
			}
		}
		BaseInteractable bestInteractable = GetBestInteractable();
		if (bestInteractable != m_bestInteractable)
		{
			if (m_bestInteractable != null)
			{
				m_bestInteractable.HighlightInteractable(highlighted: false);
			}
			m_bestInteractable = bestInteractable;
			if (m_bestInteractable != null)
			{
				m_bestInteractable.HighlightInteractable(highlighted: true);
			}
			m_onNearbyInteractableAnchor.Set(m_bestInteractable);
		}
		if (m_previousInteractEnabled != m_interactEnabled)
		{
			m_interactorEnabledEventChannel.Raise(m_interactEnabled);
			m_previousInteractEnabled = m_interactEnabled;
		}
	}

	public void TriggerAnimationEventStart()
	{
		if (m_activeInteractable != null && m_activeInteractable.AnimationSettings.m_startUseAnimationEvent != null)
		{
			m_activeInteractable.AnimationSettings.m_startUseAnimationEvent.Invoke();
		}
	}

	public void TriggerAnimationEventEnd()
	{
		if (m_activeInteractable != null && m_activeInteractable.AnimationSettings.m_stopUseAnimationEvent != null)
		{
			m_activeInteractable.AnimationSettings.m_stopUseAnimationEvent.Invoke();
		}
		else if (m_previousInteractable != null && m_previousInteractable.AnimationSettings.m_stopUseAnimationEvent != null)
		{
			m_previousInteractable.AnimationSettings.m_stopUseAnimationEvent.Invoke();
		}
	}
}
