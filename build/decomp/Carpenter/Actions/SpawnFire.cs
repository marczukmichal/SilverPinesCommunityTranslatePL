using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Combat")]
public class SpawnFire : FsmStateAction
{
	[SerializeField]
	public Transform m_explodeTransform;

	[SerializeField]
	public int m_fireAmount;

	[SerializeField]
	public float m_fireSpread;

	public override void OnEnter()
	{
		GlobalReferences.Instance.EventChannels.Fire.SpawnFire.Raise(new SpawnFireEventData
		{
			m_position = m_explodeTransform.position,
			m_amount = m_fireAmount,
			m_spread = m_fireSpread
		});
		Finish();
	}
}
