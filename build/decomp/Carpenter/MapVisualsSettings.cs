using System;
using Shapes;
using UnityEngine;
using UnityEngine.Localization;

[Serializable]
[CreateAssetMenu(menuName = "Settings/Map Visuals Settings")]
public class MapVisualsSettings : ScriptableObject
{
	[Serializable]
	public class MapIconSetting
	{
		public MapIconType m_iconType;

		public Sprite m_icon;

		public Vector2 m_pivot;

		public Vector2 m_size;

		public LocalizedString m_hoverText;

		public bool m_scaleToZoom;

		public bool m_canGroup;

		public bool m_useZoomLevelAlpha;
	}

	[Serializable]
	public struct MapAreaVisuals
	{
		[SerializeField]
		private Material m_unvisitedMaterial;

		[SerializeField]
		private Material m_unvisitedBorderMaterial;

		[SerializeField]
		private Material m_pathBorderMaterial;

		[SerializeField]
		private Material m_pathMaterial;

		[SerializeField]
		private Material m_buildingMaterial;

		[SerializeField]
		private Material m_buildingBorderMaterial;

		[SerializeField]
		private Material m_belowFloorMaterial;

		[SerializeField]
		private Material m_belowFloorBorderMaterial;

		[SerializeField]
		private Material m_regionMaterial;

		[SerializeField]
		private Material m_activeMaterial;

		[SerializeField]
		private Color m_pathUnvisitedColor;

		[SerializeField]
		private Color m_pathVisitedColor;

		[SerializeField]
		private Material m_doorStandardMaterial;

		[SerializeField]
		private Material m_doorActiveMaterial;

		[SerializeField]
		private Material m_doorVisitedMaterial;

		[SerializeField]
		private Color m_shapeRendererVisitedColor;

		[SerializeField]
		private Color m_shapeRendererUnvisitedColor;

		public Material UnvisitedMaterial => m_unvisitedMaterial;

		public Material UnvisitedBorderMaterial => m_unvisitedBorderMaterial;

		public Material PathBorderMaterial => m_pathBorderMaterial;

		public Material PathMaterial => m_pathMaterial;

		public Material BuildingMaterial => m_buildingMaterial;

		public Material BuildingBorderMaterial => m_buildingBorderMaterial;

		public Material BelowFloorMaterial => m_belowFloorMaterial;

		public Material BelowFloorBorderMaterial => m_belowFloorBorderMaterial;

		public Material RegionMaterial => m_regionMaterial;

		public Material ActiveMaterial => m_activeMaterial;

		public Color PathUnvisitedColor => m_pathUnvisitedColor;

		public Color PathVisitedColor => m_pathVisitedColor;

		public Material DoorStandardMaterial => m_doorStandardMaterial;

		public Material DoorActiveMaterial => m_doorActiveMaterial;

		public Material DoorVisitedMaterial => m_doorVisitedMaterial;

		public Color ShapeRendererVisitedColor => m_shapeRendererVisitedColor;

		public Color ShapeRendererUnvisitedColor => m_shapeRendererUnvisitedColor;
	}

	[Serializable]
	public struct MapConnectionLineSettingsShared
	{
		public Sprite m_unknownCapSprite;

		public Color m_unknownColor;

		public Sprite m_openCapSprite;

		public Color m_openColor;

		public Sprite m_lockedCapSprite;

		public Color m_lockedColor;

		public Sprite m_barrierSprite;

		public Color m_barrierColor;

		public Sprite m_plasticTarpSprite;

		public Color m_plasticTarpColor;

		public Sprite m_doorNormalSprite;

		public Color m_doorNormalColor;

		public Sprite m_doorUnknownSprite;

		public Color m_doorUnknownColor;

		public Sprite m_doorLockedSprite;

		public Color m_doorLockedColor;

		public Sprite m_floorUpSprite;

		public Sprite m_floorDownSprite;
	}

	[Serializable]
	public class ConnectionLineVisualsSettings
	{
		public ConnectionLineVisualsSettingsType m_type;

		public bool m_useAreaColor;

		public Color m_lineUnknownColor;

		public Color m_lineUnvisitedColor;

		public Color m_lineVisitedColor;

		public float m_lineThickness;

		public bool m_isDashed;

		public DashStyle m_dashStyle;

		public ShapesBlendMode m_blendMode;

		public bool m_includeBackgroundBorder;
	}

	[SerializeField]
	private MapIconSetting[] m_icons;

	[SerializeField]
	private AnimationCurve m_iconAlphaCurve;

	[SerializeField]
	private Sprite[] m_userMarkerSprites;

	[SerializeField]
	public MapAreaVisuals m_areaVisuals;

	[SerializeField]
	public MapConnectionLineSettingsShared m_connectionLineShared;

	[SerializeField]
	private ConnectionLineVisualsSettings[] m_lineVisualsSettings;

	public AnimationCurve IconAlphaCurve => m_iconAlphaCurve;

	public ConnectionLineVisualsSettings GetLineSettingsForVisualsType(ConnectionLineVisualsSettingsType type)
	{
		ConnectionLineVisualsSettings[] lineVisualsSettings = m_lineVisualsSettings;
		foreach (ConnectionLineVisualsSettings connectionLineVisualsSettings in lineVisualsSettings)
		{
			if (connectionLineVisualsSettings.m_type == type)
			{
				return connectionLineVisualsSettings;
			}
		}
		return m_lineVisualsSettings[0];
	}

	public Sprite GetSpriteForMarkerIndex(int index)
	{
		if (index >= m_userMarkerSprites.Length)
		{
			Debug.LogError("Tried to access non existant sprite for map markers at index " + index);
			index = 0;
		}
		return m_userMarkerSprites[index];
	}

	public int GetUserMarkerSpriteCount()
	{
		return m_userMarkerSprites.Length;
	}

	public MapIconSetting GetIconSettings(MapIconType iconType)
	{
		MapIconSetting[] icons = m_icons;
		foreach (MapIconSetting mapIconSetting in icons)
		{
			if (mapIconSetting.m_iconType == iconType)
			{
				return mapIconSetting;
			}
		}
		return null;
	}

	public bool IsSupportedIconType(MapIconType iconType)
	{
		MapIconSetting[] icons = m_icons;
		for (int i = 0; i < icons.Length; i++)
		{
			if (icons[i].m_iconType == iconType)
			{
				return true;
			}
		}
		return false;
	}
}
