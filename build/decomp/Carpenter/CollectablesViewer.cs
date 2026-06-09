using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CollectablesViewer : MonoBehaviour
{
	[SerializeField]
	private CollectableInventory m_collectablesInventory;

	[SerializeField]
	private GameCollectableSettings m_gameCollectableSettings;

	[SerializeField]
	private TextMeshProUGUI m_regionText;

	[SerializeField]
	private AudioEvent m_pageTurnAudioEvent;

	[SerializeField]
	private CollectablesInsertSlot[] m_slots;

	[SerializeField]
	private RectTransform m_carriedParent;

	[SerializeField]
	private TextMeshProUGUI m_artifactSlotCount;

	[SerializeField]
	private TextMeshProUGUI m_collectablesRequiredForNextSlot;

	private int m_currentPage;

	private Dictionary<AssetReference, AsyncOperationHandle<GameObject>> m_loadedAssetHandles;

	private List<CollectableVisuals> m_carriedCollectablesVisuals;

	private int m_currentlySelectedCarryIndex;

	private void Start()
	{
		for (int i = 0; i < m_slots.Length; i++)
		{
			CollectablesInsertSlot obj = m_slots[i];
			obj.OnClicked = (UnityAction<CollectablesInsertSlot>)Delegate.Combine(obj.OnClicked, new UnityAction<CollectablesInsertSlot>(OnSlotClicked));
		}
		m_currentlySelectedCarryIndex = 0;
		m_loadedAssetHandles = new Dictionary<AssetReference, AsyncOperationHandle<GameObject>>();
		m_carriedCollectablesVisuals = new List<CollectableVisuals>();
		DecorateForNewPage();
		PopulateCarriedCollectables();
		m_artifactSlotCount.text = GlobalReferences.Instance.MainInventory.MaxEquippedArtifactsCount.ToString();
	}

	private void OnEnable()
	{
		GameInputManager.GameInputActions.UI.TabLeft.performed += OnPreviousTabCallback;
		GameInputManager.GameInputActions.UI.TabRight.performed += OnNextTabCallback;
	}

	private void OnDisable()
	{
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.UI.TabLeft.performed -= OnPreviousTabCallback;
			GameInputManager.GameInputActions.UI.TabRight.performed -= OnNextTabCallback;
		}
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
		CollectableVisuals component = UnityEngine.Object.Instantiate(handle.Result, m_carriedParent).GetComponent<CollectableVisuals>();
		component.SetCollectable(collectableInstance.CollectableDefinition);
		component.OnClicked = (UnityAction<CollectableVisuals>)Delegate.Combine(component.OnClicked, new UnityAction<CollectableVisuals>(SelectCarriedCollectableVisuals));
		m_carriedCollectablesVisuals.Add(component);
		UpdateCarriedVisuals();
	}

	private void PopulateCarriedCollectables()
	{
		foreach (CollectableInstance carriedCollectable in m_collectablesInventory.CarriedCollectables)
		{
			StartCoroutine(SpawnCollectableInCarriedList(carriedCollectable));
		}
	}

	private void OnNextTabCallback(InputAction.CallbackContext obj)
	{
		NextPage();
	}

	private void OnPreviousTabCallback(InputAction.CallbackContext obj)
	{
		PreviousPage();
	}

	private void OnDestroy()
	{
		foreach (KeyValuePair<AssetReference, AsyncOperationHandle<GameObject>> loadedAssetHandle in m_loadedAssetHandles)
		{
			Addressables.Release(loadedAssetHandle.Value);
		}
	}

	public void NextPage()
	{
		m_currentPage++;
		if (m_currentPage >= m_gameCollectableSettings.Regions.Count)
		{
			m_currentPage = 0;
		}
		DecorateForNewPage();
		m_pageTurnAudioEvent.Play2D();
	}

	public void PreviousPage()
	{
		m_currentPage--;
		if (m_currentPage < 0)
		{
			m_currentPage = m_gameCollectableSettings.Regions.Count - 1;
		}
		DecorateForNewPage();
		m_pageTurnAudioEvent.Play2D();
	}

	private void DecorateForNewPage()
	{
		GameCollectableSettings.Region region = m_gameCollectableSettings.Regions[m_currentPage];
		m_regionText.text = region.RegionSettings.DisplayName;
		for (int i = 0; i < m_slots.Length; i++)
		{
			if (i >= region.Collectables.Length)
			{
				m_slots[i].gameObject.SetActive(value: false);
				continue;
			}
			bool collected = m_collectablesInventory.HasDelivered(region.Collectables[i]);
			m_slots[i].gameObject.SetActive(value: true);
			m_slots[i].SetCollectable(region.Collectables[i], collected);
		}
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
		UpdateRequiredCount();
	}

	private void UpdateRequiredCount()
	{
		int deliveredCollectableCount = GlobalReferences.Instance.CollectableInventory.DeliveredCollectableCount;
		int nextUnlockThreshold = m_gameCollectableSettings.GetNextUnlockThreshold();
		if (deliveredCollectableCount >= nextUnlockThreshold)
		{
			m_collectablesRequiredForNextSlot.text = "-";
		}
		else
		{
			m_collectablesRequiredForNextSlot.text = (nextUnlockThreshold - deliveredCollectableCount).ToString();
		}
	}

	private void SelectCarriedCollectableVisuals(CollectableVisuals collectableVisuals)
	{
		m_currentlySelectedCarryIndex = m_carriedCollectablesVisuals.IndexOf(collectableVisuals);
		UpdateCarriedVisuals();
	}

	private void OnSlotClicked(CollectablesInsertSlot slot)
	{
		if (m_carriedCollectablesVisuals.Count != 0 && slot.Collectable == m_carriedCollectablesVisuals[m_currentlySelectedCarryIndex].CollectableDefinition)
		{
			GlobalReferences.Instance.EventChannels.Collectables.DeliverCollectableDefinition.Raise(slot.Collectable);
			slot.SetCollectable(slot.Collectable, collected: true);
			CollectableVisuals collectableVisuals = m_carriedCollectablesVisuals[m_currentlySelectedCarryIndex];
			m_carriedCollectablesVisuals.Remove(collectableVisuals);
			UnityEngine.Object.Destroy(collectableVisuals.gameObject);
			m_currentlySelectedCarryIndex = 0;
			UpdateCarriedVisuals();
		}
	}
}
