using UnityEngine;

public class LevelMapAreaSeparator : MonoBehaviour
{
	[SerializeField]
	private MapConnection m_connection;

	[SerializeField]
	private bool m_connectAreas = true;

	[SerializeField]
	private bool m_revealAsLockedWhenNear;

	[SerializeField]
	private float m_revealDistance = 5f;

	private MapConnectionStatus m_status;

	public MapConnection MapConnection => m_connection;

	public bool ConnectAreas => m_connectAreas;

	public void SetMapConnectionStatus(MapConnectionStatus status)
	{
		m_status = status;
		if (MapConnection.ConnectionID != 0)
		{
			GlobalReferences.Instance.MapDynamicData.SetConnectionStatus(MapConnection.ConnectionID, status);
		}
		else
		{
			Debug.LogWarning("LevelMapAreaSeparator " + base.gameObject.name + "has a connection ID of 0 - this won't be marked on the map! Object needs to be regenerated");
		}
	}

	public void SetOpen()
	{
		SetMapConnectionStatus(MapConnectionStatus.Open);
	}

	public void SetLocked()
	{
		SetMapConnectionStatus(MapConnectionStatus.Locked);
	}

	private void Start()
	{
		if (MapConnection.ConnectionID != 0)
		{
			m_status = GlobalReferences.Instance.MapDynamicData.GetConnectionStatus(MapConnection.ConnectionID);
		}
	}

	public void Update()
	{
		if (m_revealAsLockedWhenNear && m_status == MapConnectionStatus.Unknown)
		{
			GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
			if (item != null && Vector2.Distance(base.transform.position, item.transform.position) < m_revealDistance)
			{
				SetLocked();
			}
		}
	}
}
