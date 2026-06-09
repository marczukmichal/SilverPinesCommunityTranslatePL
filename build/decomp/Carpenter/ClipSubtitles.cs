using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ClipSubtitles
{
	[SerializeField]
	private List<ClipSubtitleEntry> m_entries;

	public List<ClipSubtitleEntry> Entries => m_entries;

	public ClipSubtitles()
	{
		m_entries = new List<ClipSubtitleEntry>();
	}

	public ClipSubtitleEntry GetEntryForTimestamp(float time)
	{
		foreach (ClipSubtitleEntry entry in m_entries)
		{
			if (entry.m_startTime < time && entry.m_endTime > time)
			{
				return entry;
			}
		}
		return null;
	}
}
