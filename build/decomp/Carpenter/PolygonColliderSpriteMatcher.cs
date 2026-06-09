using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class PolygonColliderSpriteMatcher : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer m_spriteRenderer;

	[SerializeField]
	private PolygonCollider2D m_polygonCollider2D;

	private Sprite m_currentSprite;

	private void Update()
	{
		Sprite sprite = m_spriteRenderer.sprite;
		if (sprite != m_currentSprite)
		{
			UpdateShapeToSprite(m_polygonCollider2D, sprite);
			m_currentSprite = sprite;
		}
	}

	private void UpdateShapeToSprite(PolygonCollider2D collider, Sprite sprite)
	{
		if (collider != null && sprite != null && sprite.GetPhysicsShapeCount() > 0)
		{
			collider.pathCount = 1;
			List<Vector2> list = new List<Vector2>();
			if (collider.pathCount > 0)
			{
				list.Clear();
				sprite.GetPhysicsShape(0, list);
				collider.SetPath(0, list.ToArray());
			}
		}
	}
}
