using UnityEngine;
using UnityEngine.Localization;

public class MapItemIcon : MapGeneralIcon
{
	[SerializeField]
	private Sprite m_unknownItemPickupSprite;

	[SerializeField]
	private Sprite m_knownItemPickupSprite;

	[SerializeField]
	private LocalizedString m_unknownItemString;

	private MapIconState m_state;

	public override void Setup(LevelMapAOMMetadata.AppearsOnMapInstance aomInstance, MapArea3D.AreaType areaType)
	{
		base.Setup(aomInstance, areaType);
		m_state = GlobalReferences.Instance.MapDynamicData.GetMapIconStateBitMask(aomInstance.Settings.MapIconGUID);
		if (m_state.HasFlag(MapIconState.ItemRevealed))
		{
			m_icon.sprite = m_knownItemPickupSprite;
		}
		else
		{
			m_icon.sprite = m_unknownItemPickupSprite;
		}
	}

	public override string GetHoverName()
	{
		if ((m_aomSettings.m_iconType == MapIconType.ItemPickup || m_aomSettings.m_iconType == MapIconType.LootContainer) && !m_state.HasFlag(MapIconState.ItemRevealed))
		{
			return m_unknownItemString.GetLocalizedString();
		}
		return base.GetHoverName();
	}
}
