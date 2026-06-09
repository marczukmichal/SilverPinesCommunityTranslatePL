using FMOD.Studio;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Audio)]
public class PlayAudioEvent : FsmStateAction
{
	public AudioEvent m_audioEvent;

	[ObjectType(typeof(AudioEvent))]
	public FsmObject m_audioEventVariable;

	public Transform m_transform;

	public FsmOwnerDefault m_gameObject;

	public bool m_stopOnExit;

	public float m_delay;

	[HutongGames.PlayMaker.Tooltip("If true the sound will be played on the player in the world, so that 3D sounds will be heard.")]
	public bool m_playAsMinigameSound;

	private EventInstance m_fmodEvent;

	private float m_timer;

	private AudioEvent AudioEvent
	{
		get
		{
			if (m_audioEvent != null)
			{
				return m_audioEvent;
			}
			if (m_audioEventVariable != null && m_audioEventVariable.Value != null)
			{
				return m_audioEventVariable.Value as AudioEvent;
			}
			return null;
		}
	}

	public override void OnEnter()
	{
		m_timer = 0f;
		if (m_delay <= 0f)
		{
			PlaySound();
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (m_delay >= 0f)
		{
			m_timer += Time.deltaTime;
			if (m_timer >= m_delay)
			{
				PlaySound();
			}
		}
	}

	public override void OnExit()
	{
		if (m_stopOnExit && m_fmodEvent.isValid())
		{
			m_fmodEvent.stop(STOP_MODE.ALLOWFADEOUT);
			m_fmodEvent.release();
		}
	}

	private void PlaySound()
	{
		if ((bool)AudioEvent)
		{
			Vector3 position;
			if (m_transform != null)
			{
				position = m_transform.position;
			}
			else
			{
				GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(m_gameObject);
				position = ((ownerDefaultTarget != null) ? ownerDefaultTarget.transform.position : base.Owner.transform.position);
			}
			if (m_playAsMinigameSound)
			{
				Vector3 position2 = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item.transform.position;
				m_fmodEvent = AudioManager.PlayAudioEvent(m_audioEvent, position2, 1f, useOcclusion: false);
			}
			else
			{
				m_fmodEvent = AudioManager.PlayAudioEvent(m_audioEvent, position, 1f, useOcclusion: false);
			}
			Finish();
		}
	}
}
