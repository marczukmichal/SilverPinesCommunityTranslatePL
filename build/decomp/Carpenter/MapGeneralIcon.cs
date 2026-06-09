using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class MapGeneralIcon : Map3DIcon
{
	[SerializeField]
	protected MapVisualsSettings m_mapIconSettings;

	[SerializeField]
	protected Image m_icon;

	private LevelMetadata m_parentLevel;

	protected AppearsOnMap.AppearsOnMapSettings m_aomSettings;

	protected LocalizedString m_hoverText;

	private bool m_shouldScale;

	private bool m_canBeGrouped = true;

	private bool m_useZoomLevelAlpha;

	public LevelMetadata ParentLevel => m_parentLevel;

	public override bool ShouldScale => m_shouldScale;

	public override bool CanBeGrouped => m_canBeGrouped;

	public override bool UseZoomLevelAlpha => m_useZoomLevelAlpha;

	public virtual void Setup(LevelMapAOMMetadata.AppearsOnMapInstance aomInstance, MapArea3D.AreaType areaType)
	{
		m_aomSettings = aomInstance.Settings;
		MapVisualsSettings.MapIconSetting iconSettings = m_mapIconSettings.GetIconSettings(m_aomSettings.m_iconType);
		m_icon.sprite = iconSettings.m_icon;
		base.RectTransform.pivot = iconSettings.m_pivot;
		base.RectTransform.sizeDelta = iconSettings.m_size;
		m_shouldScale = iconSettings.m_scaleToZoom;
		m_canBeGrouped = iconSettings.m_canGroup;
		m_useZoomLevelAlpha = iconSettings.m_useZoomLevelAlpha;
		m_parentLevel = aomInstance.LevelMetadata;
		Color white = Color.white;
		m_icon.color = white;
		if (aomInstance.Settings.CustomText != null && !aomInstance.Settings.CustomText.IsEmpty)
		{
			m_hoverText = aomInstance.Settings.CustomText;
		}
		else if (aomInstance.ItemDefinition != null)
		{
			m_hoverText = aomInstance.ItemDefinition.ItemNameLocString;
		}
		else
		{
			m_hoverText = iconSettings.m_hoverText;
		}
	}

	protected override void UpdateDecoration()
	{
		base.UpdateDecoration();
		m_icon.color = (base.IsMuted ? new Color(0.5f, 0.5f, 0.5f, 0.5f) : Color.white);
		m_icon.raycastTarget = !base.IsMuted;
	}

	public override string GetHoverName()
	{
		if (m_hoverText == null || m_hoverText.IsEmpty)
		{
			return "<missing hover text>";
		}
		return m_hoverText.GetLocalizedString();
	}
}
