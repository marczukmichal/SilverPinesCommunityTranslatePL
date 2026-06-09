using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ArtifactsCollectablesQuickViewer : MonoBehaviour
{
	[SerializeField]
	private CollectableInventory m_collectablesInventory;

	[SerializeField]
	private GameCollectableSettings m_gameCollectableSettings;

	[SerializeField]
	private RectTransform m_collectablesParent;

	private Dictionary<AssetReference, AsyncOperationHandle<GameObject>> m_loadedAssetHandles;

	private List<CollectableVisuals> m_carriedCollectablesVisuals;

	private int m_currentlySelectedCarryIndex;

	private void Awake()
	{
		m_currentlySelectedCarryIndex = 0;
		m_carriedCollectablesVisuals = new List<CollectableVisuals>();
		m_loadedAssetHandles = new Dictionary<AssetReference, AsyncOperationHandle<GameObject>>();
	}

	private void PopulateCarriedCollectables()
	{
		DestroyExisting();
		foreach (CollectableInstance carriedCollectable in m_collectablesInventory.CarriedCollectables)
		{
			StartCoroutine(SpawnCollectableInCarriedList(carriedCollectable));
		}
	}

	private void OnEnable()
	{
		m_currentlySelectedCarryIndex = 0;
		PopulateCarriedCollectables();
	}

	private void OnDisable()
	{
	}

	private void OnDestroy()
	{
		foreach (KeyValuePair<AssetReference, AsyncOperationHandle<GameObject>> loadedAssetHandle in m_loadedAssetHandles)
		{
			Addressables.Release(loadedAssetHandle.Value);
		}
	}

	private void DestroyExisting()
	{
		foreach (CollectableVisuals carriedCollectablesVisual in m_carriedCollectablesVisuals)
		{
			UnityEngine.Object.Destroy(carriedCollectablesVisual.gameObject);
		}
		m_carriedCollectablesVisuals.Clear();
	}

	private IEnumerator SpawnCollectableInCarriedList(CollectableInstance collectableInstance)
	{
		AsyncOperationHandle<GameObject> handle;
		if (m_loadedAssetHandles.ContainsKey(collectableInstance.CollectableDefinition.CollectableVisualsTemplate))
		{
			handle = m_loadedAssetHandles[collectableInstance.CollectableDefinition.CollectableVisualsTemplate];
		}
		else
		{
			handle = Addressables.LoadAssetAsync<GameObject>(collectableInstance.CollectableDefinition.CollectableVisualsTemplate);
			m_loadedAssetHandles.Add(collectableInstance.CollectableDefinition.CollectableVisualsTemplate, handle);
		}
		yield return handle;
		CollectableVisuals component = UnityEngine.Object.Instantiate(handle.Result, m_collectablesParent).GetComponent<CollectableVisuals>();
		component.SetCollectable(collectableInstance.CollectableDefinition);
		component.OnClicked = (UnityAction<CollectableVisuals>)Delegate.Combine(component.OnClicked, new UnityAction<CollectableVisuals>(SelectCarriedCollectableVisuals));
		m_carriedCollectablesVisuals.Add(component);
		UpdateCarriedVisuals();
	}

	private void UpdateCarriedVisuals()
	{
		for (int i = 0; i < m_carriedCollectablesVisuals.Count; i++)
		{
			int index = (m_currentlySelectedCarryIndex + i) % m_carriedCollectablesVisuals.Count;
			m_carriedCollectablesVisuals[index].transform.SetSiblingIndex(m_carriedCollectablesVisuals.Count - 1 - i);
			m_carriedCollectablesVisuals[index].SetFaded(i != 0);
			bool flag = i < 5;
			m_carriedCollectablesVisuals[index].gameObject.SetActive(flag);
			if (flag)
			{
				float num = 2f;
				num -= 0.2f * (float)i;
				m_carriedCollectablesVisuals[index].transform.localScale = num * Vector3.one;
			}
		}
	}

	private void SelectCarriedCollectableVisuals(CollectableVisuals collectableVisuals)
	{
		m_currentlySelectedCarryIndex = m_carriedCollectablesVisuals.IndexOf(collectableVisuals);
		UpdateCarriedVisuals();
	}
}
