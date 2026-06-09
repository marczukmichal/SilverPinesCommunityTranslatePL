using UnityEngine;

public class LedgeGrabLevelTransition : MonoBehaviour
{
	[Header("Exit Settings")]
	[SerializeField]
	private float m_radius;

	[Header("Entry Settings")]
	[SerializeField]
	private Vector2 m_enterPositionOffset;

	[SerializeField]
	private CharacterDirection.Facing m_enterDirection;

	[SerializeField]
	private float m_spawnDistanceFromGround = 0.75f;

	public CharacterDirection.Facing EnterDirection => m_enterDirection;

	public float SpawnDistanceFromGround => m_spawnDistanceFromGround;

	public Vector3 EntryPosition
	{
		get
		{
			Vector3 position = base.transform.position;
			position.x += m_enterPositionOffset.x;
			position.y += m_enterPositionOffset.y;
			return position;
		}
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.Sets.Generic.LedgeGrabLevelTransitionSet.Add(this);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Sets.Generic.LedgeGrabLevelTransitionSet.Remove(this);
	}

	public bool IncludesLedgeGrabLocation(Vector2 position)
	{
		return Vector2.Distance(base.transform.position, position) < m_radius;
	}

	public void Trigger()
	{
		GetComponent<LevelTransition>().DoTransition();
	}

	private void OnDrawGizmosSelected()
	{
		Vector3 entryPosition = EntryPosition;
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(entryPosition, 0.1f);
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(base.transform.position, m_radius);
		Gizmos.color = Color.white;
	}
}
