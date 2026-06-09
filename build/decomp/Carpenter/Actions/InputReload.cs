using System;
using HutongGames.PlayMaker;
using UnityEngine.Events;

namespace Actions;

[ActionCategory(ActionCategory.Input)]
public class InputReload : FsmStateAction
{
	[Tooltip("Event to trigger on the input")]
	public FsmEvent m_event;

	private BaseCharacterInput m_characterInput;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterInput = base.Owner.GetCharacterInputComponent();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		BaseCharacterInput characterInput = m_characterInput;
		characterInput.OnReloadAction = (UnityAction)Delegate.Combine(characterInput.OnReloadAction, new UnityAction(OnInteract));
	}

	public override void OnExit()
	{
		base.OnExit();
		BaseCharacterInput characterInput = m_characterInput;
		characterInput.OnReloadAction = (UnityAction)Delegate.Remove(characterInput.OnReloadAction, new UnityAction(OnInteract));
	}

	private void OnInteract()
	{
		base.Fsm.Event(m_event);
	}
}
