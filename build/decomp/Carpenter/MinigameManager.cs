using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class MinigameManager : MonoBehaviour
{
	private AsyncOperationHandle<SceneInstance> m_loadedSceneHandle;

	private MinigameMetadata m_activeMinigameMetadata;

	private AssetReference m_activeAssetReference;

	public static UnityAction<Scene> OnMinigameSceneLoaded;

	private static MinigameManager m_instance;

	private void OnEnable()
	{
		m_instance = this;
	}

	private void OnDisable()
	{
		m_instance = null;
	}

	public static void SetActivateMinigameMetadata(MinigameMetadata metadata)
	{
		m_instance.SetActiveMinigameMetadata_Internal(metadata);
	}

	private void SetActiveMinigameMetadata_Internal(MinigameMetadata metadata)
	{
		if (m_activeMinigameMetadata != metadata)
		{
			if (m_loadedSceneHandle.IsValid() && m_loadedSceneHandle.IsDone)
			{
				Addressables.UnloadSceneAsync(m_loadedSceneHandle.Result, autoReleaseHandle: false);
				m_loadedSceneHandle.Release();
			}
			m_activeMinigameMetadata = metadata;
			if (metadata != null)
			{
				m_activeAssetReference = metadata.AssetReference;
				m_loadedSceneHandle = Addressables.LoadSceneAsync(m_activeAssetReference, LoadSceneMode.Additive);
				m_loadedSceneHandle.Completed += OnCompletion;
			}
		}
	}

	private void OnCompletion(AsyncOperationHandle<SceneInstance> handle)
	{
		if (m_activeMinigameMetadata == null)
		{
			if (m_loadedSceneHandle.IsValid())
			{
				Addressables.UnloadSceneAsync(m_loadedSceneHandle.Result, autoReleaseHandle: false);
				m_loadedSceneHandle.Release();
			}
		}
		else
		{
			OnMinigameSceneLoaded?.Invoke(m_loadedSceneHandle.Result.Scene);
		}
	}
}
