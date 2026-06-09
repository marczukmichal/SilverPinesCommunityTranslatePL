using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName = "Misc/Asset Reference SO")]
public class AssetReferenceSO : ScriptableObject
{
	[SerializeField]
	private AssetReference m_assetReference;

	public AssetReference AssetReference => m_assetReference;
}
