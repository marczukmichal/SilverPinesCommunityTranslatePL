using UnityEngine;

[DisallowMultipleComponent]
public class AIPatrolStrategy : BaseAIStrategy, IPersistentComponent
{
	[HideInInspector]
	[SerializeField]
	private PatrolState m_patrolState;

	[SerializeField]
	private PatrolPath m_patrolPath;

	[SerializeField]
	private float m_goalDistance = 1f;

	private PersistentDataObject m_persistentDataObject;

	public override Vector2 MoveDirection => m_patrolState.MoveDirection;

	public bool RequiresPersistentData()
	{
		return true;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		PatrolState patrolState = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PatrolState) : null);
		if (patrolState != null)
		{
			m_patrolState = patrolState;
		}
		else
		{
			m_persistentDataObject.Data = m_patrolState;
		}
	}

	private void Awake()
	{
		if (m_patrolState == null)
		{
			m_patrolState = new PatrolState();
			if (m_persistentDataObject != null)
			{
				m_persistentDataObject.Data = m_patrolState;
			}
		}
	}

	public override void UpdateStrategy()
	{
		if (!(m_patrolPath == null))
		{
			m_patrolState.Update(m_patrolPath, m_goalDistance, base.transform.position);
		}
	}
}
