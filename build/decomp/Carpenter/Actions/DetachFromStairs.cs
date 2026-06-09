using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class DetachFromStairs : FsmStateAction
{
	protected LegacyCharacterMovement m_characterMovement;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterMovement = base.Owner.GetComponent<LegacyCharacterMovement>();
		}
	}

	public override void OnEnter()
	{
		Debug.LogWarning("Old DetachFromStairs still used!?!?!?");
		Finish();
	}
}
