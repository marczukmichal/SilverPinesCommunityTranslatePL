using HutongGames.PlayMaker;
using UnityEngine;

[ActionCategory(ActionCategory.Character)]
public class SetCharacterDirectionLookAtGameObject : FsmStateAction
{
	public FsmOwnerDefault m_character;

	[UIHint(UIHint.Variable)]
	public FsmGameObject m_gameObject;

	public override void OnEnter()
	{
		if (!(m_gameObject.Value == null))
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(m_character);
			CharacterDirection component = ownerDefaultTarget.GetComponent<CharacterDirection>();
			Vector2 vector = m_gameObject.Value.transform.position - ownerDefaultTarget.transform.position;
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
}
