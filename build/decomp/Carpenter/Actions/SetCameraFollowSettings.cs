using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Camera)]
public class SetCameraFollowSettings : FsmStateAction
{
	public CameraFollowSettings m_settings;

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
		m_cameraFollow.ActiveSettings = m_settings;
	}

	public override void OnExit()
	{
		m_cameraFollow.ActiveSettings = null;
	}
}
