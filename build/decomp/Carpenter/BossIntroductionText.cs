using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class BossIntroductionText : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_mainText;

	[SerializeField]
	private TextMeshProUGUI m_subText;

	[Header("Animation Timings")]
	[SerializeField]
	private float m_charAnimTime = 0.02f;

	[SerializeField]
	private float m_charAnimTimeAddRandom = 0.02f;

	[SerializeField]
	private float m_initialShowDelay = 1.5f;

	[SerializeField]
	private float m_holdTime = 2f;

	[SerializeField]
	private float m_fadeOutTime = 4f;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_typeAudio;

	private void OnEnable()
	{
		m_mainText.maxVisibleCharacters = 0;
		m_mainText.alpha = 0f;
		m_subText.maxVisibleCharacters = 0;
		m_subText.alpha = 0f;
		GlobalReferences.Instance.EventChannels.Generic.BossIntroduction.Register(OnShowBossInfo);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Generic.BossIntroduction.Unregister(OnShowBossInfo);
	}

	private void Reset()
	{
		m_mainText = GetComponentInChildren<TextMeshProUGUI>();
	}

	private void Start()
	{
		m_mainText.text = "";
		m_mainText.gameObject.SetActive(value: false);
		m_subText.text = "";
		m_subText.gameObject.SetActive(value: false);
	}

	private void OnShowBossInfo(BossIntroductionSettings settings)
	{
		StopAllCoroutines();
		StartCoroutine(LevelLoadCoroutine(settings.MainName, settings.SubTitle));
	}

	private IEnumerator LevelLoadCoroutine(string mainTitle, string subTitle)
	{
		DOTween.Kill(m_mainText);
		DOTween.Kill(m_subText);
		m_mainText.gameObject.SetActive(value: false);
		m_mainText.text = mainTitle;
		m_mainText.maxVisibleCharacters = 0;
		m_subText.gameObject.SetActive(value: false);
		m_subText.text = subTitle;
		m_subText.maxVisibleCharacters = 0;
		yield return new WaitForSeconds(m_initialShowDelay);
		m_mainText.gameObject.SetActive(value: true);
		m_mainText.alpha = 1f;
		int charCount = 0;
		while (charCount < m_mainText.text.Length)
		{
			if (m_typeAudio != null)
			{
				m_typeAudio.Play2D();
			}
			charCount = (m_mainText.maxVisibleCharacters = charCount + 1);
			yield return new WaitForSeconds(m_charAnimTime + Random.Range(0f, m_charAnimTimeAddRandom));
		}
		m_subText.gameObject.SetActive(value: true);
		m_subText.alpha = 1f;
		charCount = 0;
		while (charCount < m_subText.text.Length)
		{
			if (m_typeAudio != null)
			{
				m_typeAudio.Play2D();
			}
			charCount = (m_subText.maxVisibleCharacters = charCount + 1);
			yield return new WaitForSeconds(m_charAnimTime + Random.Range(0f, m_charAnimTimeAddRandom));
		}
		yield return new WaitForSeconds(m_holdTime);
		m_mainText.DOFade(0f, m_fadeOutTime);
		m_subText.DOFade(0f, m_fadeOutTime);
	}
}
