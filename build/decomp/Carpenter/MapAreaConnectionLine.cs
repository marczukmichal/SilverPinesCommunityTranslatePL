using System.Collections.Generic;
using Shapes;
using UnityEngine;
using UnityEngine.Rendering;

public class MapAreaConnectionLine : MonoBehaviour
{
	[SerializeField]
	private MapConnection m_mapConnectionSettings;

	[SerializeField]
	private MapArea3D m_startingMapArea3D;

	[SerializeField]
	private MapArea3D m_endingMapArea3D;

	[SerializeField]
	private Vector3 m_startWorldPosition;

	[SerializeField]
	private Vector3 m_endWorldPosition;

	[SerializeField]
	private List<Line> m_shapeLines;

	[SerializeField]
	private List<Line> m_borderLines;

	[Header("Shared Settings")]
	[SerializeField]
	private MapVisualsSettings m_settings;

	[Header("End Pieces")]
	[SerializeField]
	private MapAreaConnectionLineEndPoint m_icon;

	[SerializeField]
	private bool m_isShortConnection;

	[SerializeField]
	private bool m_isFloorChange;

	[SerializeField]
	private ConnectionLineVisualsSettingsType m_lineVisualsSetting;

	private FloorMode m_floorMode;

	private MapConnectionStatus m_connectionStatus;

	private bool m_tooltipActive;

	private MapVisualsSettings.ConnectionLineVisualsSettings m_lineVisualsSettingsData;

	private LineMeshGenerator m_fowLineMesh;

	private LineMeshGenerator m_knownLineMesh;

	private MapVisualsSettings.ConnectionLineVisualsSettings LineVisualSettingsData
	{
		get
		{
			if (m_lineVisualsSettingsData == null)
			{
				m_lineVisualsSettingsData = m_settings.GetLineSettingsForVisualsType(m_lineVisualsSetting);
			}
			return m_lineVisualsSettingsData;
		}
	}

	public bool TooltipActive
	{
		get
		{
			if (m_tooltipActive && (m_startingMapArea3D.ShouldShowInFogOfWar() || m_endingMapArea3D.ShouldShowInFogOfWar()))
			{
				_ = m_mapConnectionSettings.ConnectionType;
				return false;
			}
			return false;
		}
	}

	public int TooltipPriority => 1;

	public string TooltipString
	{
		get
		{
			switch (m_connectionStatus)
			{
			case MapConnectionStatus.Locked:
				switch (m_mapConnectionSettings.ConnectionType)
				{
				case MapConnectionType.BreakablePadlock_OLD:
				case MapConnectionType.DoorPadlock:
					return "Breakable Padlock";
				case MapConnectionType.WoodenBarriers:
				case MapConnectionType.PlasticTarp:
				case MapConnectionType.SledgeWall:
					return "";
				default:
					return "Locked";
				}
			case MapConnectionStatus.Unknown:
				return "Unknown";
			default:
				return "?";
			}
		}
	}

	public Vector3 TooltipWorldPosition => base.transform.position;

	public void SetFloorMode(FloorMode floorMode)
	{
		if (floorMode != m_floorMode)
		{
			m_floorMode = floorMode;
		}
	}

	private Color GetLineColorForArea(MapArea3D area)
	{
		if (LineVisualSettingsData.m_useAreaColor)
		{
			Color mainColor = area.MainColor;
			if (!area.IsKnown())
			{
				mainColor.a = 0f;
			}
			if (area.MapAreaMetadata.VisualsStyling == BaseMapAreaMetadata.MapAreaStyling.Invisible)
			{
				mainColor.a = 0f;
			}
			return mainColor;
		}
		if (area.HasPlayerVisitedArea())
		{
			return LineVisualSettingsData.m_lineVisitedColor;
		}
		if (area.IsKnown())
		{
			return LineVisualSettingsData.m_lineUnvisitedColor;
		}
		return LineVisualSettingsData.m_lineUnknownColor;
	}

	private bool IsDoor()
	{
		if (m_mapConnectionSettings.ConnectionType != MapConnectionType.Door && m_mapConnectionSettings.ConnectionType != MapConnectionType.DoorApplyItem)
		{
			return m_mapConnectionSettings.ConnectionType == MapConnectionType.DoorPadlock;
		}
		return true;
	}

	public bool IsKnownByRevealMapPickup(MapRevealPickup pickup)
	{
		if (m_startingMapArea3D == null)
		{
			return false;
		}
		if (m_endingMapArea3D == null)
		{
			return false;
		}
		if (!m_startingMapArea3D.IsKnown(ignoreMapRevealPickups: true) && !m_endingMapArea3D.IsKnown(ignoreMapRevealPickups: true))
		{
			if (!pickup.IsKnown(m_startingMapArea3D.MapAreaMetadata))
			{
				return pickup.IsKnown(m_endingMapArea3D.MapAreaMetadata);
			}
			return true;
		}
		return false;
	}

	public void RefreshState()
	{
		bool tooltipActive = false;
		m_connectionStatus = GlobalReferences.Instance.MapDynamicData.GetConnectionStatus(m_mapConnectionSettings.ConnectionID);
		if (GameDebugCommands.CHEAT_MAP_SHOW_EVERYTHING && m_connectionStatus == MapConnectionStatus.Unknown)
		{
			MapConnectionType connectionType = m_mapConnectionSettings.ConnectionType;
			if ((uint)(connectionType - 3) <= 1u || connectionType == MapConnectionType.SledgeWall)
			{
				m_connectionStatus = MapConnectionStatus.Locked;
			}
		}
		bool flag = m_connectionStatus != 0 || (m_startingMapArea3D != null && m_startingMapArea3D.IsKnown()) || (m_endingMapArea3D != null && m_endingMapArea3D.IsKnown());
		if (m_floorMode == FloorMode.Below && m_lineVisualsSetting != 0 && m_lineVisualsSetting != ConnectionLineVisualsSettingsType.Interior)
		{
			flag = false;
		}
		if (m_lineVisualsSetting == ConnectionLineVisualsSettingsType.Hidden)
		{
			flag = false;
		}
		if (!flag)
		{
			m_icon.gameObject.SetActive(value: false);
			if (m_shapeLines != null)
			{
				foreach (Line shapeLine in m_shapeLines)
				{
					shapeLine.gameObject.SetActive(value: false);
				}
			}
			if (m_borderLines == null)
			{
				return;
			}
			{
				foreach (Line borderLine in m_borderLines)
				{
					borderLine.gameObject.SetActive(value: false);
				}
				return;
			}
		}
		bool flag2 = (m_startingMapArea3D != null && m_startingMapArea3D.HasPlayerVisitedArea()) || (m_endingMapArea3D != null && m_endingMapArea3D.HasPlayerVisitedArea());
		bool active = m_isShortConnection && flag2 && m_floorMode == FloorMode.Active;
		if (m_isFloorChange)
		{
			m_icon.gameObject.SetActive(flag2 && m_floorMode == FloorMode.Active);
			return;
		}
		if (m_floorMode == FloorMode.Below)
		{
			active = false;
			m_icon.gameObject.SetActive(value: false);
		}
		else if (IsDoor() && flag2)
		{
			active = true;
			Sprite sprite = m_settings.m_connectionLineShared.m_doorNormalSprite;
			Color color = m_settings.m_connectionLineShared.m_doorNormalColor;
			if (m_connectionStatus == MapConnectionStatus.Locked)
			{
				sprite = m_settings.m_connectionLineShared.m_doorLockedSprite;
				color = m_settings.m_connectionLineShared.m_doorLockedColor;
			}
			else if (m_connectionStatus == MapConnectionStatus.Unknown)
			{
				sprite = m_settings.m_connectionLineShared.m_doorUnknownSprite;
				color = m_settings.m_connectionLineShared.m_doorUnknownColor;
			}
			m_icon.SpriteRenderer.sprite = sprite;
			m_icon.SpriteRenderer.color = color;
			m_icon.gameObject.SetActive(value: true);
		}
		else
		{
			switch (m_connectionStatus)
			{
			case MapConnectionStatus.Unknown:
				m_icon.SpriteRenderer.sprite = m_settings.m_connectionLineShared.m_unknownCapSprite;
				m_icon.SpriteRenderer.color = m_settings.m_connectionLineShared.m_unknownColor;
				tooltipActive = true;
				break;
			case MapConnectionStatus.Open:
				m_icon.SpriteRenderer.sprite = m_settings.m_connectionLineShared.m_openCapSprite;
				m_icon.SpriteRenderer.color = m_settings.m_connectionLineShared.m_openColor;
				active = false;
				break;
			case MapConnectionStatus.Locked:
				if (m_mapConnectionSettings.ConnectionType == MapConnectionType.WoodenBarriers || m_mapConnectionSettings.ConnectionType == MapConnectionType.SledgeWall)
				{
					m_icon.SpriteRenderer.sprite = m_settings.m_connectionLineShared.m_barrierSprite;
					m_icon.SpriteRenderer.color = m_settings.m_connectionLineShared.m_barrierColor;
					active = true;
				}
				else if (m_mapConnectionSettings.ConnectionType == MapConnectionType.PlasticTarp)
				{
					m_icon.SpriteRenderer.sprite = m_settings.m_connectionLineShared.m_plasticTarpSprite;
					m_icon.SpriteRenderer.color = m_settings.m_connectionLineShared.m_plasticTarpColor;
					active = true;
				}
				else
				{
					m_icon.SpriteRenderer.sprite = m_settings.m_connectionLineShared.m_lockedCapSprite;
					m_icon.SpriteRenderer.color = m_settings.m_connectionLineShared.m_lockedColor;
				}
				tooltipActive = true;
				break;
			}
			if (m_connectionStatus == MapConnectionStatus.Locked)
			{
				m_icon.gameObject.SetActive(active);
			}
			else
			{
				m_icon.gameObject.SetActive(value: false);
			}
		}
		Color lineColorForArea = GetLineColorForArea(m_startingMapArea3D);
		Color lineColorForArea2 = GetLineColorForArea(m_endingMapArea3D);
		Color color2 = Color.Lerp(lineColorForArea, lineColorForArea2, 0.5f);
		color2.a = Mathf.Max(lineColorForArea.a, lineColorForArea2.a);
		foreach (Line shapeLine2 in m_shapeLines)
		{
			shapeLine2.gameObject.SetActive(value: true);
			shapeLine2.BlendMode = LineVisualSettingsData.m_blendMode;
			shapeLine2.Color = color2;
		}
		if (m_shapeLines.Count > 0)
		{
			m_shapeLines[0].ColorMode = Line.LineColorMode.Double;
			m_shapeLines[0].ColorStart = lineColorForArea;
			m_shapeLines[m_shapeLines.Count - 1].ColorMode = Line.LineColorMode.Double;
			m_shapeLines[m_shapeLines.Count - 1].ColorEnd = lineColorForArea2;
		}
		if (LineVisualSettingsData.m_includeBackgroundBorder)
		{
			Color borderColor = m_startingMapArea3D.BorderColor;
			Color borderColor2 = m_endingMapArea3D.BorderColor;
			if (!m_startingMapArea3D.IsKnown() || m_startingMapArea3D.MapAreaMetadata.VisualsStyling == BaseMapAreaMetadata.MapAreaStyling.Invisible)
			{
				borderColor.a = 0f;
			}
			if (!m_endingMapArea3D.IsKnown() || m_endingMapArea3D.MapAreaMetadata.VisualsStyling == BaseMapAreaMetadata.MapAreaStyling.Invisible)
			{
				borderColor2.a = 0f;
			}
			Color color3 = Color.Lerp(borderColor, borderColor2, 0.5f);
			color3.a = Mathf.Max(borderColor.a, borderColor2.a);
			foreach (Line borderLine2 in m_borderLines)
			{
				borderLine2.gameObject.SetActive(value: true);
				borderLine2.BlendMode = LineVisualSettingsData.m_blendMode;
				borderLine2.Color = color3;
			}
			if (m_borderLines.Count > 0)
			{
				m_borderLines[0].ColorMode = Line.LineColorMode.Double;
				m_borderLines[0].ColorStart = borderColor;
				m_borderLines[m_borderLines.Count - 1].ColorMode = Line.LineColorMode.Double;
				m_borderLines[m_borderLines.Count - 1].ColorEnd = borderColor2;
			}
		}
		m_tooltipActive = tooltipActive;
	}

	private void OnDrawGizmos()
	{
		if (Mathf.Abs(m_startWorldPosition.x - m_endWorldPosition.x) < 0.25f || Mathf.Abs(m_startWorldPosition.z - m_endWorldPosition.z) < 0.25f)
		{
			Gizmos.DrawLine(m_startWorldPosition, m_endWorldPosition);
			return;
		}
		Vector3 vector = (m_startWorldPosition + m_endWorldPosition) * 0.5f;
		if (Mathf.Abs(m_startWorldPosition.x - m_endWorldPosition.x) >= 0.25f)
		{
			Vector3 startWorldPosition = m_startWorldPosition;
			Vector3 endWorldPosition = m_endWorldPosition;
			startWorldPosition.z = (endWorldPosition.z = vector.z);
			Gizmos.DrawLine(m_startWorldPosition, startWorldPosition);
			Gizmos.DrawLine(startWorldPosition, endWorldPosition);
			Gizmos.DrawLine(endWorldPosition, m_endWorldPosition);
		}
		else
		{
			Vector3 startWorldPosition2 = m_startWorldPosition;
			Vector3 endWorldPosition2 = m_endWorldPosition;
			startWorldPosition2.x = (endWorldPosition2.x = vector.x);
			Gizmos.DrawLine(m_startWorldPosition, startWorldPosition2);
			Gizmos.DrawLine(startWorldPosition2, endWorldPosition2);
			Gizmos.DrawLine(endWorldPosition2, m_endWorldPosition);
		}
	}

	private void OnDestroy()
	{
		if (m_fowLineMesh != null)
		{
			m_fowLineMesh.Cleanup();
		}
		if (m_knownLineMesh != null)
		{
			m_knownLineMesh.Cleanup();
		}
	}

	private void GenerateFOWMeshIfNeeded()
	{
		if (m_fowLineMesh != null)
		{
			return;
		}
		m_fowLineMesh = new LineMeshGenerator(2f);
		foreach (Line shapeLine in m_shapeLines)
		{
			m_fowLineMesh.AddLine(base.transform.TransformPoint(shapeLine.Start), base.transform.TransformPoint(shapeLine.End));
		}
		m_fowLineMesh.GenerateMesh();
	}

	public void DrawFogOfWarVisibilityMeshes(CommandBuffer cmd, Material material)
	{
		if (GlobalReferences.Instance.MapDynamicData.GetConnectionStatus(m_mapConnectionSettings.ConnectionID) == MapConnectionStatus.Open)
		{
			GenerateFOWMeshIfNeeded();
			cmd.DrawMesh(m_fowLineMesh.Mesh, Matrix4x4.identity, material, 0, 0);
		}
	}

	private void GenerateKnownBackgorundMeshIfNeeded()
	{
		if (m_knownLineMesh != null)
		{
			return;
		}
		m_knownLineMesh = new LineMeshGenerator(0.2f);
		foreach (Line shapeLine in m_shapeLines)
		{
			m_knownLineMesh.AddLine(base.transform.TransformPoint(shapeLine.Start), base.transform.TransformPoint(shapeLine.End));
		}
		m_knownLineMesh.GenerateMesh();
	}

	public void DrawBackgroundMeshes(CommandBuffer cmd, Material material)
	{
		if (GlobalReferences.Instance.MapDynamicData.GetConnectionStatus(m_mapConnectionSettings.ConnectionID) != 0 || m_startingMapArea3D.IsKnown() || m_endingMapArea3D.IsKnown())
		{
			GenerateKnownBackgorundMeshIfNeeded();
			cmd.DrawMesh(m_knownLineMesh.Mesh, Matrix4x4.identity, material, 0, 0);
		}
	}
}
