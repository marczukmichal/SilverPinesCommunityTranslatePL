using UnityEngine;

public class PlayerDynamicHints : MonoBehaviour, IDamageable
{
	private CharacterInventory m_inventory;

	private void Start()
	{
		m_inventory = GetComponent<CharacterInventory>();
	}

	public void ApplyDamageInstance(DamageInstance instance)
	{
		if (instance.HealthDamageAmount > 0 && instance.DamageCategory == DamageCategory.DamageCollider && !instance.IsMeleeBlocked && m_inventory.Inventory.EquippedMeleeItem != null)
		{
			GlobalReferences.Instance.EventChannels.Hints.DynamicHint.Raise(DynamicHintEvent.TakeBlockableDamage);
		}
	}
}
