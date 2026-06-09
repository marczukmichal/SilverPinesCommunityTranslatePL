using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Settings/Explosion")]
public class ExplosionSettings : ScriptableObject
{
	[Header("Explosion Effect")]
	[SerializeField]
	private AssetReferenceGameObject m_explosionAsset;

	[Header("Damage")]
	[FormerlySerializedAs("m_newhitSettings")]
	[SerializeField]
	private HitSettings m_damageHitSettings;

	[Tooltip("The min distance which will apply full damage, at max distance the smallest amount of damage will be appled")]
	[SerializeField]
	private Vector2 m_distances;

	[Tooltip("The curve in which damage falls off from distance")]
	[SerializeField]
	private AnimationCurve m_damageFalloffCurve;

	[SerializeField]
	private CameraShakeSettings m_cameraShake;

	[SerializeField]
	private int m_spawnFireAmount;

	public AssetReferenceGameObject ExplosionAsset => m_explosionAsset;

	public HitSettings HitSettings => m_damageHitSettings;

	public float MinDistance => m_distances.x;

	public float MaxDistance => m_distances.y;

	public CameraShakeSettings CameraShake => m_cameraShake;

	public int SpawnFireAmount => m_spawnFireAmount;

	public float GetDistanceScalar(float distance)
	{
		if (distance < MinDistance)
		{
			return 1f;
		}
		if (distance < MaxDistance)
		{
			return m_damageFalloffCurve.Evaluate(distance.Remap(MinDistance, 0f, MaxDistance, 1f));
		}
		return 0f;
	}
}
