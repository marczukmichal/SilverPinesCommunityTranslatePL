using UnityEngine;

public class ArtifactsPage : MenuPage
{
	[SerializeField]
	private ArtifactsInventoryPanel m_artifactsPanel;

	public void HighlightArtfact(ItemInstance item)
	{
		m_artifactsPanel.HighlightArtifact(item);
	}

	public override bool CanExitPage()
	{
		return !m_artifactsPanel.IsAnimating;
	}
}
