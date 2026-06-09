using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class ComicViewButton : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_title;

	[SerializeField]
	private LocalizeStringEvent m_localizeString;

	[SerializeField]
	private Image m_image;

	private ComicMetadata m_comic;

	public bool SetVisibleIfSeenComic(ComicMemory memory)
	{
		bool flag = memory.HasSeenComic(m_comic);
		base.gameObject.SetActive(flag);
		return flag;
	}

	public void Setup(ComicMetadata comic)
	{
		m_comic = comic;
		m_localizeString.StringReference = comic.ComicTitle;
		if (!m_localizeString.StringReference.IsEmpty)
		{
			m_title.text = m_localizeString.StringReference.GetLocalizedString();
		}
		else
		{
			m_title.text = comic.name.Replace("Comic_", "");
		}
		m_image.sprite = comic.ComicThumbnail;
	}

	public void OnClick()
	{
		GlobalReferences.Instance.EventChannels.Comic.PlayComicFromMenu.Raise(m_comic);
		GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.InGameMenu);
	}
}
