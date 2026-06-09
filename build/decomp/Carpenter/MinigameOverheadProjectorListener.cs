using System;
using System.Collections.Generic;
using UnityEngine;

public class MinigameOverheadProjectorListener : MonoBehaviour, IPersistentComponent
{
	[Serializable]
	private class ElementTransforms
	{
		public bool m_active;

		public Vector2 m_anchoredPosition;

		public float m_rotation;
	}

	[Serializable]
	private class PersistentData
	{
		public List<ElementTransforms> m_elements;
	}

	[SerializeField]
	private RectTransform[] m_projectorElements;

	private MinigameAlignmentCheck m_alignmentCheck;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public void RegisterAlignmentMinigame(MinigameAlignmentCheck alignmentCheck)
	{
		m_alignmentCheck = alignmentCheck;
	}

	private void Update()
	{
		if (m_alignmentCheck != null && m_persistentData != null)
		{
			int num = 0;
			MinigameFreeMoveObject[] alignObjects = m_alignmentCheck.AlignObjects;
			foreach (MinigameFreeMoveObject minigameFreeMoveObject in alignObjects)
			{
				if (!minigameFreeMoveObject.gameObject.activeSelf)
				{
					m_persistentData.m_elements[num].m_active = false;
				}
				else
				{
					m_persistentData.m_elements[num].m_active = true;
					RectTransform component = minigameFreeMoveObject.GetComponent<RectTransform>();
					m_persistentData.m_elements[num].m_anchoredPosition = component.anchoredPosition;
					m_persistentData.m_elements[num].m_rotation = component.localRotation.eulerAngles.z;
				}
				num++;
			}
		}
		if (m_persistentData == null)
		{
			return;
		}
		int num2 = 0;
		foreach (ElementTransforms element in m_persistentData.m_elements)
		{
			if (!element.m_active)
			{
				m_projectorElements[num2].gameObject.SetActive(value: false);
			}
			else
			{
				m_projectorElements[num2].gameObject.SetActive(value: true);
				m_projectorElements[num2].anchoredPosition = element.m_anchoredPosition;
				m_projectorElements[num2].localRotation = Quaternion.Euler(0f, 0f, element.m_rotation);
			}
			num2++;
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
		if (m_persistentData == null)
		{
			m_persistentData = new PersistentData();
			m_persistentDataObject.Data = m_persistentData;
			m_persistentData.m_elements = new List<ElementTransforms>();
			for (int i = 0; i < m_projectorElements.Length; i++)
			{
				m_persistentData.m_elements.Add(new ElementTransforms
				{
					m_active = false
				});
			}
		}
	}
}
