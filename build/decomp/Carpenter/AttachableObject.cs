using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Localization;

public class AttachableObject : BaseInteractable, IPersistentComponent
{
	[Serializable]
	private class PersistentData
	{
		public int m_attachedID;
	}

	[Header("Attachable Object")]
	[SerializeField]
	private AttachPoint m_startingAttachPoint;

	[SerializeField]
	private float m_transferDistance = 0.5f;

	[Header("Rendering")]
	[SerializeField]
	private SpriteRenderer m_spriteRenderer;

	[SerializeField]
	private Color m_foregroundColor = Color.white;

	[SerializeField]
	private Color m_backgroundColor = Color.gray;

	[Header("Strings")]
	[SerializeField]
	private LocalizedString m_interactString;

	private AttachPoint m_activeAttachedPoint;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public override string InteractString => m_interactString.GetLocalizedString();

	public override bool CanInteract(BaseInteractor interactor)
	{
		if (!base.CanInteract(interactor))
		{
			return false;
		}
		AttachPoint transferAttachPoint = GetTransferAttachPoint();
		if (GetTransferAttachPoint() != null)
		{
			return transferAttachPoint.CanAttachObject(this);
		}
		return false;
	}

	private AttachPoint GetTransferAttachPoint()
	{
		foreach (AttachPoint item in GlobalReferences.Instance.Sets.Generic.AttachPointsSet)
		{
			if (!(item == m_activeAttachedPoint) && m_activeAttachedPoint.AttachType != item.AttachType && Vector2.Distance(item.transform.position, base.transform.position) < m_transferDistance)
			{
				return item;
			}
		}
		return null;
	}

	protected override void Start()
	{
		base.Start();
		if (m_activeAttachedPoint == null && m_startingAttachPoint != null)
		{
			m_startingAttachPoint.AttachObject(this);
		}
	}

	public void SetAttached(AttachPoint attachPoint)
	{
		if (m_activeAttachedPoint != null)
		{
			m_activeAttachedPoint.Clear();
		}
		m_activeAttachedPoint = attachPoint;
		int attachedID = -1;
		if (attachPoint != null)
		{
			attachedID = attachPoint.PersistentID;
			base.transform.SetParent(attachPoint.transform, worldPositionStays: true);
			base.transform.DOLocalMove(Vector3.zero, 0.2f);
			if (m_spriteRenderer != null)
			{
				m_spriteRenderer.DOColor((attachPoint.AttachType == AttachType.Foreground) ? m_foregroundColor : m_backgroundColor, 0.2f);
			}
		}
		if (m_persistentData != null)
		{
			m_persistentData.m_attachedID = attachedID;
		}
	}

	public override void Interact(BaseInteractor interactor)
	{
		AttachPoint transferAttachPoint = GetTransferAttachPoint();
		if (transferAttachPoint != null)
		{
			transferAttachPoint.AttachObject(this);
		}
	}

	public bool RequiresPersistentData()
	{
		return true;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData != null)
		{
			AttachPoint[] array = UnityEngine.Object.FindObjectsByType<AttachPoint>(FindObjectsSortMode.None);
			foreach (AttachPoint attachPoint in array)
			{
				if (attachPoint.PersistentID == m_persistentData.m_attachedID)
				{
					attachPoint.AttachObject(this);
					break;
				}
			}
		}
		else
		{
			m_persistentData = new PersistentData();
			m_persistentDataObject.Data = m_persistentData;
			m_startingAttachPoint.AttachObject(this);
		}
	}
}
