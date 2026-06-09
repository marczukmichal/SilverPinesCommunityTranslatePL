using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Input)]
public class InputHorizontalDirection : FsmStateAction
{
	public FsmEvent m_forwardEvent;

	public FsmEvent m_backwardsEvent;

	public float m_threshold;

	public bool m_onlyOnce;

	private BaseCharacterInput m_characterInput;

	private CharacterDirection m_direction;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterInput = base.Owner.GetCharacterInputComponent();
			m_direction = base.Owner.GetComponent<CharacterDirection>();
		}
	}

	public override void OnUpdate()
	{
		if (!m_onlyOnce)
		{
			Check();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		Check();
		if (m_onlyOnce)
		{
			Finish();
		}
	}

	private void Check()
	{
		if (Mathf.Abs(m_characterInput.MovementInput.x) > m_threshold)
		{
			if (m_direction.IsFacingDirection(m_characterInput.MovementInput.x))
			{
				base.Fsm.Event(m_forwardEvent);
			}
			else
			{
				base.Fsm.Event(m_backwardsEvent);
			}
		}
	}
}
