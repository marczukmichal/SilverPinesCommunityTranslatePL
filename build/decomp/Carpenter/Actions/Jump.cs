using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class Jump : FsmStateAction
{
	protected LegacyCharacterMovement m_charMovement;

	public Vector2 m_jumpVelocity;

	public float m_forceFallTime;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_charMovement = base.Owner.GetComponent<LegacyCharacterMovement>();
		}
	}

	public override void OnEnter()
	{
	}

	public override void OnExit()
	{
	}
}
