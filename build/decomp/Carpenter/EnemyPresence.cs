using UnityEngine;

public class EnemyPresence : MonoBehaviour
{
	public enum EnemyPresenceType
	{
		Normal,
		Large,
		Small,
		Boss
	}

	[SerializeField]
	private Transform m_root;

	[SerializeField]
	private CharacterStance m_stance;

	[SerializeField]
	private Collider2D m_collider;

	[SerializeField]
	private EnemyPresenceType m_type;

	private bool m_hasEverBeenSeen;

	private bool m_isVisible;

	private CharacterHealth m_characterHealth;

	private AIBrain m_aiBrain;

	public EnemyPresenceType EnemyType => m_type;

	public Transform RootTransform => m_root;

	public Bounds Bounds
	{
		get
		{
			if (m_stance != null)
			{
				return m_stance.GetActiveColliderBounds();
			}
			if (m_collider != null)
			{
				return m_collider.bounds;
			}
			return new Bounds(base.transform.position, new Vector3(0.25f, 0.25f, 0.25f));
		}
	}

	private void Awake()
	{
		m_characterHealth = GetComponent<CharacterHealth>();
		m_aiBrain = GetComponent<AIBrain>();
	}

	public bool IsDead()
	{
		if (m_characterHealth != null)
		{
			return m_characterHealth.IsDead;
		}
		return false;
	}

	public bool IsKnown()
	{
		return m_hasEverBeenSeen;
	}

	public bool HasDirectLineOfSight()
	{
		return m_isVisible;
	}

	public bool IsAlert()
	{
		if (m_aiBrain != null)
		{
			return m_aiBrain.CurrentAlertState == AIBrain.AlertState.Combat;
		}
		return true;
	}

	private void Reset()
	{
		m_root = base.transform;
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.Sets.Generic.EnemyPresenceSet.Add(this);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Sets.Generic.EnemyPresenceSet.Remove(this);
	}

	private void Update()
	{
		bool visible = FieldOfViewArea.IsBoundsVisible(Bounds);
		SetVisible(visible);
	}

	private void SetVisible(bool visible)
	{
		if (m_isVisible != visible)
		{
			m_isVisible = visible;
		}
		if (m_isVisible)
		{
			m_hasEverBeenSeen = true;
		}
	}
}
