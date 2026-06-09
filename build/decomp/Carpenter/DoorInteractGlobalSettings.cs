using UnityEngine;

[CreateAssetMenu(menuName = "Misc/DoorInteractGlobalSettings")]
public class DoorInteractGlobalSettings : ScriptableObject
{
	public AudioVoicedEvent m_doorLockedPadlockVoiceEvent;

	public AudioVoicedEvent m_doorLockedOtherSideVoiceEvent;

	public AudioVoicedEvent m_doorLockedPermanentlyBlockedVoiceEvent;

	public AudioVoicedEvent m_doorEnemyBlockingVoiceEvent;
}
