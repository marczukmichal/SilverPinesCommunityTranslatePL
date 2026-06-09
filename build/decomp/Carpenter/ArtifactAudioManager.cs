using FMODUnity;
using UnityEngine;

public class ArtifactAudioManager : MonoBehaviour
{
	[SerializeField]
	private ArtifactsGameSettings m_artifactGameSettings;

	[SerializeField]
	private PlayerMainInventory m_playerInventory;

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.Inventory.EquippedArtifactsChanged.Register(ArtifactsChanged);
		GlobalReferences.Instance.EventChannels.SaveLoad.OnGameReloadedEvent.Register(ApplyArtifactParameters);
		ApplyArtifactParameters();
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Inventory.EquippedArtifactsChanged.Unregister(ArtifactsChanged);
		GlobalReferences.Instance.EventChannels.SaveLoad.OnGameReloadedEvent.Unregister(ApplyArtifactParameters);
	}

	private void ArtifactsChanged(ItemEquippedEventData item)
	{
		ApplyArtifactParameters();
	}

	private void ApplyArtifactParameters()
	{
		ArtifactItemDefinition[] artifacts = m_artifactGameSettings.Artifacts;
		foreach (ArtifactItemDefinition artifactItemDefinition in artifacts)
		{
			if (!string.IsNullOrEmpty(artifactItemDefinition.FMODAudioParameter))
			{
				bool flag = m_playerInventory.IsArtifactEquipped(artifactItemDefinition);
				RuntimeManager.StudioSystem.setParameterByName(artifactItemDefinition.FMODAudioParameter, flag ? 1f : 0f);
			}
		}
	}
}
