using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Input)]
public class InputCrouch : FsmStateAction
{
	[Tooltip("Event to trigger on crouch start")]
	public FsmEvent m_onCrouchEvent;

	[Tooltip("Event to trigger on crouch end")]
	public FsmEvent m_onNotCrouchEvent;

	public bool m_canForceCrouch = true;

	public bool m_requiresStamina;

	private BaseCharacterInput m_characterInput;

	private CharacterStance m_characterStance;

	private CharacterStamina m_stamina;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterInput = base.Owner.GetCharacterInputComponent();
			m_characterStance = base.Owner.GetComponent<CharacterStance>();
			m_stamina = base.Owner.GetComponent<CharacterStamina>();
		}
	}

	public override void OnEnter()
	{
		Check();
	}

	public override void OnUpdate()
	{
		Check();
	}

	private void Check()
	{
		bool flag = m_canForceCrouch && !m_characterStance.CanStand();
		bool flag2 = m_characterInput.IsCrouching || flag;
		if (m_onCrouchEvent != null && flag2)
		{
			if (m_requiresStamina && m_stamina.AvailableStamina < 0.01f)
			{
				if (GameUtils.IsPlayer(base.Owner))
				{
					GlobalReferences.Instance.EventChannels.Gameplay.InsufficientStamina.Raise();
				}
				return;
			}
			base.Fsm.Event(m_onCrouchEvent);
		}
		if (m_onNotCrouchEvent != null && !flag2)
		{
			base.Fsm.Event(m_onNotCrouchEvent);
		}
	}
}
