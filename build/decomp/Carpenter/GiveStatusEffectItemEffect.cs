using UnityEngine;

[CreateAssetMenu(fileName = "GiveStatusEffectItemEffect", menuName = "Items/Effect/GiveStatusEffect")]
public class GiveStatusEffectItemEffect : ItemEffect
{
	[SerializeField]
	private StatusEffectDefinition m_definition;

	[SerializeField]
	private int m_amount = 100;

	public override void ApplyEffect(GameObject character, Inventory inventory)
	{
		if (character == null)
		{
			Debug.LogWarning("Tried to give status effect but no character found!");
			return;
		}
		StatusEffectReceiver component = character.GetComponent<StatusEffectReceiver>();
		if ((bool)component)
		{
			component.ApplyStatusEffect(m_definition, m_amount);
		}
		else
		{
			Debug.LogWarning("Tried to give status effect but no status effect receiverfound!");
		}
	}
}
