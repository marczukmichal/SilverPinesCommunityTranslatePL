using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Combat")]
public class ApplyHitReact : FsmStateAction
{
	public bool m_forceLookAtHitSource;

	public bool m_clearStagger;

	protected CharacterHitReact m_hitReact;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_hitReact = base.Owner.GetComponent<CharacterHitReact>();
		}
	}

	public override void OnEnter()
	{
		if (!m_hitReact.ApplyLastDamageImpactForce(m_forceLookAtHitSource))
		{
			Debug.LogError("ApplyHitReact: " + base.Owner.name + " tried to apply hit react incorrectly in state " + base.State.Name);
		}
		if (m_clearStagger)
		{
			m_hitReact.ClearStagger();
		}
	}

	public override void OnExit()
	{
		m_hitReact.ClearHitFlag();
	}
}
