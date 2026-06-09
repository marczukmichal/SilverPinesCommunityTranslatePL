using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Detectable))]
public class NoiseSource : MonoBehaviour
{
	private class ActiveNoiseImpulse
	{
		public NoiseImpulseSettings m_impulseSettings;

		public float m_timeRemaining;
	}

	[SerializeField]
	private float m_baseNoiseDistance;

	[SerializeField]
	private float m_baseNoiseIntensity;

	[SerializeField]
	private float m_updateRate = 0.1f;

	[SerializeField]
	private Vector2 m_noiseSourceOffset;

	private float m_noiseDistance;

	private float m_noiseIntensity;

	private float m_timer;

	private List<ActiveNoiseImpulse> m_activeNoiseImpulses = new List<ActiveNoiseImpulse>();

	public float NoiseDistance
	{
		get
		{
			float result = m_noiseDistance;
			foreach (ActiveNoiseImpulse activeNoiseImpulse in m_activeNoiseImpulses)
			{
				result = Mathf.Max(activeNoiseImpulse.m_impulseSettings.m_distance);
			}
			return result;
		}
	}

	public float NoiseIntensity
	{
		get
		{
			float result = m_noiseIntensity;
			foreach (ActiveNoiseImpulse activeNoiseImpulse in m_activeNoiseImpulses)
			{
				result = Mathf.Max(activeNoiseImpulse.m_impulseSettings.m_intensity);
			}
			return result;
		}
	}

	public void SetBaseNoiseValues(float noiseDistance, float noiseIntensity)
	{
		m_noiseDistance = noiseDistance;
		m_noiseIntensity = noiseIntensity;
	}

	public void ResetToDefault()
	{
		m_noiseDistance = m_baseNoiseDistance;
		m_noiseIntensity = m_baseNoiseIntensity;
	}

	private void Awake()
	{
		ResetToDefault();
	}

	private void OnEnable()
	{
		ApplyNoise();
	}

	private void Update()
	{
		m_timer -= Time.deltaTime;
		if (m_timer <= 0f)
		{
			ApplyNoise();
		}
		for (int num = m_activeNoiseImpulses.Count - 1; num >= 0; num--)
		{
			m_activeNoiseImpulses[num].m_timeRemaining -= Time.deltaTime;
			if (m_activeNoiseImpulses[num].m_timeRemaining <= 0f)
			{
				m_activeNoiseImpulses.RemoveAt(num);
			}
		}
	}

	private void ApplyNoise()
	{
		m_timer = m_updateRate;
		if (NoiseDistance <= 0f)
		{
			return;
		}
		Vector2 vector = base.transform.position;
		vector += m_noiseSourceOffset;
		foreach (AIHearing item in GlobalReferences.Instance.Sets.Generic.AIHearingSet)
		{
			Vector2 vector2 = item.EarsTransform.position;
			float num = Vector2.Distance(vector2, vector);
			if (num < NoiseDistance)
			{
				Vector2 vector3 = vector - vector2;
				Vector2 normalized = vector3.normalized;
				if (Physics2D.Raycast(vector2, normalized, vector3.magnitude, GameLayers.EnvironmentMask).collider == null)
				{
					float num2 = 1f - num / NoiseDistance;
					item.OnHeardNoise(this, NoiseIntensity * num2);
				}
			}
		}
	}

	public void TriggerImpulse(NoiseImpulseSettings impulse)
	{
		ActiveNoiseImpulse item = new ActiveNoiseImpulse
		{
			m_impulseSettings = impulse,
			m_timeRemaining = impulse.m_duration
		};
		m_activeNoiseImpulses.Add(item);
	}
}
