using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("AI")]
public class ForgetTarget : FsmStateAction
{
	private AISenses m_aiSenses;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_aiSenses = base.Owner.GetComponent<AISenses>();
		}
	}

	public override void OnEnter()
	{
		m_aiSenses.ForgetTarget();
		Finish();
	}
}
