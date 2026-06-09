using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Combat")]
public class AimForceFireWeapon : FsmStateAction
{
	private CharacterAiming m_aim;

	public override void Awake()
	{
		base.Awake();
		if (base.Owner != null)
		{
			m_aim = base.Owner.GetComponent<CharacterAiming>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_aim.SetWeaponFiring(firing: true);
		Finish();
	}

	public override void OnExit()
	{
		base.OnExit();
		m_aim.SetWeaponFiring(firing: false);
	}
}
