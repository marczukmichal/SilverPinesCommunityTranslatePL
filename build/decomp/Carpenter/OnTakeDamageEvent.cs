using System;
using HutongGames.PlayMaker;
using UnityEngine.Events;

[ActionCategory(ActionCategory.Character)]
public class OnTakeDamageEvent : FsmStateAction
{
	public FsmEvent m_event;

	private CharacterHealth m_characterHealth;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterHealth = base.Owner.GetComponent<CharacterHealth>();
		}
	}

	public override void OnEnter()
	{
		CharacterHealth characterHealth = m_characterHealth;
		characterHealth.OnTakenDamage = (UnityAction<int>)Delegate.Combine(characterHealth.OnTakenDamage, new UnityAction<int>(OnTakeDamge));
		Finish();
	}

	public override void OnExit()
	{
		base.OnExit();
		CharacterHealth characterHealth = m_characterHealth;
		characterHealth.OnTakenDamage = (UnityAction<int>)Delegate.Remove(characterHealth.OnTakenDamage, new UnityAction<int>(OnTakeDamge));
	}

	private void OnTakeDamge(int damage)
	{
		if (damage > 0)
		{
			base.Fsm.Event(m_event);
		}
	}
}
