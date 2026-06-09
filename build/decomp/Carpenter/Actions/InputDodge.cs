using System;
using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.Events;

namespace Actions;

[ActionCategory(ActionCategory.Input)]
public class InputDodge : FsmStateAction
{
	[HutongGames.PlayMaker.Tooltip("Event to trigger for roll")]
	public FsmEvent m_onDodgeEvent;

	[HutongGames.PlayMaker.Tooltip("Event to trigger on non moving")]
	public FsmEvent m_onBackDodgeEvent;

	public bool m_requiresStamina;

	public bool m_canBeAnimationBlocked;

	private static float m_maxQueuedInputTime = 0.2f;

	private BaseCharacterInput m_characterInput;

	private CharacterDirection m_direction;

	private AnimationEventsHelper m_animationEventsHelper;

	private CharacterAiming m_characterAiming;

	private StatusEffectReceiver m_statusEffectReceiver;

	private bool m_queuedInput;

	private float m_queuedInputTime;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterInput = base.Owner.GetCharacterInputComponent();
			m_direction = base.Owner.GetComponent<CharacterDirection>();
			m_animationEventsHelper = base.Owner.GetComponent<AnimationEventsHelper>();
			m_characterAiming = base.Owner.GetComponent<CharacterAiming>();
			m_statusEffectReceiver = base.Owner.GetComponent<StatusEffectReceiver>();
		}
	}

	public override void OnEnter()
	{
		m_queuedInput = false;
		BaseCharacterInput characterInput = m_characterInput;
		characterInput.OnDodgeAction = (UnityAction)Delegate.Combine(characterInput.OnDodgeAction, new UnityAction(OnDodge));
		if (m_canBeAnimationBlocked)
		{
			m_animationEventsHelper.DisableInputBlock();
		}
	}

	public override void OnExit()
	{
		BaseCharacterInput characterInput = m_characterInput;
		characterInput.OnDodgeAction = (UnityAction)Delegate.Remove(characterInput.OnDodgeAction, new UnityAction(OnDodge));
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (m_queuedInput)
		{
			if (Time.time - m_queuedInputTime > m_maxQueuedInputTime)
			{
				m_queuedInput = false;
			}
			if (!IsInputBlocked())
			{
				InputPerformed();
			}
		}
	}

	private bool IsInputBlocked()
	{
		if (m_canBeAnimationBlocked && m_animationEventsHelper.IsAnimationInputBlocked)
		{
			return true;
		}
		if (m_characterAiming != null && m_characterAiming.enabled && m_characterAiming.MovementBlocked)
		{
			return true;
		}
		if (m_statusEffectReceiver != null && m_statusEffectReceiver.IsStatusEffectActive(GlobalReferences.Instance.StatusEffects.Generic.Entangled))
		{
			return true;
		}
		return false;
	}

	private void OnDodge()
	{
		if (IsInputBlocked())
		{
			m_queuedInputTime = Time.time;
			m_queuedInput = true;
		}
		else
		{
			InputPerformed();
		}
	}

	private void InputPerformed()
	{
		bool flag = true;
		if (m_characterInput.MovementInput.x < -0.7f && m_direction.DesiredDirection == CharacterDirection.Facing.Left)
		{
			flag = false;
		}
		else if (m_characterInput.MovementInput.x > 0.7f && m_direction.DesiredDirection == CharacterDirection.Facing.Right)
		{
			flag = false;
		}
		if (flag)
		{
			if (m_onBackDodgeEvent != null)
			{
				base.Fsm.Event(m_onBackDodgeEvent);
			}
		}
		else if (m_onDodgeEvent != null)
		{
			base.Fsm.Event(m_onDodgeEvent);
		}
	}
}
