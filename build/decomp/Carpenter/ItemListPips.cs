using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemListPips : MonoBehaviour
{
	[SerializeField]
	private Image m_template;

	[Header("Selected")]
	[SerializeField]
	private float m_selectedSize = 16f;

	[SerializeField]
	private Color m_selectedColor = Color.white;

	[Header("Normal")]
	[SerializeField]
	private float m_normalSize = 12f;

	[SerializeField]
	private Color m_normalColor = Color.gray;

	private List<Image> m_activePips = new List<Image>();

	private int m_currentCount;

	private void Start()
	{
		m_template.gameObject.SetActive(value: false);
	}

	public void SetPipsCount(int count)
	{
		int count2 = m_activePips.Count;
		if (count2 < count)
		{
			for (int i = count2; i < count; i++)
			{
				Image image = Object.Instantiate(m_template, m_template.transform.parent);
				image.gameObject.SetActive(value: true);
				m_activePips.Add(image);
			}
		}
		else
		{
			for (int num = count2 - 1; num >= count; num--)
			{
				m_activePips[num].gameObject.SetActive(value: false);
			}
		}
		m_currentCount = count;
	}

	public void SetSelectedPip(int selectedIndex)
	{
		for (int i = 0; i < m_currentCount; i++)
		{
			if (i == selectedIndex)
			{
				m_activePips[i].color = m_selectedColor;
				m_activePips[i].GetComponent<RectTransform>().sizeDelta = new Vector2(m_selectedSize, m_selectedSize);
			}
			else
			{
				m_activePips[i].color = m_normalColor;
				m_activePips[i].GetComponent<RectTransform>().sizeDelta = new Vector2(m_normalSize, m_normalSize);
			}
		}
	}
}
