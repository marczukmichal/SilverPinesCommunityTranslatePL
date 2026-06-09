using System;
using UnityEngine;
using UnityEngine.Events;

public class AIBrain : BaseCharacterInput
{
	public enum AlertState
	{
		None,
		Idle,
		Combat
	}

	[SerializeField]
	private BaseAIStrategy m_idleStrategy;

	[SerializeField]
	private BaseAIStrategy m_combatStrategy;

	[SerializeField]
	private AISenses m_senses;

	[SerializeField]
	private UnityEvent m_enteredCombat;

	[DebugCommand("ai_dumb", "AI is dumb and doesn't do much", "ai_dumb <true/false>", typeof(bool), false)]
	private static bool s_aiDumb;

	private IAIBrainControlAction m_activeControlAction;

	private AlertState m_currentAlertState;

	public IAIBrainControlAction ActiveControlAction
	{
		get
		{
			return m_activeControlAction;
		}
		set
		{
			m_activeControlAction = value;
		}
	}

	public AlertState CurrentAlertState
	{
		get
		{
			return m_currentAlertState;
		}
		set
		{
			if (m_currentAlertState != value)
			{
				BaseAIStrategy strategyForAlertState = GetStrategyForAlertState(m_currentAlertState);
				if ((bool)strategyForAlertState)
				{
					strategyForAlertState.DisableStrategy();
				}
				m_currentAlertState = value;
				BaseAIStrategy strategyForAlertState2 = GetStrategyForAlertState(m_currentAlertState);
				if ((bool)strategyForAlertState2)
				{
					strategyForAlertState2.EnableStrategy();
				}
				if (m_currentAlertState == AlertState.Combat)
				{
					m_enteredCombat.Invoke();
				}
				if (m_currentAlertState == AlertState.Combat && m_senses != null)
				{
					m_senses.SetOnlyDangerSense(onlyDangerSense: false);
				}
			}
		}
	}

	public override Vector2 MovementInput
	{
		get
		{
			if (!base.enabled)
			{
				return Vector2.zero;
			}
			if (m_activeControlAction != null)
			{
				return m_activeControlAction.MoveDirection;
			}
			if (s_aiDumb)
			{
				return Vector2.zero;
			}
			BaseAIStrategy strategyForAlertState = GetStrategyForAlertState(m_currentAlertState);
			if (strategyForAlertState != null)
			{
				return strategyForAlertState.MoveDirection;
			}
			return Vector2.zero;
		}
	}

	public override CharacterDirection.Facing FacingDirectionInput
	{
		get
		{
			if (m_activeControlAction != null)
			{
				return m_activeControlAction.FacingDirectionInput;
			}
			BaseAIStrategy strategyForAlertState = GetStrategyForAlertState(m_currentAlertState);
			if (strategyForAlertState != null)
			{
				return strategyForAlertState.FacingDirectionInput;
			}
			return CharacterDirection.Facing.None;
		}
	}

	public override bool IsFiring
	{
		get
		{
			if (m_activeControlAction != null)
			{
				return m_activeControlAction.ShouldAttack;
			}
			if (s_aiDumb)
			{
				return false;
			}
			BaseAIStrategy strategyForAlertState = GetStrategyForAlertState(m_currentAlertState);
			if (strategyForAlertState != null)
			{
				return strategyForAlertState.ShouldAttack;
			}
			return false;
		}
	}

	public override bool IsSprinting
	{
		get
		{
			if (m_activeControlAction != null)
			{
				return m_activeControlAction.ShouldSprint;
			}
			BaseAIStrategy strategyForAlertState = GetStrategyForAlertState(m_currentAlertState);
			if (strategyForAlertState != null)
			{
				return strategyForAlertState.ShouldSprint;
			}
			return false;
		}
	}

	public override bool IsJumping
	{
		get
		{
			if (m_activeControlAction != null)
			{
				return m_activeControlAction.ShouldJump;
			}
			BaseAIStrategy strategyForAlertState = GetStrategyForAlertState(m_currentAlertState);
			if (strategyForAlertState != null)
			{
				return strategyForAlertState.ShouldJump;
			}
			return false;
		}
	}

	public override bool TurnDisabled
	{
		get
		{
			BaseAIStrategy strategyForAlertState = GetStrategyForAlertState(m_currentAlertState);
			if (strategyForAlertState != null)
			{
				return strategyForAlertState.TurnDisabled;
			}
			return false;
		}
	}

	private void Reset()
	{
		m_senses = GetComponent<AISenses>();
	}

	private BaseAIStrategy GetStrategyForAlertState(AlertState alertState)
	{
		return alertState switch
		{
			AlertState.Idle => m_idleStrategy, 
			AlertState.Combat => m_combatStrategy, 
			_ => null, 
		};
	}

	private void Start()
	{
		CharacterHealth component = GetComponent<CharacterHealth>();
		if (component != null)
		{
			if (component.IsDead)
			{
				OnDead();
			}
			component.OnDead.AddListener(OnDead);
			component.OnRevive = (UnityAction)Delegate.Combine(component.OnRevive, new UnityAction(OnRevive));
		}
	}

	private void OnDead()
	{
		base.enabled = false;
		CurrentAlertState = AlertState.None;
		m_senses.enabled = false;
	}

	private void OnRevive()
	{
		base.enabled = true;
		CurrentAlertState = AlertState.Idle;
		m_senses.enabled = true;
	}

	private void Update()
	{
		if (CurrentAlertState == AlertState.None)
		{
			CurrentAlertState = AlertState.Idle;
		}
		else if (!s_aiDumb)
		{
			if ((bool)m_senses)
			{
				CurrentAlertState = ((!m_senses.CurrentTarget) ? AlertState.Idle : AlertState.Combat);
			}
			BaseAIStrategy strategyForAlertState = GetStrategyForAlertState(m_currentAlertState);
			if (strategyForAlertState != null)
			{
				strategyForAlertState.UpdateStrategy();
			}
		}
	}

	public void DrawDebugInfo(Rect rect)
	{
		Rect rect2 = new Rect(rect);
		rect2.y -= 500f;
		rect2.width = 200f;
		rect2.height = 256f;
		GUI.Box(rect2, "");
		Rect position = new Rect(rect2);
		position.width = 180f;
		position.height = 20f;
		position.y = rect2.y;
		GUI.Label(position, "AlertState: " + CurrentAlertState);
		position.y += position.height;
		if (m_senses != null)
		{
			position.y = m_senses.DrawDebugInfo(rect, position.y);
		}
		BaseAIStrategy strategyForAlertState = GetStrategyForAlertState(m_currentAlertState);
		if (strategyForAlertState != null)
		{
			position.y += position.height;
			position.y = strategyForAlertState.DrawDebugInfo(rect, position.y);
		}
	}

	public override bool IsDoingAIAbility(AIAbility attack)
	{
		if (m_activeControlAction != null)
		{
			return m_activeControlAction.IsDoingAIAbility(attack);
		}
		BaseAIStrategy strategyForAlertState = GetStrategyForAlertState(m_currentAlertState);
		if (strategyForAlertState != null)
		{
			return strategyForAlertState.GetRequestedAbility() == attack;
		}
		return false;
	}
}
