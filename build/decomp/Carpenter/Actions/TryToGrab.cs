using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Sync Grab")]
public class TryToGrab : FsmStateAction
{
	public CharacterSyncGrabSettings m_grabSettings;

	public GrabCollider m_grabCollider;

	public float m_activateDelay = -1f;

	public float m_activateDuration = -1f;

	private CharacterSyncGrab m_characterSyncGrab;

	private float m_timer;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterSyncGrab = base.Owner.GetComponent<CharacterSyncGrab>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_timer = 0f;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		m_timer += Time.deltaTime;
		if ((!(m_activateDelay > 0f) || !(m_timer < m_activateDelay)) && (!(m_activateDuration > 0f) || !(m_timer > m_activateDelay + m_activateDuration)))
		{
			CharacterSyncGrab grabbableCharacter = m_grabCollider.GetGrabbableCharacter(m_grabSettings);
			if (grabbableCharacter != null && m_characterSyncGrab.CanPerformGrab(grabbableCharacter, m_grabSettings))
			{
				m_characterSyncGrab.PerformGrab(grabbableCharacter, m_grabSettings);
			}
		}
	}
}
