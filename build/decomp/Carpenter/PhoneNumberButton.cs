using TMPro;
using UnityEngine;

public class PhoneNumberButton : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_nameText;

	[SerializeField]
	private TextMeshProUGUI m_numberText;

	private string m_number;

	private PhoneMinigame m_phone;

	public void Setup(PhoneNumber number, PhoneMinigame phoneMinigame)
	{
		m_number = number.m_number;
		m_phone = phoneMinigame;
		m_nameText.text = number.PhoneName;
		string text = number.m_number;
		if (text.Length > 3)
		{
			text = text.Insert(3, "-");
		}
		m_numberText.text = text;
	}

	public void Pressed()
	{
		m_phone.SetNumber(m_number);
	}
}
