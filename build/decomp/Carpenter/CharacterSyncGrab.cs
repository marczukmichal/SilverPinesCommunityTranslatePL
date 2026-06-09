using System;
using UnityEngine;
using UnityEngine.Events;

public class CharacterSyncGrab : MonoBehaviour, IDamageable
{
	public class CharacterSyncGrabState
	{
		public CharacterSyncGrab m_instigator;

		public CharacterSyncGrab m_target;

		public CharacterSyncGrabSettings m_settings;

		public float m_timer;

		public bool m_shouldCancelGrab;

		public float m_panicJuice;
	}

	[Serializable]
	public struct CharacterGrabFSMEvents
	{
		public CharacterSyncGrabSettings m_grabSettings;

		public bool m_asInstigator;

		public string m_fsmEvent;
	}

	private const float PANIC_JUICE_THRESHOLD = 10f;

	private CharacterSyncGrabState m_activeGrabState;

	[SerializeField]
	private PlayMakerFSM m_fsm;

	[SerializeField]
	private CharacterGrabFSMEvents[] m_supportedGrabEvents;

	[SerializeField]
	private Transform m_hitLocationTransform;

	private bool m_preventGrabFlag;

	private BaseCharacterInput m_input;

	private Vector2 m_previousMovementInput;

	public UnityAction<bool> m_onGrabbed;

	private bool m_blockedExit;

	public void SetPreventGrabFlag(bool preventGrabs)
	{
		m_preventGrabFlag = preventGrabs;
	}

	private void Start()
	{
		m_input = base.gameObject.GetCharacterInputComponent();
	}

	public bool PerformGrab(CharacterSyncGrab target, CharacterSyncGrabSettings grabSettings)
	{
		if (m_activeGrabState != null)
		{
			Debug.LogError("Tried to perform a grab but a grab " + grabSettings.name + " is already active! " + base.gameObject.name + " - grab type: " + m_activeGrabState.m_settings.name);
			return false;
		}
		if (target.m_activeGrabState != null)
		{
			Debug.LogError("Tried to perform a grab" + grabSettings.name + " but the target is already in an active grab! " + target.gameObject.name + " - grab type: " + target.m_activeGrabState.m_settings.name);
			return false;
		}
		CharacterSyncGrabState characterSyncGrabState = new CharacterSyncGrabState();
		characterSyncGrabState.m_instigator = this;
		characterSyncGrabState.m_target = target;
		characterSyncGrabState.m_settings = grabSettings;
		characterSyncGrabState.m_timer = grabSettings.m_grabTime;
		StartGrabAsInstigator(characterSyncGrabState);
		target.StartGrabAsTarget(characterSyncGrabState);
		return true;
	}

	public bool CanPerformGrab(CharacterSyncGrab target, CharacterSyncGrabSettings grabSettings)
	{
		if (CanPerformGrab(grabSettings, asInstigator: true))
		{
			return target.CanPerformGrab(grabSettings, asInstigator: false);
		}
		return false;
	}

	public bool CanPerformGrab(CharacterSyncGrabSettings grabSettings, bool asInstigator)
	{
		if (m_preventGrabFlag)
		{
			return false;
		}
		if (m_activeGrabState != null)
		{
			return false;
		}
		DamageBlock component = GetComponent<DamageBlock>();
		if ((bool)component && !component.CanTakeDamage(null))
		{
			return false;
		}
		CharacterGrabFSMEvents[] supportedGrabEvents = m_supportedGrabEvents;
		for (int i = 0; i < supportedGrabEvents.Length; i++)
		{
			CharacterGrabFSMEvents characterGrabFSMEvents = supportedGrabEvents[i];
			if (characterGrabFSMEvents.m_asInstigator == asInstigator && characterGrabFSMEvents.m_grabSettings == grabSettings)
			{
				return true;
			}
		}
		return false;
	}

	private void TriggerEvent(CharacterSyncGrabSettings settings, bool asInstigator)
	{
		CharacterGrabFSMEvents[] supportedGrabEvents = m_supportedGrabEvents;
		for (int i = 0; i < supportedGrabEvents.Length; i++)
		{
			CharacterGrabFSMEvents characterGrabFSMEvents = supportedGrabEvents[i];
			if (characterGrabFSMEvents.m_grabSettings == settings && characterGrabFSMEvents.m_asInstigator == asInstigator)
			{
				m_fsm.SendEvent(characterGrabFSMEvents.m_fsmEvent);
			}
		}
	}

	private void StartGrabAsInstigator(CharacterSyncGrabState state)
	{
		m_activeGrabState = state;
		TriggerEvent(state.m_settings, asInstigator: true);
	}

	private void StartGrabAsTarget(CharacterSyncGrabState state)
	{
		m_activeGrabState = state;
		m_previousMovementInput = Vector2.zero;
		TriggerEvent(state.m_settings, asInstigator: false);
		m_onGrabbed?.Invoke(arg0: true);
		SnapToInstigator(state.m_instigator, state.m_settings);
		if (m_activeGrabState.m_settings.m_triggerDamageOnGrab)
		{
			ApplyHit();
		}
	}

	public void ExitGrab()
	{
		if (m_activeGrabState.m_timer > 0f)
		{
			m_activeGrabState.m_shouldCancelGrab = true;
		}
		m_onGrabbed?.Invoke(arg0: false);
		m_activeGrabState = null;
	}

	public bool IsInstigator()
	{
		if (m_activeGrabState != null)
		{
			return m_activeGrabState.m_instigator == this;
		}
		return false;
	}

	public CharacterSyncGrab GetInstigator()
	{
		if (m_activeGrabState != null)
		{
			return m_activeGrabState.m_instigator;
		}
		return null;
	}

	public CharacterSyncGrab GetTarget()
	{
		if (m_activeGrabState != null)
		{
			return m_activeGrabState.m_target;
		}
		return null;
	}

	public void SnapToInstigator(CharacterSyncGrab instigator, CharacterSyncGrabSettings grabSettings)
	{
		float num = grabSettings.m_offset.x;
		CharacterDirection component = instigator.GetComponent<CharacterDirection>();
		if (component != null && component.CurrentDirection == CharacterDirection.Facing.Left)
		{
			num *= -1f;
		}
		Vector3 position = instigator.transform.position;
		position.x += num;
		position.y += grabSettings.m_offset.y;
		base.transform.position = position;
	}

	public void SnapToInstigator()
	{
		if (m_activeGrabState != null)
		{
			SnapToInstigator(m_activeGrabState.m_instigator, m_activeGrabState.m_settings);
		}
	}

	public void ApplyHit()
	{
		Vector2 position = ((m_hitLocationTransform != null) ? m_hitLocationTransform.position : m_activeGrabState.m_target.transform.position);
		Vector2 direction = m_activeGrabState.m_target.transform.position - m_activeGrabState.m_instigator.transform.position;
		direction.Normalize();
		CharacterIdentifier component = base.gameObject.GetComponent<CharacterIdentifier>();
		CharacterIdentifier.CharacterFaction faction = CharacterIdentifier.CharacterFaction.Unknown;
		if (component != null)
		{
			faction = component.Faction;
		}
		DamageInstance damageInstance = new DamageInstance().PopulateFromHitSettings(m_activeGrabState.m_settings.HitSettings).SetDamageSource(base.gameObject).SetDirection(direction)
			.SetPosition(position)
			.SetStatusEffectHitResults(m_activeGrabState.m_settings.HitSettings.StatusEffect.GenerateResult(isBlocked: false))
			.ScaleDamageBasedOnFaction(faction);
		DamageBlock component2 = m_activeGrabState.m_target.gameObject.GetComponent<DamageBlock>();
		if (!component2 || component2.CanTakeDamage(damageInstance))
		{
			DamageUtilities.ApplyDamage(m_activeGrabState.m_target.gameObject, damageInstance);
		}
	}

	public void GetDebugInfo(ref string infoString)
	{
		if (m_activeGrabState != null)
		{
			if (IsInstigator())
			{
				infoString = infoString + "Grabbed: " + m_activeGrabState.m_target.name + "\n";
				infoString = infoString + "Timer: " + m_activeGrabState.m_timer;
			}
			else
			{
				infoString = infoString + "Grabbed by: " + m_activeGrabState.m_instigator.name + "\n";
			}
		}
	}

	public bool HasGrabBeenInterrupted()
	{
		if (m_activeGrabState == null)
		{
			return true;
		}
		return m_activeGrabState.m_shouldCancelGrab;
	}

	public bool ShouldEndGrab()
	{
		if (m_blockedExit)
		{
			return false;
		}
		if (m_activeGrabState == null)
		{
			return true;
		}
		CharacterHealth component = m_activeGrabState.m_target.GetComponent<CharacterHealth>();
		if (component != null && component.IsDead)
		{
			return true;
		}
		return m_activeGrabState.m_timer <= 0f;
	}

	private void Update()
	{
		if (m_activeGrabState != null)
		{
			if (IsInstigator())
			{
				m_activeGrabState.m_timer -= Time.deltaTime;
			}
			if (!IsInstigator())
			{
				UpdatePanicInput();
			}
		}
	}

	private void UpdatePanicInput()
	{
		Vector2 movementInput = m_input.MovementInput;
		float num = Vector2.Distance(movementInput, m_previousMovementInput);
		m_activeGrabState.m_panicJuice += num;
		m_previousMovementInput = movementInput;
		if (m_activeGrabState.m_panicJuice > 10f)
		{
			m_activeGrabState.m_panicJuice -= 10f;
			m_activeGrabState.m_timer -= m_activeGrabState.m_settings.m_panicSuccessTimeReduction;
		}
	}

	public void ApplyDamageInstance(DamageInstance instance)
	{
		if (m_activeGrabState != null && m_activeGrabState.m_instigator == this)
		{
			m_activeGrabState.m_timer -= m_activeGrabState.m_settings.m_takeDamageTimeReductionInstigator;
		}
	}

	public void SetGrabExitBlocked(bool blocked)
	{
		m_blockedExit = blocked;
	}
}
