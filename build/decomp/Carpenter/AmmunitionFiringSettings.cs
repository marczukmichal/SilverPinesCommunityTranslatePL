using UnityEngine;

[CreateAssetMenu(menuName = "Settings/Ammunition Firing Settings")]
public class AmmunitionFiringSettings : ScriptableObject
{
	[SerializeField]
	private int m_projectileCount = 1;

	[Header("Accuracy")]
	[SerializeField]
	private Vector2 m_spread;

	public int ProjectileCount => m_projectileCount;

	public Vector2 Spread => m_spread;
}
