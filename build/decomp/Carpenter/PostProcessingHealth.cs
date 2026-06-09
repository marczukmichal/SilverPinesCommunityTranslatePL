using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessingHealth : MonoBehaviour
{
	[SerializeField]
	private FloatVariable m_playerHealthPercentage;

	[SerializeField]
	private float m_transitionSpeed;

	[SerializeField]
	private AnimationCurve m_effectCurve;

	[SerializeField]
	private float m_pulseRate;

	private float m_targetValue;

	private float m_currentValue;

	private Volume m_volume;

	private void Awake()
	{
		m_volume = GetComponent<Volume>();
		m_volume.enabled = false;
		m_volume.weight = 0f;
	}

	public void Update()
	{
		m_targetValue = m_effectCurve.Evaluate(Mathf.Clamp01(1f - m_playerHealthPercentage.Value));
		float newValue = Mathf.MoveTowards(m_currentValue, m_targetValue, Time.deltaTime * m_transitionSpeed);
		ApplyValue(newValue);
	}

	private void ApplyValue(float newValue)
	{
		m_currentValue = newValue;
		float value = (Mathf.Sin(Time.time * m_pulseRate) + 1f) / 2f;
		value = value.Remap(0f, 1f, 0.5f, 1f);
		float num = m_currentValue * value;
		m_volume.weight = num;
		m_volume.enabled = num > 0f;
	}
}
