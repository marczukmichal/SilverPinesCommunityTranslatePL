using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Input)]
public class InputClearCrouchToggleOnExit : FsmStateAction
{
	private CharacterInputPlayer m_characterInput;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterInput = base.Owner.GetComponent<CharacterInputPlayer>();
		}
	}

	public override void OnEnter()
	{
		m_characterInput.ForceSetCrouchToggle();
	}

	public override void OnExit()
	{
		m_characterInput.ClearCrouchToggle();
	}
}
