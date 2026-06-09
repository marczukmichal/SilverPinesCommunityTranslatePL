using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Achievements")]
public class UnlockAchievement : FsmStateAction
{
	public AchievementDefinition m_achievement;

	public override void OnEnter()
	{
		if (m_achievement != null)
		{
			m_achievement.UnlockAchievement();
		}
		Finish();
	}
}
