using UnityEngine;

public class EditorAchievements : IPlatformAchievements
{
	public void Initialise()
	{
	}

	public void Destroy()
	{
	}

	public void Update()
	{
	}

	public void UnlockAchievement(AchievementDefinition achievement)
	{
		Debug.Log("Unlock achievement: " + achievement.AchievementID);
	}

	public void ResetAchievements()
	{
	}
}
