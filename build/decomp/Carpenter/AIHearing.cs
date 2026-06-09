using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class AIHearing : MonoBehaviour, IAISense
{
	private class HeardNoiseSource
	{
		public Detectable m_detectable;

		public NoiseSource m_noiseSource;

		public float m_noiseIntensity;

		public float m_lastHeardTime;
	}

	[FormerlySerializedAs("m_heardThreshold")]
	[SerializeField]
	private float m_minHeardThreshold;

	[SerializeField]
	private float m_maxHeardThreshold;

	[SerializeField]
	private float m_forgetTime;

	[SerializeField]
	private Transform m_earsTransform;

	private List<HeardNoiseSource> m_heardNoiseSources = new List<HeardNoiseSource>();

	public Transform EarsTransform
	{
		get
		{
			if (!(m_earsTransform != null))
			{
				return base.transform;
			}
			return m_earsTransform;
		}
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.Sets.Generic.AIHearingSet.Add(this);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Sets.Generic.AIHearingSet.Remove(this);
	}

	private HeardNoiseSource GetExistingKnownNoiseSource(NoiseSource noiseSource)
	{
		foreach (HeardNoiseSource heardNoiseSource in m_heardNoiseSources)
		{
			if (heardNoiseSource.m_noiseSource == noiseSource)
			{
				return heardNoiseSource;
			}
		}
		return null;
	}

	public void PopulateTargets(AISenses.DetectableResults detectableResults, bool onlyDangerSense)
	{
		if (onlyDangerSense)
		{
			return;
		}
		foreach (HeardNoiseSource heardNoiseSource in m_heardNoiseSources)
		{
			if (heardNoiseSource.m_noiseIntensity >= m_minHeardThreshold)
			{
				float detectionRate = 1f;
				if (heardNoiseSource.m_noiseIntensity < m_maxHeardThreshold)
				{
					detectionRate = heardNoiseSource.m_noiseIntensity.Remap(m_minHeardThreshold, m_maxHeardThreshold, 0f, 1f);
				}
				detectableResults.AddDetectable(heardNoiseSource.m_detectable, detectionRate);
			}
		}
	}

	public void OnHeardNoise(NoiseSource source, float intensity)
	{
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		HeardNoiseSource heardNoiseSource = GetExistingKnownNoiseSource(source);
		if (heardNoiseSource == null)
		{
			Detectable component = source.GetComponent<Detectable>();
			if (component == null)
			{
				Debug.LogError("Noise source " + source.gameObject.name + " has no detectable so we can't detect it!");
				return;
			}
			heardNoiseSource = new HeardNoiseSource();
			heardNoiseSource.m_noiseSource = source;
			heardNoiseSource.m_detectable = component;
			m_heardNoiseSources.Add(heardNoiseSource);
		}
		heardNoiseSource.m_lastHeardTime = Time.time;
		heardNoiseSource.m_noiseIntensity = Mathf.Max(heardNoiseSource.m_noiseIntensity, intensity);
	}

	private void Update()
	{
		for (int num = m_heardNoiseSources.Count - 1; num >= 0; num--)
		{
			HeardNoiseSource heardNoiseSource = m_heardNoiseSources[num];
			if (Time.time - heardNoiseSource.m_lastHeardTime >= m_forgetTime)
			{
				m_heardNoiseSources.RemoveAt(num);
			}
		}
	}
}
