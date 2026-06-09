using UnityEngine;
using UnityEngine.Events;

public class DigSpot : BaseAnimatedButtonSequence
{
	[SerializeField]
	private Transform m_digTransform;

	[SerializeField]
	private float m_digOffset;

	[SerializeField]
	private float m_range = 1f;

	[SerializeField]
	private UnityEvent m_onDigComplete;

	private Vector3 m_startingPosition;

	private void Awake()
	{
		m_startingPosition = m_digTransform.position;
	}

	private void Start()
	{
		if (!m_isDone)
		{
			Vector3 startingPosition = m_startingPosition;
			startingPosition.y += m_digOffset;
			m_digTransform.transform.position = startingPosition;
			BaseInteractable componentInChildren = m_digTransform.GetComponentInChildren<BaseInteractable>();
			if (componentInChildren != null)
			{
				componentInChildren.enabled = false;
			}
		}
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.Sets.Generic.DigSpotsSet.Add(this);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Sets.Generic.DigSpotsSet.Remove(this);
	}

	public override void OnProgressUpdated()
	{
		base.OnProgressUpdated();
		float num = Mathf.Clamp01((float)base.DoneSequences / (float)base.SequenceCount);
		Vector3 startingPosition = m_startingPosition;
		startingPosition.y += m_digOffset * (1f - num);
		m_digTransform.transform.position = startingPosition;
	}

	public override void OnComplete()
	{
		base.OnComplete();
		BaseInteractable componentInChildren = m_digTransform.GetComponentInChildren<BaseInteractable>();
		if (componentInChildren != null)
		{
			componentInChildren.enabled = true;
		}
		m_onDigComplete.Invoke();
	}

	public override void OnCancel()
	{
	}

	public bool IsInRange(Vector2 position)
	{
		return Vector2.Distance(position, base.transform.position) < m_range;
	}

	public static DigSpot FindDigSpot(Vector2 position)
	{
		foreach (DigSpot item in GlobalReferences.Instance.Sets.Generic.DigSpotsSet)
		{
			if (!item.IsDone && item.IsInRange(position))
			{
				return item;
			}
		}
		return null;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.DrawWireSphere(base.transform.position, m_range);
	}
}
