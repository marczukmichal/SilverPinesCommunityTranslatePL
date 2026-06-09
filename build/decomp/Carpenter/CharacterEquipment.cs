using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CharacterEquipment : MonoBehaviour
{
	[SerializeField]
	private Transform m_wieldedTransformRoot;

	[SerializeField]
	private bool m_enableMeleeWeaponByDefault;

	[SerializeField]
	private DamageCollider m_meleeWeaponDamageCollider;

	[Header("Enemy Melee Settings")]
	[SerializeField]
	private MeleeWeaponItemDefinition m_startWeapon;

	[SerializeField]
	private SecondaryWeaponItemDefinition m_secondaryWeapon;

	[SerializeField]
	private SpriteRenderer m_usingItemSpriteRenderer;

	[SerializeField]
	private Sprite m_coffeeSprite;

	private MeleeWeaponItemInstance m_meleeWeaponItemInstance;

	private SecondaryWeaponItemInstance m_secondaryWeaponItemInstance;

	private MeleeWeapon m_activeWeapon;

	private Projectile m_activeThrownProjectile;

	private CharacterAiming m_aiming;

	private CharacterDirection m_direction;

	private CharacterInventory m_inventory;

	private CharacterStamina m_stamina;

	private bool m_isWeaponBroken;

	private Vector2 m_throwDirection;

	public MeleeWeaponItemDefinition StartMeleeWeaponItemDefinition => m_startWeapon;

	public SecondaryWeaponItemDefinition SecondaryWeaponItemDefinition => m_secondaryWeapon;

	public MeleeWeaponItemInstance EquippedMeleeWeaponItemInstance
	{
		get
		{
			if (m_inventory != null)
			{
				return m_inventory.Inventory.EquippedMeleeItem as MeleeWeaponItemInstance;
			}
			return m_meleeWeaponItemInstance;
		}
	}

	public SecondaryWeaponItemInstance EquippedSecondaryWeaponItemInstance
	{
		get
		{
			if (m_inventory != null)
			{
				if (m_inventory.UsedSecondaryItem != null)
				{
					return m_inventory.UsedSecondaryItem;
				}
				return m_inventory.Inventory.EquippedSecondaryItem;
			}
			return m_secondaryWeaponItemInstance;
		}
	}

	public MeleeWeapon ActiveWeapon => m_activeWeapon;

	public bool HasMeleeWeapon => EquippedMeleeWeaponItemInstance != null;

	public Vector2 ThrowDirection
	{
		get
		{
			return m_throwDirection;
		}
		set
		{
			m_throwDirection = value;
		}
	}

	private void Awake()
	{
		m_aiming = GetComponent<CharacterAiming>();
		m_direction = GetComponent<CharacterDirection>();
		m_inventory = GetComponent<CharacterInventory>();
		m_stamina = GetComponent<CharacterStamina>();
	}

	private void Start()
	{
		if (m_inventory == null)
		{
			if (m_startWeapon != null)
			{
				m_meleeWeaponItemInstance = new MeleeWeaponItemInstance(m_startWeapon, 1, new Vector2Int(-1, -1), rotated: false);
			}
			if (m_secondaryWeapon != null)
			{
				m_secondaryWeaponItemInstance = new SecondaryWeaponItemInstance(m_secondaryWeapon, 1, new Vector2Int(-1, -1), rotated: false);
			}
		}
		if (m_enableMeleeWeaponByDefault && m_startWeapon != null)
		{
			EnableMeleeWeapon();
		}
		if (m_usingItemSpriteRenderer != null)
		{
			m_usingItemSpriteRenderer.gameObject.SetActive(value: false);
		}
	}

	private void UpdateEquippedWeapon(MeleeWeaponItemInstance itemInstance)
	{
		if (m_activeWeapon == null || m_activeWeapon.WeaponItemDefinition != itemInstance.WeaponDefinition || itemInstance.IsBroken() != m_isWeaponBroken)
		{
			DestroyActiveWeapon();
			GameObject gameObject = ((!itemInstance.IsBroken()) ? itemInstance.WeaponDefinition.MeleeWieldedPrefab : itemInstance.WeaponDefinition.MeleeBrokenWieldedPrefab);
			if (gameObject == null)
			{
				Debug.LogError("Weapon " + itemInstance.ItemDefinition.ItemName + " has no prefab defined for broken state (" + itemInstance.IsBroken() + ")");
			}
			else
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, m_wieldedTransformRoot);
				m_activeWeapon = gameObject2.GetComponent<MeleeWeapon>();
				m_activeWeapon.SetItemInstance(itemInstance);
				m_isWeaponBroken = itemInstance.IsBroken();
			}
		}
	}

	private void UpdateEquippedWeapon(SecondaryWeaponItemInstance itemInstance)
	{
		DestroyActiveWeapon();
		DynamicallySpawnObjectEventData eventData = new DynamicallySpawnObjectEventData(itemInstance.WeaponDefinition.ThrownProjectileSettings.ProjectilePrefab, persistent: false, Vector3.zero);
		ref UnityAction<GameObject> onSpawnedAction = ref eventData.m_onSpawnedAction;
		onSpawnedAction = (UnityAction<GameObject>)Delegate.Combine(onSpawnedAction, (UnityAction<GameObject>)delegate(GameObject spawnedObject)
		{
			spawnedObject.transform.SetParent(m_wieldedTransformRoot);
			spawnedObject.transform.SetLocalPositionAndRotation(new Vector3(0f, 0f, 0.1f), Quaternion.identity);
			m_activeThrownProjectile = spawnedObject.GetComponent<Projectile>();
			m_activeThrownProjectile.enabled = false;
			m_activeThrownProjectile.gameObject.SetActive(value: true);
			m_activeThrownProjectile.SetVisualsActive(active: true);
		});
		DynamicallySpawnedObject.Spawn(eventData);
	}

	private void DestroyActiveWeapon()
	{
		if (m_activeWeapon != null)
		{
			UnityEngine.Object.Destroy(m_activeWeapon.gameObject);
			m_activeWeapon = null;
			m_isWeaponBroken = false;
		}
		if (m_activeThrownProjectile != null)
		{
			m_activeThrownProjectile.GetComponent<DynamicallySpawnedObject>().ReleaseSafe();
			m_activeThrownProjectile = null;
		}
	}

	public void EnableMeleeWeapon()
	{
		DisableActiveWeapon();
		MeleeWeaponItemInstance equippedMeleeWeaponItemInstance = EquippedMeleeWeaponItemInstance;
		if (equippedMeleeWeaponItemInstance != null)
		{
			UpdateEquippedWeapon(equippedMeleeWeaponItemInstance);
			m_activeWeapon.gameObject.SetActive(value: true);
			m_activeWeapon.SetVisible(visible: true);
		}
	}

	public void EnableSecondaryWeapon()
	{
		DisableActiveWeapon();
		SecondaryWeaponItemInstance equippedSecondaryWeaponItemInstance = EquippedSecondaryWeaponItemInstance;
		if (equippedSecondaryWeaponItemInstance != null)
		{
			UpdateEquippedWeapon(equippedSecondaryWeaponItemInstance);
		}
	}

	public void DisableActiveWeapon(bool destroy = false)
	{
		if ((bool)m_activeWeapon)
		{
			if (destroy)
			{
				DestroyActiveWeapon();
			}
			else
			{
				m_activeWeapon.gameObject.SetActive(value: false);
			}
		}
		if ((bool)m_activeThrownProjectile)
		{
			m_activeThrownProjectile.GetComponent<DynamicallySpawnedObject>().ReleaseSafe();
			m_activeThrownProjectile = null;
		}
	}

	public void EnableWeaponDamageCollidersQuickAttack()
	{
		EnableWeaponDamageColliders(EquippedMeleeWeaponItemInstance.WeaponDefinition.DamageSettings.m_quickAttackColliderSize, EquippedMeleeWeaponItemInstance.WeaponDefinition.DamageSettings.m_quickAttackColliderOffset);
	}

	public void EnableWeaponDamageCollidersHeavyAttack()
	{
		EnableWeaponDamageColliders(EquippedMeleeWeaponItemInstance.WeaponDefinition.DamageSettings.m_heavyAttackColliderSize, EquippedMeleeWeaponItemInstance.WeaponDefinition.DamageSettings.m_heavyAttackColliderOffset);
	}

	private void EnableWeaponDamageColliders(Vector2 size, Vector2 offset)
	{
		if (m_meleeWeaponDamageCollider != null)
		{
			m_meleeWeaponDamageCollider.SetMeleeWeapon(m_activeWeapon);
			m_meleeWeaponDamageCollider.SetHitSettings(EquippedMeleeWeaponItemInstance.IsBroken() ? EquippedMeleeWeaponItemInstance.WeaponDefinition.DamageSettings.m_brokenHitSettings : EquippedMeleeWeaponItemInstance.WeaponDefinition.DamageSettings.m_normalHitSettings);
			m_meleeWeaponDamageCollider.AllowedDepthDistance = EquippedMeleeWeaponItemInstance.WeaponDefinition.DamageSettings.m_allowedDepthDistance;
			m_meleeWeaponDamageCollider.SetColliderSizeAndOffset(size, offset);
			m_meleeWeaponDamageCollider.SetDamageEnabled(enabled: true);
		}
	}

	public void DisableWeaponDamageColliders()
	{
		if (m_meleeWeaponDamageCollider != null)
		{
			m_meleeWeaponDamageCollider.SetDamageEnabled(enabled: false);
		}
	}

	public void SetMeleeWeaponShown()
	{
		if ((bool)m_activeWeapon)
		{
			m_activeWeapon.SetVisible(visible: true);
			SetMeleePositionBehind();
		}
	}

	public void SetMeleeWeaponHidden()
	{
		if ((bool)m_activeWeapon)
		{
			m_activeWeapon.SetVisible(visible: false);
		}
	}

	public void SetCurrentMeleeWeaponDamageScale(float damageScale, float staggerScale)
	{
		if (m_activeWeapon != null)
		{
			m_activeWeapon.DamageScale = damageScale;
			m_activeWeapon.StaggerScale = staggerScale;
		}
	}

	private void SetMeleeWeaponPosition(float zValue)
	{
		if (m_activeWeapon != null)
		{
			Vector3 localPosition = m_activeWeapon.transform.localPosition;
			localPosition.z = zValue;
			m_activeWeapon.transform.localPosition = localPosition;
		}
	}

	public void SetMeleePositionFront()
	{
		SetMeleeWeaponPosition(-0.2f);
	}

	public void SetMeleePositionBehind()
	{
		SetMeleeWeaponPosition(0.2f);
	}

	public void ThrowWeapon()
	{
		if (!(m_activeThrownProjectile != null))
		{
			return;
		}
		Vector3 vector = ((m_aiming != null && m_aiming.UseAimForSecondary) ? ((Vector3)m_aiming.PreviousAimVector) : ((!(m_throwDirection != Vector2.zero)) ? m_direction.GetForwardVector() : ((Vector3)m_throwDirection)));
		bool flag = m_direction.CurrentDirection == CharacterDirection.Facing.Left;
		float num = -400f;
		if (flag)
		{
			num *= -1f;
		}
		SecondaryWeaponItemDefinition weaponDefinition = EquippedSecondaryWeaponItemInstance.WeaponDefinition;
		m_activeThrownProjectile.transform.SetParent(null);
		m_activeThrownProjectile.enabled = true;
		if (m_aiming != null && m_aiming.UseAimForSecondary)
		{
			_ = (Vector3)m_aiming.PreviousAimVector;
			Vector3 position = m_activeThrownProjectile.transform.position;
			Vector3 position2 = m_aiming.SnapThrowableToAimVector(position);
			position2.z = position.z;
			m_activeThrownProjectile.transform.position = position2;
			float num2 = weaponDefinition.AimThrowingRotationOffset;
			if (flag)
			{
				num2 *= -1f;
			}
			vector = Quaternion.AngleAxis(num2, Vector3.forward) * vector;
		}
		m_activeThrownProjectile.Fire(vector, weaponDefinition.ThrownProjectileSettings, base.gameObject, weaponDefinition.ThrownProjectileSettings.Speed, num);
		m_activeThrownProjectile = null;
		if (weaponDefinition.ConsumeOnUsed && !GameDebugCommands.CHEAT_INFINITE_AMMO && m_inventory != null)
		{
			m_inventory.Inventory.ConsumeItem(EquippedSecondaryWeaponItemInstance);
		}
	}

	public void ThrowMeleeWeapon()
	{
		Debug.LogError("CharacterEquipment: ThrowMeleeWeapon is deprecated and needs updating");
	}

	public void DropMeleeWeapon(Transform replacedItemTransform = null)
	{
		Debug.LogError("CharacterEquipment: DropMeleeWeapon is deprecated and needs updating");
	}

	private IEnumerator EnableDroppedItemCollider(GameObject droppedItemGO)
	{
		yield return new WaitForSeconds(0.5f);
		Collider2D[] componentsInChildren = droppedItemGO.GetComponentsInChildren<Collider2D>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = true;
		}
		ItemPickup component = droppedItemGO.GetComponent<ItemPickup>();
		if (component != null)
		{
			component.PickupAllowed = true;
		}
	}

	public float GetMeleeAnimationSpeed()
	{
		ItemInstance equippedMeleeItem = m_inventory.Inventory.EquippedMeleeItem;
		if (equippedMeleeItem != null && equippedMeleeItem is MeleeWeaponItemInstance)
		{
			MeleeWeaponItemDefinition meleeWeaponItemDefinition = equippedMeleeItem.ItemDefinition as MeleeWeaponItemDefinition;
			if (meleeWeaponItemDefinition != null)
			{
				return meleeWeaponItemDefinition.MeleeAnimationSpeed;
			}
		}
		return 1f;
	}

	public void OnHitRebound()
	{
		PlayMakerFSM component = GetComponent<PlayMakerFSM>();
		if (component != null)
		{
			component.SendEvent("MeleeRebound");
		}
	}

	public void ShowUsingItem()
	{
		Sprite sprite = null;
		if (m_inventory.QueuedUseItemInstance != null)
		{
			sprite = m_inventory.QueuedUseItemInstance.ItemDefinition.UseHeldSprite;
		}
		if ((bool)sprite)
		{
			m_usingItemSpriteRenderer.gameObject.SetActive(value: true);
			m_usingItemSpriteRenderer.sprite = sprite;
		}
		else
		{
			HideUsingItem();
		}
	}

	public void ShowCoffeeInHand()
	{
		m_usingItemSpriteRenderer.gameObject.SetActive(value: true);
		m_usingItemSpriteRenderer.sprite = m_coffeeSprite;
	}

	public void HideUsingItem()
	{
		m_usingItemSpriteRenderer.gameObject.SetActive(value: false);
	}
}
