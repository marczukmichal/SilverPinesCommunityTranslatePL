using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class SetIsTurning : FsmStateAction
{
	protected CharacterDirection m_charDirection;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_charDirection = base.Owner.GetComponent<CharacterDirection>();
		}
	}

	public override void OnEnter()
	{
		m_charDirection.IsTurning = true;
		Finish();
	}

	public override void OnExit()
	{
		m_charDirection.IsTurning = false;
	}
}
