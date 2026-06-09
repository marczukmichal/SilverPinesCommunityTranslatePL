using UnityEngine;

[CreateAssetMenu(fileName = "StaminaReserveItemEffect", menuName = "Items/Effect/StaminaReserveItemEffect")]
public class StaminaReserveItemEffect : ItemEffect
{
	[SerializeField]
	private float m_amount = 100f;

	public override void ApplyEffect(GameObject character, Inventory inventory)
	{
		if (character == null)
		{
			Debug.LogWarning("Tried to give stamina reserve but no character found!");
			return;
		}
		CharacterStamina component = character.GetComponent<CharacterStamina>();
		if ((bool)component)
		{
			component.AddStaminaReserve(m_amount);
		}
		else
		{
			Debug.LogWarning("Tried to give stamina reserve but no stamina component found!");
		}
	}
}
