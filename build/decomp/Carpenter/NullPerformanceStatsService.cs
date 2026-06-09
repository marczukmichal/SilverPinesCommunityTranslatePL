using UnityEngine;

public class NullPerformanceStatsService : IPerformanceStatsService
{
	public void OnStartUp()
	{
		Debug.Log("NullPerformanceStatsService: OnStartUp");
	}

	public void OnCleanUp()
	{
		Debug.Log("NullPerformanceStatsService: OnCleanUp");
	}

	public void OnUpdate(float deltaTime)
	{
	}

	public void ShowStats()
	{
	}

	public void HideStats()
	{
	}

	public void DrawStatsOnGUI()
	{
	}
}
