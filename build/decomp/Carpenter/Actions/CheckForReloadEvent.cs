using System;
using HutongGames.PlayMaker;
using UnityEngine.Events;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class CheckForReloadEvent : FsmStateAction
{
	protected BaseCharacterInput m_input;

	private CharacterReloading m_characterReloading;

	private bool m_hasAttemptedReload;

	public bool m_onlyFromInventory;

	[Tooltip("Event to send if trying to reload.")]
	public FsmEvent m_reloadEvent;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_input = base.Owner.GetCharacterInputComponent();
			m_characterReloading = base.Owner.GetComponent<CharacterReloading>();
		}
	}

	public override void OnEnter()
	{
		if (!m_onlyFromInventory)
		{
			BaseCharacterInput input = m_input;
			input.OnReloadAction = (UnityAction)Delegate.Combine(input.OnReloadAction, new UnityAction(AttemptedReload));
		}
		CharacterReloading characterReloading = m_characterReloading;
		characterReloading.OnRequestedCharacterReload = (UnityAction)Delegate.Combine(characterReloading.OnRequestedCharacterReload, new UnityAction(AttemptedReload));
		if (m_characterReloading.CheckForReloadQueued())
		{
			AttemptedReload();
		}
	}

	public override void OnExit()
	{
		if (!m_onlyFromInventory)
		{
			BaseCharacterInput input = m_input;
			input.OnReloadAction = (UnityAction)Delegate.Remove(input.OnReloadAction, new UnityAction(AttemptedReload));
		}
		CharacterReloading characterReloading = m_characterReloading;
		characterReloading.OnRequestedCharacterReload = (UnityAction)Delegate.Remove(characterReloading.OnRequestedCharacterReload, new UnityAction(AttemptedReload));
	}

	private void AttemptedReload()
	{
		m_hasAttemptedReload = true;
	}

	public override void OnUpdate()
	{
		if (m_hasAttemptedReload)
		{
			m_hasAttemptedReload = false;
			if (m_characterReloading.CanReload())
			{
				base.Fsm.Event(m_reloadEvent);
			}
		}
	}
}
