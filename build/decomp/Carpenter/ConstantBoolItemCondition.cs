using UnityEngine;

[CreateAssetMenu(fileName = "NewBoolConstantCondition", menuName = "Items/Condition/Constant Bool")]
public class ConstantBoolItemCondition : ItemCondition
{
	[SerializeField]
	private bool m_value;

	public override bool Check(ItemDefinition item, Inventory inventory)
	{
		return m_value;
	}
}
