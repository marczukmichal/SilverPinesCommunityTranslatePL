using System.Collections.Generic;
using UnityEngine;

public class MapIconGroup : Map3DIcon
{
	private List<Map3DIcon> m_mapIcons = new List<Map3DIcon>();

	private RectTransform m_rectTransform;

	public List<Map3DIcon> Icons => m_mapIcons;

	public static MapIconGroup Create(RectTransform iconsParent, Map3DIcon iconA, Map3DIcon iconB)
	{
		GameObject obj = new GameObject("Map Icon Group");
		RectTransform rectTransform = obj.AddComponent<RectTransform>();
		rectTransform.sizeDelta = new Vector2(48f, 64f);
		rectTransform.pivot = new Vector2(0.5f, 0.5f);
		MapIconGroup mapIconGroup = obj.AddComponent<MapIconGroup>();
		mapIconGroup.Setup(iconsParent, iconA, iconB);
		return mapIconGroup;
	}

	public void Setup(RectTransform parent, Map3DIcon iconA, Map3DIcon iconB)
	{
		m_normalScale = (m_hoveredScale = 1f);
		m_rectTransform = GetComponent<RectTransform>();
		m_rectTransform.SetParent(parent, worldPositionStays: false);
		m_mapIcons = new List<Map3DIcon>();
		AddIcon(iconA);
		AddIcon(iconB);
	}

	public void Cleanup()
	{
		if (m_rectTransform != null)
		{
			Object.Destroy(base.gameObject);
			m_rectTransform = null;
		}
	}

	public Vector3 GetWorldPosition()
	{
		if (m_mapIcons.Count == 0)
		{
			return Vector3.zero;
		}
		Vector3 zero = Vector3.zero;
		foreach (Map3DIcon mapIcon in m_mapIcons)
		{
			zero += mapIcon.MapWorldPosition;
		}
		return zero / m_mapIcons.Count;
	}

	public void AddIcon(Map3DIcon icon)
	{
		if (!m_mapIcons.Contains(icon))
		{
			m_mapIcons.Add(icon);
			icon.transform.SetParent(m_rectTransform);
			icon.SetGroup(this);
			icon.transform.SetAsFirstSibling();
			UpdateDecoration();
		}
	}

	public void RemoveIcon(Map3DIcon icon)
	{
		m_mapIcons.Remove(icon);
		icon.transform.SetParent(m_rectTransform.parent);
		icon.RectTransform.anchoredPosition = Vector2.zero;
		icon.SetGroup(null);
		UpdateDecoration();
	}

	public void PositionGroup(Map3D map)
	{
		Vector3 vector = map.GetWorldToViewportPoint(GetWorldPosition());
		Vector2 vector4 = (m_rectTransform.anchorMin = (m_rectTransform.anchorMax = vector));
		m_rectTransform.anchoredPosition = Vector2.zero;
		PositionLocalIcons();
	}

	public void PositionLocalIcons()
	{
		Vector2 anchoredPosition = new Vector2((float)m_mapIcons.Count * 8f * 0.5f, 0f);
		for (int i = 0; i < m_mapIcons.Count; i++)
		{
			Vector2 vector3 = (m_mapIcons[i].RectTransform.anchorMin = (m_mapIcons[i].RectTransform.anchorMax = new Vector2(0.5f, 0.5f)));
			m_mapIcons[i].RectTransform.anchoredPosition = anchoredPosition;
			anchoredPosition.x -= m_mapIcons[i].RectTransform.rect.width * m_mapIcons[i].transform.localScale.x;
		}
	}

	protected override void UpdateDecoration()
	{
		base.UpdateDecoration();
		PositionLocalIcons();
		m_rectTransform.sizeDelta = new Vector3(48f + 8f * (float)m_mapIcons.Count, 64f + 4f * (float)m_mapIcons.Count);
	}
}
