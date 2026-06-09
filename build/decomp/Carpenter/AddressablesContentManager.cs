using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressablesContentManager : MonoBehaviour
{
	private class ContentData<T> where T : Object
	{
		public T m_asset;

		public AsyncOperationHandle<T> m_handle;
	}

	private static AddressablesContentManager m_instance;

	private Dictionary<AssetReference, ContentData<ScriptableObject>> m_scriptableObjects = new Dictionary<AssetReference, ContentData<ScriptableObject>>();

	public static AddressablesContentManager Instance => m_instance;

	private void OnEnable()
	{
		m_instance = this;
	}

	private T LoadAsset<T>(AssetReference assetReference, Dictionary<AssetReference, ContentData<T>> dictionary) where T : Object
	{
		if (dictionary.ContainsKey(assetReference))
		{
			return dictionary[assetReference].m_asset;
		}
		ContentData<T> contentData = new ContentData<T>();
		try
		{
			contentData.m_handle = assetReference.LoadAssetAsync<T>();
			contentData.m_handle.WaitForCompletion();
			contentData.m_asset = contentData.m_handle.Result;
		}
		catch (InvalidKeyException exception)
		{
			Debug.LogError("Failed to load addressable asset");
			Debug.LogException(exception);
			return null;
		}
		dictionary.Add(assetReference, contentData);
		return contentData.m_handle.Result;
	}

	private void UnloadAsset<T>(AssetReference assetReference, Dictionary<AssetReference, ContentData<T>> dictionary) where T : Object
	{
		bool flag = false;
		if (dictionary.ContainsKey(assetReference))
		{
			dictionary[assetReference].m_handle.Release();
			flag = true;
		}
		if (flag)
		{
			dictionary.Remove(assetReference);
		}
	}

	public ScriptableObject GetScriptableObjectAsset(AssetReference assetReference)
	{
		return LoadAsset(assetReference, m_scriptableObjects);
	}

	public T GetAsset<T>(AssetReferenceT<T> assetReference) where T : ScriptableObject
	{
		return LoadAsset(assetReference, m_scriptableObjects) as T;
	}

	public void Release(AssetReference assetReference)
	{
		UnloadAsset(assetReference, m_scriptableObjects);
	}
}
