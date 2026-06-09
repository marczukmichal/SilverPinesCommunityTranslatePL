using HutongGames.PlayMaker;
using PowerTools;

namespace Actions;

[ActionCategory("Combat")]
public class ComboMeleeInput : FsmStateAction
{
	public FsmEvent m_comboEndEvent;

	public FsmEvent m_comboContinueEvent;

	public FsmEvent m_comboExhaustedEvent;

	private BaseCharacterInput m_input;

	private SpriteAnim m_spriteAnim;

	private AnimationEventsHelper m_animEventsHelper;

	private CharacterStamina m_characterStamina;

	private bool m_inputPerformed;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_input = base.Owner.GetComponent<BaseCharacterInput>();
			m_spriteAnim = base.Owner.GetComponent<SpriteAnim>();
			m_animEventsHelper = base.Owner.GetComponent<AnimationEventsHelper>();
			m_characterStamina = base.Owner.GetComponent<CharacterStamina>();
		}
	}

	public override void OnEnter()
	{
		m_inputPerformed = false;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (m_animEventsHelper.IsAnimationInputBlocked)
		{
			m_inputPerformed = false;
		}
		else if (m_input.IsFiring)
		{
			m_inputPerformed = true;
		}
		if (m_spriteAnim.IsPlaying())
		{
			return;
		}
		if (m_inputPerformed)
		{
			if (m_characterStamina.IsExhausted)
			{
				base.Fsm.Event(m_comboExhaustedEvent);
			}
			else
			{
				base.Fsm.Event(m_comboContinueEvent);
			}
		}
		else
		{
			base.Fsm.Event(m_comboEndEvent);
		}
		Finish();
	}
}
