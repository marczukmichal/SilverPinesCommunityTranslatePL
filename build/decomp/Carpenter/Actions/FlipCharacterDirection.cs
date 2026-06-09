using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class FlipCharacterDirection : FsmStateAction
{
	[SerializeField]
	public bool m_onEnter;

	[SerializeField]
	public bool m_onExit;

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
		if (m_onEnter)
		{
			Flip();
		}
		Finish();
	}

	public override void OnExit()
	{
		if (m_onExit)
		{
			Flip();
		}
	}

	private void Flip()
	{
		switch (m_charDirection.CurrentDirection)
		{
		case CharacterDirection.Facing.Right:
			m_charDirection.CurrentDirection = CharacterDirection.Facing.Left;
			break;
		case CharacterDirection.Facing.Left:
			m_charDirection.CurrentDirection = CharacterDirection.Facing.Right;
			break;
		}
	}
}
