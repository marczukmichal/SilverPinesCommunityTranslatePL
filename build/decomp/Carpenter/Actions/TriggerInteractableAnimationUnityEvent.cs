using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Interactor")]
public class TriggerInteractableAnimationUnityEvent : FsmStateAction
{
	public bool m_isStart;

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
		if (m_isStart)
		{
			m_interactor.TriggerAnimationEventStart();
		}
		else
		{
			m_interactor.TriggerAnimationEventEnd();
		}
		Finish();
	}
}
