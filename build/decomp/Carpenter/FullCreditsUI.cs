using UnityEngine;

public class FullCreditsUI : MonoBehaviour
{
	[SerializeField]
	private RectTransform m_scrollTransform;

	[SerializeField]
	private float m_speed;

	[SerializeField]
	private GameCompletionSummary m_summary;

	private float m_proceedTimer;

	private bool m_shownSummary;

	private void Awake()
	{
		m_summary.gameObject.SetActive(value: false);
	}

	private void Start()
	{
		m_scrollTransform.anchoredPosition = Vector2.zero;
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.GameMenuState.SetInMenu(GameMenuState.GameMenu.FullCredits);
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.UI.Enable();
			GameInputManager.GameInputActions.Game.Enable();
		}
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.FullCredits);
	}

	private void Update()
	{
		float num = m_speed;
		if (GameInputManager.GameInputActions != null && (GameInputManager.GameInputActions.UI.Click.IsPressed() || GameInputManager.GameInputActions.UI.Submit.IsPressed() || GameInputManager.GameInputActions.Game.Skip.IsPressed()))
		{
			num *= 10f;
		}
		Vector2 target = new Vector3(0f, m_scrollTransform.sizeDelta.y);
		m_scrollTransform.anchoredPosition = Vector2.MoveTowards(m_scrollTransform.anchoredPosition, target, num * Time.deltaTime);
		if (m_scrollTransform.anchoredPosition.y >= target.y)
		{
			m_proceedTimer += Time.deltaTime;
		}
		else
		{
			m_proceedTimer = 0f;
		}
		if (m_proceedTimer >= 1f && !m_shownSummary)
		{
			m_shownSummary = true;
			CreditsComplete();
		}
	}

	private void CreditsComplete()
	{
		m_summary.Show();
	}
}
