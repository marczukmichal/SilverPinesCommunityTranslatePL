using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AssetLoadingManager : MonoBehaviour
{
	[Serializable]
	private struct GameObjectAssetsToLoad
	{
		public AssetReferenceGameObject m_assetReference;
	}

	[SerializeField]
	private GameObjectAssetsToLoad[] m_gameObjectAssetsToLoad;

	[SerializeField]
	private StartupLoadingScreen m_loadingScreen;

	private bool m_isDoneLoading;

	private List<AsyncOperationHandle<GameObject>> m_loadedGameObjectHandles;

	private AsyncOperationHandle m_currentOperation;

	public bool IsDoneLoading => m_isDoneLoading;

	public string CurrentState
	{
		get
		{
			if (m_isDoneLoading)
			{
				return "Done";
			}
			if (m_currentOperation.IsValid())
			{
				return m_currentOperation.DebugName;
			}
			return "Unknown";
		}
	}

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	public void LoadAssets()
	{
		m_isDoneLoading = false;
		m_loadedGameObjectHandles = new List<AsyncOperationHandle<GameObject>>();
		StartCoroutine(LoadAssetsCoroutine());
	}

	private IEnumerator LoadAssetsCoroutine()
	{
		GameObjectAssetsToLoad[] gameObjectAssetsToLoad = m_gameObjectAssetsToLoad;
		for (int i = 0; i < gameObjectAssetsToLoad.Length; i++)
		{
			GameObjectAssetsToLoad gameObjectAssetsToLoad2 = gameObjectAssetsToLoad[i];
			AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(gameObjectAssetsToLoad2.m_assetReference);
			m_currentOperation = handle;
			m_loadedGameObjectHandles.Add(handle);
			m_loadingScreen.ShowText("Loading Asset: " + handle.DebugName);
			Debug.Log("Loading Asset: " + handle.DebugName);
			yield return handle.WaitForCompletion();
			Debug.Log("Asset Loaded Complete: " + handle.DebugName);
		}
		m_isDoneLoading = true;
	}
}
