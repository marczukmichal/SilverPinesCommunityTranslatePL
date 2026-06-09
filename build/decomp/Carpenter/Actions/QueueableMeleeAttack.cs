using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class QueueableMeleeAttack : CheckForMeleeAttack
{
	public FsmFloat m_time;

	public FsmFloat m_minimumTime;

	private bool m_attackQueued;

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (m_attackQueued && m_time.Value >= m_minimumTime.Value)
		{
			m_attackQueued = false;
			OnAttack(isAttack: true);
		}
	}

	protected override void OnAttack(bool isAttack)
	{
		if (isAttack && m_time.Value < m_minimumTime.Value)
		{
			m_attackQueued = true;
		}
		else
		{
			base.OnAttack(isAttack);
		}
	}
}
