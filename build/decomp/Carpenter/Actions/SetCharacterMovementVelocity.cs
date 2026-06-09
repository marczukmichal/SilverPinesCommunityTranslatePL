using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class SetCharacterMovementVelocity : FsmStateAction
{
	public Vector2 m_velocity;

	protected CharacterMovement m_charMovement;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_charMovement = base.Owner.GetComponent<CharacterMovement>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_charMovement.SetPreviousVelocity(m_velocity);
		Finish();
	}
}
