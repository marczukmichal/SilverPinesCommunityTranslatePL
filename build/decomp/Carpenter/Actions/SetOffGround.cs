using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class SetOffGround : FsmStateAction
{
	protected CharacterMovement m_charMovement;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_charMovement = base.Owner.GetComponent<CharacterMovement>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_charMovement.SetOffGround();
		Finish();
	}
}
