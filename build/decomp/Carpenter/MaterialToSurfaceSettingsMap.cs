using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Misc/Material To Surface Settings Map")]
public class MaterialToSurfaceSettingsMap : ScriptableObject
{
	[Serializable]
	public class MaterialToSurfaceSettingsMapEntry
	{
		[SerializeField]
		private Material m_material;

		[SerializeField]
		private SurfaceSettings m_surfaceSettings;

		public Material Material => m_material;

		public SurfaceSettings SurfaceSettings => m_surfaceSettings;

		public MaterialToSurfaceSettingsMapEntry(Material material)
		{
			m_material = material;
		}
	}

	[SerializeField]
	private List<MaterialToSurfaceSettingsMapEntry> m_entries;

	private Dictionary<Material, SurfaceSettings> m_dictionary;

	private void GenerateDictionary()
	{
		m_dictionary = new Dictionary<Material, SurfaceSettings>();
		foreach (MaterialToSurfaceSettingsMapEntry entry in m_entries)
		{
			if (m_dictionary.ContainsKey(entry.Material))
			{
				Debug.LogError("MaterialToSurfaceMap has a duplicate entry for the material (" + entry.Material?.ToString() + ") please fix!", this);
			}
			else
			{
				m_dictionary.Add(entry.Material, entry.SurfaceSettings);
			}
		}
	}

	private void OnValidate()
	{
		m_dictionary = null;
	}

	public SurfaceSettings GetSurfaceSettingsForMaterial(Material material)
	{
		if (m_dictionary == null)
		{
			GenerateDictionary();
		}
		if (m_dictionary.ContainsKey(material))
		{
			return m_dictionary[material];
		}
		return null;
	}
}
