using UnityEngine;
using UnityEngine.UI;

public class MapLoreEntryIcon : MapGeneralIcon
{
	[SerializeField]
	private Image m_loreImage;

	private LoreEntry m_loreEntry;

	public LoreEntry LoreEntry => m_loreEntry;

	public override void Setup(LevelMapAOMMetadata.AppearsOnMapInstance aomInstance, MapArea3D.AreaType areaType)
	{
		base.Setup(aomInstance, areaType);
		m_loreEntry = aomInstance.LoreEntry;
		if (m_loreImage != null)
		{
			m_loreImage.sprite = m_loreEntry.LeadImage;
		}
	}
}
