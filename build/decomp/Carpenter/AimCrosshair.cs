using UnityEngine;
using UnityEngine.UI;

public class AimCrosshair : MonoBehaviour
{
	[SerializeField]
	private GameObjectAnchor m_playerAnchor;

	[SerializeField]
	private Vector2 m_worldSpaceOffset;

	[SerializeField]
	private float m_aimOffsetScale;

	[SerializeField]
	private Image m_crosshairImage;

	[SerializeField]
	private InputState m_inputState;

	[SerializeField]
	private MouseAimInputData m_mouseAimInputData;

	[SerializeField]
	private RectTransform m_parentRect;

	[SerializeField]
	private Canvas m_parentCanvas;

	private RectTransform m_rectTransform;

	private void Start()
	{
		m_rectTransform = m_crosshairImage.GetComponent<RectTransform>();
		m_crosshairImage.gameObject.SetActive(value: false);
	}
}
