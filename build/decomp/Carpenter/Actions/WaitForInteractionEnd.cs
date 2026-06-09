using System;
using HutongGames.PlayMaker;
using UnityEngine.Events;

namespace Actions;

[ActionCategory("Interactor")]
public class WaitForInteractionEnd : FsmStateAction
{
	[Tooltip("Event to trigger on interact stopped")]
	public FsmEvent m_onInteractStopped;

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
		if (!m_interactor.IsInteracting)
		{
			OnInteractStopped();
		}
		CharacterInteractor interactor = m_interactor;
		interactor.OnStopInteract = (UnityAction)Delegate.Combine(interactor.OnStopInteract, new UnityAction(OnInteractStopped));
		Finish();
	}

	public override void OnExit()
	{
		CharacterInteractor interactor = m_interactor;
		interactor.OnStopInteract = (UnityAction)Delegate.Remove(interactor.OnStopInteract, new UnityAction(OnInteractStopped));
	}

	private void OnInteractStopped()
	{
		base.Fsm.Event(m_onInteractStopped);
	}
}
