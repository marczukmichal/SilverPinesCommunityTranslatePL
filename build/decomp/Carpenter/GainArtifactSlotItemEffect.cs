using UnityEngine;

[CreateAssetMenu(fileName = "GainArtifactSlotItemEffect", menuName = "Items/Effect/Gain Artifact Slot")]
public class GainArtifactSlotItemEffect : ItemEffect
{
	public override void ApplyEffect(GameObject character, Inventory inventory)
	{
		GlobalReferences.Instance.EventChannels.Inventory.UnlockNewArtifactSlot.Raise();
	}
}
