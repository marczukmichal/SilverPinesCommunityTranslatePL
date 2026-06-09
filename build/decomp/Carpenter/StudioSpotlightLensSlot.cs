using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StudioSpotlightLensSlot : MonoBehaviour, IPersistentComponent
{
	[Serializable]
	private class PersistentData
	{
		public bool m_raised;
	}

	[SerializeField]
	private RectTransform m_moveTransform;

	[SerializeField]
	private Image m_lensImage;

	[SerializeField]
	private Vector2 m_raisedAnchorPosition;

	[SerializeField]
	private Vector2 m_loweredAnchorPosition;

	[SerializeField]
	private AudioEvent m_raiseAudioEvent;

	[SerializeField]
	private AudioEvent m_lowerAudioEvent;

	[SerializeField]
	private Interactable m_interactable;

	private bool m_raised = true;

	private bool m_hasLens;

	public UnityAction OnLensStateChanged;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public bool Raised => m_raised;

	public bool HasLens => m_hasLens;

	public bool IsFilteringLight()
	{
		if (HasLens)
		{
			return !m_raised;
		}
		return false;
	}

	public void SetHasLens()
	{
		m_hasLens = true;
		OnLensStateChanged?.Invoke();
	}

	private void Start()
	{
		m_moveTransform.anchoredPosition = (m_raised ? m_raisedAnchorPosition : m_loweredAnchorPosition);
		m_interactable.enabled = m_raised;
	}

	public void Toggle()
	{
		m_raised = !m_raised;
		AnimateToPosition();
		OnLensStateChanged?.Invoke();
		m_interactable.enabled = m_raised;
		if (m_persistentData != null)
		{
			m_persistentData.m_raised = m_raised;
		}
	}

	private void AnimateToPosition()
	{
		DOTween.Kill(m_moveTransform);
		m_moveTransform.DOAnchorPos(m_raised ? m_raisedAnchorPosition : m_loweredAnchorPosition, 0.1f).SetUpdate(isIndependentUpdate: true);
		if (m_raised && m_raiseAudioEvent != null)
		{
			m_raiseAudioEvent.Play2D();
		}
		else if (!m_raised && m_lowerAudioEvent != null)
		{
			m_lowerAudioEvent.Play2D();
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
			m_raised = m_persistentData.m_raised;
			OnLensStateChanged?.Invoke();
			m_interactable.enabled = m_raised;
			m_moveTransform.anchoredPosition = (m_raised ? m_raisedAnchorPosition : m_loweredAnchorPosition);
		}
		else
		{
			m_persistentData = new PersistentData();
			m_persistentDataObject.Data = m_persistentData;
		}
	}
}
