using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class MapPhoto : Map3DIcon
{
	[SerializeField]
	private RawImage m_photoImage;

	private PhotoData m_photoData;

	private static readonly LocalizedString m_localizedString = new LocalizedString("UIGame", "Map_Photo");

	public override Vector3 InfoPanelOffset => new Vector2(0f, -32f);

	public PhotoData PhotoData => m_photoData;

	public override bool ShouldScale => true;

	public override bool CanBeGrouped => false;

	public void SetPhotoData(PhotoData photoData)
	{
		m_photoData = photoData;
		m_photoImage.texture = photoData.Texture;
	}

	public override string GetHoverName()
	{
		return m_localizedString.GetLocalizedString();
	}
}
