using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FortuneTellerMinigame : MonoBehaviour, IPersistentComponent
{
	[Serializable]
	private struct SelectableOption
	{
		public TextMeshProUGUI m_text;

		public float m_angle;

		public GameObject m_activateGameObject;
	}

	private enum FortuneTellerState
	{
		None,
		WaitingForToken,
		Active
	}

	[Serializable]
	private class PersistentData
	{
		public List<int> m_selectedIndices;

		public bool m_hasTokenCurrently;

		public int m_pointerIndex;
	}

	[SerializeField]
	private SelectableOption[] m_options;

	[SerializeField]
	private RectTransform m_pointer;

	[SerializeField]
	private float m_pointerAnimTime = 0.3f;

	[SerializeField]
	private int m_startingIndex;

	[Header("Text Colors")]
	[SerializeField]
	private Color m_highlightedOptionColor;

	[SerializeField]
	private Color m_normalOptionColor;

	[SerializeField]
	private Color m_expendedOptionColor;

	[Header("Misc")]
	[SerializeField]
	private Image m_darkenOverlay;

	[Header("Token Slot")]
	[SerializeField]
	private Interactable m_tokenSlot;

	private List<int> m_selectedIndices = new List<int>();

	private int m_pointerIndex;

	private FortuneTellerState m_state;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	private FortuneTellerState State
	{
		get
		{
			return m_state;
		}
		set
		{
			if (m_state != value)
			{
				m_state = value;
				m_tokenSlot.SetInteractEnabled(m_state != FortuneTellerState.Active);
				m_darkenOverlay.gameObject.SetActive(m_state != FortuneTellerState.Active);
				if (m_persistentData != null)
				{
					m_persistentData.m_hasTokenCurrently = m_state == FortuneTellerState.Active;
				}
			}
		}
	}

	private void Start()
	{
		if (m_state == FortuneTellerState.None)
		{
			SetPointerIndex(m_startingIndex, animated: false);
			State = FortuneTellerState.WaitingForToken;
		}
	}

	public void RightButtonPressed()
	{
		if (m_state == FortuneTellerState.Active)
		{
			OffsetPointerIndex(1);
		}
	}

	public void LeftButtonPressed()
	{
		if (m_state == FortuneTellerState.Active)
		{
			OffsetPointerIndex(-1);
		}
	}

	private void OffsetPointerIndex(int offset)
	{
		int value = m_pointerIndex + offset;
		value = Mathf.Clamp(value, 0, m_options.Length - 1);
		if (value != m_pointerIndex)
		{
			SetPointerIndex(value, animated: true);
		}
	}

	private void SetPointerIndex(int newSelected, bool animated)
	{
		m_pointerIndex = newSelected;
		if (m_persistentData != null)
		{
			m_persistentData.m_pointerIndex = m_pointerIndex;
		}
		DOTween.Kill(m_pointer);
		if (animated)
		{
			m_pointer.DORotate(new Vector3(0f, 0f, m_options[m_pointerIndex].m_angle), m_pointerAnimTime).SetEase(Ease.OutBounce).SetUpdate(isIndependentUpdate: true);
		}
		else
		{
			m_pointer.rotation = Quaternion.Euler(0f, 0f, m_options[m_pointerIndex].m_angle);
		}
		RefreshTextColors();
	}

	private void RefreshTextColors()
	{
		for (int i = 0; i < m_options.Length; i++)
		{
			Color color = m_normalOptionColor;
			if (m_selectedIndices.Contains(i))
			{
				color = m_expendedOptionColor;
			}
			else if (i == m_pointerIndex)
			{
				color = m_highlightedOptionColor;
			}
			m_options[i].m_text.color = color;
		}
	}

	public void SelectOption()
	{
		if (m_state == FortuneTellerState.Active && !m_selectedIndices.Contains(m_pointerIndex))
		{
			State = FortuneTellerState.WaitingForToken;
			m_tokenSlot.ResetInteractData();
			m_selectedIndices.Add(m_pointerIndex);
			if (m_persistentData != null)
			{
				m_persistentData.m_selectedIndices.Add(m_pointerIndex);
			}
			if ((bool)m_options[m_pointerIndex].m_activateGameObject)
			{
				m_options[m_pointerIndex].m_activateGameObject.SetActive(value: true);
			}
			RefreshTextColors();
		}
	}

	public void AddToken()
	{
		State = FortuneTellerState.Active;
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
			m_selectedIndices = new List<int>(m_persistentData.m_selectedIndices);
			SetPointerIndex(m_persistentData.m_pointerIndex, animated: false);
			if (m_persistentData.m_hasTokenCurrently)
			{
				State = FortuneTellerState.Active;
			}
			else
			{
				State = FortuneTellerState.WaitingForToken;
			}
			{
				foreach (int selectedIndex in m_selectedIndices)
				{
					if ((bool)m_options[selectedIndex].m_activateGameObject)
					{
						m_options[selectedIndex].m_activateGameObject.SetActive(value: true);
					}
				}
				return;
			}
		}
		m_persistentData = new PersistentData();
		m_persistentDataObject.Data = m_persistentData;
		m_persistentData.m_selectedIndices = new List<int>();
	}
}
