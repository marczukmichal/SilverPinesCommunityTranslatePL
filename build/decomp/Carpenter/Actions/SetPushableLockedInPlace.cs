using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Interactable")]
public class SetPushableLockedInPlace : FsmStateAction
{
	public BasePushableObject m_pushable;

	public Transform m_targetTransformPosition;

	public override void OnEnter()
	{
		m_pushable.LockInPlace(m_targetTransformPosition.transform);
		Finish();
	}
}
