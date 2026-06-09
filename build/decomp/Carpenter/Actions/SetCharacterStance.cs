using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class SetCharacterStance : FsmStateAction
{
	public FsmOwnerDefault m_gameObject;

	[SerializeField]
	public CharacterStance.Stance m_stance;

	protected CharacterStance m_characterStance;

	public bool m_revertToStandingOnExit = true;

	public override void OnEnter()
	{
		GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(m_gameObject);
		m_characterStance = ownerDefaultTarget.GetComponent<CharacterStance>();
		if (m_characterStance == null)
		{
			Debug.LogError("SetCharacterStance is used but no CharacterStance component is set! In State: " + base.State.Name + " for gameobject " + base.Owner.name);
			Finish();
		}
		else
		{
			m_characterStance.CurrentStance = m_stance;
			Finish();
		}
	}

	public override void OnExit()
	{
		if (m_revertToStandingOnExit)
		{
			m_characterStance.CurrentStance = CharacterStance.Stance.Standing;
		}
	}
}
