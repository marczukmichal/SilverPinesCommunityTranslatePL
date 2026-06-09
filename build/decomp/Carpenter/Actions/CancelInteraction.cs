using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Interactor")]
public class CancelInteraction : FsmStateAction
{
	[SerializeField]
	public bool m_onEnter;

	[SerializeField]
	public bool m_onExit;

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
		if (m_onEnter)
		{
			m_interactor.StopInteracting();
		}
		Finish();
	}

	public override void OnExit()
	{
		if (m_onExit)
		{
			m_interactor.StopInteracting();
		}
	}
}
