using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class DoorVisuals : MonoBehaviour
{
	[Serializable]
	public class DoorHealthDamageStates
	{
		[SerializeField]
		public int m_health;

		[SerializeField]
		public GameObject m_enabledChildObject;

		[SerializeField]
		public AssetReference m_damageFX;
	}

	public enum DoorAudioEvent
	{
		None,
		Open,
		SprintOpen,
		Close,
		TakeDamage,
		Break,
		Locked,
		Unlock
	}

	[Header("Interaction")]
	[SerializeField]
	private Transform m_promptPosition;

	[Header("Health")]
	[SerializeField]
	private int m_startingDoorHealth = -1;

	[SerializeField]
	private bool m_allowPassthroughProjectiles;

	[SerializeField]
	private DoorHealthDamageStates[] m_damageStates;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_doorOpenAudio;

	[SerializeField]
	private AudioEvent m_doorSprintOpenAudio;

	[SerializeField]
	private AudioEvent m_doorCloseAudio;

	[SerializeField]
	private AudioEvent m_doorTakeDamageAudio;

	[SerializeField]
	private AudioEvent m_doorBreakAudio;

	[SerializeField]
	private AudioEvent m_doorLockedAudioEvent;

	[SerializeField]
	private AudioEvent m_doorUnlockedAudioEvent;

	[SerializeField]
	private bool m_shouldOccludeAudio = true;

	[Header("Apply Interact")]
	[SerializeField]
	private GameObject m_applyInteractPrefab;

	[Header("Door Break")]
	[SerializeField]
	private AssetReference m_breakDoorFX;

	[Header("Surface Settings")]
	[SerializeField]
	private SurfaceSettings m_surfaceSettings;

	[Header("Locked State")]
	[SerializeField]
	private GameObject m_lockedStateGameObject;

	[SerializeField]
	private GameObject m_unlockedStateGameObject;

	public Transform PromptPosition => m_promptPosition;

	public int StartingDoorHealth => m_startingDoorHealth;

	public bool AllowPassthroughProjectiles => m_allowPassthroughProjectiles;

	public DoorHealthDamageStates[] DamageStates => m_damageStates;

	public AudioEvent DoorOpenAudio => m_doorOpenAudio;

	public AudioEvent DoorSprintOpenAudio => m_doorSprintOpenAudio;

	public AudioEvent DoorCloseAudio => m_doorCloseAudio;

	public AudioEvent DoorTakeDamageAudio => m_doorTakeDamageAudio;

	public AudioEvent DoorBreakAudio => m_doorBreakAudio;

	public AudioEvent DoorLockedAudio => m_doorLockedAudioEvent;

	public AudioEvent DoorUnlockedAudio => m_doorUnlockedAudioEvent;

	public bool ShouldOccludeAudio => m_shouldOccludeAudio;

	public GameObject ApplyInteractPrefab => m_applyInteractPrefab;

	public AssetReference BreakDoorFX => m_breakDoorFX;

	public SurfaceSettings SurfaceSettings => m_surfaceSettings;

	public void PlayDoorAudioEvent(DoorAudioEvent eventType, float delay = 0f)
	{
		StartCoroutine(PlayDoorAudioEvent_Coroutine(eventType, delay));
	}

	private IEnumerator PlayDoorAudioEvent_Coroutine(DoorAudioEvent eventType, float delay)
	{
		if (delay > 0f)
		{
			yield return new WaitForSeconds(delay);
		}
		Vector3 position = base.transform.position;
		if (PromptPosition != null)
		{
			position = PromptPosition.position;
		}
		switch (eventType)
		{
		case DoorAudioEvent.Open:
			m_doorOpenAudio?.Play(position);
			break;
		case DoorAudioEvent.SprintOpen:
			m_doorSprintOpenAudio?.Play(position);
			break;
		case DoorAudioEvent.Close:
			m_doorCloseAudio?.Play(position);
			break;
		case DoorAudioEvent.TakeDamage:
			m_doorTakeDamageAudio?.Play(position);
			break;
		case DoorAudioEvent.Break:
			m_doorBreakAudio?.Play(position);
			break;
		case DoorAudioEvent.Locked:
			m_doorLockedAudioEvent?.Play(position);
			break;
		case DoorAudioEvent.Unlock:
			m_doorUnlockedAudioEvent?.Play(position);
			break;
		}
	}

	public void SetUnlockedState(bool unlocked)
	{
		if (m_lockedStateGameObject != null)
		{
			m_lockedStateGameObject.SetActive(!unlocked);
		}
		if (m_unlockedStateGameObject != null)
		{
			m_unlockedStateGameObject.SetActive(unlocked);
		}
	}
}
