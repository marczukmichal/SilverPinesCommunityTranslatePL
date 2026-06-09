using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MapMarkerOptionsButton : MonoBehaviour
{
	[SerializeField]
	private Image m_image;

	[SerializeField]
	private Button m_button;

	public UnityAction<int> OnButtonClicked;

	private int m_spriteIndex;

	public int SpriteIndex => m_spriteIndex;

	public void SetIndex(int index, Sprite sprite)
	{
		m_image.sprite = sprite;
		m_spriteIndex = index;
	}

	public void OnButtonPressed()
	{
		OnButtonClicked?.Invoke(m_spriteIndex);
	}

	public void SetHighlight(bool highlight)
	{
		m_image.color = (highlight ? Color.white : Color.gray);
	}
}
