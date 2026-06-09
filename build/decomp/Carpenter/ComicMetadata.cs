using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "ComicMetadata", menuName = "Misc/Comic Metadata")]
public class ComicMetadata : ScriptableObject
{
	[SerializeField]
	private AssetReference m_comicPrefabAssetReference;

	[SerializeField]
	private bool m_stayFadedOnExit;

	[SerializeField]
	private LocalizedString m_comicTitle;

	[SerializeField]
	private Sprite m_comicThumbnail;

	[SerializeField]
	private string m_comicID;

	public AssetReference AssetReference => m_comicPrefabAssetReference;

	public bool StayFadedOnExit => m_stayFadedOnExit;

	public LocalizedString ComicTitle => m_comicTitle;

	public Sprite ComicThumbnail => m_comicThumbnail;

	public string ComicID => m_comicID;
}
