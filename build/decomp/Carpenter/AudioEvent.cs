using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAudioEvent", menuName = "Audio/Audio Event")]
public class AudioEvent : ScriptableObject
{
	[SerializeField]
	private EventReference m_FMODEvent;

	public EventReference FMODEvent => m_FMODEvent;

	public static void Play(AudioEvent audioEvent, Vector3 position, bool useOcclusion = true, float volume = 1f)
	{
		if (!(audioEvent == null))
		{
			GlobalReferences.Instance.EventChannels.Audio.PlayAudio.Raise(new PlayAudioEventData(audioEvent, position, useOcclusion, volume));
		}
	}

	public static void Play2D(AudioEvent audioEvent, float volume = 1f)
	{
		if (!(audioEvent == null))
		{
			Play(audioEvent, Vector3.zero, useOcclusion: false, volume);
		}
	}

	public void Play2D()
	{
		Play2D(this);
	}

	public void Play2D(float volume = 1f)
	{
		Play2D(this, volume);
	}

	public void Play(Vector3 position, bool useOcclusion = true, float volume = 1f)
	{
		Play(this, position, useOcclusion, volume);
	}

	public static void PlayWithParameterLabel(AudioEvent audioEvent, Vector3 position, string parameterName, string parameterLabel, bool useOcclusion, float volume = 1f)
	{
		if (!(audioEvent == null))
		{
			GlobalReferences.Instance.EventChannels.Audio.PlayAudio.Raise(new PlayAudioEventData(audioEvent, position, parameterName, parameterLabel, useOcclusion, volume));
		}
	}

	public void PlayWithParameteterLabel(Vector3 position, string parameterName, string parameterLabel, bool useOcclusion = true, float volume = 1f)
	{
		PlayWithParameterLabel(this, position, parameterName, parameterLabel, useOcclusion, volume);
	}

	public static void PlayWithParameter(AudioEvent audioEvent, Vector3 position, string parameterName, float parameterValue, bool useOcclusion, float volume = 1f)
	{
		if (!(audioEvent == null))
		{
			GlobalReferences.Instance.EventChannels.Audio.PlayAudio.Raise(new PlayAudioEventData(audioEvent, position, parameterName, parameterValue, useOcclusion, volume));
		}
	}

	public void PlayWithParameteter(Vector3 position, string parameterName, float parameterValue, bool useOcclusion = true, float volume = 1f)
	{
		PlayWithParameter(this, position, parameterName, parameterValue, useOcclusion, volume);
	}
}
