using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class StairsPortalMovement : FsmStateAction
{
	protected StairsPortalUser m_stairsPortalUser;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_stairsPortalUser = base.Owner.GetComponent<StairsPortalUser>();
		}
	}

	public override void OnEnter()
	{
		m_stairsPortalUser.DoInstantMovement();
		Finish();
	}

	public override void OnExit()
	{
		base.OnExit();
		m_stairsPortalUser.Clear();
	}
}
