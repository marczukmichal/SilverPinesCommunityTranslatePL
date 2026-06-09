using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

public class PostProcessingEventListener : MonoBehaviour
{
	[FormerlySerializedAs("m_eventChannel")]
	[SerializeField]
	private BoolGameEventChannel m_boolEventChannel;

	[SerializeField]
	private FloatGameEventChannel m_floatEventChannel;

	[SerializeField]
	private float m_transitionSpeed = 1f;

	[SerializeField]
	private bool m_ignoreDeltaTime;

	private float m_targetValue;

	private float m_currentValue;

	private Volume m_volume;

	private void Awake()
	{
		m_volume = GetComponent<Volume>();
		m_volume.enabled = false;
	}

	private void OnEnable()
	{
		if (m_boolEventChannel != null)
		{
			m_boolEventChannel.Register(OnBoolValueChanged);
		}
		if (m_floatEventChannel != null)
		{
			m_floatEventChannel.Register(OnFloatValueChanged);
		}
	}

	private void OnDisable()
	{
		if (m_boolEventChannel != null)
		{
			m_boolEventChannel.Unregister(OnBoolValueChanged);
		}
		if (m_floatEventChannel != null)
		{
			m_floatEventChannel.Unregister(OnFloatValueChanged);
		}
	}

	private void OnFloatValueChanged(float value)
	{
		m_targetValue = value;
	}

	private void OnBoolValueChanged(bool value)
	{
		m_targetValue = (value ? 1f : 0f);
	}

	public void Update()
	{
		float num = Mathf.MoveTowards(m_currentValue, m_targetValue, (m_ignoreDeltaTime ? Time.unscaledDeltaTime : Time.deltaTime) * m_transitionSpeed);
		if (num != m_currentValue)
		{
			ApplyValue(num);
		}
	}

	private void ApplyValue(float newValue)
	{
		m_currentValue = newValue;
		m_volume.weight = newValue;
		m_volume.enabled = newValue > 0f;
	}
}
