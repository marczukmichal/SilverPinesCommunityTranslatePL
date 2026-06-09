using UnityEngine;

public struct PlayAudioEventData
{
	public AudioEvent m_audioEvent;

	public Vector3 m_position;

	public float m_volume;

	public string m_parameterName;

	public string m_parameterLabel;

	public float m_parameterValue;

	public bool m_useOcclusion;

	public PlayAudioEventData(AudioEvent audioEvent, Vector3 position, bool useOcclusion, float volume = 1f)
	{
		m_audioEvent = audioEvent;
		m_position = position;
		m_volume = volume;
		m_parameterName = null;
		m_parameterLabel = null;
		m_parameterValue = 0f;
		m_useOcclusion = useOcclusion;
	}

	public PlayAudioEventData(AudioEvent audioEvent, Vector3 position, string parameterName, string parameterLabel, bool useOcclusion, float volume = 1f)
		: this(audioEvent, position, useOcclusion, volume)
	{
		m_parameterName = parameterName;
		m_parameterLabel = parameterLabel;
	}

	public PlayAudioEventData(AudioEvent audioEvent, Vector3 position, string parameterName, float parameterValue, bool useOcclusion, float volume = 1f)
		: this(audioEvent, position, useOcclusion, volume)
	{
		m_parameterName = parameterName;
		m_parameterValue = parameterValue;
	}
}
