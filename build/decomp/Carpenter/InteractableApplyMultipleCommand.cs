using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[Serializable]
public class InteractableApplyMultipleCommand : BaseInteractableCommand, IApplyItem, IPersistentComponent
{
	[Serializable]
	public struct ApplyItemDefinition
	{
		[Header("Item Definition")]
		public ItemDefinition m_itemDefinition;

		public bool m_consumeItem;

		[Header("Events")]
		public UnityEvent m_onApplied;

		[FormerlySerializedAs("m_onFistAppliedOnly")]
		public UnityEvent m_onFirstAppliedOnly;

		public UnityEvent m_onReloadOnly;
	}

	public enum CompletionMode
	{
		Never,
		CompleteOnAllAdded,
		CompleteOnAnyAdded
	}

	[Serializable]
	private class PersistentData
	{
		public List<int> m_completedIndices;
	}

	[SerializeField]
	private ApplyItemDefinition[] m_items;

	[FormerlySerializedAs("m_applyItemInteractUIPrefab")]
	[SerializeField]
	private GameObject m_interactUIPrefab;

	[SerializeField]
	private CompletionMode m_completionMode;

	private bool m_menuClosed;

	private ItemInstance m_usedItem;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public GameObject GetApplyItemInteractUIPrefab()
	{
		return m_interactUIPrefab;
	}

	public override bool RequiresPersistentData()
	{
		return true;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData != null)
		{
			foreach (int completedIndex in m_persistentData.m_completedIndices)
			{
				m_items[completedIndex].m_onApplied.Invoke();
				m_items[completedIndex].m_onReloadOnly.Invoke();
			}
			return;
		}
		m_persistentData = new PersistentData();
		m_persistentData.m_completedIndices = new List<int>();
		m_persistentDataObject.Data = m_persistentData;
	}

	private bool IsComplete()
	{
		switch (m_completionMode)
		{
		case CompletionMode.Never:
			return false;
		case CompletionMode.CompleteOnAllAdded:
		{
			bool result2 = true;
			if (m_persistentData != null)
			{
				for (int j = 0; j < m_items.Length; j++)
				{
					if (!m_persistentData.m_completedIndices.Contains(j))
					{
						result2 = false;
						break;
					}
				}
			}
			return result2;
		}
		case CompletionMode.CompleteOnAnyAdded:
		{
			bool result = false;
			if (m_persistentData != null)
			{
				for (int i = 0; i < m_items.Length; i++)
				{
					if (m_persistentData.m_completedIndices.Contains(i))
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}
		default:
			return false;
		}
	}

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		if (m_persistentData == null)
		{
			Debug.LogError("InteractableApplyMultipleCommand missing PersistentData");
		}
		if (IsComplete())
		{
			yield break;
		}
		m_menuClosed = false;
		m_usedItem = null;
		GlobalReferences.Instance.Anchors.Inventory.ApplyItemInteractableAnchor.Set(this);
		GlobalReferences.Instance.EventChannels.Inventory.ApplyItemToInteractable.Register(OnTryApplyItem);
		GlobalReferences.Instance.EventChannels.InGameMenu.ShowInventoryMenuTab.Raise();
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Combine(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
		yield return new WaitUntil(() => m_menuClosed || m_usedItem != null);
		if (m_usedItem == null)
		{
			result.m_cancel = true;
		}
		else
		{
			if (m_interactUIPrefab != null)
			{
				yield return new WaitForSecondsRealtime(1f);
			}
			for (int i = 0; i < m_items.Length; i++)
			{
				if ((m_persistentData == null || !m_persistentData.m_completedIndices.Contains(i)) && m_items[i].m_itemDefinition == m_usedItem.ItemDefinition)
				{
					m_items[i].m_onApplied.Invoke();
					m_items[i].m_onFirstAppliedOnly.Invoke();
					if (m_items[i].m_consumeItem)
					{
						GlobalReferences.Instance.EventChannels.Inventory.RequestRemoveItem.Raise(new RemoveItemInstanceEventData
						{
							m_itemInstance = m_usedItem,
							m_itemAmount = 1
						});
					}
					if (m_persistentData != null)
					{
						m_persistentData.m_completedIndices.Add(i);
					}
					break;
				}
			}
			switch (m_completionMode)
			{
			case CompletionMode.Never:
				result.m_cancel = true;
				break;
			case CompletionMode.CompleteOnAllAdded:
				if (!IsComplete())
				{
					result.m_cancel = true;
				}
				break;
			}
		}
		OnExit();
	}

	private void OnExit()
	{
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Remove(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
		GlobalReferences.Instance.EventChannels.Inventory.ApplyItemToInteractable.Unregister(OnTryApplyItem);
		GlobalReferences.Instance.Anchors.Inventory.ApplyItemInteractableAnchor.Set(null);
		GlobalReferences.Instance.EventChannels.Inventory.ShowApplyItemInteract.Raise(value: false);
		GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.InGameMenu);
	}

	public override void Cancel()
	{
		base.Cancel();
		OnExit();
	}

	private void OnGameMenuChanged(GameMenuState.GameMenu menu)
	{
		if (!menu.HasFlag(GameMenuState.GameMenu.InGameMenu))
		{
			m_menuClosed = true;
		}
	}

	private void OnTryApplyItem(ItemInstance itemInstance)
	{
		TryApplyItemEventData tryApplyItemEventData = new TryApplyItemEventData
		{
			m_item = itemInstance,
			m_isKey = itemInstance.ItemDefinition.IsKey,
			m_success = false
		};
		if (CanApplyItem(itemInstance))
		{
			m_usedItem = itemInstance;
			tryApplyItemEventData.m_success = true;
		}
		else
		{
			Debug.LogWarning("Tried to apply an invalid item to an interact");
		}
		GlobalReferences.Instance.EventChannels.Generic.TryApplyItem.Raise(tryApplyItemEventData);
	}

	public bool CanApplyItem(ItemInstance itemInstance)
	{
		if (itemInstance != null)
		{
			for (int i = 0; i < m_items.Length; i++)
			{
				if (m_items[i].m_itemDefinition == itemInstance.ItemDefinition)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void Reset()
	{
		base.Reset();
		if (m_persistentData != null)
		{
			m_persistentData.m_completedIndices = new List<int>();
		}
		m_usedItem = null;
	}
}
