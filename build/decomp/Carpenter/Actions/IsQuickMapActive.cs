using System;
using HutongGames.PlayMaker;
using UnityEngine.Events;

namespace Actions;

[ActionCategory("Special")]
public class IsQuickMapActive : FsmStateAction
{
	public FsmEvent m_activeEvent;

	public FsmEvent m_inactiveEvent;

	public bool m_checkOnUpdate;

	private PlayerQuickMapControls m_quickMapControls;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_quickMapControls = base.Owner.GetComponent<PlayerQuickMapControls>();
		}
	}

	public override void OnEnter()
	{
		if (m_checkOnUpdate)
		{
			PlayerQuickMapControls quickMapControls = m_quickMapControls;
			quickMapControls.m_onQuickMapActiveChanged = (UnityAction<bool>)Delegate.Combine(quickMapControls.m_onQuickMapActiveChanged, new UnityAction<bool>(ShowingQuickMapChanged));
		}
		if (m_quickMapControls.ShowingMap)
		{
			base.Fsm.Event(m_activeEvent);
		}
		else
		{
			base.Fsm.Event(m_inactiveEvent);
		}
		Finish();
	}

	public override void OnExit()
	{
		if (m_checkOnUpdate)
		{
			PlayerQuickMapControls quickMapControls = m_quickMapControls;
			quickMapControls.m_onQuickMapActiveChanged = (UnityAction<bool>)Delegate.Remove(quickMapControls.m_onQuickMapActiveChanged, new UnityAction<bool>(ShowingQuickMapChanged));
		}
	}

	private void ShowingQuickMapChanged(bool active)
	{
		if (active)
		{
			base.Fsm.Event(m_activeEvent);
		}
		else
		{
			base.Fsm.Event(m_inactiveEvent);
		}
	}
}
