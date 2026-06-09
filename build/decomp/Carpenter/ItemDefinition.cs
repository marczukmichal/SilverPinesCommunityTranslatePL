using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "NewItem", menuName = "Items/Item Definition")]
public class ItemDefinition : AddressableScriptableObject<ItemDefinition>
{
	public enum UseItemResult
	{
		Consume,
		Reusable,
		ChangeItemDefinition,
		ReplaceWithItem
	}

	[Serializable]
	public struct ActivatableSettings
	{
		[SerializeField]
		private Sprite m_activatedSprite;

		[SerializeField]
		private LocalizedString m_activateActionText;

		[SerializeField]
		private LocalizedString m_deactivateActionText;

		[SerializeField]
		private LocalizedString m_activatedItemCellText;

		[SerializeField]
		private LocalizedString m_deactivatedItemCellText;

		public Sprite ActivatedSprite => m_activatedSprite;

		public string ActivateActionText => m_activateActionText.GetLocalizedString();

		public LocalizedString ActivateActionStringReference => m_activateActionText;

		public string DeactivateActionText => m_deactivateActionText.GetLocalizedString();

		public LocalizedString DeactivateActionStringReference => m_deactivateActionText;

		public string ActivatedItemCellText => m_activatedItemCellText.GetLocalizedString();

		public string DeactivatedItemCellText => m_deactivatedItemCellText.GetLocalizedString();
	}

	[Serializable]
	public struct AudioSettings
	{
		[SerializeField]
		private AudioEvent m_onAddedToInventory;

		[SerializeField]
		private AudioEvent m_onEquip;

		[SerializeField]
		private AudioEvent m_onUnEquip;

		[SerializeField]
		private AudioEvent m_onStartMoving;

		[SerializeField]
		private AudioEvent m_onPlaced;

		public AudioEvent OnAddedToInventory => m_onAddedToInventory;

		public AudioEvent OnEquip => m_onEquip;

		public AudioEvent OnUnEquip => m_onUnEquip;

		public AudioEvent OnStartMoving => m_onStartMoving;

		public AudioEvent OnPlaced => m_onPlaced;
	}

	[Serializable]
	public struct DifficultyResourceValuesSettings
	{
		[SerializeField]
		private int m_health;

		[SerializeField]
		private int m_ammo;

		[SerializeField]
		private int m_melee;

		public int Health => m_health;

		public int Ammo => m_ammo;

		public int Melee => m_melee;
	}

	[SerializeField]
	private LocalizedString m_itemNameStringReference;

	[SerializeField]
	private LocalizedString m_itemDescriptionStringReference;

	[SerializeField]
	private Sprite m_inventorySprite;

	[SerializeField]
	private Sprite m_useHeldSprite;

	[SerializeField]
	private List<ItemCondition> m_useConditions;

	[SerializeField]
	private List<ItemEffect> m_useEffects;

	[SerializeField]
	private UseItemResult m_useItemPostResult;

	[SerializeField]
	private ItemDefinition m_useResulttemDefinition;

	[SerializeField]
	private string m_useFSMEvent;

	[SerializeField]
	private List<ItemCondition> m_discardConditions;

	[SerializeField]
	private List<ItemCombinationRule> m_itemCombinations;

	[SerializeField]
	private GameObject m_combinedNewItemAnimation;

	[SerializeField]
	private bool m_canBeActivated;

	[SerializeField]
	private ActivatableSettings m_activatableSettings;

	[SerializeField]
	private bool m_isKey;

	[SerializeField]
	private Vector2Int m_itemSize = new Vector2Int(1, 1);

	[SerializeField]
	private int m_maxStackSize = 1;

	[SerializeField]
	private AudioSettings m_audioSettings;

	[SerializeField]
	private float m_baseShopPrice;

	[SerializeField]
	private int m_shopStackSize = 1;

	[SerializeField]
	private DifficultyResourceValuesSettings m_difficultyResourceValues;

	[SerializeField]
	private bool m_isImportantItem = true;

	[SerializeField]
	private AssetReferenceGameObject m_droppedItemPrefab;

	[SerializeField]
	private float m_inventoryItemCellSpriteSizeScalar = 1f;

	[Tooltip("Size of this item will be scaled (after taking base value from dropped item prefab if available) by this value")]
	[SerializeField]
	private float m_vendingMachineSizeScalar = 1f;

	[SerializeField]
	private bool m_hideFromQuickItem;

	public LocalizedString ItemNameLocString => m_itemNameStringReference;

	public string ItemName => LocalizationUtility.GetLocalizedString(m_itemNameStringReference, base.name);

	public virtual string Description
	{
		get
		{
			if (m_itemDescriptionStringReference.IsEmpty)
			{
				return "MISSING DESCRIPTION";
			}
			return m_itemDescriptionStringReference.GetLocalizedString();
		}
	}

	public Sprite InventorySprite => m_inventorySprite;

	public Sprite UseHeldSprite => m_useHeldSprite;

	public List<ItemEffect> UseEffects => m_useEffects;

	public UseItemResult UseItemPostResult => m_useItemPostResult;

	public ItemDefinition UseResultChangeItemDefinition => m_useResulttemDefinition;

	public string UseFSMEvent => m_useFSMEvent;

	public bool UsingItemHasState => !string.IsNullOrEmpty(UseFSMEvent);

	public GameObject CombinedNewItemAnimationElement => m_combinedNewItemAnimation;

	public ActivatableSettings Activatable => m_activatableSettings;

	public bool IsKey => m_isKey;

	public Vector2Int ItemSize => m_itemSize;

	public int MaxStackSize => m_maxStackSize;

	public AudioSettings Audio => m_audioSettings;

	public float BaseShopPrice => m_baseShopPrice;

	public int ShopStackSize => m_shopStackSize;

	public DifficultyResourceValuesSettings DifficultyResourceValues => m_difficultyResourceValues;

	public bool IsImportantItem
	{
		get
		{
			if (!m_isImportantItem)
			{
				return m_isKey;
			}
			return true;
		}
	}

	public bool IsDroppable
	{
		get
		{
			if (!IsImportantItem)
			{
				if (m_droppedItemPrefab != null)
				{
					return m_droppedItemPrefab.HasAsset();
				}
				return false;
			}
			return false;
		}
	}

	public AssetReferenceGameObject DroppedItemPrefab => m_droppedItemPrefab;

	public float InventoryItemCellSpriteSizeScalar => m_inventoryItemCellSpriteSizeScalar;

	public float VendingMachineSizeScalar => m_vendingMachineSizeScalar;

	public bool HideFromQuickItem => m_hideFromQuickItem;

	public virtual bool HideUseButton()
	{
		return false;
	}

	public bool CanUse(Inventory inventory, bool checkConditions)
	{
		if (m_useEffects.Count == 0 && string.IsNullOrEmpty(m_useFSMEvent))
		{
			return false;
		}
		bool result = true;
		if (checkConditions)
		{
			foreach (ItemCondition useCondition in m_useConditions)
			{
				if (!useCondition.Check(this, inventory))
				{
					return false;
				}
			}
			return result;
		}
		return result;
	}

	public virtual bool CanEquip(Inventory inventory)
	{
		return false;
	}

	public bool CanDiscard(Inventory inventory)
	{
		if (m_discardConditions.Count == 0)
		{
			return false;
		}
		bool result = true;
		foreach (ItemCondition discardCondition in m_discardConditions)
		{
			if (!discardCondition.Check(this, inventory))
			{
				return false;
			}
		}
		return result;
	}

	public virtual bool CanBeCombinedWithAnything()
	{
		if (m_itemCombinations.Count <= 0)
		{
			return m_isKey;
		}
		return true;
	}

	public static ItemCombinationRule GetCombinationResult(ItemDefinition itemA, ItemDefinition itemB, out bool consumeItemA, out bool consumeItemB)
	{
		consumeItemA = true;
		consumeItemB = true;
		foreach (ItemCombinationRule itemCombination in itemA.m_itemCombinations)
		{
			bool flag = itemCombination.CombineWith == itemB;
			if (itemCombination.Type == ItemCombinationRule.CombineType.RepairMelee && itemB is MeleeWeaponItemDefinition)
			{
				flag = true;
			}
			if (flag)
			{
				if (itemCombination.SourceResult == ItemCombinationRule.CombineSourceResult.Reusable)
				{
					consumeItemA = false;
				}
				if (itemCombination.Type != 0 && itemCombination.Type != ItemCombinationRule.CombineType.ProduceArtifact)
				{
					consumeItemB = false;
				}
				if (itemCombination.Type == ItemCombinationRule.CombineType.RepairMelee)
				{
					consumeItemA = false;
				}
				return itemCombination;
			}
		}
		foreach (ItemCombinationRule itemCombination2 in itemB.m_itemCombinations)
		{
			bool flag2 = itemCombination2.CombineWith == itemA;
			if (itemCombination2.Type == ItemCombinationRule.CombineType.RepairMelee && itemA is MeleeWeaponItemDefinition)
			{
				flag2 = true;
			}
			if (flag2)
			{
				if (itemCombination2.SourceResult == ItemCombinationRule.CombineSourceResult.Reusable)
				{
					consumeItemB = false;
				}
				if (itemCombination2.Type != 0 && itemCombination2.Type != ItemCombinationRule.CombineType.ProduceArtifact)
				{
					consumeItemA = false;
				}
				if (itemCombination2.Type == ItemCombinationRule.CombineType.RepairMelee)
				{
					consumeItemB = false;
				}
				return itemCombination2;
			}
		}
		return null;
	}

	public static bool CanBeCombined(ItemDefinition itemA, ItemDefinition itemB)
	{
		bool consumeItemA;
		bool consumeItemB;
		return GetCombinationResult(itemA, itemB, out consumeItemA, out consumeItemB) != null;
	}

	public virtual bool CanBeCombinedWith(ItemDefinition other)
	{
		if (other == this && m_maxStackSize > 1)
		{
			return true;
		}
		if ((other is KeyRingItemDefinition && m_isKey) || (this is KeyRingItemDefinition && other.m_isKey))
		{
			return true;
		}
		return CanBeCombined(this, other);
	}

	public bool CanBeActivated()
	{
		return m_canBeActivated;
	}

	public virtual bool CanHaveShortcutSet(Inventory inventory)
	{
		if (!CanBeActivated() && !CanEquip(inventory))
		{
			return CanUse(inventory, checkConditions: false);
		}
		return true;
	}

	public virtual bool HasSpecialAction()
	{
		return false;
	}

	public virtual LocalizedString GetSpecialActionText()
	{
		return null;
	}

	public int GetDifficultyResourceValue(GameDifficultyResourceScoreType type)
	{
		float f = 0f;
		switch (type)
		{
		case GameDifficultyResourceScoreType.Health:
			f = DifficultyResourceValues.Health;
			break;
		case GameDifficultyResourceScoreType.Ammo:
			f = DifficultyResourceValues.Ammo;
			break;
		case GameDifficultyResourceScoreType.Melee:
			f = DifficultyResourceValues.Melee;
			break;
		case GameDifficultyResourceScoreType.NetWorth:
			f = BaseShopPrice;
			break;
		}
		return Mathf.RoundToInt(f);
	}
}
