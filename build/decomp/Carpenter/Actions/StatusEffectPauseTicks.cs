using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Status Effects")]
public class StatusEffectPauseTicks : FsmStateAction
{
	protected StatusEffectReceiver m_statusEffectReceiver;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_statusEffectReceiver = base.Owner.GetComponent<StatusEffectReceiver>();
		}
	}

	public override void OnEnter()
	{
		m_statusEffectReceiver.SetTicksPaused(paused: true);
	}

	public override void OnExit()
	{
		m_statusEffectReceiver.SetTicksPaused(paused: false);
	}
}
