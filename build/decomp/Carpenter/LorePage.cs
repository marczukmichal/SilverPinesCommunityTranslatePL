using System;
using UnityEngine;
using UnityEngine.Localization;

[Serializable]
public class LorePage
{
	public enum Layout
	{
		Text,
		Image
	}

	[SerializeField]
	private LocalizedString m_textStringReference;

	[Tooltip("Image that appears when viewing this page - if left blank then it will be the lore lead image")]
	[SerializeField]
	private Sprite m_image;

	[SerializeField]
	private Layout m_pageLayout;

	public LocalizedString TextStringReference => m_textStringReference;

	public Sprite Image => m_image;

	public Layout PageLayout => m_pageLayout;
}
