using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Combat")]
public class GetLastHitDirection : FsmStateAction
{
	[HutongGames.PlayMaker.Tooltip("Event to trigger when attacked by an enemy we are facing")]
	public FsmEvent m_isFacingAttacker;

	[HutongGames.PlayMaker.Tooltip("Event to trigger when attacked by an enemy behind us")]
	public FsmEvent m_isNotFacingAttacker;

	private CharacterHitReact m_hitReact;

	private CharacterDirection m_characterDirection;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_hitReact = base.Owner.GetComponent<CharacterHitReact>();
			m_characterDirection = base.Owner.GetComponent<CharacterDirection>();
		}
	}

	public override void OnEnter()
	{
		if (m_hitReact.LastHit != null)
		{
			if (m_characterDirection.IsFacingDirection(0f - m_hitReact.LastHit.Direction.x))
			{
				base.Fsm.Event(m_isFacingAttacker);
			}
			else
			{
				base.Fsm.Event(m_isNotFacingAttacker);
			}
		}
		else
		{
			Debug.LogError("Tried to use GetLastHitDirection but there is no last hit!");
			base.Fsm.Event(m_isFacingAttacker);
		}
		Finish();
	}
}
