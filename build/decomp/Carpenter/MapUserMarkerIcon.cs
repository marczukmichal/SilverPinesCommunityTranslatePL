using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class MapUserMarkerIcon : Map3DIcon
{
	[SerializeField]
	private MapVisualsSettings m_mapIconSettings;

	[SerializeField]
	private Image m_icon;

	private MapUserMarkerData m_markerData;

	private static readonly LocalizedString m_localizedString = new LocalizedString("UIGame", "Map_UserMarker");

	public override bool ShouldScale => true;

	public override Vector3 InfoPanelOffset => new Vector2(0f, -16f);

	public MapUserMarkerData MarkerData => m_markerData;

	public void SetMapUserMarkerData(MapUserMarkerData markerData)
	{
		m_markerData = markerData;
		UpdateIconSprite();
	}

	private void UpdateIconSprite()
	{
		m_icon.sprite = m_mapIconSettings.GetSpriteForMarkerIndex(m_markerData.IconSpriteIndex);
	}

	public void SetIconIndex(int index)
	{
		m_markerData.IconSpriteIndex = index;
		UpdateIconSprite();
	}

	public void NextIcon()
	{
		int iconSpriteIndex = m_markerData.IconSpriteIndex;
		iconSpriteIndex++;
		iconSpriteIndex %= m_mapIconSettings.GetUserMarkerSpriteCount();
		m_markerData.IconSpriteIndex = iconSpriteIndex;
		UpdateIconSprite();
	}

	protected override void UpdateDecoration()
	{
		base.UpdateDecoration();
		m_icon.color = (base.IsMuted ? new Color(0.5f, 0.5f, 0.5f, 0.5f) : Color.white);
	}

	public override string GetHoverName()
	{
		return m_localizedString.GetLocalizedString();
	}
}
