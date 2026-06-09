using UnityEngine;
using UnityEngine.AddressableAssets;

public class MinigameMetadata : ScriptableObject
{
	[SerializeField]
	private AssetReference m_sceneAssetReference;

	[SerializeField]
	private bool m_doFullscreenFade;

	[SerializeField]
	private bool m_blurBackground = true;

	[SerializeField]
	private bool m_canPhotograph;

	public AssetReference AssetReference => m_sceneAssetReference;

	public bool DoFullscreenFade => m_doFullscreenFade;

	public bool BlurBackground => m_blurBackground;

	public bool CanPhotograph => m_canPhotograph;
}
