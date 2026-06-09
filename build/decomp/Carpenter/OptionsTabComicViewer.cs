using System.Collections.Generic;
using UnityEngine;

public class OptionsTabComicViewer : OptionsMenuTab
{
	[Header("Comic")]
	[SerializeField]
	private GameObject m_buttonTemplate;

	[SerializeField]
	private ComicDatabase m_comicDatabase;

	[SerializeField]
	private ComicMemory m_comicMemory;

	private List<ComicViewButton> m_comicButtons;

	protected override void PopulateOptionsTab()
	{
		base.PopulateOptionsTab();
		m_comicButtons = new List<ComicViewButton>();
		m_buttonTemplate.gameObject.SetActive(value: false);
		ComicMetadata[] comics = m_comicDatabase.Comics;
		foreach (ComicMetadata comic in comics)
		{
			GameObject obj = Object.Instantiate(m_buttonTemplate, m_optionsListContainer);
			obj.SetActive(value: true);
			ComicViewButton component = obj.GetComponent<ComicViewButton>();
			component.Setup(comic);
			m_comicButtons.Add(component);
		}
		CheckForVisibility();
	}

	private void CheckForVisibility()
	{
		bool flag = false;
		foreach (ComicViewButton comicButton in m_comicButtons)
		{
			if (comicButton.SetVisibleIfSeenComic(m_comicMemory) && !flag)
			{
				flag = true;
				m_selectGameObject = comicButton.gameObject;
			}
		}
	}

	private void OnEnable()
	{
		CheckForVisibility();
	}
}
