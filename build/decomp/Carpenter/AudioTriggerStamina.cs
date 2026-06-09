using UnityEngine;

public class AudioTriggerStamina : MonoBehaviour
{
	[SerializeField]
	private FloatVariable m_floatVariable;

	[SerializeField]
	private float m_transitionSpeed;

	private AudioSource m_audioSource;

	private float m_currentValue;

	[SerializeField]
	private float m_minValue;

	private void Awake()
	{
		m_audioSource = GetComponent<AudioSource>();
		m_audioSource.enabled = false;
	}

	public void Update()
	{
		float num = Mathf.Clamp01(1f - m_floatVariable.Value);
		if (num < m_minValue)
		{
			num = 0f;
		}
		float num2 = Mathf.MoveTowards(m_currentValue, num, Time.deltaTime * m_transitionSpeed);
		if (num2 != m_currentValue)
		{
			ApplyValue(num2);
		}
	}

	private void ApplyValue(float newValue)
	{
		m_currentValue = newValue;
		m_audioSource.volume = newValue;
		bool flag = newValue > 0f;
		if (m_audioSource.enabled != flag)
		{
			m_audioSource.enabled = flag;
			if (flag)
			{
				m_audioSource.Play();
			}
		}
	}
}
