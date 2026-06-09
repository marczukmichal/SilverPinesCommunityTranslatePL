using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ComputerLoginWindow : MonoBehaviour
{
	[SerializeField]
	private string m_requiredUsername;

	[SerializeField]
	private string m_requiredPassword;

	[SerializeField]
	private TMP_InputField m_usernameInputField;

	[SerializeField]
	private TMP_InputField m_passwordField;

	[SerializeField]
	private TextMeshProUGUI m_errorMessageText;

	[SerializeField]
	private string m_incorrectUserMessage;

	[SerializeField]
	private string m_incorrectPasswordMessage;

	[SerializeField]
	private UnityEvent m_onLoginSuccess;

	public void TryLogin()
	{
		bool flag = m_usernameInputField.text.ToLower().Equals(m_requiredUsername.ToLower());
		bool flag2 = m_passwordField.text.ToLower().Equals(m_requiredPassword.ToLower());
		if (flag && flag2)
		{
			LoginSuccess();
			return;
		}
		m_errorMessageText.gameObject.SetActive(value: true);
		StartCoroutine(FlashErrorMessageText());
		if (flag)
		{
			m_errorMessageText.text = m_incorrectPasswordMessage;
		}
		else
		{
			m_errorMessageText.text = m_incorrectUserMessage;
		}
	}

	public void LoginSuccess()
	{
		m_errorMessageText.gameObject.SetActive(value: false);
		m_onLoginSuccess.Invoke();
	}

	private IEnumerator FlashErrorMessageText()
	{
		m_errorMessageText.gameObject.SetActive(value: true);
		for (int i = 0; i < 8; i++)
		{
			yield return new WaitForSeconds(0.25f);
			m_errorMessageText.gameObject.SetActive(value: false);
			yield return new WaitForSeconds(0.25f);
			m_errorMessageText.gameObject.SetActive(value: true);
		}
	}
}
