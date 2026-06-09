using UnityEngine;

[CreateAssetMenu(fileName = "NewHealingItemCondition", menuName = "Items/Condition/Healing Item Condition")]
public class HealingItemCondition : ItemCondition
{
	public enum HealingType
	{
		NormalHeal
	}

	[SerializeField]
	private HealingType m_healingType;

	public override bool Check(ItemDefinition item, Inventory inventory)
	{
		GameObject item2 = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (item2 != null)
		{
			CharacterHealth component = item2.GetComponent<CharacterHealth>();
			if (m_healingType == HealingType.NormalHeal)
			{
				return component.Health < component.MaxHealth;
			}
		}
		return false;
	}
}
