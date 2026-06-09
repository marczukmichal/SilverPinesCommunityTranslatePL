using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComputerMessagesViewer : MonoBehaviour
{
	[SerializeField]
	private ComputerMessages m_messages;

	[SerializeField]
	private Button m_messagesButtonTemplate;

	[SerializeField]
	private TextMeshProUGUI m_text;

	public void Start()
	{
		if (!(m_messages != null))
		{
			return;
		}
		ComputerMessages.ComputerMessage[] messages = m_messages.m_messages;
		for (int i = 0; i < messages.Length; i++)
		{
			ComputerMessages.ComputerMessage message = messages[i];
			Button button = Object.Instantiate(m_messagesButtonTemplate, m_messagesButtonTemplate.transform.parent);
			button.name = message.m_subject;
			button.gameObject.SetActive(value: true);
			button.GetComponentInChildren<TextMeshProUGUI>().text = message.m_subject;
			button.onClick.AddListener(delegate
			{
				ShowMessage(message);
			});
			message.m_onShownMessage.Invoke();
		}
	}

	private void ShowMessage(ComputerMessages.ComputerMessage message)
	{
		m_text.text = message.m_content;
		message.m_onShownMessage.Invoke();
	}
}
