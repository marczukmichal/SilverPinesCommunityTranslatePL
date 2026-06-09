using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Special")]
public class ApplyMaxStaminaUpgrade : FsmStateAction
{
	protected CharacterStamina m_characterStamina;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterStamina = base.Owner.GetComponent<CharacterStamina>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_characterStamina.UpgradeMaxStamina();
		Finish();
	}
}
