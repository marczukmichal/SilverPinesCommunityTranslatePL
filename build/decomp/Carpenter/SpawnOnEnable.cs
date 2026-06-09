using UnityEngine;
using UnityEngine.AddressableAssets;

public class SpawnOnEnable : MonoBehaviour
{
	[SerializeField]
	private AssetReferenceGameObject m_asset;

	private void OnEnable()
	{
		if (m_asset != null && m_asset.HasAsset())
		{
			DynamicallySpawnedObject.Spawn(m_asset, persistent: false, base.transform.position, base.transform.rotation);
		}
	}
}
