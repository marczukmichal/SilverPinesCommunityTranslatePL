using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Interactable")]
public class InteractableEventDone : FsmStateAction
{
	private Interactable m_newInteractable;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_newInteractable = base.Owner.GetComponent<Interactable>();
		}
	}

	public override void OnEnter()
	{
		m_newInteractable.FSMEventDone = true;
		Finish();
	}
}
