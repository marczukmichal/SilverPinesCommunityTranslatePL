using HutongGames.PlayMaker;
using UnityEngine;

[ActionCategory(ActionCategory.Character)]
public class SetCharacterDirectionLookAtTarget : FsmStateAction
{
	public FsmOwnerDefault m_character;

	private AISenses m_aiSenses;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_aiSenses = base.Owner.GetComponent<AISenses>();
		}
	}

	public override void OnEnter()
	{
		if ((bool)m_aiSenses.CurrentTarget)
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(m_character);
			CharacterDirection component = ownerDefaultTarget.GetComponent<CharacterDirection>();
			Vector2 vector = m_aiSenses.CurrentTarget.transform.position - ownerDefaultTarget.transform.position;
			if (vector.x > 0f)
			{
				component.CurrentDirection = CharacterDirection.Facing.Right;
			}
			else if (vector.x < 0f)
			{
				component.CurrentDirection = CharacterDirection.Facing.Left;
			}
		}
		Finish();
	}
}
