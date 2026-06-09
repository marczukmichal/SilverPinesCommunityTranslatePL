using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessingStatusEffect : MonoBehaviour
{
	[SerializeField]
	private StatusEffectsVariable m_playerStatusEffectsVariable;

	[SerializeField]
	private StatusEffectDefinition m_statusEffect;

	[SerializeField]
	private float m_transitionSpeed;

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

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.StatusEffects.PlayerStatusEffectUpdated.Register(StatusEffectUpdated);
		if (m_playerStatusEffectsVariable.Value == null)
		{
			return;
		}
		foreach (StatusEffectInstance item in m_playerStatusEffectsVariable.Value)
		{
			if (item.Definition == m_statusEffect)
			{
				StatusEffectUpdated(item);
			}
		}
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.StatusEffects.PlayerStatusEffectUpdated.Unregister(StatusEffectUpdated);
	}

	private void StatusEffectUpdated(StatusEffectInstance statusEffectInstance)
	{
		if (statusEffectInstance.Definition == m_statusEffect)
		{
			m_targetValue = (statusEffectInstance.IsActive ? 1f : 0f);
		}
	}

	public void Update()
	{
		float newValue = Mathf.MoveTowards(m_currentValue, m_targetValue, Time.deltaTime * m_transitionSpeed);
		ApplyValue(newValue);
	}

	private void ApplyValue(float newValue)
	{
		m_currentValue = newValue;
		float num = m_currentValue;
		if (m_pulseRate > 0f)
		{
			float value = (Mathf.Sin(Time.time * m_pulseRate) + 1f) / 2f;
			value = value.Remap(0f, 1f, 0.5f, 1f);
			num = m_currentValue * value;
		}
		m_volume.weight = num;
		m_volume.enabled = num > 0f;
	}
}
