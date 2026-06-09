using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ComicMemory", menuName = "Misc/Comic Memory")]
public class ComicMemory : ScriptableObject
{
	[SerializeField]
	private List<string> m_seenComicIds = new List<string>();

	public void RecordSeenComic(ComicMetadata comicMetadata)
	{
		if (!m_seenComicIds.Contains(comicMetadata.ComicID))
		{
			m_seenComicIds.Add(comicMetadata.ComicID);
		}
	}

	public bool HasSeenComic(ComicMetadata comicMetadata)
	{
		return m_seenComicIds.Contains(comicMetadata.ComicID);
	}

	public void Clear()
	{
		m_seenComicIds = new List<string>();
	}

	public List<string> GetPersistentData()
	{
		return m_seenComicIds;
	}

	public void ReadFromPersistentData(List<string> data)
	{
		m_seenComicIds = new List<string>(data);
	}
}
