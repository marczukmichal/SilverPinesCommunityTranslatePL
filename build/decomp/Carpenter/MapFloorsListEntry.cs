using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapFloorsListEntry : MonoBehaviour
{
	public enum FloorIconState
	{
		Inactive,
		Active,
		InactiveWithPlayer
	}

	[SerializeField]
	private Color m_floorIconActive;

	[SerializeField]
	private Color m_floorIconInactive;

	[SerializeField]
	private Color m_floorIconPlayer;

	[SerializeField]
	private GameObject m_line;

	[SerializeField]
	private Image m_image;

	[Header("Text")]
	[SerializeField]
	private TextMeshProUGUI m_text;

	[SerializeField]
	private Color m_textActiveColor;

	[SerializeField]
	private float m_textActiveFontSize = 72f;

	[SerializeField]
	private Vector2 m_textActiveAnchorPosition;

	[SerializeField]
	private Color m_textInactiveColor;

	[SerializeField]
	private float m_textInactiveFontSize = 28f;

	[SerializeField]
	private Vector2 m_textInactiveAnchorPosition;

	private Map3DFloor m_floor;

	public Map3DFloor Floor => m_floor;

	public void Setup(Map3DFloor floor)
	{
		m_floor = floor;
		m_text.text = floor.FloorName;
	}

	public void SetState(FloorIconState state)
	{
		bool flag = state == FloorIconState.Active;
		m_line.SetActive(flag);
		switch (state)
		{
		case FloorIconState.Inactive:
			m_image.color = m_floorIconInactive;
			break;
		case FloorIconState.Active:
			m_image.color = m_floorIconActive;
			break;
		case FloorIconState.InactiveWithPlayer:
			m_image.color = m_floorIconPlayer;
			break;
		}
		m_text.color = (flag ? m_textActiveColor : m_textInactiveColor);
		m_text.fontSize = (flag ? m_textActiveFontSize : m_textInactiveFontSize);
		m_text.rectTransform.anchoredPosition = (flag ? m_textActiveAnchorPosition : m_textInactiveAnchorPosition);
	}
}
