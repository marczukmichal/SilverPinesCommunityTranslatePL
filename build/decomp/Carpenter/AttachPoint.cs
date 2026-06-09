using UnityEngine;

public class AttachPoint : MonoBehaviour
{
	[SerializeField]
	private AttachType m_attachType;

	[ReadOnly]
	[SerializeField]
	private int m_persistentID;

	private AttachableObject m_attachedObject;

	public AttachType AttachType => m_attachType;

	public int PersistentID => m_persistentID;

	public AttachableObject AttachedObject => m_attachedObject;

	private void OnValidate()
	{
		AttachPoint[] array = Object.FindObjectsByType<AttachPoint>(FindObjectsSortMode.None);
		bool flag = false;
		while (!flag)
		{
			flag = true;
			AttachPoint[] array2 = array;
			foreach (AttachPoint attachPoint in array2)
			{
				if (!(attachPoint == this) && attachPoint.PersistentID == m_persistentID)
				{
					flag = false;
					m_persistentID++;
					break;
				}
			}
		}
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.Sets.Generic.AttachPointsSet.Add(this);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Sets.Generic.AttachPointsSet.Remove(this);
	}

	public bool CanAttachObject(AttachableObject attachable)
	{
		return m_attachedObject == null;
	}

	public void AttachObject(AttachableObject attachable)
	{
		m_attachedObject = attachable;
		attachable.SetAttached(this);
	}

	public void Clear()
	{
		m_attachedObject = null;
	}

	public void ForceDetatch()
	{
		if (m_attachedObject != null)
		{
			m_attachedObject.SetAttached(null);
			m_attachedObject = null;
		}
	}
}
