using System;
using FMODUnity;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "NewMusicSettings", menuName = "Audio/Music Settings")]
public class MusicSettings : ScriptableObject
{
	[Header("FMOD Events")]
	[SerializeField]
	private EventReference m_startFMODEvent;

	[SerializeField]
	private EventReference m_combatFMODEvent;

	[SerializeField]
	private bool m_forceNoCombatMusic;

	public EventReference StartFMODEvent => m_startFMODEvent;

	public EventReference CombatFMODEvent => m_combatFMODEvent;

	public bool ForceNoCombatMusic => m_forceNoCombatMusic;
}
