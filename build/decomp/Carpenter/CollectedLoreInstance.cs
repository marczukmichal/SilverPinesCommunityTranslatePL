using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public class CollectedLoreInstance
{
	[SerializeField]
	private AssetReference m_loreEntryAssetReference;

	private LoreEntry m_loadedLoreEntry;

	public AssetReference LoreEntryAssetReference => m_loreEntryAssetReference;

	public LoreEntry LoreEntry
	{
		get
		{
			if (m_loadedLoreEntry == null)
			{
				m_loadedLoreEntry = AddressablesContentManager.Instance.GetScriptableObjectAsset(m_loreEntryAssetReference) as LoreEntry;
			}
			return m_loadedLoreEntry;
		}
	}

	public CollectedLoreInstance(LoreEntry lore)
	{
		m_loadedLoreEntry = lore;
		m_loreEntryAssetReference = lore.AssetReference;
	}
}
