using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Items/Refillable Item Definition")]
public class RefillableItemDefinition : ItemDefinition
{
	[SerializeField]
	private int m_maxUses = 1;

	[SerializeField]
	private ItemDefinition m_refillItemDefinition;

	[SerializeField]
	private bool m_requiresUsesToUse = true;

	[SerializeField]
	private bool m_autoConsumeUse = true;

	public int MaxUses => m_maxUses;

	public ItemDefinition RefillItemDefinition => m_refillItemDefinition;

	public bool RequiresUsesToUse => m_requiresUsesToUse;

	public bool AutoConsumeUse => m_autoConsumeUse;

	public override bool CanBeCombinedWithAnything()
	{
		return true;
	}
}
