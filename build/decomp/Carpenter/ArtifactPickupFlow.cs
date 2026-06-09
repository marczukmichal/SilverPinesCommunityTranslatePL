using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class ArtifactPickupFlow : MonoBehaviour
{
	[SerializeField]
	private PlayableDirector m_director;

	[SerializeField]
	private Image m_artifactImage;

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.Inventory.ArtifactItemCollected.Register(OnArtifactCollected);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Inventory.ArtifactItemCollected.Unregister(OnArtifactCollected);
	}

	private void OnArtifactCollected(ItemInstance artifact)
	{
		GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.InGameMenu);
		m_artifactImage.sprite = artifact.ItemDefinition.InventorySprite;
		m_director.Play();
	}
}
