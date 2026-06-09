using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelMapAOMMetadata
{
	[Serializable]
	public class AppearsOnMapInstance
	{
		[SerializeField]
		private Vector3 m_worldPosition;

		[SerializeField]
		private AppearsOnMap.AppearsOnMapSettings m_settings;

		[SerializeField]
		private string m_objectName;

		[SerializeField]
		private ItemDefinition m_itemDefinition;

		[SerializeField]
		private LoreEntry m_loreEntry;

		[SerializeField]
		private LootTableSettings m_lootSettings;

		[SerializeField]
		private LevelMetadata m_levelMetadata;

		public Vector3 WorldPosition => m_worldPosition;

		public AppearsOnMap.AppearsOnMapSettings Settings => m_settings;

		public string ObjectName => m_objectName;

		public ItemDefinition ItemDefinition => m_itemDefinition;

		public LoreEntry LoreEntry => m_loreEntry;

		public LootTableSettings LootSettings => m_lootSettings;

		public LevelMetadata LevelMetadata
		{
			get
			{
				return m_levelMetadata;
			}
			set
			{
				m_levelMetadata = value;
			}
		}

		public AppearsOnMapInstance(AppearsOnMap appearsOnMapComponent)
		{
		}
	}

	[SerializeField]
	private List<AppearsOnMapInstance> m_aomInstances;

	public IReadOnlyCollection<AppearsOnMapInstance> AppearsOnMapInstances => m_aomInstances.AsReadOnly();
}
