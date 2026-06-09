using System.Collections;
using Steamworks;
using TMPro;
using UnityEngine;

public class SteamUserWatermark : MonoBehaviour
{
	[SerializeField]
	private bool m_moves;

	[SerializeField]
	private bool m_onlyPlaytestBuild;

	private TextMeshProUGUI m_text;

	private RectTransform m_rectTransform;

	private bool m_hasSteamID;

	private void Start()
	{
		bool flag = false;
		if (m_onlyPlaytestBuild)
		{
			flag = true;
		}
		if (flag)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		m_text = GetComponent<TextMeshProUGUI>();
		m_rectTransform = GetComponent<RectTransform>();
		StartCoroutine(RefreshLoop());
	}

	private IEnumerator RefreshLoop()
	{
		while (true)
		{
			if (!m_hasSteamID)
			{
				try
				{
					CSteamID steamID = SteamUser.GetSteamID();
					m_text.text = steamID.ToString();
					m_hasSteamID = true;
					if (!m_moves)
					{
						break;
					}
				}
				catch
				{
					m_text.text = "-";
				}
			}
			yield return new WaitForSecondsRealtime(Random.Range(1f, 3f));
			if (m_moves)
			{
				Reposition();
			}
		}
	}

	private void Reposition()
	{
		Vector2 vector = new Vector2(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f));
		Vector2 vector4 = (m_rectTransform.anchorMin = (m_rectTransform.anchorMax = vector));
	}
}
