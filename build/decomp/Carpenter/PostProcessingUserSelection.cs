using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Volume))]
public class PostProcessingUserSelection : MonoBehaviour
{
	[SerializeField]
	private Volume m_volume;

	[SerializeField]
	private UserSelectablePostProcessingSettings m_postProcessingSettings;

	private void Reset()
	{
		m_volume = GetComponent<Volume>();
	}

	private void OnEnable()
	{
		UpdatePostProcessing();
		GlobalReferences.Instance.EventChannels.UserPreferences.GraphicsQualityChanged.Register(UpdatePostProcessing);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.UserPreferences.GraphicsQualityChanged.Unregister(UpdatePostProcessing);
	}

	private void UpdatePostProcessing()
	{
		string activePostProcessing = GlobalReferences.Instance.UserPreferences.DisplaySettings.ActivePostProcessing;
		m_volume.sharedProfile = m_postProcessingSettings.GetProfile(activePostProcessing);
	}
}
