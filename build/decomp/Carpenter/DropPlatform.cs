using UnityEngine;

public class DropPlatform : MonoBehaviour
{
	[SerializeField]
	private DropPlatformSet m_dropPlatformSet;

	private Collider2D m_collider;

	public Collider2D Collider
	{
		get
		{
			if (m_collider == null)
			{
				m_collider = GetComponent<Collider2D>();
			}
			return m_collider;
		}
	}

	private void Awake()
	{
		m_collider = GetComponent<Collider2D>();
	}

	private void OnEnable()
	{
		m_dropPlatformSet.Add(this);
	}

	private void OnDisable()
	{
		m_dropPlatformSet.Remove(this);
	}
}
