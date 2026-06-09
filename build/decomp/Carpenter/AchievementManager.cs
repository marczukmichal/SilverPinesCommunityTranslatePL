using UnityEngine;

public class AchievementManager : MonoBehaviour
{
	private IPlatformAchievements m_platformAchievements;

	private void Awake()
	{
		Initialise();
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.Anchors.Achievements.AchievementManagerAnchor.Set(this);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Anchors.Achievements.AchievementManagerAnchor.Set(null);
	}

	private void Initialise()
	{
		m_platformAchievements = new SteamAchievements();
		m_platformAchievements.Initialise();
	}

	private void OnDestroy()
	{
		m_platformAchievements.Destroy();
	}

	private void Update()
	{
		m_platformAchievements.Update();
	}

	public void UnlockAchievement(AchievementDefinition achievement)
	{
		m_platformAchievements.UnlockAchievement(achievement);
	}

	[DebugCommand("achievements_reset", "Resets platform achievement data", "", null, false)]
	private void ResetAchievements()
	{
		m_platformAchievements.ResetAchievements();
	}
}
