using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public class PersistentDataDynamicallySpawnedObject
{
	[SerializeField]
	private string m_guid;

	[SerializeField]
	private AssetReference m_assetReference;

	[SerializeField]
	private string m_levelName;

	[SerializeField]
	public Vector3 m_position;

	[SerializeField]
	public Quaternion m_rotation;

	[SerializeField]
	public Vector3 m_scale;

	public string GUID => m_guid;

	public AssetReference AssetReference => m_assetReference;

	public bool MatchesLevel(LevelMetadata level)
	{
		if (level == null)
		{
			return false;
		}
		return m_levelName.Equals(level.name);
	}

	public PersistentDataDynamicallySpawnedObject(string guid, AssetReference assetReference, LevelMetadata level)
	{
		m_guid = guid;
		m_assetReference = assetReference;
		m_levelName = level.name;
	}
}
