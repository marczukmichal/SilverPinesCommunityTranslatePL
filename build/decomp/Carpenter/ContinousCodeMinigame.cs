using UnityEngine;
using UnityEngine.Events;

public class ContinousCodeMinigame : MonoBehaviour
{
	[SerializeField]
	private string m_targetCode;

	[SerializeField]
	private UnityEvent m_onCorrect;

	private bool m_complete;

	private string m_activeInput;

	public void RecordInput(string input)
	{
		if (!m_complete)
		{
			m_activeInput += input;
			if (m_activeInput.Length > m_targetCode.Length)
			{
				m_activeInput = m_activeInput.Substring(1);
			}
			if (m_activeInput.Equals(m_targetCode))
			{
				CompleteMinigame();
			}
		}
	}

	private void CompleteMinigame()
	{
		m_complete = true;
		m_onCorrect.Invoke();
	}
}
