using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class CharacterTeleport : FsmStateAction
{
	public FsmOwnerDefault m_gameObject;

	public FsmGameObject m_targetObjectPos;

	public FsmVector3 m_targetPosition;

	public bool m_snapToGround;

	private Vector3 TeleportPosition()
	{
		if (m_targetObjectPos.Value != null)
		{
			return m_targetObjectPos.Value.transform.position;
		}
		return m_targetPosition.Value;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(m_gameObject);
		Vector3 position = TeleportPosition();
		ownerDefaultTarget.GetComponent<CharacterMovement>().TeleportToPosition(position, m_snapToGround);
		Finish();
	}
}
