public interface IPerformanceStatsService
{
	void OnStartUp();

	void OnCleanUp();

	void OnUpdate(float deltaTime);

	void ShowStats();

	void HideStats();

	void DrawStatsOnGUI();
}
