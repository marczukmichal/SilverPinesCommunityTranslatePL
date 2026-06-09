using System.Collections;
using TMPro;
using UnityEngine;

public class ComputerMemoryMonitor : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_text;

	[SerializeField]
	private ComputerDesktop m_desktop;

	private IEnumerator Start()
	{
		m_text.text = "";
		while (true)
		{
			yield return new WaitForSecondsRealtime(Random.Range(0.75f, 1.5f));
			m_text.text = "MEMUSE " + m_desktop.GetCurrentMemoryUsage();
		}
	}
}
