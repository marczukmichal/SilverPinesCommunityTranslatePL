using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("AI")]
public class ForceTarget : FsmStateAction
{
	public GameObjectAnchor m_targetAnchor;

	public float m_forceTargetTime;

	public AISenses m_aiSenses;

	public override void OnEnter()
	{
		GameObject item = m_targetAnchor.Item;
		if (item != null)
		{
			Detectable component = item.GetComponent<Detectable>();
			if (component != null)
			{
				m_aiSenses.ForceTarget(component, m_forceTargetTime);
			}
			else
			{
				Debug.LogError("Tried to force target to object " + item?.ToString() + " but it has no detectable component!");
			}
		}
		Finish();
	}
}
