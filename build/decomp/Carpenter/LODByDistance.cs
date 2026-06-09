using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(LODGroup))]
public class LODByDistance : MonoBehaviour
{
	[Serializable]
	private struct LODDistanceSetting
	{
		public float m_highDistance;

		public float m_mediumDistance;

		public float m_lowDistance;
	}

	private LODGroup m_lodGroup;

	[SerializeField]
	private List<LODDistanceSetting> m_lodDistances;

	private LODGroup LODGroup
	{
		get
		{
			if (m_lodGroup == null)
			{
				m_lodGroup = GetComponent<LODGroup>();
			}
			return m_lodGroup;
		}
	}

	private int GetLODIndex()
	{
		int num = 0;
		float z = base.transform.position.z;
		GraphicsQualityLevel graphicsQualityLevel = (Application.isPlaying ? GlobalReferences.Instance.UserPreferences.GraphicsQualitySettings.WorldDetail : GraphicsQualityLevel.High);
		foreach (LODDistanceSetting lodDistance in m_lodDistances)
		{
			if (z < graphicsQualityLevel switch
			{
				GraphicsQualityLevel.High => lodDistance.m_highDistance, 
				GraphicsQualityLevel.Medium => lodDistance.m_mediumDistance, 
				_ => lodDistance.m_lowDistance, 
			})
			{
				return num;
			}
			num++;
		}
		return LODGroup.lodCount - 1;
	}

	private void UpdateLOD()
	{
		LODGroup.ForceLOD(GetLODIndex());
	}

	private void Start()
	{
		UpdateLOD();
	}

	private void OnEnable()
	{
		if (Application.isPlaying)
		{
			GlobalReferences.Instance.EventChannels.UserPreferences.GraphicsQualityChanged.Register(UpdateLOD);
		}
	}

	private void OnDisable()
	{
		if (Application.isPlaying)
		{
			GlobalReferences.Instance.EventChannels.UserPreferences.GraphicsQualityChanged.Unregister(UpdateLOD);
		}
	}

	private void OnValidate()
	{
		if (m_lodDistances == null || m_lodDistances.Count <= 0)
		{
			m_lodDistances = new List<LODDistanceSetting>();
			LODDistanceSetting lODDistanceSetting = default(LODDistanceSetting);
			lODDistanceSetting.m_highDistance = 5f;
			lODDistanceSetting.m_mediumDistance = 2.5f;
			lODDistanceSetting.m_lowDistance = -5f;
			LODDistanceSetting item = lODDistanceSetting;
			m_lodDistances.Add(item);
		}
	}
}
