using FronkonGames.Glitches.Distortions;
using FronkonGames.Glitches.Interferences;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingDangerIntensity : MonoBehaviour
{
	[SerializeField]
	private float m_transitionSpeed;

	[SerializeField]
	private float m_vignettePulseSpeed = 1f;

	[SerializeField]
	private float m_vignettePulseScale = 0.5f;

	[SerializeField]
	private AnimationCurve m_effectCurve;

	[Header("Interference")]
	[SerializeField]
	private float m_maxInterference;

	private float m_targetValue;

	private float m_currentValue;

	private Volume m_volume;

	private Vignette m_vignette;

	private float m_vignetteStartingIntensity;

	private FilmGrain m_filmGrain;

	private float m_filmGrainStartingIntensity;

	private ColorAdjustments m_colorAdjustments;

	private float m_contrastStartingValue;

	private float m_saturationStartingValue;

	private CarpenterDangerVolumeComponent m_danger;

	private Interferences.Settings m_interference;

	private Fisheye.Settings m_fisheye;

	private void Awake()
	{
		m_interference = Interferences.Instance.settings;
		m_volume = GetComponent<Volume>();
		m_volume.enabled = false;
		m_volume.weight = 1f;
		m_volume.profile.TryGet<Vignette>(out m_vignette);
		m_volume.profile.TryGet<FilmGrain>(out m_filmGrain);
		m_volume.profile.TryGet<ColorAdjustments>(out m_colorAdjustments);
		m_volume.profile.TryGet<CarpenterDangerVolumeComponent>(out m_danger);
		m_vignetteStartingIntensity = m_vignette.intensity.value;
		m_filmGrainStartingIntensity = m_filmGrain.intensity.value;
		m_contrastStartingValue = m_colorAdjustments.contrast.value;
		m_saturationStartingValue = m_colorAdjustments.saturation.value;
	}

	private void OnDisable()
	{
		m_interference.intensity = 0f;
	}

	public void Update()
	{
		m_targetValue = GlobalReferences.Instance.Variables.Generic.NearbyEnemyPresence.Value;
		float newValue = Mathf.MoveTowards(m_currentValue, m_targetValue, Time.deltaTime * m_transitionSpeed);
		ApplyValue(newValue);
	}

	private void ApplyValue(float newValue)
	{
		m_currentValue = newValue;
		float num = m_effectCurve.Evaluate(newValue);
		m_filmGrain.intensity.value = Mathf.Lerp(0f, m_filmGrainStartingIntensity, num);
		m_colorAdjustments.contrast.value = Mathf.Lerp(0f, m_contrastStartingValue, num);
		m_colorAdjustments.saturation.value = Mathf.Lerp(0f, m_saturationStartingValue, num);
		float num2 = Mathf.Sin(Time.time * m_vignettePulseSpeed);
		num2 = 1f - m_vignettePulseScale + num2 * m_vignettePulseScale;
		float num3 = Mathf.Lerp(0f, m_vignetteStartingIntensity, num);
		num3 *= num2;
		m_vignette.intensity.value = num3;
		m_danger.m_intensity.value = num;
		m_interference.intensity = newValue * m_maxInterference;
		m_volume.enabled = newValue > 0f;
	}
}
