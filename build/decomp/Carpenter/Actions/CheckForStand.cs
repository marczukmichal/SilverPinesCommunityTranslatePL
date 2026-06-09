using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class CheckForStand : FsmStateAction
{
	[HutongGames.PlayMaker.Tooltip("Event to send if trying to stand.")]
	public FsmEvent m_shouldStandEvent;

	[SerializeField]
	public bool m_everyFrame;

	private BaseCharacterInput m_input;

	private CharacterStance m_characterStance;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_input = base.Owner.GetCharacterInputComponent();
			m_characterStance = base.Owner.GetComponent<CharacterStance>();
		}
	}

	public override void OnEnter()
	{
		Check();
		if (!m_everyFrame)
		{
			Finish();
		}
	}

	public override void OnUpdate()
	{
		Check();
	}

	private void Check()
	{
		if (!m_input.IsCrouching && m_characterStance.CanStand())
		{
			base.Fsm.Event(m_shouldStandEvent);
		}
	}
}
