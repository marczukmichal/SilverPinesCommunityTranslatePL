using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Combat")]
public class SetAutoFaceDirectionOnHit : FsmStateAction
{
	public AutoFaceDirectionOnHit m_autoFaceSetting;

	public bool m_resetOnExit;

	private CharacterHitReact m_characterHitReact;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterHitReact = base.Owner.GetComponent<CharacterHitReact>();
		}
	}

	public override void OnEnter()
	{
		m_characterHitReact.AutoFaceAttackOnHit = m_autoFaceSetting;
		Finish();
	}

	public override void OnExit()
	{
		if (m_resetOnExit)
		{
			m_characterHitReact.AutoFaceAttackOnHit = AutoFaceDirectionOnHit.Never;
		}
		Finish();
	}
}
