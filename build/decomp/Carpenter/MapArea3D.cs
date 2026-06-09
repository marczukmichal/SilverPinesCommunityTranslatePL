using System;
using Shapes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.Splines;

[ExecuteAlways]
public class MapArea3D : MonoBehaviour
{
	public enum AreaType
	{
		Path,
		Location,
		Spline,
		Region
	}

	private enum State
	{
		None,
		Unvisited,
		Visited,
		Active,
		Unknown
	}

	[FormerlySerializedAs("m_meshes")]
	[SerializeField]
	private MeshRenderer[] m_materialSwapMeshes;

	[SerializeField]
	private BaseMapAreaMetadata m_mapAreaMetadata;

	[Tooltip("These are gameobjects that should be enabled/disabled depend on the current status. Hidden if unvisited.")]
	[SerializeField]
	private GameObject[] m_hideOnUnvisited;

	[SerializeField]
	private UnityEvent m_onHasVisitedEvent;

	[SerializeField]
	private UnityEvent m_onNotVisitedEvent;

	[SerializeField]
	private AreaType m_type;

	[SerializeField]
	private Vector3 m_startOffset;

	[SerializeField]
	private Vector3 m_endOffset;

	[SerializeField]
	private float[] m_referenceMarkers;

	[SerializeField]
	private float m_areaHeight = 1f;

	[SerializeField]
	private float m_zSize;

	[Header("Shape Renderer")]
	[SerializeField]
	private ShapeRenderer m_shapeRenderer;

	[Header("Label")]
	[SerializeField]
	private GameObject m_label;

	[SerializeField]
	private LevelMetadata.LevelSpacialType m_spacialType;

	private MapBuildingDoor[] m_doors;

	private MapArea3DDetail[] m_details;

	private SplineContainer m_splineContainer;

	private Color m_mainColor;

	private Color m_borderColor;

	private Mesh m_mesh;

	private State m_state;

	private FloorMode m_floorMode;

	private bool m_floorStateDirty;

	public BaseMapAreaMetadata MapAreaMetadata => m_mapAreaMetadata;

	public AreaType MapAreaType => m_type;

	public Color MainColor => m_mainColor;

	public Color BorderColor => m_borderColor;

	private SplineContainer SplineContainer
	{
		get
		{
			if (m_splineContainer == null)
			{
				m_splineContainer = GetComponent<SplineContainer>();
			}
			return m_splineContainer;
		}
	}

	private void Awake()
	{
		GetReferences();
	}

	public void SetFloorMode(FloorMode floorMode)
	{
		if (floorMode != m_floorMode)
		{
			m_floorMode = floorMode;
			m_floorStateDirty = true;
		}
	}

	private void GetReferences()
	{
		if (m_details == null)
		{
			m_details = GetComponentsInChildren<MapArea3DDetail>();
		}
		if (m_doors == null)
		{
			m_doors = GetComponentsInChildren<MapBuildingDoor>();
		}
		if (m_splineContainer == null)
		{
			m_splineContainer = GetComponent<SplineContainer>();
		}
	}

	public bool ShouldShowPlayerPosition()
	{
		if (m_type != 0)
		{
			return m_type == AreaType.Spline;
		}
		return true;
	}

	private void Reset()
	{
		m_materialSwapMeshes = GetComponents<MeshRenderer>();
	}

	private void OnDestroy()
	{
		if (m_mesh != null)
		{
			UnityEngine.Object.Destroy(m_mesh);
		}
	}

	public void Refresh(BaseMapAreaMetadata currentPlayerArea)
	{
		if (m_mapAreaMetadata == null)
		{
			Debug.LogWarning("MapArea3D " + base.name + " has invalid MapAreaMetadata - Please rebuild the map!");
			return;
		}
		GetReferences();
		bool num = IsPlayerInArea(currentPlayerArea);
		bool flag = HasPlayerVisitedArea();
		bool flag2 = IsKnown();
		State state = State.Unvisited;
		if (num)
		{
			state = State.Active;
		}
		else if (flag)
		{
			state = State.Visited;
		}
		else if (!flag2)
		{
			state = State.Unknown;
		}
		bool flag3 = flag && m_floorMode == FloorMode.Active;
		if (state != m_state || m_floorStateDirty)
		{
			m_state = state;
			m_floorStateDirty = false;
			MapVisualsSettings.MapAreaVisuals visualsSettings = GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Item.VisualsSettings;
			Material material = visualsSettings.UnvisitedMaterial;
			Material material2 = visualsSettings.PathBorderMaterial;
			_ = visualsSettings.PathVisitedColor;
			Color color = visualsSettings.ShapeRendererVisitedColor;
			bool flag4 = false;
			bool flag5 = true;
			switch (m_state)
			{
			case State.Unvisited:
				flag4 = true;
				if (m_floorMode == FloorMode.Below)
				{
					material = visualsSettings.BelowFloorMaterial;
					material2 = visualsSettings.BelowFloorBorderMaterial;
					break;
				}
				material = visualsSettings.UnvisitedMaterial;
				_ = visualsSettings.PathUnvisitedColor;
				color = visualsSettings.ShapeRendererUnvisitedColor;
				material2 = visualsSettings.UnvisitedBorderMaterial;
				break;
			case State.Visited:
				switch (m_type)
				{
				case AreaType.Path:
					if (m_floorMode == FloorMode.Below)
					{
						material = visualsSettings.BelowFloorMaterial;
						material2 = visualsSettings.BelowFloorBorderMaterial;
					}
					else if (m_spacialType == LevelMetadata.LevelSpacialType.Interior)
					{
						material = visualsSettings.BuildingMaterial;
						material2 = visualsSettings.BuildingBorderMaterial;
					}
					else
					{
						material = visualsSettings.PathMaterial;
						material2 = visualsSettings.PathBorderMaterial;
					}
					break;
				case AreaType.Location:
					material = visualsSettings.BuildingMaterial;
					break;
				case AreaType.Region:
					material = visualsSettings.RegionMaterial;
					break;
				}
				break;
			case State.Active:
				switch (m_type)
				{
				case AreaType.Path:
					if (m_floorMode == FloorMode.Below)
					{
						material = visualsSettings.BelowFloorMaterial;
						material2 = visualsSettings.BelowFloorBorderMaterial;
					}
					else if (m_spacialType == LevelMetadata.LevelSpacialType.Interior)
					{
						material = visualsSettings.BuildingMaterial;
						material2 = visualsSettings.BuildingBorderMaterial;
					}
					else
					{
						material = visualsSettings.PathMaterial;
						material2 = visualsSettings.PathBorderMaterial;
					}
					break;
				case AreaType.Location:
					material = visualsSettings.ActiveMaterial;
					break;
				case AreaType.Region:
					material = visualsSettings.ActiveMaterial;
					break;
				}
				break;
			case State.Unknown:
				flag4 = true;
				flag5 = false;
				break;
			}
			if (m_mapAreaMetadata.VisualsStyling == BaseMapAreaMetadata.MapAreaStyling.Invisible)
			{
				flag5 = false;
			}
			MeshRenderer[] materialSwapMeshes = m_materialSwapMeshes;
			foreach (MeshRenderer meshRenderer in materialSwapMeshes)
			{
				if (meshRenderer == null)
				{
					Debug.LogWarning("MapArea3D " + base.gameObject.name + " has a null mesh setup in m_materialSwapMeshes array");
					continue;
				}
				meshRenderer.enabled = flag5;
				meshRenderer.sharedMaterial = material;
				if (meshRenderer.sharedMaterials.Length > 1)
				{
					Material[] sharedMaterials = meshRenderer.sharedMaterials;
					sharedMaterials[1] = material2;
					meshRenderer.sharedMaterials = sharedMaterials;
				}
			}
			GameObject[] hideOnUnvisited = m_hideOnUnvisited;
			foreach (GameObject gameObject in hideOnUnvisited)
			{
				if (gameObject == null)
				{
					Debug.LogWarning("MapArea3D " + base.gameObject.name + " has a null hideObject setup in array");
				}
				else
				{
					gameObject.SetActive(!flag4);
				}
			}
			if (m_shapeRenderer != null)
			{
				m_shapeRenderer.Color = color;
			}
			if (flag4)
			{
				m_onNotVisitedEvent.Invoke();
			}
			else
			{
				m_onHasVisitedEvent.Invoke();
			}
			if (m_label != null)
			{
				m_label.gameObject.SetActive(flag3);
			}
			m_mainColor = material.color;
			m_borderColor = material2.color;
		}
		MapBuildingDoor[] doors = m_doors;
		for (int i = 0; i < doors.Length; i++)
		{
			doors[i].Refresh();
		}
		MapArea3DDetail[] details = m_details;
		for (int i = 0; i < details.Length; i++)
		{
			details[i].SetVisible(flag3);
		}
	}

	private void OnValidate()
	{
		if (m_referenceMarkers != null)
		{
			Array.Sort(m_referenceMarkers);
		}
	}

	public bool IsPlayerInArea(BaseMapAreaMetadata currentPlayerArea)
	{
		return m_mapAreaMetadata == currentPlayerArea;
	}

	public bool IsMatchingUnmappedAreaLocationParent()
	{
		if (m_mapAreaMetadata == null)
		{
			return false;
		}
		return m_mapAreaMetadata.IsUnmappedLocationParent();
	}

	public bool HasPlayerVisitedArea()
	{
		if (GameDebugCommands.CHEAT_MAP_SHOW_EVERYTHING)
		{
			return true;
		}
		if (m_mapAreaMetadata == null)
		{
			return false;
		}
		return m_mapAreaMetadata.HasPlayerVisitedArea();
	}

	public bool TracksLevel(LevelMetadata levelMetadata)
	{
		return m_mapAreaMetadata.TracksLevel(levelMetadata);
	}

	public void GetPlayerMarkerWorldPosition(ref Vector3 position, ref Quaternion rotation)
	{
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		Vector3 worldPosition = Vector3.zero;
		if (item != null)
		{
			worldPosition = item.transform.position;
		}
		position = Get3DMapWorldPositionFromLevelPosition(worldPosition);
		rotation = Get3DMapWorldRotationFromLevelPosition(worldPosition);
	}

	public Vector3 Get3DMapWorldPositionFromLevelPosition(Vector3 worldPosition, bool ignoreZ = false)
	{
		if (m_mapAreaMetadata is PathMapAreaMetadata pathMapAreaMetadata)
		{
			float num = worldPosition.x.Remap(pathMapAreaMetadata.m_inLevelData.MapBoundsRect.xMin, pathMapAreaMetadata.m_inLevelData.MapBoundsRect.xMax, 0f, 1f);
			float num2 = worldPosition.y.Remap(pathMapAreaMetadata.m_inLevelData.MapBoundsRect.yMin, pathMapAreaMetadata.m_inLevelData.MapBoundsRect.yMax, 0f, 1f);
			if (m_referenceMarkers != null && m_referenceMarkers.Length != 0)
			{
				num = RemapToReferenceMarkers(num);
			}
			Vector3 worldPositionFromNormalised = GetWorldPositionFromNormalised(num, ignoreZ);
			worldPositionFromNormalised.y += num2 * m_areaHeight;
			return worldPositionFromNormalised;
		}
		return base.transform.position;
	}

	public Quaternion Get3DMapWorldRotationFromLevelPosition(Vector3 worldPosition)
	{
		if (m_type == AreaType.Spline && m_mapAreaMetadata is PathMapAreaMetadata pathMapAreaMetadata)
		{
			float t = worldPosition.x.Remap(pathMapAreaMetadata.m_inLevelData.MapBoundsRect.xMin, pathMapAreaMetadata.m_inLevelData.MapBoundsRect.xMax, 0f, 1f);
			if (m_referenceMarkers != null && m_referenceMarkers.Length != 0)
			{
				t = RemapToReferenceMarkers(t);
			}
			m_splineContainer.Spline.Evaluate(t, out var _, out var tangent, out var _);
			return Quaternion.LookRotation(tangent) * Quaternion.Euler(0f, -90f, 0f);
		}
		return Quaternion.identity;
	}

	private float RemapToReferenceMarkers(float t)
	{
		if (m_mapAreaMetadata is PathMapAreaMetadata pathMapAreaMetadata)
		{
			if (pathMapAreaMetadata.m_inLevelData.ReferencesPoints.Length != m_referenceMarkers.Length)
			{
				Debug.LogError(base.gameObject.name + " and equivalent map area do not have the same number of reference markers! RemapToReferenceMarkers failed!");
				return t;
			}
			GameUtils.RemapPair[] array = new GameUtils.RemapPair[pathMapAreaMetadata.m_inLevelData.ReferencesPoints.Length + 2];
			array[0].m_worldReferenceValue = 0f;
			array[0].m_mapReferenceValue = 0f;
			for (int i = 0; i < pathMapAreaMetadata.m_inLevelData.ReferencesPoints.Length; i++)
			{
				array[i + 1].m_worldReferenceValue = pathMapAreaMetadata.m_inLevelData.ReferencesPoints[i];
				array[i + 1].m_mapReferenceValue = m_referenceMarkers[i];
			}
			array[^1].m_worldReferenceValue = 1f;
			array[^1].m_mapReferenceValue = 1f;
			return GameUtils.MapPositionRemap(t, array);
		}
		return t;
	}

	public Vector3 GetWorldPositionFromNormalised(float normalisedLerp, bool ignoreZ = false)
	{
		if (m_type == AreaType.Spline)
		{
			SplineContainer.Spline.Evaluate(normalisedLerp, out var position, out var _, out var _);
			return base.transform.TransformPoint(position);
		}
		Vector3 startOffset = m_startOffset;
		Vector3 endOffset = m_endOffset;
		if (ignoreZ)
		{
			startOffset.z = 0f;
			endOffset.z = 0f;
		}
		Vector3 position2 = Vector3.Lerp(startOffset, endOffset, normalisedLerp);
		return base.transform.TransformPoint(position2);
	}

	private float InverseGetWorldPosition(Vector3 position)
	{
		Vector3 vector = m_endOffset - m_startOffset;
		return Mathf.Clamp01(Vector3.Dot(position - m_startOffset, vector) / Vector3.Dot(vector, vector));
	}

	public bool ShouldShowInFogOfWar()
	{
		if (m_state != State.Active)
		{
			return m_state == State.Visited;
		}
		return true;
	}

	public bool IsKnown(bool ignoreMapRevealPickups = false)
	{
		if (GameDebugCommands.CHEAT_MAP_SHOW_EVERYTHING)
		{
			return true;
		}
		return GlobalReferences.Instance.MapDynamicData.IsKnown(m_mapAreaMetadata, ignoreMapRevealPickups);
	}

	public void DrawBackgroundMeshes(CommandBuffer cmd, Material material)
	{
		if (IsKnown())
		{
			MeshFilter component = GetComponent<MeshFilter>();
			if (component != null)
			{
				cmd.DrawMesh(component.mesh, component.transform.localToWorldMatrix, material, 0, 0);
			}
		}
	}

	public void DrawFogOfWarVisibilityMeshes(CommandBuffer cmd, Material material)
	{
		if (ShouldShowInFogOfWar())
		{
			GenerateMeshIfNeeded();
			if (m_mesh != null)
			{
				Matrix4x4 identity = Matrix4x4.identity;
				cmd.DrawMesh(m_mesh, identity, material, 0, 0);
			}
		}
	}

	private void GenerateMeshIfNeeded()
	{
		bool flag = false;
		if (m_mesh != null && m_materialSwapMeshes.Length != 0 && m_mapAreaMetadata is PathMapAreaMetadata)
		{
			MapDynamicData.MapAreaDynamicData mapAreaDynamicData = GlobalReferences.Instance.MapDynamicData.GetMapAreaDynamicData(m_mapAreaMetadata);
			if (mapAreaDynamicData != null && mapAreaDynamicData.m_isDirty)
			{
				flag = true;
			}
		}
		if (!((m_mesh == null && m_materialSwapMeshes.Length != 0) || flag))
		{
			return;
		}
		Bounds bounds = m_materialSwapMeshes[0].bounds;
		MeshRenderer[] materialSwapMeshes = m_materialSwapMeshes;
		foreach (MeshRenderer meshRenderer in materialSwapMeshes)
		{
			if (meshRenderer != null)
			{
				bounds.Encapsulate(meshRenderer.bounds);
			}
		}
		if (m_mapAreaMetadata is PathMapAreaMetadata)
		{
			MapDynamicData.MapAreaDynamicData mapAreaDynamicData2 = GlobalReferences.Instance.MapDynamicData.GetMapAreaDynamicData(m_mapAreaMetadata);
			if (mapAreaDynamicData2 != null)
			{
				Vector3 worldPositionFromNormalised = GetWorldPositionFromNormalised(mapAreaDynamicData2.m_minXNormalizePosition);
				Vector3 worldPositionFromNormalised2 = GetWorldPositionFromNormalised(mapAreaDynamicData2.m_maxXNormalizePosition);
				Vector3 min = bounds.min;
				Vector3 max = bounds.max;
				min.x = worldPositionFromNormalised.x;
				max.x = worldPositionFromNormalised2.x;
				bounds.SetMinMax(min, max);
				mapAreaDynamicData2.m_isDirty = false;
			}
		}
		bounds.Expand(1f);
		Vector3 min2 = bounds.min;
		Vector3 max2 = bounds.max;
		Vector3 size = bounds.size;
		size.y = 0.01f;
		bounds.size = size;
		if (m_mesh == null)
		{
			m_mesh = new Mesh();
		}
		Vector3[] array = new Vector3[8];
		int[] triangles = new int[36]
		{
			0, 2, 1, 2, 3, 1, 2, 6, 3, 6,
			7, 3, 6, 4, 7, 4, 5, 7, 4, 0,
			5, 0, 1, 5, 1, 3, 5, 3, 7, 5,
			6, 2, 4, 2, 0, 4
		};
		array[0] = new Vector3(min2.x, min2.y, min2.z);
		array[1] = new Vector3(max2.x, min2.y, min2.z);
		array[2] = new Vector3(min2.x, max2.y, min2.z);
		array[3] = new Vector3(max2.x, max2.y, min2.z);
		array[4] = new Vector3(min2.x, min2.y, max2.z);
		array[5] = new Vector3(max2.x, min2.y, max2.z);
		array[6] = new Vector3(min2.x, max2.y, max2.z);
		array[7] = new Vector3(max2.x, max2.y, max2.z);
		m_mesh.vertices = array;
		m_mesh.triangles = triangles;
		m_mesh.RecalculateNormals();
		m_mesh.RecalculateBounds();
	}

	public Vector3 GetRegionMapIconPosition()
	{
		Vector3 position = base.transform.position;
		if (m_mapAreaMetadata is RegionMapAreaMetadata regionMapAreaMetadata)
		{
			position += regionMapAreaMetadata.RegionMapIconOffset;
		}
		return position;
	}
}
