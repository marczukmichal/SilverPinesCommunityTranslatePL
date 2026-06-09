using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Misc/Taking Photo State Data")]
public class TakingPhotoStateData : ScriptableObject
{
	public Vector2 m_offset;

	public float m_size;

	public float m_zoomNormalized;

	public UnityAction<bool> OnSetActive;

	private bool m_isActive;

	public bool IsActive => m_isActive;

	public void Reset()
	{
		m_offset = Vector2.zero;
		m_size = 0.5f;
	}

	public void SetActive(bool active)
	{
		if (m_isActive != active)
		{
			m_isActive = active;
			OnSetActive?.Invoke(active);
		}
	}
}
