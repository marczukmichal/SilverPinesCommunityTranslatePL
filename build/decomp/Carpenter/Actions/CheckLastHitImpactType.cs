using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Combat")]
public class CheckLastHitImpactType : FsmStateAction
{
	public FsmEvent m_impactSmall;

	public FsmEvent m_impactMedium;

	public FsmEvent m_impactLarge;

	public bool m_checkDirection;

	public FsmEvent m_impactSmallFromBehind;

	public FsmEvent m_impactMediumFromBehind;

	public FsmEvent m_impactLargeFromBehind;

	private CharacterHitReact m_hitReact;

	private CharacterDirection m_direction;

	private StatusEffectReceiver m_effectReceiver;

	public override void Awake()
	{
		base.Awake();
		if (base.Owner != null)
		{
			m_hitReact = base.Owner.GetComponent<CharacterHitReact>();
			m_direction = base.Owner.GetComponent<CharacterDirection>();
			m_effectReceiver = base.Owner.GetComponent<StatusEffectReceiver>();
		}
	}

	public override void OnEnter()
	{
		bool flag = false;
		ImpactType impactType = m_hitReact.LastHit.ImpactType;
		if (m_checkDirection && m_hitReact.LastHit.ImpactReactDirection != ImpactReactDirection.ForceLookAtSource && m_direction.IsFacingDirection(m_hitReact.LastHit.Direction.x))
		{
			flag = true;
		}
		if (m_effectReceiver != null && m_effectReceiver.IsStatusEffectActive(GlobalReferences.Instance.StatusEffects.Generic.Knockdown))
		{
			impactType = ImpactType.Large;
		}
		switch (impactType)
		{
		case ImpactType.Large:
			base.Fsm.Event(flag ? m_impactLargeFromBehind : m_impactLarge);
			break;
		case ImpactType.Medium:
			base.Fsm.Event(flag ? m_impactMediumFromBehind : m_impactMedium);
			break;
		default:
			base.Fsm.Event(flag ? m_impactSmallFromBehind : m_impactSmall);
			break;
		}
		Finish();
	}
}
