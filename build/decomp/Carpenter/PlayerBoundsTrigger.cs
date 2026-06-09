using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class PlayerBoundsTrigger
{
	[SerializeField]
	private Vector2 m_size;

	[SerializeField]
	private Vector2 m_offset;

	public UnityAction OnEnter;

	public UnityAction OnExit;

	private bool m_isInArea;

	public Vector2 Size
	{
		get
		{
			return m_size;
		}
		set
		{
			m_size = value;
		}
	}

	public Vector2 Offset
	{
		get
		{
			return m_offset;
		}
		set
		{
			m_offset = value;
		}
	}

	public void Update(Transform transform)
	{
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		bool flag = false;
		if (item != null)
		{
			flag = IsInBound(transform, item);
		}
		if (flag != m_isInArea)
		{
			m_isInArea = flag;
			if (m_isInArea)
			{
				OnEnter?.Invoke();
			}
			else
			{
				OnExit?.Invoke();
			}
		}
	}

	private Bounds GetBounds(Transform transform, float depth = 100f)
	{
		return new Bounds(size: new Vector3(m_size.x, m_size.y, depth), center: transform.TransformPoint(m_offset));
	}

	private bool IsInBound(Transform transform, GameObject player)
	{
		if (GetBounds(transform).Contains(player.transform.position))
		{
			return true;
		}
		return false;
	}
}
