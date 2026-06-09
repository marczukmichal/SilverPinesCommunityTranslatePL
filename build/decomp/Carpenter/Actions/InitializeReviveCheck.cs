using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class InitializeReviveCheck : FsmStateAction
{
	[Tooltip("Event to trigger if can be revived")]
	public FsmEvent m_onReviveEvent = new FsmEvent("Dead/Revive");

	private CharacterNecromancy m_necromancy;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_necromancy = base.Owner.GetComponent<CharacterNecromancy>();
		}
	}

	private void Check()
	{
		if (m_necromancy != null && m_necromancy.CanRevive)
		{
			base.Fsm.Event(m_onReviveEvent);
		}
	}

	public override void OnEnter()
	{
		Check();
		Finish();
	}
}
