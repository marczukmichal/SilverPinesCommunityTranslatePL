using UnityEngine;

public class BackgroundSniperCover : MonoBehaviour
{
	[SerializeField]
	private float m_randomImpactPositionRange = 0.5f;

	private BoxCollider m_collider;

	private void Awake()
	{
		m_collider = GetComponent<BoxCollider>();
	}

	public void PerformImpactEffect(Quaternion rotation, Vector3 aimPosition)
	{
		Vector3 position = base.transform.position;
		position.y = aimPosition.y;
		position.z += 0.2f;
		position.x += Random.Range(0f - m_randomImpactPositionRange, m_randomImpactPositionRange);
		ProjectileUtils.PerformImpactEffect(base.gameObject, ImpactType.Medium, aimPosition, rotation, spawnBulletHole: false);
	}
}
