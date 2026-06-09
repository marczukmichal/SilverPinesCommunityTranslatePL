using UnityEngine;

public class AudioTriggerLowHealth : MonoBehaviour
{
	[SerializeField]
	private AudioEvent m_lowHealthAudioEvent;

	[SerializeField]
	private FloatVariable m_playerHealthPercentage;

	[SerializeField]
	private float m_maxHealthPercent;

	[SerializeField]
	private float m_minHealthPercent;

	[SerializeField]
	private float m_fastRefireRate;

	[SerializeField]
	private float m_slowRefireRate;

	[SerializeField]
	private float m_timeUntilQuiet = 3f;

	[SerializeField]
	private float m_quietVolume = 0.25f;

	private float m_currentTimer;

	private float m_volumeTimer;

	private void OnEnable()
	{
		m_playerHealthPercentage.RegisterListener(OnValueChanged);
	}

	private void OnDisable()
	{
		m_playerHealthPercentage.UnregisterListener(OnValueChanged);
	}

	private void OnValueChanged(float arg0)
	{
		m_volumeTimer = 0f;
	}

	public void Update()
	{
		if (m_playerHealthPercentage.Value < m_maxHealthPercent && m_playerHealthPercentage.Value > 0f)
		{
			m_currentTimer += Time.unscaledDeltaTime;
			float num = GameUtils.RemapClamped(m_playerHealthPercentage.Value, m_minHealthPercent, m_maxHealthPercent, m_fastRefireRate, m_slowRefireRate);
			if (m_currentTimer > num)
			{
				m_currentTimer = 0f;
				m_lowHealthAudioEvent.Play2D((m_volumeTimer > m_timeUntilQuiet) ? m_quietVolume : 1f);
			}
			m_volumeTimer += Time.unscaledDeltaTime;
		}
	}
}
