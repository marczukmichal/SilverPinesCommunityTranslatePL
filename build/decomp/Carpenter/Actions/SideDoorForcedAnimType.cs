using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Interactor")]
public class SideDoorForcedAnimType : FsmStateAction
{
	public NewSideDoor.DoorForcedPlayerMovement m_forcedMovement;

	public FsmEvent m_event;

	public bool m_requireInput;

	private CharacterInteractor m_interactor;

	private BaseCharacterInput m_characterInput;

	public override void Awake()
	{
		base.Awake();
		if (!(base.Owner == null))
		{
			m_interactor = base.Owner.GetComponent<CharacterInteractor>();
			m_characterInput = base.Owner.GetCharacterInputComponent();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		NewSideDoor newSideDoor = m_interactor.ActiveInteractable as NewSideDoor;
		if ((!m_requireInput || Mathf.Abs(m_characterInput.MovementInput.x) > 0.25f) && newSideDoor != null && newSideDoor.ForcedPlayerMovement == m_forcedMovement)
		{
			base.Fsm.Event(m_event);
		}
		Finish();
	}
}
