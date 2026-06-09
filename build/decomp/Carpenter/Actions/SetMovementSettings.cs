using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class SetMovementSettings : FsmStateAction
{
	[SerializeField]
	public CharacterMovementSettings m_movementSettings;

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
		if (m_charMovement != null)
		{
			m_charMovement.SetMovementSettingsOverride(m_movementSettings);
		}
		Finish();
	}

	public override void OnExit()
	{
		if (m_charMovement != null)
		{
			m_charMovement.SetMovementSettingsOverride(null);
		}
	}
}
