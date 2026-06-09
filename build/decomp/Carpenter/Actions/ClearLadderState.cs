using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class ClearLadderState : FsmStateAction
{
	protected CharacterTraversalUtils m_traversal;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_traversal = base.Owner.GetComponent<CharacterTraversalUtils>();
		}
	}

	public override void OnEnter()
	{
		if (m_traversal != null)
		{
			m_traversal.AttachToLadder(null);
		}
		Finish();
	}
}
