using UnityEngine;

public class FootstepEffectArea : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField]
	private FootstepEffectAreaSettings m_footstepAreaEffectSettings;

	[Header("Bounds")]
	[SerializeField]
	private Vector2 m_size = new Vector2(3f, 1f);

	[Header("Retrigger Minimum Time")]
	[SerializeField]
	private float m_retriggerTime;

	private float m_lastTriggerTime;

	public Vector2 Size => m_size;

	private void OnEnable()
	{
		m_lastTriggerTime = Time.time + m_retriggerTime;
		GlobalReferences.Instance.Sets.Generic.FootstepEffectAreaSet.Add(this);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Sets.Generic.FootstepEffectAreaSet.Remove(this);
	}

	public bool Contains(Vector2 position)
	{
		return new Bounds(size: new Vector3(m_size.x, m_size.y, 100f), center: base.transform.position).Contains(position);
	}

	public static FootstepEffectArea GetActive(Vector2 position)
	{
		foreach (FootstepEffectArea item in GlobalReferences.Instance.Sets.Generic.FootstepEffectAreaSet)
		{
			if (item.Contains(position))
			{
				return item;
			}
		}
		return null;
	}

	public void PlayEffect(Vector3 footPosition)
	{
		if (!(m_lastTriggerTime + m_retriggerTime > Time.time))
		{
			if (m_footstepAreaEffectSettings.AudioEvent != null)
			{
				m_footstepAreaEffectSettings.AudioEvent.Play(footPosition);
			}
			m_lastTriggerTime = Time.time;
		}
	}

	private void OnDrawGizmos()
	{
		Color red = Color.red;
		red.a = 0.05f;
		DrawGizmosShared(red);
	}

	private void OnDrawGizmosSelected()
	{
		DrawGizmosShared(Color.red);
	}

	private void DrawGizmosShared(Color color)
	{
		Gizmos.color = color;
		Gizmos.DrawWireCube(size: new Vector3(m_size.x, m_size.y), center: base.transform.position);
		Gizmos.color = Color.white;
	}
}
