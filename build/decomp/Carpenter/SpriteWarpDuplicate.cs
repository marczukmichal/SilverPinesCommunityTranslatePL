using UnityEngine;

public class SpriteWarpDuplicate : MonoBehaviour
{
	[SerializeField]
	private float m_updateDelay = 0.1f;

	[SerializeField]
	private SpriteRenderer m_warpSpriteRenderer;

	[SerializeField]
	private SpriteRenderer m_targetSpriteRenderer;

	[SerializeField]
	private Vector2 m_randomPositionAmount;

	[SerializeField]
	private float m_zOffset;

	[SerializeField]
	private float m_minBlurScale = 4f;

	[SerializeField]
	private float m_maxBlurScale = 16f;

	private float m_updateTimer;

	private CharacterHealth m_health;

	private Sprite m_trackedActiveSprite;

	private bool m_trackedFlipX;

	private void OnEnable()
	{
		m_warpSpriteRenderer.enabled = true;
	}

	private void OnDisable()
	{
		m_warpSpriteRenderer.enabled = false;
	}

	private void Start()
	{
		m_health = GetComponentInParent<CharacterHealth>();
		if (m_health != null)
		{
			if (m_health.IsDead)
			{
				base.enabled = false;
			}
			else
			{
				m_health.OnDead.AddListener(OnCharacterDead);
			}
		}
		if (m_targetSpriteRenderer.material.HasFloat("_Saturation"))
		{
			m_warpSpriteRenderer.material.SetFloat("_Saturation", m_targetSpriteRenderer.material.GetFloat("_Saturation"));
		}
		if (m_targetSpriteRenderer.material.HasFloat("_HueShift"))
		{
			m_warpSpriteRenderer.material.SetFloat("_HueShift", m_targetSpriteRenderer.material.GetFloat("_HueShift"));
		}
		if (m_targetSpriteRenderer.material.HasFloat("_Contrast"))
		{
			m_warpSpriteRenderer.material.SetFloat("_Contrast", m_targetSpriteRenderer.material.GetFloat("_Contrast"));
		}
	}

	private void OnCharacterDead()
	{
		if (m_health != null)
		{
			base.enabled = false;
		}
	}

	private void Reset()
	{
		m_warpSpriteRenderer = GetComponent<SpriteRenderer>();
	}

	private void LateUpdate()
	{
		if (!(m_health != null) || !m_health.IsDead)
		{
			m_updateTimer += Time.deltaTime;
			if (m_updateTimer >= m_updateDelay || m_warpSpriteRenderer.sprite != m_targetSpriteRenderer.sprite)
			{
				UpdateWarp();
			}
		}
	}

	private void UpdateWarp()
	{
		Sprite sprite = m_targetSpriteRenderer.sprite;
		if (sprite != m_trackedActiveSprite)
		{
			m_warpSpriteRenderer.sprite = m_trackedActiveSprite;
			m_warpSpriteRenderer.flipX = m_trackedFlipX;
			m_warpSpriteRenderer.flipY = m_targetSpriteRenderer.flipY;
			m_warpSpriteRenderer.sortingLayerID = m_targetSpriteRenderer.sortingLayerID;
			m_warpSpriteRenderer.sortingOrder = m_targetSpriteRenderer.sortingOrder - 1;
			m_trackedActiveSprite = sprite;
			m_trackedFlipX = m_targetSpriteRenderer.flipX;
		}
		Vector3 vector = new Vector3(Random.Range(0f - m_randomPositionAmount.x, m_randomPositionAmount.x), Random.Range(0f - m_randomPositionAmount.y, m_randomPositionAmount.y), m_zOffset);
		Vector3 localPosition = m_warpSpriteRenderer.transform.localPosition;
		m_warpSpriteRenderer.transform.localPosition = m_targetSpriteRenderer.transform.localPosition + vector;
		float num = Vector3.Distance(m_warpSpriteRenderer.transform.localPosition, localPosition);
		float value = Mathf.Lerp(m_minBlurScale, m_maxBlurScale, Mathf.Clamp01(num / ((m_randomPositionAmount.x + m_randomPositionAmount.y) / 2f)));
		m_warpSpriteRenderer.material.SetFloat("_BlurScale", value);
		m_updateTimer = 0f;
	}
}
