using System;
using HutongGames.PlayMaker;
using UnityEngine.Events;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class CheckForStompAttack : FsmStateAction
{
	public FsmEvent m_onStompEvent;

	public bool m_forceAllowStomp;

	protected BaseCharacterInput m_input;

	private StompActionCheck m_stompActionCheck;

	private CharacterStamina m_stamina;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_input = base.Owner.GetCharacterInputComponent();
			m_stompActionCheck = base.Owner.GetComponentInChildren<StompActionCheck>();
			m_stamina = base.Owner.GetComponent<CharacterStamina>();
		}
	}

	public override void OnEnter()
	{
		BaseCharacterInput input = m_input;
		input.OnStompAction = (UnityAction)Delegate.Combine(input.OnStompAction, new UnityAction(OnAttack));
		m_stompActionCheck.StompEnabled = true;
		if (m_forceAllowStomp)
		{
			m_stompActionCheck.ForceAllowStomp = true;
		}
	}

	public override void OnExit()
	{
		BaseCharacterInput input = m_input;
		input.OnStompAction = (UnityAction)Delegate.Remove(input.OnStompAction, new UnityAction(OnAttack));
		m_stompActionCheck.StompEnabled = false;
		if (m_forceAllowStomp)
		{
			m_stompActionCheck.ForceAllowStomp = false;
		}
	}

	private void OnAttack()
	{
		if (!m_stompActionCheck.CanStomp)
		{
			return;
		}
		if (m_stamina.IsExhausted)
		{
			if (GameUtils.IsPlayer(base.Owner))
			{
				GlobalReferences.Instance.EventChannels.Gameplay.InsufficientStamina.Raise();
			}
		}
		else
		{
			base.Fsm.Event(m_onStompEvent);
		}
	}
}
