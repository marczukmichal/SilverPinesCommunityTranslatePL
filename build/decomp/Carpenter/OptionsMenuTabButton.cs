using UnityEngine;
using UnityEngine.UI;

public class OptionsMenuTabButton : MonoBehaviour
{
	[SerializeField]
	private Image m_backgroundImage;

	[SerializeField]
	private GameObject m_tabGameObject;

	[Header("Colors")]
	[SerializeField]
	private Color m_normalBackgroundColor;

	[SerializeField]
	private Color m_tabActiveBackgroundColor;

	private bool m_isTabActive;

	public GameObject TabGameObject => m_tabGameObject;

	public bool IsTabActive => m_isTabActive;

	public void SetTabActive(bool tabActive)
	{
		if (m_isTabActive != tabActive)
		{
			m_isTabActive = tabActive;
			m_backgroundImage.color = (m_isTabActive ? m_tabActiveBackgroundColor : m_normalBackgroundColor);
		}
	}
}
