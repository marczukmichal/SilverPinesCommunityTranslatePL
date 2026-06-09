using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CharacterGroundShadowDecal : MonoBehaviour
{
	[SerializeField]
	private Transform m_basePosition;

	[SerializeField]
	private float m_maxProjectionHeight;

	[SerializeField]
	private AnimationCurve m_alphaDistanceCurve;

	[SerializeField]
	private DecalProjector m_decalProjector;

	[SerializeField]
	private float m_heightOffset;

	private void Update()
	{
		RaycastHit2D raycastHit2D = Physics2D.Raycast(m_basePosition.position, Vector2.down, m_maxProjectionHeight, GameLayers.EnvironmentMask);
		if (raycastHit2D.collider != null)
		{
			m_decalProjector.gameObject.SetActive(value: true);
			Vector3 position = m_decalProjector.transform.position;
			position.y = raycastHit2D.point.y + m_heightOffset;
			m_decalProjector.transform.position = position;
			float num = m_basePosition.position.y - raycastHit2D.point.y - m_heightOffset;
			m_decalProjector.fadeFactor = m_alphaDistanceCurve.Evaluate(Mathf.Clamp01(num / m_maxProjectionHeight));
		}
		else
		{
			m_decalProjector.gameObject.SetActive(value: false);
		}
	}
}
