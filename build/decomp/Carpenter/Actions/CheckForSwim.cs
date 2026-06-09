using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class CheckForSwim : FsmStateAction
{
	public FsmEvent m_startSwimEvent;

	public FsmEvent m_stopSwimEvent;

	public float m_submergeDepth = 1.5f;

	public override void Reset()
	{
		base.Reset();
		m_startSwimEvent = new FsmEvent("Movement/Swim/Enter");
		m_stopSwimEvent = new FsmEvent("Movement/Swim/Exit");
	}

	public override void OnUpdate()
	{
		Check();
	}

	private void Check()
	{
		if (GameplayWaterBounds.GetActiveWaterBound(base.Owner.transform.position, m_submergeDepth) != null)
		{
			base.Fsm.Event(m_startSwimEvent);
		}
		else
		{
			base.Fsm.Event(m_stopSwimEvent);
		}
	}
}
