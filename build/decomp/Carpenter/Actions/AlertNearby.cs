using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("AI")]
public class AlertNearby : FsmStateAction
{
	public float m_forceTargetTime;

	public AISensesSet m_aiSensesSet;

	public float m_distance;

	private AISenses m_aiSenses;

	public override void Awake()
	{
		base.Awake();
		if (base.Owner != null)
		{
			m_aiSenses = base.Owner.GetComponent<AISenses>();
		}
	}

	public override void OnEnter()
	{
		m_aiSenses.AlertNeraby(m_distance, m_forceTargetTime);
		Finish();
	}
}
