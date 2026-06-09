using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Interactor")]
public class StartQueuedInteraction : FsmStateAction
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
		m_interactor.StartQueuedInteraction();
		Finish();
	}
}
