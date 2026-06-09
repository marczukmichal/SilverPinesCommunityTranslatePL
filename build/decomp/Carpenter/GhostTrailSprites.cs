using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostTrailSprites : MonoBehaviour
{
	[SerializeField]
	private float m_ghostDelay = 0.1f;

	[SerializeField]
	private float m_ghostLifetime = 0.5f;

	[SerializeField]
	private Color m_ghostColor = new Color(1f, 1f, 1f, 0.5f);

	[SerializeField]
	private SpriteRenderer m_spriteRenderer;

	private bool m_spawning;

	private List<GameObject> m_activeGhosts;

	public void DisableTrail()
	{
		m_spawning = false;
	}

	private void Start()
	{
		m_activeGhosts = new List<GameObject>();
		m_spawning = true;
		StartCoroutine(SpawnGhosts());
	}

	private void OnDestroy()
	{
		if (m_activeGhosts == null)
		{
			return;
		}
		foreach (GameObject activeGhost in m_activeGhosts)
		{
			Object.Destroy(activeGhost);
		}
	}

	private IEnumerator SpawnGhosts()
	{
		while (m_spawning)
		{
			CreateGhost();
			yield return new WaitForSeconds(m_ghostDelay);
		}
	}

	private void CreateGhost()
	{
		GameObject gameObject = new GameObject("Ghost");
		m_activeGhosts.Add(gameObject);
		gameObject.transform.position = base.transform.position;
		gameObject.transform.localScale = base.transform.localScale;
		SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
		spriteRenderer.sprite = m_spriteRenderer.sprite;
		spriteRenderer.flipX = m_spriteRenderer.flipX;
		spriteRenderer.flipY = m_spriteRenderer.flipY;
		spriteRenderer.sortingLayerID = m_spriteRenderer.sortingLayerID;
		spriteRenderer.sortingOrder = m_spriteRenderer.sortingOrder - 1;
		spriteRenderer.color = m_ghostColor;
		StartCoroutine(FadeAndDestroy(spriteRenderer, m_ghostLifetime));
	}

	private IEnumerator FadeAndDestroy(SpriteRenderer ghostSr, float duration)
	{
		float elapsed = 0f;
		Color originalColor = ghostSr.color;
		while (elapsed < duration)
		{
			float a = Mathf.Lerp(originalColor.a, 0f, elapsed / duration);
			ghostSr.color = new Color(originalColor.r, originalColor.g, originalColor.b, a);
			elapsed += Time.deltaTime;
			yield return null;
		}
		m_activeGhosts.Remove(ghostSr.gameObject);
		Object.Destroy(ghostSr.gameObject);
	}
}
