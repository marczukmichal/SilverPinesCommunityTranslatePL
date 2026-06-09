using System;
using HutongGames.PlayMaker;
using UnityEngine.Events;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class CheckForInteract : FsmStateAction
{
	protected BaseCharacterInput m_input;

	protected CharacterInteractor m_interactor;

	private CharacterReloading m_characterReloading;

	[Tooltip("Event to send if trying to reload.")]
	public FsmEvent m_reloadEvent;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_input = base.Owner.GetCharacterInputComponent();
			m_interactor = base.Owner.GetComponent<CharacterInteractor>();
			m_characterReloading = base.Owner.GetComponent<CharacterReloading>();
		}
	}

	public override void OnEnter()
	{
		m_interactor.InteractEnabled = true;
		BaseCharacterInput input = m_input;
		input.OnInteractAction = (UnityAction)Delegate.Combine(input.OnInteractAction, new UnityAction(OnTryInteractNearby));
		BaseCharacterInput input2 = m_input;
		input2.OnInteractUpAction = (UnityAction)Delegate.Combine(input2.OnInteractUpAction, new UnityAction(m_interactor.AttemptInteractUpNearby));
		BaseCharacterInput input3 = m_input;
		input3.OnInteractDownAction = (UnityAction)Delegate.Combine(input3.OnInteractDownAction, new UnityAction(m_interactor.AttemptInteractDownNearby));
		Finish();
	}

	public override void OnExit()
	{
		m_interactor.InteractEnabled = false;
		BaseCharacterInput input = m_input;
		input.OnInteractAction = (UnityAction)Delegate.Remove(input.OnInteractAction, new UnityAction(OnTryInteractNearby));
		BaseCharacterInput input2 = m_input;
		input2.OnInteractUpAction = (UnityAction)Delegate.Remove(input2.OnInteractUpAction, new UnityAction(m_interactor.AttemptInteractUpNearby));
		BaseCharacterInput input3 = m_input;
		input3.OnInteractDownAction = (UnityAction)Delegate.Remove(input3.OnInteractDownAction, new UnityAction(m_interactor.AttemptInteractDownNearby));
	}

	private void OnTryInteractNearby()
	{
		m_interactor.AttemptInteractNearby();
	}
}
