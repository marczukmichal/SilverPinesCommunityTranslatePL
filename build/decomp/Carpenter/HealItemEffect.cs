using UnityEngine;

[CreateAssetMenu(fileName = "HealItemEffect", menuName = "Items/Effect/Heal")]
public class HealItemEffect : ItemEffect
{
	[Tooltip("Amount to heal in health units")]
	[SerializeField]
	private int m_healAmount;

	public override void ApplyEffect(GameObject character, Inventory inventory)
	{
		if (character == null)
		{
			Debug.LogWarning("Tried to heal but no character found!");
			return;
		}
		CharacterHealth component = character.GetComponent<CharacterHealth>();
		if ((bool)component)
		{
			component.Health += m_healAmount;
		}
		else
		{
			Debug.LogWarning("Tried to heal but no character health component found!");
		}
	}
}
