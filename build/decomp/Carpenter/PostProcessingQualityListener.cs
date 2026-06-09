using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Volume))]
public class PostProcessingQualityListener : MonoBehaviour
{
	[SerializeField]
	private Volume m_volume;

	private Bloom m_bloom;

	private DepthOfField m_dof;

	private void Reset()
	{
		m_volume = GetComponent<Volume>();
	}

	private void OnEnable()
	{
		m_volume.profile.TryGet<Bloom>(out m_bloom);
		m_volume.profile.TryGet<DepthOfField>(out m_dof);
		UpdatePostProcessing();
		GlobalReferences.Instance.EventChannels.UserPreferences.GraphicsQualityChanged.Register(UpdatePostProcessing);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.UserPreferences.GraphicsQualityChanged.Unregister(UpdatePostProcessing);
	}

	private void UpdatePostProcessing()
	{
		if (m_dof != null)
		{
			switch (GlobalReferences.Instance.UserPreferences.GraphicsQualitySettings.DepthOfField)
			{
			case PostProcessingLevel.Off:
				m_dof.mode.value = DepthOfFieldMode.Off;
				break;
			case PostProcessingLevel.Low:
				m_dof.mode.value = DepthOfFieldMode.Off;
				break;
			case PostProcessingLevel.High:
				m_dof.mode.value = DepthOfFieldMode.Bokeh;
				break;
			}
		}
		if (m_bloom != null)
		{
			switch (GlobalReferences.Instance.UserPreferences.GraphicsQualitySettings.Bloom)
			{
			case PostProcessingLevel.Off:
				m_bloom.active = false;
				break;
			case PostProcessingLevel.Low:
				m_bloom.active = true;
				m_bloom.highQualityFiltering.value = false;
				m_bloom.downscale.value = BloomDownscaleMode.Quarter;
				break;
			case PostProcessingLevel.High:
				m_bloom.active = true;
				m_bloom.highQualityFiltering.value = true;
				m_bloom.downscale.value = BloomDownscaleMode.Half;
				break;
			}
		}
	}
}
