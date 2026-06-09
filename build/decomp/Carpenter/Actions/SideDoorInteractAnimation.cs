using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Interactor")]
public class SideDoorInteractAnimation : FsmStateAction
{
	public bool m_isDoorOpen;

	public NewSideDoor.DoorAnimationVariant m_animationVariant;

	private CharacterInteractor m_interactor;

	public override void Awake()
	{
		base.Awake();
		if (!(base.Owner == null))
		{
			m_interactor = base.Owner.GetComponent<CharacterInteractor>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		NewSideDoor newSideDoor = m_interactor.ActiveInteractable as NewSideDoor;
		if (newSideDoor != null)
		{
			newSideDoor.TriggerDoorInteractFromCharacter(base.Owner, m_isDoorOpen, m_animationVariant);
		}
	}
}
