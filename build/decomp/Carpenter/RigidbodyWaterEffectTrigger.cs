using UnityEngine;
using UnityEngine.AddressableAssets;

public class RigidbodyWaterEffectTrigger : MonoBehaviour
{
	[SerializeField]
	private GameplayWaterBounds m_water;

	[SerializeField]
	private float m_submergedDepth;

	[SerializeField]
	private AssetReference m_impactEffect;

	private Collider2D m_collider;

	private bool m_hasTriggered;

	private void Awake()
	{
		m_collider = GetComponent<Collider2D>();
		if (m_water == null || m_impactEffect == null || !m_impactEffect.HasAsset())
		{
			base.enabled = false;
		}
	}

	private void Update()
	{
		if (!m_hasTriggered)
		{
			Bounds bounds = m_collider.bounds;
			_ = bounds.min;
			if (m_water.IsPositionSubmerged(bounds.min, m_submergedDepth))
			{
				m_hasTriggered = true;
				Vector3 min = bounds.min;
				min.z = base.transform.position.z;
				DynamicallySpawnedObject.Spawn(m_impactEffect, persistent: false, min);
			}
		}
	}
}
