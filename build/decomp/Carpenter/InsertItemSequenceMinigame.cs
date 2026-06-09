using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.UI;

public class InsertItemSequenceMinigame : MonoBehaviour, IPersistentComponent
{
	[Serializable]
	public struct InsertableItemSettings
	{
		public ItemDefinition m_itemDefinition;

		[Header("UI Elements")]
		[Tooltip("Object to enable when this item can be re-inserted")]
		public GameObject m_reinsertUIObject;

		public UnityEvent m_onInsertItem;

		public void SetReinsertableVisibile(bool visible)
		{
			Graphic component = m_reinsertUIObject.GetComponent<Graphic>();
			Button component2 = m_reinsertUIObject.GetComponent<Button>();
			if (component != null)
			{
				component.DOFade(visible ? 1f : 0f, 0.5f);
			}
			component2.enabled = visible;
		}
	}

	[Serializable]
	private class PersistentData
	{
		public List<AssetReferenceT<ItemDefinition>> m_addedItemDefinitions;
	}

	[SerializeField]
	private InsertableItemSettings[] m_itemSettings;

	[SerializeField]
	private ItemDefinition[] m_targetItemSequence;

	[SerializeField]
	private UnityEvent m_onSequenceCorrect;

	[SerializeField]
	private GameObject[] m_insertCountObjects;

	private List<ItemDefinition> m_activeSequenceItems = new List<ItemDefinition>();

	private List<ItemDefinition> m_availableItems = new List<ItemDefinition>();

	private bool m_isResetting;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	private void Start()
	{
		RefreshStartState();
	}

	private void RefreshStartState()
	{
		InsertableItemSettings[] itemSettings = m_itemSettings;
		for (int i = 0; i < itemSettings.Length; i++)
		{
			InsertableItemSettings insertableItemSettings = itemSettings[i];
			if (insertableItemSettings.m_reinsertUIObject != null)
			{
				bool flag = IsItemReinsertable(insertableItemSettings.m_itemDefinition);
				Graphic component = insertableItemSettings.m_reinsertUIObject.GetComponent<Graphic>();
				Button component2 = insertableItemSettings.m_reinsertUIObject.GetComponent<Button>();
				if (component != null)
				{
					Color color = component.color;
					color.a = (flag ? 1f : 0f);
					component.color = color;
				}
				component2.enabled = flag;
			}
		}
		UpdateInsertCountGameObjects();
	}

	private bool IsItemReinsertable(ItemDefinition item)
	{
		if (m_availableItems.Contains(item))
		{
			return !m_activeSequenceItems.Contains(item);
		}
		return false;
	}

	public void InsertItem(ItemDefinition item)
	{
		if (!m_availableItems.Contains(item))
		{
			m_availableItems.Add(item);
			if (m_persistentData != null)
			{
				m_persistentData.m_addedItemDefinitions.Add(item.AssetReference);
			}
		}
		m_activeSequenceItems.Add(item);
		InsertableItemSettings[] itemSettings = m_itemSettings;
		for (int i = 0; i < itemSettings.Length; i++)
		{
			InsertableItemSettings insertableItemSettings = itemSettings[i];
			if (insertableItemSettings.m_itemDefinition == item)
			{
				insertableItemSettings.m_onInsertItem.Invoke();
			}
		}
		RefreshReinsertableItems();
		ValidateSequence();
		UpdateInsertCountGameObjects();
	}

	private void RefreshReinsertableItems()
	{
		InsertableItemSettings[] itemSettings = m_itemSettings;
		for (int i = 0; i < itemSettings.Length; i++)
		{
			InsertableItemSettings insertableItemSettings = itemSettings[i];
			if (insertableItemSettings.m_reinsertUIObject != null)
			{
				bool reinsertableVisibile = IsItemReinsertable(insertableItemSettings.m_itemDefinition);
				insertableItemSettings.SetReinsertableVisibile(reinsertableVisibile);
			}
		}
	}

	private void UpdateInsertCountGameObjects()
	{
		for (int i = 0; i < m_insertCountObjects.Length; i++)
		{
			m_insertCountObjects[i].SetActive(i < m_activeSequenceItems.Count);
		}
	}

	private void FlashCountGameObjets(bool on)
	{
		for (int i = 0; i < m_insertCountObjects.Length; i++)
		{
			m_insertCountObjects[i].SetActive(on);
		}
	}

	private void ValidateSequence()
	{
		if (m_activeSequenceItems.Count != m_targetItemSequence.Length)
		{
			return;
		}
		bool flag = true;
		for (int i = 0; i < m_targetItemSequence.Length; i++)
		{
			if (m_activeSequenceItems[i] != m_targetItemSequence[i])
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			StartCoroutine(SequenceCorrect());
		}
		else
		{
			StartCoroutine(ResetSequence(1f));
		}
	}

	private IEnumerator SequenceCorrect()
	{
		m_isResetting = true;
		for (int i = 0; i < 6; i++)
		{
			FlashCountGameObjets(on: false);
			yield return new WaitForSeconds(0.1f);
			FlashCountGameObjets(on: true);
			yield return new WaitForSeconds(0.2f);
		}
		yield return new WaitForSeconds(1f);
		m_onSequenceCorrect.Invoke();
	}

	public void EjectCoins()
	{
		if (!m_isResetting && m_activeSequenceItems.Count != 0)
		{
			StartCoroutine(ResetSequence(0f));
		}
	}

	private IEnumerator ResetSequence(float initialDelay)
	{
		m_isResetting = true;
		yield return new WaitForSeconds(initialDelay);
		foreach (ItemDefinition itemDefinition in m_activeSequenceItems)
		{
			InsertableItemSettings[] itemSettings = m_itemSettings;
			for (int i = 0; i < itemSettings.Length; i++)
			{
				InsertableItemSettings insertableItemSettings = itemSettings[i];
				if (insertableItemSettings.m_itemDefinition == itemDefinition && insertableItemSettings.m_reinsertUIObject != null)
				{
					insertableItemSettings.SetReinsertableVisibile(visible: true);
					yield return new WaitForSeconds(0.5f);
				}
			}
		}
		m_activeSequenceItems.Clear();
		UpdateInsertCountGameObjects();
		m_isResetting = false;
	}

	public bool RequiresPersistentData()
	{
		return true;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData != null)
		{
			m_availableItems.Clear();
			foreach (AssetReferenceT<ItemDefinition> addedItemDefinition in m_persistentData.m_addedItemDefinitions)
			{
				m_availableItems.Add(AddressablesContentManager.Instance.GetAsset(addedItemDefinition));
			}
			RefreshStartState();
		}
		else
		{
			m_persistentData = new PersistentData();
			m_persistentDataObject.Data = m_persistentData;
			m_persistentData.m_addedItemDefinitions = new List<AssetReferenceT<ItemDefinition>>();
		}
	}
}
