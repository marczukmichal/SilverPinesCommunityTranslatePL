using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class CharacterStance : MonoBehaviour
{
	[Serializable]
	public class StanceGameObjectChange
	{
		public GameObject m_gameObject;

		public bool m_enable;
	}

	[Serializable]
	public class StanceSettings
	{
		public Stance m_stanceType;

		public Vector2 m_colliderOffset;

		public Vector2 m_colliderSize;

		public bool m_useCollision = true;

		public StanceGameObjectChange[] m_associatedGameObjects;
	}

	public enum Stance
	{
		Standing,
		Crouching,
		NoCollision,
		Prone,
		Deprecated_AutoSpriteShape
	}

	[Header("Stance Settings")]
	[SerializeField]
	public StanceSettings[] m_stanceSettings;

	[Header("Colliders")]
	[FormerlySerializedAs("m_collider")]
	[SerializeField]
	private CapsuleCollider2D m_movementCollider;

	[SerializeField]
	private CapsuleCollider2D m_triggerCollider;

	[SerializeField]
	private Vector2 m_collisionCheckSize;

	[SerializeField]
	private Transform m_animRoot;

	private Rigidbody2D m_rigidbody;

	public UnityAction<Stance> m_onStanceChanged;

	private List<GameObject> m_trackedGameObjects;

	[SerializeField]
	private SpriteRenderer m_autoSpriteRenderer;

	private CharacterMovement m_movement;

	private Stance m_currentStance;

	public StanceSettings[] StanceSettingsArray => m_stanceSettings;

	public StanceSettings StandingSettings => GetStanceSettings(Stance.Standing);

	public Stance CurrentStance
	{
		get
		{
			return m_currentStance;
		}
		set
		{
			if (m_currentStance != value)
			{
				if (value == Stance.Deprecated_AutoSpriteShape)
				{
					Debug.LogError(base.gameObject.name + " tried to set CharacterStance to Deprecated_AutoSpriteShape which is no longer supported! Please fix", this);
				}
				StanceSettings stanceSettings = GetStanceSettings(value);
				StanceSettings stanceSettings2 = GetStanceSettings(m_currentStance);
				bool useCollision = stanceSettings.m_useCollision;
				bool useCollision2 = stanceSettings2.m_useCollision;
				m_currentStance = value;
				ApplyStanceSetting(value);
				m_onStanceChanged?.Invoke(value);
				if (useCollision != useCollision2 && m_movementCollider != null)
				{
					m_movementCollider.gameObject.SetActive(useCollision);
				}
			}
		}
	}

	public StanceSettings GetStanceSettings(Stance stance)
	{
		StanceSettings[] stanceSettings = m_stanceSettings;
		foreach (StanceSettings stanceSettings2 in stanceSettings)
		{
			if (stanceSettings2.m_stanceType == stance)
			{
				return stanceSettings2;
			}
		}
		Debug.LogError("Tried to get undefined stance settings for stance type " + stance.ToString() + " on gameobject " + base.gameObject.name);
		return null;
	}

	private void ApplyStanceSetting(Stance stance)
	{
		ApplyStanceSetting(GetStanceSettings(stance));
	}

	private void ApplyStanceSetting(StanceSettings stanceSettings)
	{
		if (m_movementCollider != null)
		{
			m_movementCollider.offset = stanceSettings.m_colliderOffset;
			m_movementCollider.size = stanceSettings.m_colliderSize;
			m_movementCollider.direction = ((m_movementCollider.size.x > m_movementCollider.size.y) ? CapsuleDirection2D.Horizontal : CapsuleDirection2D.Vertical);
			if (m_triggerCollider != null)
			{
				m_triggerCollider.offset = m_movementCollider.offset;
				m_triggerCollider.size = m_movementCollider.size;
				m_triggerCollider.direction = m_movementCollider.direction;
			}
		}
		foreach (GameObject trackedGameObject in m_trackedGameObjects)
		{
			bool active = false;
			StanceGameObjectChange[] associatedGameObjects = stanceSettings.m_associatedGameObjects;
			foreach (StanceGameObjectChange stanceGameObjectChange in associatedGameObjects)
			{
				if (stanceGameObjectChange.m_gameObject == trackedGameObject && stanceGameObjectChange.m_enable)
				{
					active = true;
				}
			}
			trackedGameObject.SetActive(active);
		}
	}

	public CapsuleCollider2D GetActiveCollider()
	{
		return m_movementCollider;
	}

	private void Awake()
	{
		m_currentStance = Stance.Standing;
		m_movement = GetComponent<CharacterMovement>();
		m_trackedGameObjects = new List<GameObject>();
		StanceSettings[] stanceSettings = m_stanceSettings;
		for (int i = 0; i < stanceSettings.Length; i++)
		{
			StanceGameObjectChange[] associatedGameObjects = stanceSettings[i].m_associatedGameObjects;
			foreach (StanceGameObjectChange stanceGameObjectChange in associatedGameObjects)
			{
				if (!m_trackedGameObjects.Contains(stanceGameObjectChange.m_gameObject))
				{
					m_trackedGameObjects.Add(stanceGameObjectChange.m_gameObject);
				}
			}
		}
		ApplyStanceSetting(m_currentStance);
	}

	public bool CanStand()
	{
		bool result = true;
		Vector3 position = m_animRoot.position;
		StanceSettings stanceSettings = GetStanceSettings(Stance.Standing);
		position.y += stanceSettings.m_colliderOffset.y + stanceSettings.m_colliderSize.y * 0.5f;
		position.y -= m_collisionCheckSize.y * 0.5f;
		Vector2 collisionCheckSize = m_collisionCheckSize;
		collisionCheckSize.x = m_movement.VerticalSize;
		Collider2D[] array = Physics2D.OverlapBoxAll(position, collisionCheckSize, 0f, GameLayers.CharacterNavigationMask);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].gameObject != base.gameObject)
			{
				PlatformEffector2D component = array[i].GetComponent<PlatformEffector2D>();
				if ((!(component != null) || !component.useOneWay) && (!(m_movement != null) || !(m_movement.AttachedStairs != null) || !m_movement.AttachedStairs.ShouldIgnoreCollider(array[i])))
				{
					result = false;
					break;
				}
			}
		}
		return result;
	}

	private void OnDrawGizmos()
	{
		if (!(m_animRoot == null))
		{
			Gizmos.color = Color.yellow;
			StanceSettings stanceSettings = GetStanceSettings(Stance.Standing);
			Vector3 position = m_animRoot.position;
			position.y += stanceSettings.m_colliderOffset.y + stanceSettings.m_colliderSize.y * 0.5f;
			position.y -= m_collisionCheckSize.y * 0.5f;
			Gizmos.DrawWireCube(position, m_collisionCheckSize);
			Gizmos.color = Color.white;
		}
	}

	public void SetIgnoreCollider(Collider2D collider, bool ignore)
	{
		if (m_movementCollider != null)
		{
			Physics2D.IgnoreCollision(m_movementCollider, collider, ignore);
		}
	}

	public Bounds GetStandingBounds()
	{
		Vector3 position = base.transform.position;
		StanceSettings stanceSettings = GetStanceSettings(Stance.Standing);
		position.x += stanceSettings.m_colliderOffset.x;
		position.y += stanceSettings.m_colliderOffset.y;
		return new Bounds(position, stanceSettings.m_colliderSize);
	}

	public Bounds GetActiveColliderBounds()
	{
		if (m_currentStance == Stance.NoCollision)
		{
			return GetStandingBounds();
		}
		return m_movementCollider.bounds;
	}
}
