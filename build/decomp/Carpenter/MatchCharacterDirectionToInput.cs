using HutongGames.PlayMaker;
using UnityEngine;

[ActionCategory(ActionCategory.Character)]
public class MatchCharacterDirectionToInput : FsmStateAction
{
	protected CharacterDirection m_charDirection;

	private BaseCharacterInput m_input;

	public override void Awake()
	{
		base.Awake();
		if (!(base.Owner == null))
		{
			m_charDirection = base.Owner.GetComponent<CharacterDirection>();
			m_input = base.Owner.GetComponent<BaseCharacterInput>();
		}
	}

	public override void OnUpdate()
	{
		Vector2 movementInput = m_input.MovementInput;
		if (movementInput.x > 0f)
		{
			m_charDirection.CurrentDirection = CharacterDirection.Facing.Right;
		}
		else if (movementInput.x < 0f)
		{
			m_charDirection.CurrentDirection = CharacterDirection.Facing.Left;
		}
	}
}
