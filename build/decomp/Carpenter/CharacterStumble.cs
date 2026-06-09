using System.Collections.Generic;
using UnityEngine;

public class CharacterStumble : MonoBehaviour
{
	private class StumbleCollision
	{
		public StumbleCollider m_stumbleCollider;

		public bool m_isActive;

		public float m_endTime;
	}

	[SerializeField]
	private float m_lingerTime = 1f;

	private List<StumbleCollision> m_overlapStumbles = new List<StumbleCollision>();

	private CharacterDirection m_direction;

	private void Start()
	{
		m_direction = GetComponent<CharacterDirection>();
	}

	public void ApplyStumbleOverlap(StumbleCollider stumbleCollider)
	{
		foreach (StumbleCollision overlapStumble in m_overlapStumbles)
		{
			if (overlapStumble.m_stumbleCollider == stumbleCollider)
			{
				overlapStumble.m_isActive = true;
				return;
			}
		}
		m_overlapStumbles.Add(new StumbleCollision
		{
			m_stumbleCollider = stumbleCollider,
			m_isActive = true
		});
	}

	public void RemoveStumbleOverlap(StumbleCollider stumbleCollider)
	{
		foreach (StumbleCollision overlapStumble in m_overlapStumbles)
		{
			if (overlapStumble.m_stumbleCollider == stumbleCollider)
			{
				overlapStumble.m_isActive = false;
				overlapStumble.m_endTime = Time.time;
				break;
			}
		}
	}

	public bool ShouldStumble()
	{
		foreach (StumbleCollision overlapStumble in m_overlapStumbles)
		{
			if (overlapStumble.m_isActive)
			{
				return true;
			}
		}
		return false;
	}

	public void ClearRecentStumbleCollisions()
	{
		foreach (StumbleCollision overlapStumble in m_overlapStumbles)
		{
			if (!overlapStumble.m_isActive)
			{
				overlapStumble.m_endTime = 0f;
			}
		}
	}

	public bool ShouldFall()
	{
		int num = 0;
		int num2 = 0;
		foreach (StumbleCollision overlapStumble in m_overlapStumbles)
		{
			if (overlapStumble.m_isActive && overlapStumble.m_stumbleCollider.CanTriggerFall)
			{
				num++;
			}
			else if (Time.time - overlapStumble.m_endTime < m_lingerTime)
			{
				num2++;
			}
		}
		if (num >= 2)
		{
			return true;
		}
		if (num == 1)
		{
			return num2 > 0;
		}
		return false;
	}

	public void ApplyStumbeSeperationToOthers()
	{
		foreach (StumbleCollision overlapStumble in m_overlapStumbles)
		{
			Vector3 forwardVector = m_direction.GetForwardVector();
			Vector2 moveDirection = new Vector2(forwardVector.x * -1f, forwardVector.y);
			if (overlapStumble.m_isActive)
			{
				overlapStumble.m_stumbleCollider.ApplySuccesfulStumbleSeperationBehaviour(moveDirection, 1f);
			}
		}
	}
}
