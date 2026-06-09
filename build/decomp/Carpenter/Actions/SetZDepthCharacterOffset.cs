using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class SetZDepthCharacterOffset : FsmStateAction
{
	public FsmOwnerDefault m_gameObject;

	public float m_zOffset;

	public float m_overrideDepthMoveSpeed = -1f;

	private CharacterMovement m_characterMovement;

	public override void OnEnter()
	{
		GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(m_gameObject);
		m_characterMovement = ownerDefaultTarget.GetComponent<CharacterMovement>();
		if (m_characterMovement != null)
		{
			m_characterMovement.DepthStateOffset = m_zOffset;
			if (m_overrideDepthMoveSpeed > 0f)
			{
				m_characterMovement.ZDepthMoveSpeedOverride = m_overrideDepthMoveSpeed;
			}
		}
		else
		{
			Vector3 position = base.Owner.transform.position;
			position.z = m_zOffset;
			ownerDefaultTarget.transform.position = position;
		}
		Finish();
	}

	public override void OnExit()
	{
		GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(m_gameObject);
		m_characterMovement = ownerDefaultTarget.GetComponent<CharacterMovement>();
		if (m_characterMovement != null)
		{
			m_characterMovement.DepthStateOffset = 0f;
			if (m_overrideDepthMoveSpeed > 0f)
			{
				m_characterMovement.ZDepthMoveSpeedOverride = -1f;
			}
		}
		else
		{
			Vector3 position = base.Owner.transform.position;
			position.z = 0f;
			ownerDefaultTarget.transform.position = position;
		}
	}
}
