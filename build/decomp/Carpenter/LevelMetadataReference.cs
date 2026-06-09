using UnityEngine;

public class LevelMetadataReference : MonoBehaviour
{
	[SerializeField]
	private LevelMetadata m_levelMetadata;

	[SerializeField]
	private LevelMetadataAnchor m_anchor;

	public LevelMetadata Metadata => m_levelMetadata;

	private void Awake()
	{
		m_anchor.Set(Metadata);
	}
}
