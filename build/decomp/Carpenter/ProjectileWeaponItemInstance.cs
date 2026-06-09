using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

[Serializable]
public class ProjectileWeaponItemInstance : ItemInstance
{
	[SerializeField]
	private int m_ammoCount;

	[SerializeField]
	private int m_firedCount;

	[SerializeField]
	private string m_loadedAmmoTypeAssetGUID;

	private AmmunitionItemDefinition m_loadedAmmoType;

	[SerializeField]
	private bool m_requiresAction;

	[SerializeReference]
	private List<ItemInstance> m_attachedUpgrades = new List<ItemInstance>();

	public UnityAction OnWeaponAmmoUpdated;

	public int AmmoCount
	{
		get
		{
			return m_ammoCount;
		}
		private set
		{
			if (m_ammoCount != value)
			{
				m_ammoCount = value;
				OnWeaponAmmoUpdated?.Invoke();
			}
		}
	}

	public int FiredCount => m_firedCount;

	public AmmunitionItemDefinition LoadedAmmoType
	{
		get
		{
			if (m_loadedAmmoType == null)
			{
				AssetReference assetReference = new AssetReference(m_loadedAmmoTypeAssetGUID);
				m_loadedAmmoType = AddressablesContentManager.Instance.GetScriptableObjectAsset(assetReference) as AmmunitionItemDefinition;
			}
			return m_loadedAmmoType;
		}
		set
		{
			m_loadedAmmoType = value;
			m_loadedAmmoTypeAssetGUID = m_loadedAmmoType.AssetGUID;
		}
	}

	public bool RequiresAction => m_requiresAction;

	public override Sprite ItemSprite
	{
		get
		{
			foreach (ItemInstance attachedUpgrade in m_attachedUpgrades)
			{
				ProjectileWeaponUpgradeDefinition projectileWeaponUpgradeDefinition = attachedUpgrade.ItemDefinition as ProjectileWeaponUpgradeDefinition;
				if (projectileWeaponUpgradeDefinition.Settings.InventoryItemSpriteChange != null)
				{
					return projectileWeaponUpgradeDefinition.Settings.InventoryItemSpriteChange;
				}
			}
			return base.ItemSprite;
		}
	}

	public IReadOnlyCollection<ItemInstance> AttachedUpgrades => m_attachedUpgrades.AsReadOnly();

	public ProjectileWeaponSettings WeaponSettings => (base.ItemDefinition as ProjectileWeaponItemDefinition).WeaponSettings;

	public ProjectileSettings AmmoProjectileSettings => LoadedAmmoType.AmmoProjectileSettings;

	public void ClearActionRequirement()
	{
		m_requiresAction = false;
	}

	public ProjectileWeaponItemInstance(ProjectileWeaponItemDefinition definition, int stackSize, Vector2Int position, bool rotated)
		: base(definition, 1, position, rotated)
	{
		m_loadedAmmoType = definition.GetDefaultAmmunitionType();
		m_loadedAmmoTypeAssetGUID = m_loadedAmmoType.AssetGUID;
		m_ammoCount = Mathf.Min(stackSize, GetMaxAmmoCapacity());
	}

	public void ResetFiredCount()
	{
		m_firedCount = 0;
	}

	public bool CanFire()
	{
		if (AmmoCount > 0)
		{
			return !m_requiresAction;
		}
		return false;
	}

	public void Fire()
	{
		AmmoCount--;
		m_firedCount++;
		if (WeaponSettings.UseAction)
		{
			m_requiresAction = true;
		}
	}

	public void AddAmmo(int amount)
	{
		AmmoCount += amount;
	}

	public int UnloadAmmo()
	{
		int ammoCount = m_ammoCount;
		AmmoCount = 0;
		return ammoCount;
	}

	public int GetAutoReloadAmount()
	{
		if (WeaponSettings.AutoReloadAmount == -1)
		{
			return GetMaxAmmoCapacity();
		}
		return WeaponSettings.AutoReloadAmount;
	}

	public int GetEmptyAmmoCapacity()
	{
		return GetMaxAmmoCapacity() - AmmoCount;
	}

	public int GetMaxAmmoCapacity()
	{
		int num = WeaponSettings.MaxAmmo;
		foreach (ItemInstance attachedUpgrade in m_attachedUpgrades)
		{
			ProjectileWeaponUpgradeDefinition projectileWeaponUpgradeDefinition = attachedUpgrade.ItemDefinition as ProjectileWeaponUpgradeDefinition;
			num += projectileWeaponUpgradeDefinition.Settings.MagazineSizeIncreased;
		}
		return num;
	}

	public float GetRefireTime()
	{
		float num = WeaponSettings.RefireTime;
		foreach (ItemInstance attachedUpgrade in m_attachedUpgrades)
		{
			ProjectileWeaponUpgradeDefinition projectileWeaponUpgradeDefinition = attachedUpgrade.ItemDefinition as ProjectileWeaponUpgradeDefinition;
			num += projectileWeaponUpgradeDefinition.Settings.RefireTimeAdjustment;
		}
		return num;
	}

	public void SwitchAmmoType(AmmunitionItemDefinition newAmmoType, int availableAmount)
	{
		LoadedAmmoType = newAmmoType;
		AmmoCount = Mathf.Min(availableAmount, GetMaxAmmoCapacity());
		OnWeaponAmmoUpdated?.Invoke();
	}

	public bool AttachUpgrade(ItemInstance newUpgrade)
	{
		if (!(newUpgrade.ItemDefinition is ProjectileWeaponUpgradeDefinition))
		{
			Debug.LogError("Tried to upgrade a weapon with an item that is not an upgrade! " + newUpgrade.ItemDefinition.ItemName);
			return false;
		}
		ProjectileWeaponUpgradeDefinition projectileWeaponUpgradeDefinition = newUpgrade.ItemDefinition as ProjectileWeaponUpgradeDefinition;
		if (!projectileWeaponUpgradeDefinition.IsCompatibleWith(base.ItemDefinition as ProjectileWeaponItemDefinition))
		{
			Debug.LogError("Tried to upgrade a weapon with an invalid upgrade item " + newUpgrade.ItemDefinition.ItemName + " for item type " + base.ItemDefinition.ItemName);
			return false;
		}
		if (!projectileWeaponUpgradeDefinition.CanHaveMultiple)
		{
			foreach (ItemInstance attachedUpgrade in m_attachedUpgrades)
			{
				if (attachedUpgrade.ItemDefinition == newUpgrade.ItemDefinition)
				{
					Debug.LogError("Tried to upgrade a weapon with an upgrade " + attachedUpgrade.ItemDefinition.ItemName + " that is already applied to weapon " + base.ItemDefinition.ItemName);
					return false;
				}
			}
		}
		m_attachedUpgrades.Add(newUpgrade);
		return true;
	}

	public bool HasUpgradeTag(string tag)
	{
		foreach (ItemInstance attachedUpgrade in m_attachedUpgrades)
		{
			if ((attachedUpgrade.ItemDefinition as ProjectileWeaponUpgradeDefinition).Settings.Tag.Equals(tag))
			{
				return true;
			}
		}
		return false;
	}

	public override int GetDifficultyResourceValue(GameDifficultyResourceScoreType type)
	{
		int num = base.GetDifficultyResourceValue(type);
		if (m_loadedAmmoType != null)
		{
			num += LoadedAmmoType.GetDifficultyResourceValue(type) * m_ammoCount;
		}
		return num;
	}

	public AssetReference GetReloadActionViewAsset()
	{
		foreach (ItemInstance attachedUpgrade in m_attachedUpgrades)
		{
			ProjectileWeaponUpgradeDefinition projectileWeaponUpgradeDefinition = attachedUpgrade.ItemDefinition as ProjectileWeaponUpgradeDefinition;
			if (projectileWeaponUpgradeDefinition != null && projectileWeaponUpgradeDefinition.Settings.ReloadActionViewUpgrade.HasAsset())
			{
				return projectileWeaponUpgradeDefinition.Settings.ReloadActionViewUpgrade;
			}
		}
		return WeaponSettings.ReloadActionViewAsset;
	}
}
