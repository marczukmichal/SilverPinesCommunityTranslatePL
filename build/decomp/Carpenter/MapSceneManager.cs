using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class MapSceneManager : MonoBehaviour
{
	[SerializeField]
	private AssetReference m_worldMapScene;

	[SerializeField]
	private MapViewMetadata m_testMapData;

	private static RenderTexture m_renderTexture;

	private AsyncOperationHandle<SceneInstance> m_worldMapLoadedHandle;

	private Map3D m_worldMap3D;

	private AsyncOperationHandle<SceneInstance> m_loadedSubMapHandle;

	private Map3D m_loadedSubMap3D;

	private MapViewMetadata m_loadedMapViewMetadata;

	private bool m_mapActive;

	private MapViewMetadata m_activeMapViewMetadata;

	public static RenderTexture RenderTexture => m_renderTexture;

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.Map.SetMapActive.Register(SetMapActive);
		GlobalReferences.Instance.EventChannels.Map.SetActiveMapView.Register(SetActiveMapViewMetadata);
		StartCoroutine(LoadWorldMapCoroutine());
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Map.SetMapActive.Unregister(SetMapActive);
		GlobalReferences.Instance.EventChannels.Map.SetActiveMapView.Unregister(SetActiveMapViewMetadata);
	}

	private void OnDestroy()
	{
		if (m_renderTexture != null)
		{
			m_renderTexture.Release();
			Object.DestroyImmediate(m_renderTexture);
		}
		if (GlobalReferences.Instance != null)
		{
			GlobalReferences.Instance.EventChannels.Map.SetMapActive.Unregister(SetMapActive);
			GlobalReferences.Instance.EventChannels.Map.SetActiveMapView.Unregister(SetActiveMapViewMetadata);
		}
	}

	private void CheckForUpdateRenderTextureSize()
	{
		if (m_renderTexture.width != Screen.width || m_renderTexture.height != Screen.height)
		{
			m_renderTexture.Release();
			m_renderTexture.width = Screen.width;
			m_renderTexture.height = Screen.height;
			m_renderTexture.Create();
		}
	}

	private void Awake()
	{
		m_renderTexture = new RenderTexture(Screen.width, Screen.height, 32);
		m_renderTexture.antiAliasing = 4;
	}

	private void SetMapActive(bool active)
	{
		if (m_mapActive == active)
		{
			return;
		}
		m_mapActive = active;
		if (active)
		{
			CheckForUpdateRenderTextureSize();
			if (m_activeMapViewMetadata == null)
			{
				m_worldMap3D.gameObject.SetActive(value: true);
				m_worldMap3D.SetRenderTexture(m_renderTexture);
				m_worldMap3D.SetMapActive(enable: true);
			}
			else if (m_loadedSubMap3D != null)
			{
				m_loadedSubMap3D.gameObject.SetActive(value: true);
				m_loadedSubMap3D.SetRenderTexture(m_renderTexture);
				m_loadedSubMap3D.SetMapActive(enable: true);
			}
		}
		else
		{
			if (m_worldMap3D != null)
			{
				m_worldMap3D.gameObject.SetActive(value: false);
				m_worldMap3D.SetMapActive(enable: false);
			}
			if (m_loadedSubMap3D != null)
			{
				m_loadedSubMap3D.gameObject.SetActive(value: false);
				m_loadedSubMap3D.SetMapActive(enable: false);
			}
		}
	}

	private void SetActiveMapViewMetadata(MapViewMetadata mapViewMetadata)
	{
		if (m_activeMapViewMetadata != mapViewMetadata)
		{
			if (m_activeMapViewMetadata == null)
			{
				m_worldMap3D.gameObject.SetActive(value: false);
			}
			else if (m_loadedSubMap3D != null)
			{
				m_loadedSubMap3D.gameObject.SetActive(value: false);
			}
			if (m_loadedSubMap3D != null && mapViewMetadata != null && mapViewMetadata != m_loadedMapViewMetadata)
			{
				m_loadedMapViewMetadata = null;
				UnloadMap(ref m_loadedSubMapHandle, ref m_loadedSubMap3D);
			}
			m_activeMapViewMetadata = mapViewMetadata;
			if (mapViewMetadata == null)
			{
				m_worldMap3D.gameObject.SetActive(value: true);
				m_worldMap3D.SetMapActive(enable: true);
			}
			else if (mapViewMetadata == m_loadedMapViewMetadata)
			{
				m_loadedSubMap3D.gameObject.SetActive(value: true);
				m_loadedSubMap3D.SetMapActive(enable: true);
			}
			else
			{
				StartCoroutine(LoadMapCoroutine(mapViewMetadata, activate: true));
			}
		}
	}

	private IEnumerator LoadWorldMapCoroutine()
	{
		m_worldMapLoadedHandle = Addressables.LoadSceneAsync(m_worldMapScene, LoadSceneMode.Additive);
		yield return m_worldMapLoadedHandle;
		m_worldMap3D = ActivateMapScene(m_worldMapLoadedHandle.Result.Scene);
	}

	private IEnumerator LoadMapCoroutine(MapViewMetadata mapViewMetadata, bool activate)
	{
		m_loadedMapViewMetadata = mapViewMetadata;
		UnloadMap(ref m_loadedSubMapHandle, ref m_loadedSubMap3D);
		m_loadedSubMapHandle = Addressables.LoadSceneAsync(mapViewMetadata.MapViewScene, LoadSceneMode.Additive);
		yield return m_loadedSubMapHandle;
		m_loadedSubMap3D = ActivateMapScene(m_loadedSubMapHandle.Result.Scene);
		if (activate)
		{
			m_loadedSubMap3D.SetMapActive(enable: true);
		}
	}

	private Map3D ActivateMapScene(Scene scene)
	{
		GameObject[] rootGameObjects = scene.GetRootGameObjects();
		for (int i = 0; i < rootGameObjects.Length; i++)
		{
			Map3D componentInChildren = rootGameObjects[i].GetComponentInChildren<Map3D>(includeInactive: true);
			if (componentInChildren != null)
			{
				componentInChildren.SetRenderTexture(m_renderTexture);
				return componentInChildren;
			}
		}
		return null;
	}

	private void UnloadMap(ref AsyncOperationHandle<SceneInstance> handle, ref Map3D map3d)
	{
		if (handle.IsValid())
		{
			if (map3d != null)
			{
				map3d.SetMapActive(enable: false);
				map3d = null;
			}
			Addressables.UnloadSceneAsync(handle);
			handle.WaitForCompletion();
		}
	}
}
