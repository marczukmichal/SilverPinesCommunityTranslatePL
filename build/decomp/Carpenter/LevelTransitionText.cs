using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class LevelTransitionText : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_text;

	[SerializeField]
	private TimeOfDay m_timeOfDay;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

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

	[SerializeField]
	private AudioEvent m_levelRevealStinger;

	private LevelRegionSettings m_activeRegion;

	private bool m_tempDisabled;

	private void OnEnable()
	{
		m_text.maxVisibleCharacters = 0;
		m_text.alpha = 0f;
		m_canvasGroup.alpha = 0f;
		GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Register(OnActiveLevelMetadataChanged);
		GlobalReferences.Instance.EventChannels.LevelTransition.TempDisableLevelTransitionText.Register(OnTempDisable);
		if (GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Item != null)
		{
			m_activeRegion = GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Item.Region;
		}
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Unregister(OnActiveLevelMetadataChanged);
		GlobalReferences.Instance.EventChannels.LevelTransition.TempDisableLevelTransitionText.Unregister(OnTempDisable);
	}

	private void OnTempDisable(bool disable)
	{
		m_tempDisabled = disable;
	}

	private void OnActiveLevelMetadataChanged(LevelMetadata levelMetadata)
	{
		if (levelMetadata != null && m_activeRegion != levelMetadata.Region)
		{
			m_activeRegion = levelMetadata.Region;
			if (m_activeRegion != null && m_activeRegion.ShowLevelIntro)
			{
				OnShowLevelText(m_activeRegion.DisplayName);
			}
		}
	}

	private void Reset()
	{
		m_text = GetComponentInChildren<TextMeshProUGUI>();
	}

	private void Start()
	{
		m_text.text = "";
		m_text.gameObject.SetActive(value: false);
	}

	private void OnShowLevelText(string newLevelName)
	{
		StopAllCoroutines();
		StartCoroutine(LevelLoadCoroutine(newLevelName));
	}

	private IEnumerator LevelLoadCoroutine(string newLevelName)
	{
		DOTween.Kill(m_text);
		m_text.gameObject.SetActive(value: false);
		m_text.text = newLevelName;
		m_text.maxVisibleCharacters = 0;
		m_canvasGroup.alpha = 0f;
		yield return new WaitForSeconds(m_initialShowDelay);
		yield return new WaitUntil(() => !m_tempDisabled);
		m_canvasGroup.DOFade(1f, 0.5f);
		m_text.gameObject.SetActive(value: true);
		m_text.alpha = 1f;
		if (m_levelRevealStinger != null)
		{
			m_levelRevealStinger.Play2D();
		}
		int charCount = 0;
		while (charCount < m_text.text.Length)
		{
			if (m_typeAudio != null)
			{
				m_typeAudio.Play2D();
			}
			charCount = (m_text.maxVisibleCharacters = charCount + 1);
			yield return new WaitForSeconds(m_charAnimTime + Random.Range(0f, m_charAnimTimeAddRandom));
		}
		yield return new WaitForSeconds(m_holdTime);
		m_canvasGroup.DOFade(0f, m_fadeOutTime);
	}
}
