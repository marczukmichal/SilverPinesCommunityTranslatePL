using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Camera)]
public class CameraAdjust : FsmStateAction
{
	public Vector2 m_offset = Vector2.zero;

	protected CharacterCameraFollow m_cameraFollow;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_cameraFollow = base.Owner.GetComponent<CharacterCameraFollow>();
		}
	}

	public override void OnEnter()
	{
		m_cameraFollow.AimOffset = m_offset;
	}

	public override void OnExit()
	{
		m_cameraFollow.AimOffset = Vector2.zero;
	}
}
