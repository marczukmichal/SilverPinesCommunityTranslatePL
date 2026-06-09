using UnityEngine;

public class GraphicsQualityListener : MonoBehaviour
{
	private enum GraphicsCategory
	{
		Lighting,
		Shadows,
		WorldDetail,
		Effects
	}

	private enum GraphicsLevelAdjustment
	{
		Enable,
		Disable
	}

	[Header("Graphics Category")]
	[SerializeField]
	private GraphicsCategory m_category;

	[Header("Quality Levels")]
	[SerializeField]
	private GraphicsLevelAdjustment m_adjustmentHigh;

	[SerializeField]
	private GraphicsLevelAdjustment m_adjustmentMedium;

	[SerializeField]
	private GraphicsLevelAdjustment m_adjustmentLow;

	private void Awake()
	{
		GlobalReferences.Instance.EventChannels.UserPreferences.GraphicsQualityChanged.Register(OnGraphicsQualityChanged);
		OnGraphicsQualityChanged();
	}

	private void OnDestroy()
	{
		GlobalReferences.Instance.EventChannels.UserPreferences.GraphicsQualityChanged.Unregister(OnGraphicsQualityChanged);
	}

	private GraphicsLevelAdjustment GetAdjustmentMode(GraphicsQualityLevel level)
	{
		return level switch
		{
			GraphicsQualityLevel.High => m_adjustmentHigh, 
			GraphicsQualityLevel.Medium => m_adjustmentMedium, 
			GraphicsQualityLevel.Low => m_adjustmentLow, 
			_ => m_adjustmentHigh, 
		};
	}

	private void OnGraphicsQualityChanged()
	{
		GraphicsQualitySettings graphicsQualitySettings = GlobalReferences.Instance.UserPreferences.GraphicsQualitySettings;
		GraphicsQualityLevel level = GraphicsQualityLevel.High;
		switch (m_category)
		{
		case GraphicsCategory.Lighting:
			level = graphicsQualitySettings.Lighting;
			break;
		case GraphicsCategory.Shadows:
			level = graphicsQualitySettings.Shadow;
			break;
		case GraphicsCategory.WorldDetail:
			level = graphicsQualitySettings.WorldDetail;
			break;
		case GraphicsCategory.Effects:
			level = graphicsQualitySettings.Effects;
			break;
		}
		switch (GetAdjustmentMode(level))
		{
		case GraphicsLevelAdjustment.Enable:
			base.gameObject.SetActive(value: true);
			break;
		case GraphicsLevelAdjustment.Disable:
			base.gameObject.SetActive(value: false);
			break;
		}
	}
}
