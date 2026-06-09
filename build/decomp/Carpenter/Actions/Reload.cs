using System;
using HutongGames.PlayMaker;
using UnityEngine.Events;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class Reload : FsmStateAction
{
	[Tooltip("Event to trigger on reload done")]
	public FsmEvent m_onReloadDone;

	public bool m_isCrouching;

	protected CharacterReloading m_reloading;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_reloading = base.Owner.GetComponent<CharacterReloading>();
		}
	}

	public override void OnEnter()
	{
		CharacterReloading reloading = m_reloading;
		reloading.OnReloadDone = (UnityAction)Delegate.Combine(reloading.OnReloadDone, new UnityAction(OnReloadDone));
		m_reloading.StartReload(m_isCrouching);
	}

	public override void OnExit()
	{
		CharacterReloading reloading = m_reloading;
		reloading.OnReloadDone = (UnityAction)Delegate.Remove(reloading.OnReloadDone, new UnityAction(OnReloadDone));
		m_reloading.CancelReload();
	}

	private void OnReloadDone()
	{
		base.Fsm.Event(m_onReloadDone);
		Finish();
	}
}
