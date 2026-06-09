using UnityEngine;
using UnityEngine.UI;

public class ContinousCodeButton : MonoBehaviour
{
	[SerializeField]
	private char m_value;

	[SerializeField]
	private Button m_button;

	private void Awake()
	{
		m_button.onClick.AddListener(OnClicked);
	}

	private void OnClicked()
	{
		ContinousCodeMinigame[] componentsInParent = GetComponentsInParent<ContinousCodeMinigame>();
		for (int i = 0; i < componentsInParent.Length; i++)
		{
			componentsInParent[i].RecordInput(m_value.ToString());
		}
	}
}
