using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class AddressableReferenceLoader
{
	public static TAsset LoadValidSync<TAsset, TAssetReference>(TAssetReference reference) where TAsset : Object where TAssetReference : AssetReferenceT<TAsset>
	{
		IList<TAsset> list = Addressables.LoadAssetsAsync<TAsset>(Addressables.LoadResourceLocationsAsync(reference, typeof(TAsset)).WaitForCompletion(), null).WaitForCompletion();
		if (list != null && list.Count > 0)
		{
			return list[0];
		}
		return null;
	}
}
