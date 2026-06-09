using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Interactor")]
public class InitialiseInteraction : FsmStateAction
{
	private CharacterInteractor m_interactor;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_interactor = base.Owner.GetComponent<CharacterInteractor>();
		}
	}

	public override void OnEnter()
	{
		m_interactor.DoInteractFromFSM();
		string fSMEventStringForInteract = m_interactor.GetFSMEventStringForInteract();
		base.Fsm.Event(fSMEventStringForInteract);
		Finish();
	}
}
