using UnityEngine;

public class GondolaSwing : MonoBehaviour
{
	[Header("X Rotation")]
	[SerializeField]
	private Vector2 m_xSwingAmount;

	[SerializeField]
	private float m_xSwingSpeed;

	[Header("Z Rotation")]
	[SerializeField]
	private Vector2 m_zSwingAmount;

	[SerializeField]
	private float m_zSwingSpeed;

	private float m_currentTimeValue;

	private void Start()
	{
		m_currentTimeValue = 0f;
		AttachPlayer(GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item);
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Register(AttachPlayer);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Unregister(AttachPlayer);
	}

	private void Update()
	{
		m_currentTimeValue += Time.deltaTime * Mathf.PerlinNoise1D(Time.time);
		float x = GameUtils.Remap(Mathf.Sin(m_currentTimeValue * m_xSwingSpeed), -1f, 1f, m_xSwingAmount.x, m_xSwingAmount.y);
		float z = GameUtils.Remap(Mathf.Sin(m_currentTimeValue * m_zSwingSpeed), -1f, 1f, m_zSwingAmount.x, m_zSwingAmount.y);
		Quaternion rotation = Quaternion.Euler(x, 0f, z);
		base.transform.rotation = rotation;
	}

	private void AttachPlayer(GameObject player)
	{
		if (player != null)
		{
			player.transform.SetParent(base.transform);
		}
	}

	private void DetachPlayer(GameObject player)
	{
		if (player != null)
		{
			player.transform.SetParent(null);
		}
	}
}
