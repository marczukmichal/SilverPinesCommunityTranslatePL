using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Special")]
public class ShowBossIntroductionUI : FsmStateAction
{
	public BossIntroductionSettings m_bossIntroductionSettings;

	public override void OnEnter()
	{
		base.OnEnter();
		GlobalReferences.Instance.EventChannels.Generic.BossIntroduction.Raise(m_bossIntroductionSettings);
		GlobalReferences.Instance.EventChannels.Generic.PlayerInputOverride.Raise(PlayerInputOverride.Static);
		GlobalReferences.Instance.EventChannels.Generic.SetCutsceneIsPlaying.Raise(value: true);
		Finish();
	}

	public override void OnExit()
	{
		base.OnExit();
		GlobalReferences.Instance.EventChannels.Generic.SetCutsceneIsPlaying.Raise(value: false);
		GlobalReferences.Instance.EventChannels.Generic.PlayerInputOverride.Raise(PlayerInputOverride.None);
	}
}
