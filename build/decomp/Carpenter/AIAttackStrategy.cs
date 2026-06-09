using System;
using UnityEngine;
using UnityEngine.Events;

public abstract class AIAttackStrategy : BaseAIStrategy, IAICombatPawn
{
	private enum DesiredTargetCloseness
	{
		Close,
		Medium,
		Far,
		VeryFar,
		ExtremeFar,
		Static
	}

	[SerializeField]
	private AICombatPawnType m_aiCombatPawnType;

	[SerializeField]
	protected AISenses m_senses;

	[SerializeField]
	protected bool m_canTurnAwayFromTarget;

	protected bool m_retreatMode;

	[SerializeField]
	private DesiredTargetCloseness m_desiredTargetCloseness;

	protected Detectable m_currentTarget;

	protected AICombatDirector m_currentCombatDirector;

	protected CharacterDirection m_direction;

	protected Vector3 m_directorGoalPos;

	protected bool m_requestingAttack;

	protected bool m_incapacitated;

	private float m_updateTimer;

	private static readonly float m_aiMovementUpdateRate = 0.1f;

	protected bool CanTurnAwayFromTarget
	{
		get
		{
			if (m_retreatMode)
			{
				return true;
			}
			return m_canTurnAwayFromTarget;
		}
	}

	public bool RequestingAttack => m_requestingAttack;

	public AICombatPawnType GetAICombatPawnType()
	{
		return m_aiCombatPawnType;
	}

	public void SetRetreatMode(bool enabled)
	{
		m_retreatMode = enabled;
	}

	private int GetCombatSpotIndex()
	{
		return m_desiredTargetCloseness switch
		{
			DesiredTargetCloseness.Close => 0, 
			DesiredTargetCloseness.Medium => 1, 
			DesiredTargetCloseness.Far => 2, 
			DesiredTargetCloseness.VeryFar => 3, 
			DesiredTargetCloseness.ExtremeFar => 4, 
			DesiredTargetCloseness.Static => -1, 
			_ => 0, 
		};
	}

	protected virtual void Start()
	{
		StatusEffectReceiver component = GetComponent<StatusEffectReceiver>();
		if (component != null)
		{
			component.m_onStatusEffectActiveChanged = (UnityAction<StatusEffectInstance, bool>)Delegate.Combine(component.m_onStatusEffectActiveChanged, new UnityAction<StatusEffectInstance, bool>(OnStatusEffectsChanged));
		}
		m_direction = GetComponent<CharacterDirection>();
	}

	private void OnStatusEffectsChanged(StatusEffectInstance statusEffect, bool active)
	{
		if (statusEffect.Definition == GlobalReferences.Instance.StatusEffects.Generic.Knockdown)
		{
			m_incapacitated = active;
			if (m_incapacitated && m_currentCombatDirector != null)
			{
				m_currentCombatDirector.Unregister(this);
			}
		}
	}

	public override void EnableStrategy()
	{
		base.EnableStrategy();
		m_currentTarget = m_senses.CurrentTarget;
		AICombatDirector component = m_currentTarget.GetComponent<AICombatDirector>();
		if ((object)component != null)
		{
			m_currentCombatDirector = component;
			m_directorGoalPos = component.GetGoalPosition(this, GetCombatSpotIndex());
		}
	}

	public override void DisableStrategy()
	{
		base.DisableStrategy();
		if (m_currentCombatDirector != null)
		{
			m_currentCombatDirector.Unregister(this);
			m_currentCombatDirector = null;
		}
	}

	private void OnDestroy()
	{
		if (m_currentCombatDirector != null)
		{
			m_currentCombatDirector.Unregister(this);
		}
	}

	public override void UpdateStrategy()
	{
		base.UpdateStrategy();
		if (m_incapacitated)
		{
			return;
		}
		m_updateTimer += Time.deltaTime;
		if (m_updateTimer < m_aiMovementUpdateRate)
		{
			return;
		}
		m_updateTimer = 0f;
		if (m_currentTarget != m_senses.CurrentTarget)
		{
			if (m_currentCombatDirector != null)
			{
				m_currentCombatDirector.Unregister(this);
				m_currentCombatDirector = null;
			}
			m_currentTarget = m_senses.CurrentTarget;
			AICombatDirector component = m_currentTarget.GetComponent<AICombatDirector>();
			if ((object)component != null)
			{
				m_currentCombatDirector = component;
			}
		}
		if (!(m_currentCombatDirector != null))
		{
			return;
		}
		Vector3 goalPosition = m_currentCombatDirector.GetGoalPosition(this, GetCombatSpotIndex());
		if (!CanTurnAwayFromTarget)
		{
			Vector2 position = m_currentTarget.transform.position;
			if (m_direction.IsTurning || (m_direction.IsFacingPoint(position) && !m_direction.IsFacingPoint(goalPosition)))
			{
				m_directorGoalPos = base.transform.position;
			}
			else
			{
				m_directorGoalPos = goalPosition;
			}
		}
		else
		{
			m_directorGoalPos = goalPosition;
		}
	}

	public Vector3 GetNavigationPosition()
	{
		return base.transform.position;
	}

	public void SetRequestingAttack(bool requesting)
	{
		m_requestingAttack = requesting;
	}

	public bool IsRequestingAttack()
	{
		return m_requestingAttack;
	}

	public abstract bool PermitAttack();

	public abstract AttackRequestResult GetAttackRequestResult();
}
