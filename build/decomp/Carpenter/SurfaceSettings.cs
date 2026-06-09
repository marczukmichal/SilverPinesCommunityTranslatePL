using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName = "Settings/Surface")]
public class SurfaceSettings : ScriptableObject
{
	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_footstepAudioEvent;

	[SerializeField]
	private string m_audioSurfaceParameterLabel;

	[SerializeField]
	private AudioEvent m_landAudioEvent;

	[SerializeField]
	private AudioEvent m_runIntoAudioEvent;

	[Header("Visual Effects")]
	[SerializeField]
	private AssetReference m_smallImpactEffectAsset;

	[SerializeField]
	private AssetReference m_mediumImpactEffectAsset;

	[SerializeField]
	private AssetReference m_largeImpactEffectAsset;

	[SerializeField]
	private AssetReference m_footstepParticleEffectAsset;

	[SerializeField]
	private AssetReference m_landParticleEffectAsset;

	[SerializeField]
	private AssetReference m_bulletHoleAsset;

	[Header("Water Surfaces Only")]
	[Tooltip("Effect that is spawned on a character when they are walking through a surface, only used for water / liquid surfaces")]
	[SerializeField]
	private ParticleSystem m_standingInEffectParticleSystem;

	[SerializeField]
	private AssetReference m_movementSplashEffectAsset;

	public AudioEvent FootstepAudioEvent => m_footstepAudioEvent;

	public string AudioSurfaceParameterLabel => m_audioSurfaceParameterLabel;

	public AudioEvent LandAudioEvent => m_landAudioEvent;

	public AudioEvent RunIntoAudioEvent => m_runIntoAudioEvent;

	public AssetReference FootstepEffectPrefab => m_footstepParticleEffectAsset;

	public AssetReference LandEffectPrefab => m_landParticleEffectAsset;

	public AssetReference BulletHolePrefab => m_bulletHoleAsset;

	public ParticleSystem StandingInEffectPrefab => m_standingInEffectParticleSystem;

	public AssetReference MovementSplashEffectPrefab => m_movementSplashEffectAsset;

	public AssetReference GetImpactEffect(ImpactType impactType)
	{
		return impactType switch
		{
			ImpactType.Large => m_smallImpactEffectAsset, 
			ImpactType.Medium => m_mediumImpactEffectAsset, 
			ImpactType.Small => m_largeImpactEffectAsset, 
			_ => null, 
		};
	}
}
