using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class DiedFromWeakpoint : FsmStateAction
{
	[Tooltip("Event to trigger if killed by weakpoint damage")]
	public FsmEvent m_onWeakpointKilled;

	[Tooltip("Event to trigger if killed by normal damage")]
	public FsmEvent m_onNormalKilled;

	private CharacterHealth m_characterHealth;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterHealth = base.Owner.GetComponent<CharacterHealth>();
		}
	}

	public override void OnEnter()
	{
		Check();
		Finish();
	}

	private void Check()
	{
		if (m_onWeakpointKilled != null && m_characterHealth.DiedFromWeakpoint)
		{
			base.Fsm.Event(m_onWeakpointKilled);
		}
		if (m_onNormalKilled != null && !m_characterHealth.DiedFromWeakpoint)
		{
			base.Fsm.Event(m_onNormalKilled);
		}
	}
}
