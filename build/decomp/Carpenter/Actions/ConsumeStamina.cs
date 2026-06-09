using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Combat")]
public class ConsumeStamina : FsmStateAction
{
	public CharacterStamina.StaminaEventType m_staminaEventType;

	public bool m_constantDrain;

	public bool m_blockRegenWhileInState;

	private CharacterStamina m_stamina;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_stamina = base.Owner.GetComponent<CharacterStamina>();
		}
	}

	public override void OnEnter()
	{
		if (m_staminaEventType == CharacterStamina.StaminaEventType.None && !m_blockRegenWhileInState)
		{
			Debug.LogError("Incorrectly configured stamina consumption setup for state: " + base.State.Name);
		}
		if (!m_constantDrain)
		{
			m_stamina.ConsumeStamina(m_staminaEventType);
		}
	}

	public override void OnUpdate()
	{
		if (m_constantDrain)
		{
			m_stamina.ConsumeStamina(m_staminaEventType, Time.deltaTime);
		}
		else if (m_blockRegenWhileInState)
		{
			m_stamina.ConsumeStamina(0f);
		}
	}
}
