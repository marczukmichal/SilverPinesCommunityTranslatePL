using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class UpdateCharacterDirection : FsmStateAction
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
			m_charDirection.ApplyDirectionChange();
		}
		Finish();
	}

	public override void OnExit()
	{
		if (m_onExit)
		{
			m_charDirection.ApplyDirectionChange();
		}
	}
}
