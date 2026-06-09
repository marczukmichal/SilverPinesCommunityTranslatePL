using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Volume))]
public class PostProcessingUserGamma : MonoBehaviour
{
	[SerializeField]
	private Volume m_volume;

	private LiftGammaGain m_liftGammaGain;

	private void Reset()
	{
		m_volume = GetComponent<Volume>();
	}

	private void OnEnable()
	{
		m_volume.profile.TryGet<LiftGammaGain>(out m_liftGammaGain);
		UpdatePostProcessing();
		GlobalReferences.Instance.EventChannels.UserPreferences.GraphicsQualityChanged.Register(UpdatePostProcessing);
		GlobalReferences.Instance.EventChannels.UserPreferences.SetTemporaryGamma.Register(SetTemporaryGamma);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.UserPreferences.GraphicsQualityChanged.Unregister(UpdatePostProcessing);
		GlobalReferences.Instance.EventChannels.UserPreferences.SetTemporaryGamma.Unregister(SetTemporaryGamma);
	}

	private void UpdatePostProcessing()
	{
		float gammaAdjustment = GlobalReferences.Instance.UserPreferences.DisplaySettings.GammaAdjustment;
		m_liftGammaGain.gamma.Override(new Vector4(1f, 1f, 1f, gammaAdjustment));
	}

	private void SetTemporaryGamma(float newGamma)
	{
		m_liftGammaGain.gamma.Override(new Vector4(1f, 1f, 1f, newGamma));
	}
}
