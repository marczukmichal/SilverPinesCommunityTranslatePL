using System;
using UnityEngine;

public class MapDetailsLayer : MonoBehaviour
{
	[Serializable]
	private struct MeshMaterialFloorSettings
	{
		public Map3DFloor m_floor;

		public Vector2 m_offset;
	}

	[SerializeField]
	private float m_minZoomLevel;

	[SerializeField]
	private float m_maxZoomLevel;

	[SerializeField]
	private AnimationCurve m_curve;

	[SerializeField]
	private MeshMaterialFloorSettings[] m_floorSettings;

	private MeshRenderer[] m_meshes;

	internal static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

	internal static readonly int MainTexture = Shader.PropertyToID("_BaseMap");

	public void Awake()
	{
		GetMeshes();
	}

	private void GetMeshes()
	{
		m_meshes = GetComponentsInChildren<MeshRenderer>(includeInactive: true);
	}

	public void OnZoomLevelUpdate(float zoomLevel)
	{
		if (m_meshes == null)
		{
			GetMeshes();
			if (m_meshes == null)
			{
				return;
			}
		}
		float a = 0f;
		if (zoomLevel >= m_minZoomLevel && zoomLevel <= m_maxZoomLevel && m_curve != null)
		{
			a = m_curve.Evaluate(GameUtils.Remap(zoomLevel, m_minZoomLevel, m_maxZoomLevel, 0f, 1f));
		}
		MeshRenderer[] meshes = m_meshes;
		foreach (MeshRenderer obj in meshes)
		{
			Color color = obj.material.GetColor(BaseColor);
			color.a = a;
			obj.material.SetColor(BaseColor, color);
		}
	}

	public void OnFloorChanged(Map3DFloor activeFloor)
	{
		if (activeFloor == null)
		{
			return;
		}
		MeshRenderer component = GetComponent<MeshRenderer>();
		MeshMaterialFloorSettings[] floorSettings = m_floorSettings;
		for (int i = 0; i < floorSettings.Length; i++)
		{
			MeshMaterialFloorSettings meshMaterialFloorSettings = floorSettings[i];
			if (meshMaterialFloorSettings.m_floor == activeFloor)
			{
				Vector2 offset = meshMaterialFloorSettings.m_offset;
				component.material.SetTextureOffset(MainTexture, offset);
				break;
			}
		}
	}
}
