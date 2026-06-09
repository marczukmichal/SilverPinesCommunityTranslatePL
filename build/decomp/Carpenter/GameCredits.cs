using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameCredits : MonoBehaviour
{
	[Serializable]
	public class CreditsEntry
	{
		public string m_pageTitle;

		[FormerlySerializedAs("m_entries")]
		public string[] m_names;

		public float m_delay;
	}

	[Header("Credits")]
	[FormerlySerializedAs("m_entries")]
	[SerializeField]
	private CreditsEntry[] m_creditsPages;

	[Header("UI")]
	[SerializeField]
	private Image m_gameLogo;

	[SerializeField]
	private Image m_background;

	[SerializeField]
	private TextMeshProUGUI m_titleText;

	[SerializeField]
	private TextMeshProUGUI[] m_nameTexts;

	[SerializeField]
	private CanvasGroup m_pageCanvasGroup;

	[Header("Anim Times")]
	[SerializeField]
	private float m_logoInitialFadeInTime = 3f;

	[SerializeField]
	private float m_logoWaitTime = 2f;

	[SerializeField]
	private float m_nameFadeInTime = 2f;

	[SerializeField]
	private float m_nameWaitTime = 2f;

	[SerializeField]
	private float m_fadeOutTime = 5f;

	[SerializeField]
	private UnityEvent m_onDoneEvent;

	private IEnumerator Start()
	{
		Color fadedWhite = Color.white;
		fadedWhite.a = 0f;
		Color originalBackgroundColor = m_background.color;
		Color fadedBackgroundColor = originalBackgroundColor;
		fadedBackgroundColor.a = 0f;
		m_gameLogo.gameObject.SetActive(value: false);
		m_pageCanvasGroup.alpha = 0f;
		m_background.color = fadedBackgroundColor;
		yield return new WaitForSeconds(1f);
		m_gameLogo.gameObject.SetActive(value: true);
		m_gameLogo.color = fadedWhite;
		m_background.DOColor(originalBackgroundColor, m_logoInitialFadeInTime);
		yield return m_gameLogo.DOFade(1f, m_logoInitialFadeInTime).SetUpdate(isIndependentUpdate: false).WaitForCompletion();
		yield return new WaitForSeconds(m_logoWaitTime);
		CreditsEntry[] creditsPages = m_creditsPages;
		foreach (CreditsEntry entry in creditsPages)
		{
			yield return ShowPageAnimation(entry);
		}
		m_background.DOColor(fadedBackgroundColor, m_fadeOutTime);
		yield return m_gameLogo.DOFade(0f, m_fadeOutTime).SetUpdate(isIndependentUpdate: false).WaitForCompletion();
		m_onDoneEvent.Invoke();
	}

	private IEnumerator ShowPageAnimation(CreditsEntry entry)
	{
		TextMeshProUGUI[] nameTexts = m_nameTexts;
		foreach (TextMeshProUGUI obj in nameTexts)
		{
			obj.text = "";
			obj.gameObject.SetActive(value: false);
		}
		m_titleText.text = entry.m_pageTitle;
		yield return m_pageCanvasGroup.DOFade(1f, m_nameFadeInTime).SetUpdate(isIndependentUpdate: false).WaitForCompletion();
		int index = 0;
		string[] names = entry.m_names;
		foreach (string text in names)
		{
			if (index >= m_nameTexts.Length)
			{
				Debug.LogError("Too many name entries for the number of text fields for game credits!");
				break;
			}
			m_nameTexts[index].text = text;
			m_nameTexts[index].gameObject.SetActive(value: true);
			Color color = m_nameTexts[index].color;
			color.a = 0f;
			m_nameTexts[index].color = color;
			yield return m_nameTexts[index].DOFade(1f, m_nameFadeInTime).SetUpdate(isIndependentUpdate: false).WaitForCompletion();
			yield return new WaitForSeconds(m_nameWaitTime);
			index++;
		}
		yield return new WaitForSeconds(entry.m_delay);
		yield return m_pageCanvasGroup.DOFade(0f, 1f).SetUpdate(isIndependentUpdate: false).WaitForCompletion();
		yield return new WaitForSeconds(1f);
	}
}
