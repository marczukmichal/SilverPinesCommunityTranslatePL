using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class ContextualAttackPrompt : MonoBehaviour
{
	private enum State
	{
		None,
		Stomp
	}

	[Header("UI")]
	[SerializeField]
	private RectTransform m_container;

	[SerializeField]
	private RectTransform m_positionTransform;

	[SerializeField]
	private RectTransform m_parentRect;

	[SerializeField]
	private TextMeshProUGUI m_text;

	[SerializeField]
	private Canvas m_parentCanvas;

	[Header("Strings")]
	[SerializeField]
	private LocalizedString m_stompString;

	private StompActionCheck m_cachedStompAttackCheck;

	private State m_promptState;

	public StompActionCheck StompAttackCheck
	{
		get
		{
			if (m_cachedStompAttackCheck == null)
			{
				GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
				if (item != null)
				{
					m_cachedStompAttackCheck = item.GetComponentInChildren<StompActionCheck>();
				}
			}
			return m_cachedStompAttackCheck;
		}
	}

	private State PromptState
	{
		set
		{
			if (m_promptState != value)
			{
				m_promptState = value;
				if (m_promptState == State.Stomp)
				{
					m_text.text = m_stompString.GetLocalizedString();
				}
				m_container.gameObject.SetActive(m_promptState != State.None);
			}
		}
	}

	private void Start()
	{
		m_container.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		State promptState = State.None;
		if (StompAttackCheck != null && StompAttackCheck.HasStompTarget && Camera.main != null)
		{
			promptState = State.Stomp;
			PositionAtWorldPosition(StompAttackCheck.PromptPosition);
		}
		PromptState = promptState;
	}

	private void PositionAtWorldPosition(Vector3 worldPosition)
	{
		Vector3 vector = Camera.main.WorldToScreenPoint(worldPosition);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(m_parentRect, vector, m_parentCanvas.worldCamera, out var localPoint);
		m_positionTransform.anchoredPosition = localPoint;
	}
}
