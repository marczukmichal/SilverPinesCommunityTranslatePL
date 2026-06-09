using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "NewItem", menuName = "Items/Artifact Item Definition")]
public class ArtifactItemDefinition : ItemDefinition
{
	[SerializeField]
	private ArtifactEffectInstance[] m_artifactEffects;

	[SerializeField]
	private string m_fmodAudioParameter;

	private static LocalizedString m_unidentifiedArtifactString = new LocalizedString("Items", "ArtifactEffect_Unknown");

	public string FMODAudioParameter => m_fmodAudioParameter;

	public string GetArtifactEffectDescription(bool poweredUp)
	{
		string text = "";
		if (poweredUp)
		{
			bool flag = true;
			ArtifactEffectInstance[] artifactEffects = m_artifactEffects;
			foreach (ArtifactEffectInstance artifactEffectInstance in artifactEffects)
			{
				if (!flag)
				{
					text += "\n";
				}
				flag = false;
				text = ((artifactEffectInstance.EffectDefinition.OutcomeType != 0) ? (text + "<color=#c00><b>-</b> " + artifactEffectInstance.EffectDescription + "</color>") : (text + "<color=#ccd><b>+</b> " + artifactEffectInstance.EffectDescription + "</color>"));
			}
		}
		else
		{
			text += m_unidentifiedArtifactString.GetLocalizedString();
		}
		return text;
	}

	public ArtifactEffectInstance GetMatchingArtifactEffect(ArtifactEffectDefinition definition)
	{
		ArtifactEffectInstance[] artifactEffects = m_artifactEffects;
		foreach (ArtifactEffectInstance artifactEffectInstance in artifactEffects)
		{
			if (artifactEffectInstance.EffectDefinition == definition)
			{
				return artifactEffectInstance;
			}
		}
		return null;
	}

	public override bool CanEquip(Inventory inventory)
	{
		return true;
	}

	public override bool CanHaveShortcutSet(Inventory inventory)
	{
		return false;
	}
}
