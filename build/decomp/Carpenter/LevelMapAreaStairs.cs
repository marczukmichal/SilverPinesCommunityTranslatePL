using UnityEngine;

public class LevelMapAreaStairs : MonoBehaviour
{
	[SerializeField]
	private MapConnection m_connection;

	public MapConnection MapConnection => m_connection;

	public void SetMapConnectionStatus(MapConnectionStatus status)
	{
		if (MapConnection.ConnectionID != 0)
		{
			GlobalReferences.Instance.MapDynamicData.SetConnectionStatus(MapConnection.ConnectionID, status);
		}
		else
		{
			Debug.LogWarning("LevelMapAreaStairs " + base.gameObject.name + "has a connection ID of 0 - this won't be marked on the map! Object needs to be regenerated");
		}
	}
}
