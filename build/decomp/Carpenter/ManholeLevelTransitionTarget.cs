using UnityEngine;

public class ManholeLevelTransitionTarget : MonoBehaviour
{
	[SerializeField]
	private LevelTransition m_levelTransition;

	[SerializeField]
	private PersistentSceneTriggerEvents m_sceneTriggerEvents;

	public LevelTransition LevelTransition => m_levelTransition;

	public LevelTransition DoLevelTransition()
	{
		m_sceneTriggerEvents.SetState(set: true);
		return m_levelTransition;
	}
}
