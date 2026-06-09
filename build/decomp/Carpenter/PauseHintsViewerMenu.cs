using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseHintsViewerMenu : MonoBehaviour
{
	[SerializeField]
	private PauseHintInfo[] m_pauseHints;

	[SerializeField]
	private GameObject m_buttonTemplate;

	[SerializeField]
	private PauseHintInfoUI m_hintInfoUI;

	private PauseHintInfo m_activeHint;

	private void Start()
	{
		PauseHintInfo[] pauseHints = m_pauseHints;
		foreach (PauseHintInfo hintInfo in pauseHints)
		{
			Button component = Object.Instantiate(m_buttonTemplate, m_buttonTemplate.transform.parent).GetComponent<Button>();
			component.gameObject.SetActive(value: true);
			component.GetComponentInChildren<TextMeshProUGUI>().text = hintInfo.TitleText;
			component.onClick.AddListener(delegate
			{
				PlayHint(hintInfo);
			});
		}
	}

	private void PlayHint(PauseHintInfo pauseHintInfo)
	{
		m_hintInfoUI.gameObject.SetActive(value: true);
		m_hintInfoUI.SetHintData(pauseHintInfo);
	}
}
