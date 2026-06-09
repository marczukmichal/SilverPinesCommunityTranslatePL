using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Utilities")]
public class GetAttachedGameObjectForAttachPoint : FsmStateAction
{
	public AttachPoint m_attachPoint;

	[UIHint(UIHint.Variable)]
	[HutongGames.PlayMaker.Tooltip("Store the GameObject hit in the last collision.")]
	public FsmGameObject m_storeGameObject;

	public override void OnEnter()
	{
		AttachableObject attachedObject = m_attachPoint.AttachedObject;
		GameObject value = null;
		if (attachedObject != null)
		{
			value = attachedObject.gameObject;
		}
		m_storeGameObject.Value = value;
		Finish();
	}
}
