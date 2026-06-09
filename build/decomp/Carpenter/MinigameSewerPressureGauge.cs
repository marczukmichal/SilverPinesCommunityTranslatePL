using UnityEngine;

public class MinigameSewerPressureGauge : MonoBehaviour
{
	[SerializeField]
	private float m_minRotation;

	[SerializeField]
	private float m_maxRotation;

	[SerializeField]
	private float m_minValue;

	[SerializeField]
	private float m_maxValue;

	[SerializeField]
	private RectTransform m_indicatingHand;

	[SerializeField]
	private float m_randomVariationScalar;

	[SerializeField]
	private float m_randomVariationFrequency;

	[SerializeField]
	private AnimationCurve m_randomVariationScalarCurve;

	private float m_currentValue;

	public void SetValue(float value)
	{
		m_currentValue = value;
	}

	private void Update()
	{
		float value = Mathf.InverseLerp(m_minValue, m_maxValue, m_currentValue);
		value = Mathf.Clamp01(value);
		value += Mathf.Sin(Time.time * m_randomVariationFrequency) * (value * m_randomVariationScalarCurve.Evaluate(value));
		float z = Mathf.Lerp(m_minRotation, m_maxRotation, value);
		m_indicatingHand.localRotation = Quaternion.Euler(0f, 0f, z);
	}
}
