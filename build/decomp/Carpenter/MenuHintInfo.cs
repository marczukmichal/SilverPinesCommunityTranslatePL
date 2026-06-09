using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Video;

[CreateAssetMenu(menuName = "Hints/Menu Hint Info")]
public class MenuHintInfo : ScriptableObject
{
	public enum AssociatedMenuPage
	{
		Inventory,
		Artifacts,
		Map
	}

	[SerializeField]
	private LocalizedString m_titleStringReference;

	[SerializeField]
	private LocalizedString m_hintStringReference;

	[SerializeField]
	private Sprite m_image;

	[SerializeField]
	private VideoClip m_videoClip;

	[SerializeField]
	private AssociatedMenuPage m_menuPage;

	public string TitleText => m_titleStringReference.GetLocalizedString();

	public string HintText => m_hintStringReference.GetLocalizedString();

	public Sprite Image => m_image;

	public VideoClip VideoClip => m_videoClip;

	public AssociatedMenuPage MenuPage => m_menuPage;
}
