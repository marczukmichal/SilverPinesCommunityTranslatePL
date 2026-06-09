using System;
using FMODUnity;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "NewLevelReverb", menuName = "Audio/Level Reverb")]
public class LevelReverbSettings : ScriptableObject
{
	[Header("FMOD Events")]
	[SerializeField]
	private EventReference m_reverbFMODEvent;

	public EventReference ReverbFMODEvent => m_reverbFMODEvent;
}
