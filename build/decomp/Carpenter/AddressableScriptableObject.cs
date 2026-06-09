using UnityEngine;
using UnityEngine.AddressableAssets;

public class AddressableScriptableObject<T> : ScriptableObject where T : ScriptableObject
{
	[HideInInspector]
	[SerializeField]
	protected string m_assetGUID;

	public string AssetGUID => m_assetGUID;

	public AssetReferenceT<T> AssetReference => new AssetReferenceT<T>(m_assetGUID);
}
