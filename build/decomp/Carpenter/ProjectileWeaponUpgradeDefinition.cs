using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Items/Projectile Weapon Upgrade Definition")]
public class ProjectileWeaponUpgradeDefinition : ItemDefinition
{
	[Serializable]
	public struct UpgradeSettings
	{
		[SerializeField]
		private int m_magazineSizeIncrease;

		[SerializeField]
		private string m_tag;

		[SerializeField]
		private bool m_enableZoom;

		[SerializeField]
		private float m_zoomScale;

		[SerializeField]
		private float m_refireTimeAdjustment;

		[SerializeField]
		private bool m_changeItemSize;

		[SerializeField]
		private Vector2Int m_newItemSize;

		[SerializeField]
		private Sprite m_inventoryItemSpritechanged;

		[SerializeField]
		private AssetReference m_reloadActionViewUpgrade;

		public int MagazineSizeIncreased => m_magazineSizeIncrease;

		public string Tag => m_tag;

		public bool Zoom => m_enableZoom;

		public float ZoomScale => m_zoomScale;

		public float RefireTimeAdjustment => m_refireTimeAdjustment;

		public bool ChangeItemSisze => m_changeItemSize;

		public Vector2Int ChangedItemSize => m_newItemSize;

		public Sprite InventoryItemSpriteChange => m_inventoryItemSpritechanged;

		public AssetReference ReloadActionViewUpgrade => m_reloadActionViewUpgrade;
	}

	[SerializeField]
	private ProjectileWeaponItemDefinition[] m_compatibleWeapons;

	[SerializeField]
	private bool m_canHaveMultiple;

	[SerializeField]
	private UpgradeSettings m_upgradeSettings;

	public bool CanHaveMultiple => m_canHaveMultiple;

	public UpgradeSettings Settings => m_upgradeSettings;

	public bool IsCompatibleWith(ProjectileWeaponItemDefinition weaponDefinition)
	{
		ProjectileWeaponItemDefinition[] compatibleWeapons = m_compatibleWeapons;
		for (int i = 0; i < compatibleWeapons.Length; i++)
		{
			if (compatibleWeapons[i] == weaponDefinition)
			{
				return true;
			}
		}
		return false;
	}

	public override bool CanBeCombinedWith(ItemDefinition other)
	{
		if (other is ProjectileWeaponItemDefinition weaponDefinition && IsCompatibleWith(weaponDefinition))
		{
			return true;
		}
		return base.CanBeCombinedWith(other);
	}

	public override bool CanBeCombinedWithAnything()
	{
		return true;
	}
}
