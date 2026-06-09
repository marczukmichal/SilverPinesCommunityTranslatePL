using HutongGames.PlayMaker;
using UnityEngine;

[ActionCategory(ActionCategory.Character)]
public class SetCharacterDirectionLookAtPosition : FsmStateAction
{
	public FsmOwnerDefault m_character;

	[UIHint(UIHint.Variable)]
	public FsmVector3 m_position;

	public override void OnEnter()
	{
		_ = m_position.Value;
		GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(m_character);
		CharacterDirection component = ownerDefaultTarget.GetComponent<CharacterDirection>();
		Vector2 vector = m_position.Value - ownerDefaultTarget.transform.position;
		if (vector.x > 0f)
		{
			component.CurrentDirection = CharacterDirection.Facing.Right;
		}
		else if (vector.x < 0f)
		{
			component.CurrentDirection = CharacterDirection.Facing.Left;
		}
		Finish();
	}
}
