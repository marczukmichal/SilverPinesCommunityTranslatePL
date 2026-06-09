using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class CheckForStairs : FsmStateAction
{
	public FsmEvent m_attachToStairsEvent;

	public override void Awake()
	{
		_ = base.Owner == null;
	}

	public override void Reset()
	{
		base.Reset();
		m_attachToStairsEvent = new FsmEvent("Movement/Stairs/Enter");
	}

	public override void OnUpdate()
	{
	}

	private void Check()
	{
	}

	public override void OnExit()
	{
	}

	private void AttachToStairs(Stairs stairs)
	{
	}
}
