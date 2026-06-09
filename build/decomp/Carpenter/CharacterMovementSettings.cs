using UnityEngine;

[CreateAssetMenu(menuName = "Settings/Character Movement")]
public class CharacterMovementSettings : ScriptableObject
{
	[SerializeField]
	private float m_moveSpeed;

	[Tooltip("How far the character needs to move while in water to trigger a water splash effect. Set to 0 for no splashes")]
	[SerializeField]
	private float m_distanceToTriggerSplashEffect;

	public float MoveSpeed => m_moveSpeed;

	public float DistanceToTriggerSplashEffect => m_distanceToTriggerSplashEffect;
}
