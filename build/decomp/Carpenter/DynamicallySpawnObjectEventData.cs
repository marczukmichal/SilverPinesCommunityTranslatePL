using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

public struct DynamicallySpawnObjectEventData
{
	public Vector3 m_position;

	public Quaternion m_rotation;

	public Vector3 m_scale;

	public AssetReference m_assetReference;

	public bool m_persistent;

	public string m_guid;

	public UnityAction<GameObject> m_onSpawnedAction;

	public DynamicallySpawnObjectEventData(AssetReference assetReference, bool persistent, Vector3 position)
		: this(assetReference, persistent, position, Quaternion.identity)
	{
	}

	public DynamicallySpawnObjectEventData(AssetReference assetReference, bool persistent, Vector3 position, Quaternion rotation)
		: this(assetReference, persistent, position, rotation, Vector3.one)
	{
	}

	public DynamicallySpawnObjectEventData(AssetReference assetReference, bool persistent, Vector3 position, Quaternion rotation, Vector3 scale)
	{
		m_position = position;
		m_rotation = rotation;
		m_scale = scale;
		m_persistent = persistent;
		m_assetReference = assetReference;
		m_onSpawnedAction = null;
		m_guid = null;
	}
}
