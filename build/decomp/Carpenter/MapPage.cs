using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class MapPage : MenuPage
{
	[SerializeField]
	private UIMap3DPanel m_mapViewer;

	[SerializeField]
	private GameObject m_statusContainer;

	[SerializeField]
	private MenuInputPrompts.AvaialbleInput m_addMarker;

	[SerializeField]
	private MenuInputPrompts.AvaialbleInput m_viewPhotos;

	[SerializeField]
	private MenuInputPrompts.AvaialbleInput m_deletePhoto;

	[SerializeField]
	private MenuInputPrompts.AvaialbleInput m_selectInput;

	[Header("Lore")]
	[SerializeField]
	private LoreInventory m_loreInventory;

	[FormerlySerializedAs("m_loreSummaryPanel")]
	private Map3DIcon m_highlightedIcon;

	private List<MenuInputPrompts.AvaialbleInput> m_mapAvailableInputs = new List<MenuInputPrompts.AvaialbleInput>();

	public override MenuInputPrompts.AvaialbleInput[] AvailableInputs
	{
		get
		{
			m_mapAvailableInputs.Clear();
			if (m_mapViewer.CanAddMarker())
			{
				m_mapAvailableInputs.Add(m_addMarker);
			}
			if (m_mapViewer.CanSelect())
			{
				m_mapAvailableInputs.Add(m_selectInput);
			}
			if (m_mapViewer.HasPhotos())
			{
				m_mapAvailableInputs.Add(m_viewPhotos);
			}
			if (m_mapViewer.CanDeletePhoto())
			{
				m_mapAvailableInputs.Add(m_deletePhoto);
			}
			MenuInputPrompts.AvaialbleInput[] availableInputs = base.AvailableInputs;
			foreach (MenuInputPrompts.AvaialbleInput item in availableInputs)
			{
				m_mapAvailableInputs.Add(item);
			}
			return m_mapAvailableInputs.ToArray();
		}
	}

	private void OnEnable()
	{
		UIMap3DPanel mapViewer = m_mapViewer;
		mapViewer.OnMapIconClicked = (UnityAction<Map3DIcon>)Delegate.Combine(mapViewer.OnMapIconClicked, new UnityAction<Map3DIcon>(MapIconClicked));
		UIMap3DPanel mapViewer2 = m_mapViewer;
		mapViewer2.OnMapIconHovered = (UnityAction<Map3DIcon>)Delegate.Combine(mapViewer2.OnMapIconHovered, new UnityAction<Map3DIcon>(MapIconHovered));
	}

	private void OnDisable()
	{
		UnregisterEventListeners();
		UIMap3DPanel mapViewer = m_mapViewer;
		mapViewer.OnMapIconClicked = (UnityAction<Map3DIcon>)Delegate.Remove(mapViewer.OnMapIconClicked, new UnityAction<Map3DIcon>(MapIconClicked));
		UIMap3DPanel mapViewer2 = m_mapViewer;
		mapViewer2.OnMapIconHovered = (UnityAction<Map3DIcon>)Delegate.Remove(mapViewer2.OnMapIconHovered, new UnityAction<Map3DIcon>(MapIconHovered));
	}

	public override void Show(bool instant = false, bool onAwake = false)
	{
		base.Show(instant, onAwake);
		m_mapViewer.MapViewActive = true;
		m_statusContainer.SetActive(value: false);
		GameInputManager.GameInputActions.UI.SecondaryMenuAction.performed += TryPlayingAudioLog;
	}

	public override void Hide(bool instant = false)
	{
		base.Hide(instant);
		m_mapViewer.MapViewActive = false;
		m_statusContainer.SetActive(value: true);
		UnregisterEventListeners();
	}

	private void UnregisterEventListeners()
	{
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.UI.SecondaryMenuAction.performed -= TryPlayingAudioLog;
		}
	}

	private void MapIconClicked(Map3DIcon icon)
	{
		if (icon is MapLoreEntryIcon mapLoreEntryIcon)
		{
			GlobalReferences.Instance.EventChannels.Lore.ShowLoreEntryInNotesPage.Raise(mapLoreEntryIcon.LoreEntry);
		}
	}

	private void MapIconHovered(Map3DIcon icon)
	{
		m_highlightedIcon = icon;
	}

	public void LocateLoreOnMap(LoreEntry loreEntry)
	{
		if (loreEntry != null)
		{
			m_mapViewer.SetQueuedFocusLoreEntry(loreEntry);
		}
	}

	private void TryPlayingAudioLog(InputAction.CallbackContext callback)
	{
		if (!callback.performed)
		{
			return;
		}
		if (GlobalReferences.Instance.Anchors.Lore.AudioLogPlayerAnchor.Item.IsPlaying)
		{
			GlobalReferences.Instance.EventChannels.Lore.StopPlayingAudioLog.Raise();
			return;
		}
		LoreEntry loreEntry = null;
		if (m_highlightedIcon != null && m_highlightedIcon is MapLoreEntryIcon mapLoreEntryIcon)
		{
			loreEntry = mapLoreEntryIcon.LoreEntry;
		}
		if (loreEntry != null && loreEntry.IsAudioLog)
		{
			GlobalReferences.Instance.EventChannels.Lore.PlayLoreEntryAsAudioLog.Raise(loreEntry);
		}
	}

	public override bool OnBackInput()
	{
		return m_mapViewer.OnBackInput();
	}

	public void SetMapShouldReposition()
	{
		m_mapViewer.SetMapShouldReposition();
	}
}
