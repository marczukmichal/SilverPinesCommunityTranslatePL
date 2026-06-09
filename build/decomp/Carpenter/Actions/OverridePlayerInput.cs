using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Input)]
public class OverridePlayerInput : FsmStateAction
{
	public PlayerInputOverride m_playerInputOverride;

	public bool m_resetOnExit;

	public override void OnEnter()
	{
		base.OnEnter();
		GlobalReferences.Instance.EventChannels.Generic.PlayerInputOverride.Raise(m_playerInputOverride);
		Finish();
	}

	public override void OnExit()
	{
		base.OnExit();
		if (m_resetOnExit)
		{
			GlobalReferences.Instance.EventChannels.Generic.PlayerInputOverride.Raise(PlayerInputOverride.None);
		}
	}
}
