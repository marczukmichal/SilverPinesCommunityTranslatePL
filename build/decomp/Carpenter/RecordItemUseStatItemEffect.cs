using UnityEngine;

[CreateAssetMenu(fileName = "RecordItemUseStatItemEffect", menuName = "Items/Effect/Record Stat")]
public class RecordItemUseStatItemEffect : ItemEffect
{
	public enum StatType
	{
		Healing,
		FoodAndDrink
	}

	[SerializeField]
	private StatType m_statType;

	[SerializeField]
	private int m_amount = 1;

	public override void ApplyEffect(GameObject character, Inventory inventory)
	{
		switch (m_statType)
		{
		case StatType.Healing:
			GlobalReferences.Instance.DataStore.Data.m_stats.m_healingItemsUsed += m_amount;
			break;
		case StatType.FoodAndDrink:
			GlobalReferences.Instance.DataStore.Data.m_stats.m_foodAndDrinkItemsUsed += m_amount;
			break;
		}
	}
}
