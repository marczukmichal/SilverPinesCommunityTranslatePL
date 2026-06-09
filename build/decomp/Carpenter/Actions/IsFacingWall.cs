using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("AI")]
public class IsFacingWall : FsmStateAction
{
	[HutongGames.PlayMaker.Tooltip("Minimum time to be facing a wall to trigger")]
	public float m_minTime = 0.2f;

	[RequiredField]
	[HutongGames.PlayMaker.Tooltip("Character collider to use for collision check, should be the main movement collider")]
	public CapsuleCollider2D m_movementCollider;

	[HutongGames.PlayMaker.Tooltip("Event to trigger when stuck facing a wall")]
	public FsmEvent m_facingWallEvent;

	public float m_checkDistance = 1f;

	public bool m_onEnterOnly;

	private CharacterDirection m_direction;

	private LegacyCharacterMovement m_movement;

	private float m_timer;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_direction = base.Owner.GetComponent<CharacterDirection>();
			m_movement = base.Owner.GetComponent<LegacyCharacterMovement>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_timer = 0f;
		if (m_onEnterOnly)
		{
			Check();
			Finish();
		}
	}

	public override void OnUpdate()
	{
		Check();
	}

	private void Check()
	{
		bool flag = AIUtilities.IsInFrontOfWall(layerMask: (!(m_movement != null)) ? GameLayers.EnvironmentMask : m_movement.EnvironmentLayerMask, collider: m_movementCollider, forwardDir: m_direction.GetForwardVector(), checkDistance: m_checkDistance);
		if (m_onEnterOnly)
		{
			if (flag)
			{
				base.Fsm.Event(m_facingWallEvent);
			}
			return;
		}
		if (flag)
		{
			if (m_onEnterOnly)
			{
				return;
			}
			m_timer += Time.deltaTime;
		}
		else
		{
			m_timer = 0f;
		}
		if (m_timer >= m_minTime)
		{
			base.Fsm.Event(m_facingWallEvent);
		}
	}
}
