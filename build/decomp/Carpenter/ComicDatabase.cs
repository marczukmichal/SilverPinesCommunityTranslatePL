using UnityEngine;

[CreateAssetMenu(fileName = "ComicDatabase", menuName = "Misc/Comic Database")]
public class ComicDatabase : ScriptableObject
{
	[SerializeField]
	private ComicMetadata[] m_comics;

	public ComicMetadata[] Comics => m_comics;
}
