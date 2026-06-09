using Team17;
using UnityEngine;

public class PerformanceStatsServiceManager : MonoBehaviour
{
	private IPerformanceStatsService _service;

	private void Awake()
	{
		if (!Services.TryGet<IPerformanceStatsService>(out _service))
		{
			Debug.LogError("PerformanceStatsServiceManager - No performance stats service found");
		}
		else
		{
			Debug.Log("PerformanceStatsServiceManager - Using service: " + _service.GetType().Name);
		}
	}

	private void Start()
	{
		_service?.OnStartUp();
	}

	private void OnDestroy()
	{
		_service?.OnCleanUp();
	}

	private void Update()
	{
		_service?.OnUpdate(Time.deltaTime);
	}

	private void OnGUI()
	{
		_service?.DrawStatsOnGUI();
	}

	[DebugCommand("pstats_on", "Shows performance stats", "", null, false)]
	private void Debug_ShowPerformanceStats()
	{
		_service?.ShowStats();
	}

	[DebugCommand("pstats_off", "Hides performance stats", "", null, false)]
	private void Debug_HidePerformanceStats()
	{
		_service?.HideStats();
	}
}
