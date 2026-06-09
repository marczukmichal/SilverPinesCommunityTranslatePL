public interface IPlatformAchievements
{
	void Initialise();

	void Update();

	void Destroy();

	void UnlockAchievement(AchievementDefinition achievement);

	void ResetAchievements();
}
