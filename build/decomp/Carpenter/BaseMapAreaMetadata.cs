using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public abstract class BaseMapAreaMetadata : AddressableScriptableObject<BaseMapAreaMetadata>
{
	[Serializable]
	public class MapAreaConnection
	{
		public float m_referencePoint;

		public BaseMapAreaMetadata m_targetMapArea;

		public MapConnection m_connection;

		public LevelTransition.LeaveDirection m_leaveDirection;

		public float m_connectionDistanceOffset = 5f;

		public bool m_ignoreForGraphTraversal;

		public bool m_allowFloorChange;

		public ConnectionLineVisualsSettingsType m_lineVisualsSetting;

		public ConnectionIconPositioning m_iconPositioning;

		public void CopySettingsFrom(MapAreaConnection other)
		{
			m_referencePoint = other.m_referencePoint;
			m_targetMapArea = other.m_targetMapArea;
			m_connection = other.m_connection;
			m_leaveDirection = other.m_leaveDirection;
			if (other.m_allowFloorChange)
			{
				m_allowFloorChange = true;
			}
			if (other.m_ignoreForGraphTraversal)
			{
				m_ignoreForGraphTraversal = true;
			}
		}
	}

	[Serializable]
	public class TransformSettings
	{
		public Vector2 m_positionOffset = Vector2.zero;

		public Vector2 m_scale = Vector2.one;
	}

	public enum MapAreaStyling
	{
		Default,
		Invisible
	}

	[SerializeField]
	protected LevelMapAOMMetadata m_mapAOMMetadata = new LevelMapAOMMetadata();

	[SerializeField]
	protected List<MapAreaConnection> m_connections = new List<MapAreaConnection>();

	[SerializeField]
	private TransformSettings m_transformSettings;

	[Tooltip("1.0 is the top of the area, 0.5 is center (default) and 0 is the bottom")]
	[Range(0f, 1f)]
	[SerializeField]
	private float m_markersVerticalPosition = 0.5f;

	[SerializeField]
	private LocalizedString m_labelString;

	[SerializeField]
	private bool m_showLabelOnMap;

	[SerializeField]
	private MapAreaStyling m_visualsStyling;

	[SerializeField]
	protected int m_mapFloor;

	[SerializeField]
	private Mesh m_overrideMesh;

	public LevelMapAOMMetadata MapAOMMetadata => m_mapAOMMetadata;

	public List<MapAreaConnection> Connections => m_connections;

	public TransformSettings AreaTransformSettings => m_transformSettings;

	public float MarkersVerticalPosition => m_markersVerticalPosition;

	public bool ShowLabelOnMap => m_showLabelOnMap;

	public MapAreaStyling VisualsStyling => m_visualsStyling;

	public LocalizedString LabelString
	{
		get
		{
			if (ShowLabelOnMap)
			{
				if (m_labelString != null && !m_labelString.IsEmpty)
				{
					return m_labelString;
				}
				if (GetMainTrackedLevelMetadata() != null)
				{
					return GetMainTrackedLevelMetadata().DisplayNameLocalizedString;
				}
			}
			return null;
		}
	}

	public int MapFloor => m_mapFloor;

	public Mesh OverrideMesh => m_overrideMesh;

	public MapAreaConnection GetMatchingConnection(MapAreaConnection matchConnection)
	{
		if (m_connections != null)
		{
			foreach (MapAreaConnection connection in m_connections)
			{
				if (connection.m_connection.ConnectionID == matchConnection.m_connection.ConnectionID)
				{
					return connection;
				}
			}
		}
		return null;
	}

	public MapAreaConnection GetMatchingConnection(MapConnection matchConnection)
	{
		if (m_connections != null)
		{
			foreach (MapAreaConnection connection in m_connections)
			{
				if (connection.m_connection.ConnectionID == matchConnection.ConnectionID)
				{
					return connection;
				}
			}
		}
		return null;
	}

	public void UpdateDynamicData(MapDynamicData dynamicData, GameObject player)
	{
		MapDynamicData.MapAreaDynamicData mapAreaDynamicData = dynamicData.GetMapAreaDynamicData(this);
		UpdateDynamicData(mapAreaDynamicData, player);
	}

	protected virtual void UpdateDynamicData(MapDynamicData.MapAreaDynamicData dynamicData, GameObject player)
	{
		dynamicData.m_hasVisited = true;
	}

	public abstract Vector2 GetSize();

	public virtual bool HasPlayerVisitedArea()
	{
		return GlobalReferences.Instance.MapDynamicData.HasEnteredArea(this);
	}

	public abstract LevelMetadata GetMainTrackedLevelMetadata();

	public abstract bool TracksLevel(LevelMetadata levelMetadata);

	public virtual bool IsPlayerInArea()
	{
		LevelMetadata item = GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Item;
		if (TracksLevel(item))
		{
			return true;
		}
		return false;
	}

	public virtual float GetDistanceFromPlayer()
	{
		return -1f;
	}

	public bool IsUnmappedLocationParent()
	{
		LevelMetadata item = GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Item;
		if (item.MapAreaAutoGenerationMode == LevelMetadata.MapAreaGenerationMode.Unmapped)
		{
			return item.GetPrimaryMapAreaMetadata() == this;
		}
		return false;
	}
}
