using System;
using FMODUnity;
using UnityEngine;

[Serializable]
public class LoreAudioLogSettings
{
	[SerializeField]
	private EventReference m_FMODEvent;

	public EventReference FMODEvent => m_FMODEvent;
}
