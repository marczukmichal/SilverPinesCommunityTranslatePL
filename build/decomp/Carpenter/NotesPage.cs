using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NotesPage : MenuPage
{
	[SerializeField]
	private LoreReader m_loreReader;

	[SerializeField]
	private NotesList m_notesList;

	private List<MenuInputPrompts.AvaialbleInput> m_activeInputs = new List<MenuInputPrompts.AvaialbleInput>();

	public override MenuInputPrompts.AvaialbleInput[] AvailableInputs
	{
		get
		{
			m_activeInputs.Clear();
			MenuInputPrompts.AvaialbleInput[] availableInputs = base.AvailableInputs;
			foreach (MenuInputPrompts.AvaialbleInput item in availableInputs)
			{
				m_activeInputs.Add(item);
			}
			availableInputs = m_notesList.AvailableInputs;
			foreach (MenuInputPrompts.AvaialbleInput item2 in availableInputs)
			{
				m_activeInputs.Add(item2);
			}
			availableInputs = m_loreReader.AvailableInputs;
			foreach (MenuInputPrompts.AvaialbleInput item3 in availableInputs)
			{
				m_activeInputs.Add(item3);
			}
			return m_activeInputs.ToArray();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		NotesList notesList = m_notesList;
		notesList.OnRequestShowLore = (UnityAction<LoreEntry>)Delegate.Combine(notesList.OnRequestShowLore, new UnityAction<LoreEntry>(OnRequestShowLore));
		NotesList notesList2 = m_notesList;
		notesList2.OnRequestLocateLoreOnMap = (UnityAction<LoreEntry>)Delegate.Combine(notesList2.OnRequestLocateLoreOnMap, new UnityAction<LoreEntry>(OnShowOnMap));
	}

	private void OnShowOnMap(LoreEntry loreEntry)
	{
		GlobalReferences.Instance.EventChannels.Lore.ShowLoreEntryOnMapPage.Raise(loreEntry);
	}

	private void OnRequestShowLore(LoreEntry loreEntry)
	{
		m_loreReader.ShowLore(loreEntry);
	}

	public override void Show(bool instant = false, bool onAwake = false)
	{
		base.Show(instant, onAwake);
	}

	public override void Hide(bool instant = false)
	{
		base.Hide(instant);
	}

	public override void SelectDefaultSelectable()
	{
		m_notesList.SetSelectedGameObject();
	}

	public override bool OnBackInput()
	{
		return base.OnBackInput();
	}

	public void JumpToLoreEntry(LoreEntry entry)
	{
		m_notesList.JumpToLoreEntry(entry);
	}
}
