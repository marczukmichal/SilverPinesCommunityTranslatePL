using System;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using UnityEngine;

public class AIFSMAttackStrategy : AIAttackStrategy
{
	[Flags]
	public enum AbilityRequirements
	{
		None = 0,
		RequiresMeleeWeapon = 1,
		NotWithMeleeWeapon = 2,
		FacingTarget = 4,
		FSMCustomRequirement1 = 8,
		FSMCustomRequirement2 = 0x10,
		LineOfSight = 0x20,
		IsBehindDoor = 0x40,
		TargetUnreachable = 0x80,
		NotFacingTarget = 0x100,
		PlaymakerVariableTrue = 0x200,
		PlaymakerVariableFalse = 0x400,
		IsGrounded = 0x800,
		TargetIsFacingMe = 0x1000,
		TargetIsNotFacingMe = 0x2000,
		TargetIsAboveMe = 0x4000,
		TargetIsOnSameLevel = 0x8000,
		TargetIsBelowMe = 0x10000
	}

	[Serializable]
	public class AbilitySettings
	{
		[UnityEngine.Tooltip("Dev only name, doesn't do anything just for organising / comments")]
		public string m_name;

		[UnityEngine.Tooltip("Which AI Ability enum to trigger for this ability")]
		public AIAbility m_abilityType;

		[UnityEngine.Tooltip("Minimum distance from the target to allow this ability to be triggered")]
		public float m_minDistance;

		[UnityEngine.Tooltip("Maximum distance from the target to allow this ability to be triggered")]
		public float m_maxDistance;

		[UnityEngine.Tooltip("If true then we ignore height for this attack for distance calculations")]
		public bool m_ignoreHeightForDistance;

		[UnityEngine.Tooltip("How much time ahead to predict enemy and local position for this ability")]
		public float m_lookAheadPredictionTime;

		[UnityEngine.Tooltip("Any special situational requirements for this ability to be able to be triggered")]
		public AbilityRequirements m_requirement;

		[SerializeField]
		public string m_playmakerVariableName;

		[SerializeField]
		public PlayMakerFSM m_playmakerVariableFSMToCheck;

		[UnityEngine.Tooltip("If this ability requires attack permission from the AI combat director")]
		public bool m_requireAttackPermission;

		[UnityEngine.Tooltip("If true then the cooldown blocks any ability from triggering, not just this ability")]
		public bool m_cooldownBlocksAllAbilities = true;

		[UnityEngine.Tooltip("Time until ability can be used again")]
		public Vector2 m_cooldownDuration;

		[UnityEngine.Tooltip("If multiple ability are valid at this distance, then this is used to weight the options")]
		public float m_weight;

		[UnityEngine.Tooltip("If greater than 0, this is a chance that this ability just doesn't fire and goes on cooldown. Useful for abilities where you want it to be a bit more random if they will be used.")]
		public float m_ignoreChance;

		[UnityEngine.Tooltip("Event to trigger in state machine when triggered")]
		public string m_fsmEvent;

		public bool m_alwaysSendAIEvent;

		public float m_minRandomDelay;

		public float m_maxRandomDelay;

		[NonSerialized]
		public float m_cooldownTimer;

		[NonSerialized]
		public bool m_abilityConfirmed;

		[NonSerialized]
		public float m_requestTime;

		[NonSerialized]
		public float m_delayTimer;
	}

	public enum AttackStrategyMovementMode
	{
		Normal,
		AlwaysForward
	}

	public enum AttackBehaviorMovementMode
	{
		UseCombatDirectorPosition,
		MoveToTargetWithPrediction,
		NoMovementUpdate
	}

	[SerializeField]
	private PlayMakerFSM m_aiFSM;

	[SerializeField]
	private float m_goalDistance;

	[SerializeField]
	private float m_minMoveDistance = 0.5f;

	[SerializeField]
	private bool m_canMoveVertically;

	[NonSerialized]
	public bool m_customRequirementFlag1;

	[NonSerialized]
	public bool m_customRequirementFlag2;

	[SerializeField]
	private AbilitySettings[] m_abilities;

	[SerializeField]
	private float m_walkBackwardsDistance = 2f;

	[SerializeField]
	private bool m_ignoreYHeight;

	[UnityEngine.Tooltip("This is a cooldown for all abilities that is set when gameobject is first activated, to prevent 'cheap' attacks when an enemy is activated and immediatly attacks")]
	private float m_spawnCooldownDelay = 0.5f;

	private CharacterDirection.Facing m_desiredFacingDirection;

	private Vector2 m_moveDirection;

	private Vector2 m_forceMoveInput;

	private bool m_shouldAttack;

	private bool m_shouldSprint;

	private Vector2 m_moveGoal;

	private CharacterMovement m_movement;

	private CharacterDamageRage m_rage;

	private AIVision m_aiVision;

	private AbilitySettings m_queuedAbility;

	private AbilitySettings m_activeAbility;

	private float m_abilityActiveTimer;

	private bool m_characterStateCanAttack;

	private Vector2 m_predictedMyPosition;

	private Vector2 m_predictedTargetPosition;

	private float m_characterCooldownTimer;

	private bool m_isTurnDisabled;

	private bool m_isAtGoal;

	private bool m_blockCooldowns;

	[SerializeField]
	private AttackStrategyMovementMode m_movementMode;

	public override Vector2 MoveDirection => m_moveDirection;

	public override CharacterDirection.Facing FacingDirectionInput => m_desiredFacingDirection;

	public override bool ShouldAttack => m_shouldAttack;

	public override bool ShouldSprint => m_shouldSprint;

	public float CharacterCooldownTimer
	{
		get
		{
			return m_characterCooldownTimer;
		}
		set
		{
			m_characterCooldownTimer = value;
		}
	}

	public bool CharacterStateCanAttack
	{
		get
		{
			return m_characterStateCanAttack;
		}
		set
		{
			m_characterStateCanAttack = value;
		}
	}

	public override bool TurnDisabled => m_isTurnDisabled;

	public bool BlockCooldowns
	{
		get
		{
			return m_blockCooldowns;
		}
		set
		{
			m_blockCooldowns = value;
		}
	}

	private float GetGoalDistance()
	{
		if (m_isAtGoal)
		{
			return Mathf.Max(m_minMoveDistance, m_goalDistance);
		}
		return m_goalDistance;
	}

	public void SetForceMoveInput(Vector2 input)
	{
		m_forceMoveInput = input;
	}

	public void ResetForceMoveInput()
	{
		m_forceMoveInput = Vector2.zero;
	}

	protected override void Start()
	{
		base.Start();
		m_movement = GetComponent<CharacterMovement>();
		m_aiVision = GetComponentInChildren<AIVision>();
		m_rage = GetComponent<CharacterDamageRage>();
		m_characterCooldownTimer = m_spawnCooldownDelay;
	}

	public void SetMoveGoal(Vector3 position)
	{
		m_moveGoal = position;
	}

	public void SetAttackFlag(bool attack)
	{
		m_shouldAttack = attack;
	}

	public void SetSprintflag(bool sprint)
	{
		m_shouldSprint = sprint;
	}

	public override void EnableStrategy()
	{
		base.EnableStrategy();
		m_aiFSM.enabled = true;
		m_aiFSM.SendEvent("Enable");
		if (m_currentTarget != null)
		{
			m_moveGoal = m_currentTarget.transform.position;
		}
	}

	public override void DisableStrategy()
	{
		base.DisableStrategy();
		m_aiFSM.enabled = false;
		m_aiFSM.SendEvent("Disable");
		SetRequestingAttack(requesting: false);
	}

	public override void UpdateStrategy()
	{
		base.UpdateStrategy();
		Vector2 moveDirection = m_moveDirection;
		m_moveDirection = Vector2.zero;
		if (m_activeAbility != null && !(m_activeAbility.m_delayTimer >= 0f) && !m_retreatMode)
		{
			return;
		}
		m_desiredFacingDirection = CharacterDirection.Facing.None;
		if (m_movementMode == AttackStrategyMovementMode.AlwaysForward)
		{
			m_moveDirection = m_direction.GetForwardVector();
			return;
		}
		if (m_forceMoveInput != Vector2.zero)
		{
			m_moveDirection = m_forceMoveInput;
			m_isTurnDisabled = false;
			return;
		}
		Vector2 moveGoal = m_moveGoal;
		Vector2 vector = GetNavigationPosition();
		if (m_ignoreYHeight)
		{
			moveGoal.y = 0f;
			vector.y = 0f;
		}
		Vector2 vector2 = moveGoal - vector;
		float num = Vector2.Distance(moveGoal, vector);
		if (num >= GetGoalDistance())
		{
			Vector2 vector3 = vector2;
			if (!m_canMoveVertically)
			{
				vector2.y = 0f;
			}
			m_moveDirection = vector2.normalized;
			if (!m_canMoveVertically)
			{
				if (vector3.y > 0.5f)
				{
					m_moveDirection.y = 1f;
				}
				else if (vector3.y < -0.5f)
				{
					m_moveDirection.y = -1f;
				}
			}
		}
		else if (Mathf.Sign(vector2.x) != Mathf.Sign(moveDirection.x))
		{
			m_moveDirection = Vector2.zero;
		}
		m_isTurnDisabled = m_direction.CurrentDirection != m_direction.DesiredDirection;
		Vector2 position = m_currentTarget.transform.position;
		if (m_direction.IsFacingPoint(position) && !m_direction.IsFacingPoint(moveGoal) && num < m_walkBackwardsDistance)
		{
			m_isTurnDisabled = true;
		}
	}

	private void Update()
	{
		if (!BlockCooldowns)
		{
			float num = Time.deltaTime;
			if (m_rage != null && m_rage.IsRaging)
			{
				num *= m_rage.CooldownReducationRateWhileRaging;
			}
			AbilitySettings[] abilities = m_abilities;
			foreach (AbilitySettings abilitySettings in abilities)
			{
				if (abilitySettings.m_cooldownTimer > 0f)
				{
					abilitySettings.m_cooldownTimer -= num;
				}
			}
			if (m_characterCooldownTimer > 0f)
			{
				m_characterCooldownTimer -= num;
			}
		}
		if (m_abilityActiveTimer > 0f)
		{
			m_abilityActiveTimer -= Time.deltaTime;
			if (m_abilityActiveTimer <= 0f)
			{
				m_activeAbility = null;
			}
		}
		if (m_activeAbility != null && m_activeAbility.m_delayTimer > 0f)
		{
			m_activeAbility.m_delayTimer -= Time.deltaTime;
		}
	}

	private void OnDrawGizmos()
	{
		if (Application.isPlaying && m_moveGoal != Vector2.zero)
		{
			GizmoExtensions.DrawToFromArrow(base.transform.position, m_moveGoal);
		}
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(m_predictedMyPosition, 0.1f);
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(m_predictedTargetPosition, 0.1f);
		Gizmos.color = Color.white;
	}

	public void SetGoalToDirectorNavigationPos()
	{
		Vector2 position = m_directorGoalPos;
		SetGoalToPosition(position, GetGoalDistance());
	}

	public bool SetGoalToPosition(Vector2 position, float goalDistance)
	{
		Vector2 b = GetNavigationPosition();
		float num = Vector2.Distance(position, b);
		if (num < goalDistance)
		{
			if (!m_isAtGoal)
			{
				Vector2 position2 = m_currentTarget.transform.position;
				if (!m_direction.IsFacingPoint(position2))
				{
					m_desiredFacingDirection = ((b.x < position2.x) ? CharacterDirection.Facing.Right : CharacterDirection.Facing.Left);
				}
			}
			m_isAtGoal = true;
			m_moveGoal = position;
		}
		else if (!m_isAtGoal || !(num < m_minMoveDistance))
		{
			m_desiredFacingDirection = CharacterDirection.Facing.None;
			m_isAtGoal = false;
			m_moveGoal = position;
		}
		return m_isAtGoal;
	}

	public float GetDistanceFromAttackTarget(float lookaheadTime, bool ignoreHeight)
	{
		Vector2 vector = m_currentTarget.transform.position;
		CharacterMovement component = m_currentTarget.GetComponent<CharacterMovement>();
		if (component != null)
		{
			vector += component.PredictedPositionOffsetSmoothed * lookaheadTime;
		}
		else
		{
			Rigidbody2D component2 = m_currentTarget.GetComponent<Rigidbody2D>();
			if (component2 != null)
			{
				vector += component2.linearVelocity * lookaheadTime;
			}
		}
		Vector2 vector2 = GetNavigationPosition();
		if (ignoreHeight)
		{
			vector2.y = 0f;
			vector.y = 0f;
		}
		m_predictedTargetPosition = vector;
		m_predictedMyPosition = vector2;
		return Vector2.Distance(vector2, vector);
	}

	public override bool PermitAttack()
	{
		if (m_queuedAbility != null && CheckAbilitySettingsFlags(m_queuedAbility))
		{
			bool flag = false;
			if (m_queuedAbility.m_ignoreChance > 0f && UnityEngine.Random.Range(0f, 1f) < m_queuedAbility.m_ignoreChance)
			{
				flag = true;
			}
			if (flag)
			{
				ConfirmAbility(m_queuedAbility);
				m_queuedAbility = null;
			}
			else
			{
				SetAbilityActive(m_queuedAbility);
			}
			return true;
		}
		m_queuedAbility = null;
		return false;
	}

	public void ForceAbility(string abilityName)
	{
		AbilitySettings[] abilities = m_abilities;
		foreach (AbilitySettings abilitySettings in abilities)
		{
			if (abilitySettings.m_name.Equals(abilityName))
			{
				SetAbilityActive(abilitySettings);
				return;
			}
		}
		Debug.LogError("Tried to do force ability for " + abilityName + " but no ability found for " + base.gameObject.name);
	}

	private void SetAbilityActive(AbilitySettings settings)
	{
		m_activeAbility = settings;
		m_queuedAbility = null;
		m_activeAbility.m_abilityConfirmed = false;
		m_activeAbility.m_delayTimer = UnityEngine.Random.Range(settings.m_minRandomDelay, settings.m_maxRandomDelay);
		m_activeAbility.m_requestTime = Time.time + m_activeAbility.m_delayTimer;
		m_abilityActiveTimer = 0.5f + m_activeAbility.m_delayTimer;
		if (settings.m_alwaysSendAIEvent && !string.IsNullOrEmpty(settings.m_fsmEvent))
		{
			m_aiFSM.SendEvent(settings.m_fsmEvent);
		}
	}

	private bool CheckAbilitySettingsFlags(AbilitySettings ability)
	{
		if (ability.m_requirement.HasFlag(AbilityRequirements.RequiresMeleeWeapon))
		{
			CharacterEquipment component = GetComponent<CharacterEquipment>();
			if (component == null || !component.HasMeleeWeapon)
			{
				return false;
			}
		}
		if (ability.m_requirement.HasFlag(AbilityRequirements.NotWithMeleeWeapon))
		{
			CharacterEquipment component2 = GetComponent<CharacterEquipment>();
			if (component2 != null && component2.HasMeleeWeapon)
			{
				return false;
			}
		}
		if (ability.m_requirement.HasFlag(AbilityRequirements.FacingTarget) && !m_direction.IsFacingPoint(m_currentTarget.transform.position))
		{
			return false;
		}
		if (ability.m_requirement.HasFlag(AbilityRequirements.NotFacingTarget) && m_direction.IsFacingPoint(m_currentTarget.transform.position))
		{
			return false;
		}
		if (ability.m_requirement.HasFlag(AbilityRequirements.FSMCustomRequirement1) && !m_customRequirementFlag1)
		{
			return false;
		}
		if (ability.m_requirement.HasFlag(AbilityRequirements.FSMCustomRequirement2) && !m_customRequirementFlag2)
		{
			return false;
		}
		if (ability.m_requirement.HasFlag(AbilityRequirements.LineOfSight))
		{
			if (m_aiVision != null)
			{
				return m_aiVision.CanSeeTarget;
			}
			Debug.LogError("Ability " + ability.m_name + " for character " + base.gameObject.name + " is checking for LineOfSight but no AIVision component was found!");
			return false;
		}
		if (ability.m_requirement.HasFlag(AbilityRequirements.IsBehindDoor) && !IsFacingDoor(ability.m_maxDistance))
		{
			return false;
		}
		if (ability.m_requirement.HasFlag(AbilityRequirements.TargetUnreachable) && !IsTargetUnreachable())
		{
			return false;
		}
		if (ability.m_requirement.HasFlag(AbilityRequirements.PlaymakerVariableTrue) || ability.m_requirement.HasFlag(AbilityRequirements.PlaymakerVariableFalse))
		{
			NamedVariable variable = ability.m_playmakerVariableFSMToCheck.Fsm.Variables.GetVariable(ability.m_playmakerVariableName);
			if (variable != null && variable.RawValue is bool)
			{
				if (ability.m_requirement.HasFlag(AbilityRequirements.PlaymakerVariableTrue))
				{
					return (bool)variable.RawValue;
				}
				return !(bool)variable.RawValue;
			}
			return false;
		}
		if (ability.m_requirement.HasFlag(AbilityRequirements.IsGrounded) && (m_movement == null || !m_movement.IsGrounded))
		{
			return false;
		}
		if (ability.m_requirement.HasFlag(AbilityRequirements.TargetIsFacingMe))
		{
			CharacterDirection component3 = m_currentTarget.gameObject.GetComponent<CharacterDirection>();
			if (component3 != null && !component3.IsFacingPoint(base.transform.position))
			{
				return false;
			}
		}
		if (ability.m_requirement.HasFlag(AbilityRequirements.TargetIsNotFacingMe))
		{
			CharacterDirection component4 = m_currentTarget.gameObject.GetComponent<CharacterDirection>();
			if (component4 != null && component4.IsFacingPoint(base.transform.position))
			{
				return false;
			}
		}
		if (ability.m_requirement.HasFlag(AbilityRequirements.TargetIsAboveMe) && m_currentTarget.transform.position.y - base.transform.position.y < 1f)
		{
			return false;
		}
		if (ability.m_requirement.HasFlag(AbilityRequirements.TargetIsOnSameLevel) && Mathf.Abs(m_currentTarget.transform.position.y - base.transform.position.y) >= 1f)
		{
			return false;
		}
		if (ability.m_requirement.HasFlag(AbilityRequirements.TargetIsBelowMe) && m_currentTarget.transform.position.y - base.transform.position.y > -1f)
		{
			return false;
		}
		return true;
	}

	private bool IsFacingDoor(float distance)
	{
		Vector2 origin = base.transform.position;
		origin.y += 0.5f;
		Vector2 direction = m_direction.GetForwardVector();
		RaycastHit2D[] array = Physics2D.RaycastAll(origin, direction, distance);
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit2D raycastHit2D = array[i];
			if (raycastHit2D.collider != null)
			{
				NewSideDoor componentInParent = raycastHit2D.collider.GetComponentInParent<NewSideDoor>();
				if (componentInParent != null && componentInParent.IsClosed())
				{
					return true;
				}
				SideDoor componentInParent2 = raycastHit2D.collider.GetComponentInParent<SideDoor>();
				if (componentInParent2 != null && componentInParent2.IsClosed())
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool IsTargetUnreachable()
	{
		Vector2 vector = m_currentTarget.transform.position;
		Vector2 vector2 = base.transform.position;
		Vector2 vector3 = vector - vector2;
		float magnitude = vector3.magnitude;
		Collider2D collider = m_movement.Collider;
		Vector2 origin = collider.bounds.center;
		Vector2 size = collider.bounds.size;
		size *= 0.9f;
		magnitude -= size.x;
		if (magnitude < size.magnitude)
		{
			return false;
		}
		if (Physics2D.BoxCast(origin, size, 0f, vector3.normalized, magnitude, m_movement.CollisionMask).collider != null)
		{
			return true;
		}
		return false;
	}

	private List<AbilitySettings> GetValidAbilities()
	{
		List<AbilitySettings> list = new List<AbilitySettings>();
		if (m_characterCooldownTimer <= 0f && !LevelManager.LevelTransitionActive && !GameCutsceneManager.CutsceneActive)
		{
			AbilitySettings[] abilities = m_abilities;
			foreach (AbilitySettings abilitySettings in abilities)
			{
				if (!abilitySettings.m_requirement.HasFlag(AbilityRequirements.IsBehindDoor))
				{
					float distanceFromAttackTarget = GetDistanceFromAttackTarget(abilitySettings.m_lookAheadPredictionTime, abilitySettings.m_ignoreHeightForDistance);
					if (distanceFromAttackTarget < abilitySettings.m_minDistance || distanceFromAttackTarget > abilitySettings.m_maxDistance)
					{
						continue;
					}
				}
				if (!(abilitySettings.m_cooldownTimer > 0f) && CheckAbilitySettingsFlags(abilitySettings))
				{
					list.Add(abilitySettings);
				}
			}
		}
		return list;
	}

	public void DoAttackBehavior(AttackBehaviorMovementMode movementMode)
	{
		switch (movementMode)
		{
		case AttackBehaviorMovementMode.UseCombatDirectorPosition:
			SetGoalToDirectorNavigationPos();
			break;
		case AttackBehaviorMovementMode.MoveToTargetWithPrediction:
		{
			Vector2 position = m_currentTarget.transform.position;
			position += m_currentTarget.GetComponent<CharacterMovement>().PredictedPositionOffsetSmoothed * 0.5f;
			SetGoalToPosition(position, GetGoalDistance());
			break;
		}
		}
		if (!CharacterStateCanAttack)
		{
			SetRequestingAttack(requesting: false);
		}
		else
		{
			if (m_activeAbility != null)
			{
				return;
			}
			List<AbilitySettings> validAbilities = GetValidAbilities();
			if (validAbilities.Count > 0)
			{
				if ((m_queuedAbility = PickRandomAbility(validAbilities)).m_requireAttackPermission)
				{
					SetRequestingAttack(requesting: true);
				}
				else
				{
					PermitAttack();
				}
			}
			else
			{
				LookAtTargetIfNotLookingAtThem();
				SetRequestingAttack(requesting: false);
			}
		}
	}

	private void LookAtTargetIfNotLookingAtThem()
	{
		Vector2 position = m_currentTarget.transform.position;
		if (!m_direction.IsFacingPoint(position))
		{
			m_desiredFacingDirection = ((base.transform.position.x < position.x) ? CharacterDirection.Facing.Right : CharacterDirection.Facing.Left);
		}
		else
		{
			m_desiredFacingDirection = CharacterDirection.Facing.None;
		}
	}

	private AbilitySettings PickRandomAbility(List<AbilitySettings> validAbilities)
	{
		if (validAbilities.Count == 1)
		{
			return validAbilities[0];
		}
		float num = 0f;
		foreach (AbilitySettings validAbility in validAbilities)
		{
			num += validAbility.m_weight;
		}
		float num2 = UnityEngine.Random.Range(0f, num);
		float num3 = 0f;
		foreach (AbilitySettings validAbility2 in validAbilities)
		{
			num3 += validAbility2.m_weight;
			if (num3 >= num2)
			{
				return validAbility2;
			}
		}
		return null;
	}

	public void ExitAttackBehavior()
	{
		SetRequestingAttack(requesting: false);
	}

	public override AIAbility GetRequestedAbility()
	{
		if (m_activeAbility != null && m_activeAbility.m_delayTimer <= 0f)
		{
			return m_activeAbility.m_abilityType;
		}
		return AIAbility.None;
	}

	public void ConfirmAbility(AIAbility abilityType)
	{
		if (m_activeAbility != null && m_activeAbility.m_abilityType == abilityType)
		{
			ConfirmAbility(m_activeAbility);
			if (!string.IsNullOrEmpty(m_activeAbility.m_fsmEvent))
			{
				m_aiFSM.SendEvent(m_activeAbility.m_fsmEvent);
			}
		}
	}

	private void ConfirmAbility(AbilitySettings ability)
	{
		ability.m_abilityConfirmed = true;
		float random = ability.m_cooldownDuration.GetRandom();
		if (ability.m_cooldownBlocksAllAbilities)
		{
			m_characterCooldownTimer = random;
		}
		else
		{
			ability.m_cooldownTimer = random;
		}
	}

	public override AttackRequestResult GetAttackRequestResult()
	{
		if (m_activeAbility != null)
		{
			if (m_activeAbility.m_abilityConfirmed)
			{
				return AttackRequestResult.Confirmed;
			}
			if (Time.time > m_activeAbility.m_requestTime + 0.1f)
			{
				return AttackRequestResult.Failed;
			}
			return AttackRequestResult.Waiting;
		}
		return AttackRequestResult.Failed;
	}

	public override float DrawDebugInfo(Rect boxRect, float yPos)
	{
		Rect position = new Rect(boxRect);
		position.height = 20f;
		position.y = yPos;
		GUI.Label(position, "Requesting Attack: " + m_requestingAttack);
		position.y += position.height;
		GUI.Label(position, "Abilities:");
		position.y += position.height;
		GUI.Label(position, "Distance from target: " + GetDistanceFromAttackTarget(0f, ignoreHeight: false));
		position.y += position.height;
		GUI.Label(position, "Character Cooldown: " + m_characterCooldownTimer);
		position.y += position.height;
		position.x += 8f;
		AbilitySettings[] abilities = m_abilities;
		foreach (AbilitySettings abilitySettings in abilities)
		{
			string text = abilitySettings.m_name;
			if (abilitySettings == m_activeAbility)
			{
				text += "(Active)";
			}
			if (abilitySettings == m_queuedAbility)
			{
				text += "(Queued)";
			}
			GUI.Label(position, text);
			position.y += position.height;
			position.x += 8f;
			if (abilitySettings.m_cooldownTimer > 0f)
			{
				GUI.Label(position, "Cooldown: " + abilitySettings.m_cooldownTimer);
				position.y += position.height;
			}
			if (abilitySettings.m_lookAheadPredictionTime > 0f)
			{
				GUI.Label(position, "Pred. Tgt. Dist: " + GetDistanceFromAttackTarget(abilitySettings.m_lookAheadPredictionTime, abilitySettings.m_ignoreHeightForDistance));
				position.y += position.height;
			}
			position.x -= 8f;
		}
		return position.y;
	}
}
