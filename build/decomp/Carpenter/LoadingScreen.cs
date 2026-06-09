using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_text;

	[SerializeField]
	private GameObject m_container;

	private bool m_active;

	private Coroutine m_activeCoroutine;

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.LevelTransition.ShowLoadingScreen.Register(ToggleLoadingScreen);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.LevelTransition.ShowLoadingScreen.Unregister(ToggleLoadingScreen);
		m_container.SetActive(value: false);
		m_active = false;
	}

	private void ToggleLoadingScreen(bool showing)
	{
		if (m_active != showing)
		{
			m_container.SetActive(showing);
			m_active = showing;
			if (showing)
			{
				m_activeCoroutine = StartCoroutine(LoadingScreenCoroutine());
			}
			else
			{
				StopCoroutine(m_activeCoroutine);
			}
		}
	}

	private IEnumerator LoadingScreenCoroutine()
	{
		DOTween.Kill(m_text);
		m_text.alpha = 0f;
		yield return new WaitForSecondsRealtime(0.5f);
		m_text.DOFade(1f, 1f);
		while (m_active)
		{
			m_text.maxVisibleCharacters = m_text.text.Length - 3;
			while (m_text.maxVisibleCharacters < m_text.text.Length)
			{
				yield return new WaitForSecondsRealtime(0.1f);
				m_text.maxVisibleCharacters++;
			}
			yield return new WaitForSecondsRealtime(1f);
		}
	}
}
