using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

public class ThunderLightning : MonoBehaviour
{
	[Header("Weather Settings")]
	[SerializeField]
	private Vector2 m_timeBetweenStrikes;

	[SerializeField]
	private Weather m_activeWeather;

	[SerializeField]
	private float m_rainThresholdForLightning = 0.75f;

	[SerializeField]
	private bool m_onlyFromScript;

	[Header("Lightning Strike")]
	[SerializeField]
	private Light m_lightSource;

	[SerializeField]
	private Volume m_lightningPostProcessing;

	[SerializeField]
	private Vector2 m_lightIntensity;

	[Header("Audio")]
	[SerializeField]
	private AudioTrigger m_audioTrigger;

	[SerializeField]
	private Vector2 m_audioDelay;

	[Header("Lightning Settings")]
	[SerializeField]
	private int m_maxLoops = 4;

	[SerializeField]
	private Vector2 m_fadeOutTimeRange = new Vector2(0.025f, 0.1f);

	[SerializeField]
	private Vector2 m_fullTime = new Vector2(0.05f, 0.1f);

	[SerializeField]
	private Vector2 m_offTime = new Vector2(0.05f, 0.1f);

	private IEnumerator Start()
	{
		if (m_onlyFromScript)
		{
			yield break;
		}
		while (true)
		{
			float random = m_timeBetweenStrikes.GetRandom();
			yield return new WaitForSeconds(random);
			if (m_activeWeather.CurrentRainAmount > m_rainThresholdForLightning)
			{
				StartCoroutine(DoThunder());
				yield return DoStrike();
			}
		}
	}

	public void TriggerLightningStrike()
	{
		StartCoroutine(DoStrike());
		if (m_audioTrigger != null)
		{
			m_audioTrigger.TriggerAudio();
		}
	}

	private IEnumerator DoThunder()
	{
		if (m_audioTrigger != null)
		{
			yield return new WaitForSeconds(m_audioDelay.GetRandom());
			m_audioTrigger.TriggerAudio();
		}
	}

	private IEnumerator DoStrike()
	{
		int loops = Random.Range(1, m_maxLoops);
		for (int i = 0; i < loops; i++)
		{
			float fadeOutTime = m_fadeOutTimeRange.GetRandom();
			float random = m_fullTime.GetRandom();
			float offTime = m_offTime.GetRandom();
			if (m_lightningPostProcessing != null)
			{
				m_lightningPostProcessing.enabled = true;
				m_lightningPostProcessing.weight = 1f;
			}
			if (m_lightSource != null)
			{
				m_lightSource.enabled = true;
				m_lightSource.intensity = m_lightIntensity.GetRandom();
			}
			yield return new WaitForSeconds(random);
			if (m_lightSource != null)
			{
				m_lightSource.DOIntensity(0f, fadeOutTime);
			}
			if (m_lightningPostProcessing != null)
			{
				DOTween.To(() => m_lightningPostProcessing.weight, delegate(float x)
				{
					m_lightningPostProcessing.weight = x;
				}, 0f, fadeOutTime);
			}
			yield return new WaitForSeconds(fadeOutTime);
			if (m_lightSource != null)
			{
				m_lightSource.enabled = false;
			}
			if (m_lightningPostProcessing != null)
			{
				m_lightningPostProcessing.weight = 0f;
			}
			yield return new WaitForSeconds(offTime);
		}
		if (m_lightningPostProcessing != null)
		{
			m_lightningPostProcessing.enabled = false;
		}
	}
}
