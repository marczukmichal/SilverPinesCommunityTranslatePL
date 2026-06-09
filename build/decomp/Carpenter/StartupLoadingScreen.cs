using System.Collections;
using TMPro;
using UnityEngine;

public class StartupLoadingScreen : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_text;

	[SerializeField]
	private TextMeshProUGUI m_throbberText;

	private bool m_isActive;

	private string[] m_throbLines = new string[3] { ".", "..", "..." };

	private void Show()
	{
		StartCoroutine(ThrobCoroutine());
	}

	public void Hide()
	{
		m_text.text = "";
		m_throbberText.text = "";
		StopAllCoroutines();
		m_isActive = false;
	}

	public void ShowText(string text)
	{
		base.gameObject.SetActive(value: true);
		if (!m_isActive)
		{
			Show();
		}
		m_text.text = "";
	}

	private IEnumerator ThrobCoroutine()
	{
		while (true)
		{
			m_throbberText.text = m_throbLines[0];
			yield return new WaitForSeconds(0.2f);
			m_throbberText.text = m_throbLines[1];
			yield return new WaitForSeconds(0.2f);
			m_throbberText.text = m_throbLines[2];
			yield return new WaitForSeconds(0.2f);
		}
	}
}
