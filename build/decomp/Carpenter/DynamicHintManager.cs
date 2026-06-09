using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DynamicHintManager : MonoBehaviour
{
	[Header("Hints")]
	[SerializeField]
	private HintInfo m_attackHint;

	[SerializeField]
	private HintInfo m_aimHint;

	[SerializeField]
	private HintInfo m_inventoryHint;

	[SerializeField]
	private HintInfo m_shootHint;

	[SerializeField]
	private HintInfo m_throwHint;

	[SerializeField]
	private HintInfo m_blockHint;

	[SerializeField]
	private HintInfo m_reloadHint;

	[SerializeField]
	private HintInfo m_quickHealHint;

	[SerializeField]
	private HintInfo m_quickItemUse;

	[Header("Pause Hints")]
	[SerializeField]
	private PauseHintInfo m_attackMeleePauseHint;

	[SerializeField]
	private PauseHintInfo m_weaponReloadPauseHint;

	[SerializeField]
	private PauseHintInfo m_meleeWeaponDamagedHint;

	[SerializeField]
	private PauseHintInfo m_lorePauseHint;

	[SerializeField]
	private ItemDefinition m_healingItemDefinition;

	[SerializeField]
	private ItemDefinition m_bottleItemDefinition;

	[SerializeField]
	private PauseHintInfo m_combineHealingItemsPauseHint;

	[SerializeField]
	private PauseHintInfo m_quickItemPauseHint;

	[SerializeField]
	private PauseHintInfo m_camereaPauseHint;

	[Header("Menu Hints")]
	[SerializeField]
	private MenuHintInfo m_firstArtifactMenuHint;

	[SerializeField]
	private MenuHintInfo m_poisonedMenuHint;

	[SerializeField]
	private MenuHintInfo m_noArtifactSlotsMenuHint;

	[SerializeField]
	private MenuHintInfo m_repairMenuHint;

	[SerializeField]
	private MenuHintInfo m_combineHealingItemsMenuHint;

	[SerializeField]
	private MenuHintInfo m_useStorageMenuHint;

	private HashSet<DynamicHintEvent> m_seenHintEvents;

	private int m_loreCountSeen;

	private void Start()
	{
		ClearData();
		OnLoadSave(GlobalReferences.Instance.DataStore.Data);
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Register(OnActivePlayerChanged);
		GlobalReferences.Instance.EventChannels.Hints.DynamicHint.Register(OnDynamicHintEvent);
		PlayerMainInventory mainInventory = GlobalReferences.Instance.MainInventory;
		mainInventory.OnItemAddedToInventory = (UnityAction<ItemInstance>)Delegate.Combine(mainInventory.OnItemAddedToInventory, new UnityAction<ItemInstance>(OnItemAddedToInventory));
		GlobalReferences.Instance.EventChannels.SaveLoad.PersistentDataPopulateForSave.Register(OnPrepareSave);
		GlobalReferences.Instance.EventChannels.SaveLoad.PersistentDataOnLoaded.Register(OnLoadSave);
		GlobalReferences.Instance.EventChannels.Generic.ColdStartupSetupScene.Register(ClearData);
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransitionCompleted.Register(OnLevelTransitionComplete);
		GlobalReferences.Instance.EventChannels.Inventory.OnFirstArtifactPickedUp.Register(OnFirstArtifactPickedUp);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Unregister(OnActivePlayerChanged);
		GlobalReferences.Instance.EventChannels.Hints.DynamicHint.Unregister(OnDynamicHintEvent);
		PlayerMainInventory mainInventory = GlobalReferences.Instance.MainInventory;
		mainInventory.OnItemAddedToInventory = (UnityAction<ItemInstance>)Delegate.Remove(mainInventory.OnItemAddedToInventory, new UnityAction<ItemInstance>(OnItemAddedToInventory));
		GlobalReferences.Instance.EventChannels.SaveLoad.PersistentDataPopulateForSave.Unregister(OnPrepareSave);
		GlobalReferences.Instance.EventChannels.SaveLoad.PersistentDataOnLoaded.Unregister(OnLoadSave);
		GlobalReferences.Instance.EventChannels.Generic.ColdStartupSetupScene.Unregister(ClearData);
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransitionCompleted.Unregister(OnLevelTransitionComplete);
		GlobalReferences.Instance.EventChannels.Inventory.OnFirstArtifactPickedUp.Unregister(OnFirstArtifactPickedUp);
	}

	private void ClearData()
	{
		m_loreCountSeen = 0;
		m_seenHintEvents = new HashSet<DynamicHintEvent>();
	}

	private void OnPrepareSave(PersistentData persistentData)
	{
		persistentData.DynamicHintsData = new PersistentDataDynamicHints
		{
			m_loreCountSeen = m_loreCountSeen,
			m_triggeredHintEvents = new List<DynamicHintEvent>(m_seenHintEvents)
		};
	}

	private void OnLoadSave(PersistentData persistentData)
	{
		if (persistentData.DynamicHintsData != null)
		{
			m_seenHintEvents = new HashSet<DynamicHintEvent>(persistentData.DynamicHintsData.m_triggeredHintEvents);
			m_loreCountSeen = persistentData.DynamicHintsData.m_loreCountSeen;
		}
		else
		{
			m_seenHintEvents = new HashSet<DynamicHintEvent>();
			m_loreCountSeen = 0;
		}
	}

	private void OnDynamicHintEvent(DynamicHintEvent hintEvent)
	{
		if (m_seenHintEvents.Contains(hintEvent))
		{
			return;
		}
		bool flag = true;
		switch (hintEvent)
		{
		case DynamicHintEvent.AddItemToInventory:
			GlobalReferences.Instance.EventChannels.Hints.ShowHintInfo.Raise(m_inventoryHint);
			break;
		case DynamicHintEvent.EquippedMeleeWeapon:
			StartCoroutine(DelayedPauseHint(m_attackMeleePauseHint, 1f));
			break;
		case DynamicHintEvent.EquippedRangedWeapon:
			GlobalReferences.Instance.EventChannels.Hints.ShowHintInfo.Raise(m_aimHint);
			break;
		case DynamicHintEvent.EquippedSecondaryWeapon:
			GlobalReferences.Instance.EventChannels.Hints.ShowHintInfo.Raise(m_throwHint);
			break;
		case DynamicHintEvent.StartAiming:
			GlobalReferences.Instance.EventChannels.Hints.ShowHintInfo.Raise(m_shootHint);
			break;
		case DynamicHintEvent.PickedUpAmmoReload:
			StartCoroutine(DelayedPauseHint(m_weaponReloadPauseHint, 1f));
			break;
		case DynamicHintEvent.ShouldReloadHint:
			GlobalReferences.Instance.EventChannels.Hints.ShowHintInfo.Raise(m_reloadHint);
			break;
		case DynamicHintEvent.CanRepairMelee:
			GlobalReferences.Instance.EventChannels.Hints.ShowMenuHintInfo.Raise(m_repairMenuHint);
			break;
		case DynamicHintEvent.CombineHealingItems:
			GlobalReferences.Instance.EventChannels.Hints.ShowMenuHintInfo.Raise(m_combineHealingItemsMenuHint);
			break;
		case DynamicHintEvent.QuickHealCombat:
		case DynamicHintEvent.QuickHealReminder:
			GlobalReferences.Instance.EventChannels.Hints.ShowHintInfo.Raise(m_quickItemUse);
			break;
		case DynamicHintEvent.FirstArtifact:
			GlobalReferences.Instance.EventChannels.Hints.ShowMenuHintInfo.Raise(m_firstArtifactMenuHint);
			break;
		case DynamicHintEvent.Poisoned:
			GlobalReferences.Instance.EventChannels.Hints.ShowMenuHintInfo.Raise(m_poisonedMenuHint);
			break;
		case DynamicHintEvent.NoArtifactSlots:
			GlobalReferences.Instance.EventChannels.Hints.ShowMenuHintInfo.Raise(m_noArtifactSlotsMenuHint);
			break;
		case DynamicHintEvent.UseStorage:
			GlobalReferences.Instance.EventChannels.Hints.ShowMenuHintInfo.Raise(m_useStorageMenuHint);
			break;
		case DynamicHintEvent.QuickItemMain:
			if (GlobalReferences.Instance.MainInventory.GetUsableItemCount() >= 1)
			{
				StartCoroutine(DelayedPauseHint(m_quickItemPauseHint, 1f));
			}
			else
			{
				flag = false;
			}
			break;
		case DynamicHintEvent.QuickItemSecondUsableItem:
			if (GlobalReferences.Instance.MainInventory.GetUsableItemCount() >= 2)
			{
				GlobalReferences.Instance.EventChannels.Hints.ShowHintInfo.Raise(m_quickItemUse);
			}
			else
			{
				flag = false;
			}
			break;
		case DynamicHintEvent.Camera:
			if (!GlobalReferences.Instance.Variables.Mechanics.PV_MapDisabled.Value && GlobalReferences.Instance.Variables.Progression.PV_PickedUpCamera.Value)
			{
				GlobalReferences.Instance.EventChannels.Hints.ShowPauseHintInfo.Raise(m_camereaPauseHint);
			}
			else
			{
				flag = false;
			}
			break;
		}
		if (flag)
		{
			m_seenHintEvents.Add(hintEvent);
		}
	}

	private void OnItemAddedToInventory(ItemInstance item)
	{
		if (GlobalReferences.Instance.MainInventory.GetItemOfType(m_bottleItemDefinition) != null && GlobalReferences.Instance.MainInventory.GetItemOfType(m_healingItemDefinition) != null)
		{
			OnDynamicHintEvent(DynamicHintEvent.CombineHealingItems);
		}
	}

	private IEnumerator DelayedPauseHint(PauseHintInfo pauseHint, float delay)
	{
		yield return new WaitForSeconds(delay);
		GlobalReferences.Instance.EventChannels.Hints.ShowPauseHintInfo.Raise(pauseHint);
	}

	private void OnLevelTransitionComplete()
	{
		StopAllCoroutines();
		StartCoroutine(DelayedLevelChangeCheckCoroutine());
	}

	private bool ShouldRemindAboutHealing()
	{
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (item != null)
		{
			CharacterInventory component = item.GetComponent<CharacterInventory>();
			CharacterHealth component2 = item.GetComponent<CharacterHealth>();
			if (component2 != null && component != null && component2.HealthPercentage <= 0.7f && component.Inventory.HasAnyHealingItem())
			{
				return true;
			}
		}
		return false;
	}

	private IEnumerator DelayedLevelChangeCheckCoroutine()
	{
		yield return new WaitForSeconds(2f);
		if (ShouldRemindAboutHealing())
		{
			OnDynamicHintEvent(DynamicHintEvent.QuickHealReminder);
		}
	}

	private void OnActivePlayerChanged(GameObject player)
	{
		if (player != null)
		{
			CharacterHealth component = player.GetComponent<CharacterHealth>();
			if (component != null)
			{
				component.OnTakenDamage = (UnityAction<int>)Delegate.Combine(component.OnTakenDamage, new UnityAction<int>(OnPlayerTakeDamage));
			}
		}
	}

	private void OnPlayerTakeDamage(int damageAmount)
	{
		if (ShouldRemindAboutHealing())
		{
			OnDynamicHintEvent(DynamicHintEvent.QuickHealCombat);
		}
	}

	private void OnFirstArtifactPickedUp()
	{
		OnDynamicHintEvent(DynamicHintEvent.FirstArtifact);
	}
}
