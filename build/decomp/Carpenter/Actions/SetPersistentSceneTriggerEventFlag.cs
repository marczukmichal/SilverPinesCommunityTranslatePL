using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Utilities")]
public class SetPersistentSceneTriggerEventFlag : FsmStateAction
{
	[SerializeField]
	public PersistentSceneTriggerEvents m_persistentSceneTriggerEvent;

	[SerializeField]
	public FsmGameObject m_gameObject;

	public bool m_shouldSet = true;

	public override void OnEnter()
	{
		if (m_persistentSceneTriggerEvent != null)
		{
			m_persistentSceneTriggerEvent.SetState(m_shouldSet);
		}
		else if (m_gameObject.Value != null)
		{
			PersistentSceneTriggerEvents component = m_gameObject.Value.GetComponent<PersistentSceneTriggerEvents>();
			if (component != null)
			{
				component.SetState(m_shouldSet);
			}
		}
		Finish();
	}
}
