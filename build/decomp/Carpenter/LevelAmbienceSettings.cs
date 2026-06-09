using System;
using FMODUnity;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "NewLevelAmbience", menuName = "Audio/Level Ambience")]
public class LevelAmbienceSettings : ScriptableObject
{
	[Header("FMOD Events")]
	[SerializeField]
	private EventReference m_ambienceFMODEvent;

	public EventReference AmbienceFMODEvent => m_ambienceFMODEvent;
}
