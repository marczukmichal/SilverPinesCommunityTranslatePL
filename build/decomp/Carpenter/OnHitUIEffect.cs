using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class OnHitUIEffect : MonoBehaviour
{
	private class BloodSplatter
	{
		public Image m_image;

		public float m_delay;
	}

	[SerializeField]
	private FloatVariable m_playerHealthPercentage;

	[FormerlySerializedAs("m_baseAlphaFromHealth")]
	[SerializeField]
	private AnimationCurve m_criticalHealthCurve;

	[SerializeField]
	private Color m_healthyColor;

	[SerializeField]
	private Color m_criticalColor;

	[SerializeField]
	private float m_fadeSpeed = 1f;

	private List<BloodSplatter> m_bloodSplatters;

	private float m_currentHealth;

	private List<BloodSplatter> m_splatterBucket = new List<BloodSplatter>();

	private Color m_targetBaseColor;

	private void Awake()
	{
		Image[] componentsInChildren = GetComponentsInChildren<Image>(includeInactive: true);
		m_bloodSplatters = new List<BloodSplatter>();
		Image[] array = componentsInChildren;
		foreach (Image image in array)
		{
			m_bloodSplatters.Add(new BloodSplatter
			{
				m_image = image
			});
		}
		Color white = Color.white;
		white.a = 0f;
		foreach (BloodSplatter bloodSplatter in m_bloodSplatters)
		{
			bloodSplatter.m_image.color = white;
		}
		m_targetBaseColor = white;
	}

	private void OnEnable()
	{
		m_currentHealth = m_playerHealthPercentage.Value;
		m_playerHealthPercentage.RegisterListener(OnHealthValueChanged);
	}

	private void OnDisable()
	{
		m_playerHealthPercentage.UnregisterListener(OnHealthValueChanged);
	}

	public void OnHealthValueChanged(float newValue)
	{
		float t = m_criticalHealthCurve.Evaluate(m_playerHealthPercentage.Value);
		bool num = m_playerHealthPercentage.Value < m_currentHealth;
		Color color = (m_targetBaseColor = Color.Lerp(m_healthyColor, m_criticalColor, t));
		if (num)
		{
			float num2 = m_currentHealth - m_playerHealthPercentage.Value;
			m_splatterBucket.Clear();
			foreach (BloodSplatter bloodSplatter2 in m_bloodSplatters)
			{
				m_splatterBucket.Add(bloodSplatter2);
			}
			int value = Mathf.RoundToInt(num2 / 0.05f);
			value = Mathf.Clamp(value, 3, m_splatterBucket.Count);
			for (int i = 0; i < value; i++)
			{
				int index = Random.Range(0, m_splatterBucket.Count);
				BloodSplatter bloodSplatter = m_splatterBucket[index];
				m_splatterBucket.Remove(bloodSplatter);
				bloodSplatter.m_image.color = Color.white;
				bloodSplatter.m_delay = Random.Range(0.1f, 0.5f);
			}
		}
		m_currentHealth = m_playerHealthPercentage.Value;
	}

	private void Update()
	{
		foreach (BloodSplatter bloodSplatter in m_bloodSplatters)
		{
			if (bloodSplatter.m_delay > 0f)
			{
				bloodSplatter.m_delay -= Time.deltaTime;
				continue;
			}
			Color color = Vector4.MoveTowards(bloodSplatter.m_image.color, m_targetBaseColor, Time.deltaTime * m_fadeSpeed);
			bloodSplatter.m_image.color = color;
		}
	}
}
