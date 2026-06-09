using UnityEngine;

[CreateAssetMenu(fileName = "RemoveStatusEffectItemEffect", menuName = "Items/Effect/RemoveStatusEffect")]
public class RemoveStatusEffectItemEffect : ItemEffect
{
	[SerializeField]
	private StatusEffectDefinition m_definition;

	public override void ApplyEffect(GameObject character, Inventory inventory)
	{
		if (character == null)
		{
			Debug.LogWarning("Tried to remove status effect but no character found!");
			return;
		}
		StatusEffectReceiver component = character.GetComponent<StatusEffectReceiver>();
		if ((bool)component)
		{
			component.RemoveStatusEffect(m_definition);
		}
		else
		{
			Debug.LogWarning("Tried to remove status effect but no status effect receiverfound!");
		}
	}
}
