using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Input)]
public class InputJump : FsmStateAction
{
	[Tooltip("Event to trigger on the input")]
	public FsmEvent m_event;

	public bool m_requiresStamina;

	private BaseCharacterInput m_characterInput;

	private StatusEffectReceiver m_statusEffectReceiver;

	private CharacterStamina m_stamina;

	private CharacterMovement m_movement;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterInput = base.Owner.GetCharacterInputComponent();
			m_statusEffectReceiver = base.Owner.GetComponent<StatusEffectReceiver>();
			m_stamina = base.Owner.GetComponent<CharacterStamina>();
			m_movement = base.Owner.GetComponent<CharacterMovement>();
		}
	}

	public override void OnUpdate()
	{
		if ((m_statusEffectReceiver != null && m_statusEffectReceiver.IsStatusEffectActive(GlobalReferences.Instance.StatusEffects.Generic.Entangled)) || !m_movement.IsJumpEnabled() || !m_characterInput.IsJumping)
		{
			return;
		}
		if (m_requiresStamina && m_stamina.IsExhausted)
		{
			if (GameUtils.IsPlayer(base.Owner))
			{
				GlobalReferences.Instance.EventChannels.Gameplay.InsufficientStamina.Raise();
			}
		}
		else
		{
			base.Fsm.Event(m_event);
		}
	}
}
