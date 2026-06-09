using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("AI")]
public class SetGoalToRetreat : FsmStateAction
{
	public FsmEvent m_retreatDone;

	private AISenses m_aiSenses;

	private AIFSMAttackStrategy m_strategy;

	private Vector2 m_retreatPosition;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_aiSenses = base.Owner.GetComponent<AISenses>();
			m_strategy = base.Owner.GetComponent<AIFSMAttackStrategy>();
		}
	}

	private bool CanReachPosition(Vector2 currentPosition, Vector2 targetPosition)
	{
		currentPosition.y += 0.5f;
		targetPosition.y += 0.5f;
		return !Physics2D.Linecast(currentPosition, targetPosition, GameLayers.CharacterNavigationMask);
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_strategy.SetRetreatMode(enabled: true);
		UpdateRetreatPosition(instant: true);
	}

	public override void OnExit()
	{
		base.OnExit();
		m_strategy.SetRetreatMode(enabled: false);
	}

	private void UpdateRetreatPosition(bool instant)
	{
		float num = 2f;
		float num2 = 2f;
		Vector2 vector = m_aiSenses.CurrentTarget.transform.position;
		Vector2 vector2 = base.Owner.transform.position;
		Vector2 vector3 = vector - vector2;
		float magnitude = vector3.magnitude;
		vector3.Normalize();
		Vector2 vector4 = vector2 - vector3 * num;
		Vector2 vector5 = vector - vector3 * (0f - num);
		Vector2 vector6;
		if (CanReachPosition(vector, vector4))
		{
			vector6 = vector4;
		}
		else if (magnitude < num2 && CanReachPosition(vector, vector5))
		{
			vector6 = vector5;
		}
		else
		{
			base.Fsm.Event(m_retreatDone);
			vector6 = vector4;
		}
		if (instant)
		{
			m_retreatPosition = vector6;
		}
		else
		{
			m_retreatPosition = Vector2.MoveTowards(m_retreatPosition, vector6, Time.deltaTime * 4f);
		}
	}

	public override void OnUpdate()
	{
		if ((bool)m_aiSenses.CurrentTarget)
		{
			UpdateRetreatPosition(instant: false);
			m_strategy.SetMoveGoal(m_retreatPosition);
		}
	}
}
