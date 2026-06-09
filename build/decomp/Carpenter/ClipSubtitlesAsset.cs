using UnityEngine;

[CreateAssetMenu(fileName = "NewSubtitles", menuName = "Misc/Subtitles Assset")]
public class ClipSubtitlesAsset : ScriptableObject
{
	[SerializeField]
	private ClipSubtitles m_subtitles;

	public ClipSubtitles Subtitles => m_subtitles;
}
