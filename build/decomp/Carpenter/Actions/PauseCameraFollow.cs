using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Camera)]
public class PauseCameraFollow : FsmStateAction
{
	public bool m_enable = true;

	public bool m_revertOnExit;

	public override void OnEnter()
	{
		GlobalReferences.Instance.EventChannels.Camera.PauseCameraFollow.Raise(m_enable);
		Finish();
	}

	public override void OnExit()
	{
		if (m_revertOnExit)
		{
			GlobalReferences.Instance.EventChannels.Camera.PauseCameraFollow.Raise(!m_enable);
		}
	}
}
