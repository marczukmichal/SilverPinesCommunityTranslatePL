using System;
using TMPro;
using UnityEngine;

public class SystemClock : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_text;

	private void Update()
	{
		DateTime now = DateTime.Now;
		m_text.text = now.ToString("HH:mm");
	}
}
