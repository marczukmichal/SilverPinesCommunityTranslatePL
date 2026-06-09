using DG.Tweening;
using UnityEngine;

public class StudioSpotlightLightListener : MonoBehaviour
{
	[SerializeField]
	private Light m_light;

	[SerializeField]
	private ProgressionVariableInt m_lightMaskVariable;

	private void OnEnable()
	{
		m_lightMaskVariable.RegisterListener(OnValueChanged);
		ChanceToValue(m_lightMaskVariable.Value, animate: false);
	}

	private void OnDisable()
	{
		m_lightMaskVariable.UnregisterListener(OnValueChanged);
	}

	private void OnValueChanged(int value)
	{
		ChanceToValue(value, animate: true);
	}

	private void ChanceToValue(int value, bool animate)
	{
		float r = 1f - (((StudioSpotlightCYMLensFlags)value).HasFlag(StudioSpotlightCYMLensFlags.Cyan) ? 1f : 0f);
		float g = 1f - (((StudioSpotlightCYMLensFlags)value).HasFlag(StudioSpotlightCYMLensFlags.Magenta) ? 1f : 0f);
		float b = 1f - (((StudioSpotlightCYMLensFlags)value).HasFlag(StudioSpotlightCYMLensFlags.Yellow) ? 1f : 0f);
		DOTween.Kill(m_light);
		Color color = new Color(r, g, b);
		if (animate)
		{
			m_light.DOColor(color, 0.3f).SetUpdate(isIndependentUpdate: true);
		}
		else
		{
			m_light.color = color;
		}
	}
}
