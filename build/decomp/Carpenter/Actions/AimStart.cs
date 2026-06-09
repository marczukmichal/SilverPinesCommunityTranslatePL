using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Combat")]
public class AimStart : FsmStateAction
{
	private CharacterAiming m_characterAiming;

	private CharacterInputPlayer m_characterInput;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterAiming = base.Owner.GetComponent<CharacterAiming>();
			m_characterInput = base.Owner.GetComponent<CharacterInputPlayer>();
		}
	}

	public override void OnEnter()
	{
		m_characterInput.OnStartCharacterAiming();
		m_characterAiming.ResetAim();
		Finish();
	}
}
