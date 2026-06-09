using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticlesCharacterCollisionLayer : MonoBehaviour
{
	[SerializeField]
	private float m_allowedOffset = 0.01f;

	private void Start()
	{
		ParticleSystem component = GetComponent<ParticleSystem>();
		if (component != null)
		{
			ParticleSystem.CollisionModule collision = component.collision;
			if (IsCorrectZValueForCharacterCollision())
			{
				collision.collidesWith = (int)collision.collidesWith | (1 << GameLayers.DamageableLayer);
			}
			else
			{
				collision.collidesWith = (int)collision.collidesWith & ~(1 << GameLayers.DamageableLayer);
			}
		}
	}

	private bool IsCorrectZValueForCharacterCollision()
	{
		float num = 0f;
		return Mathf.Abs(base.transform.position.z - num) < m_allowedOffset;
	}
}
