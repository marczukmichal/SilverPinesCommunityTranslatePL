using HutongGames.PlayMaker;
using PowerTools;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class LedgeClimb : StopAllMovement
{
	public Vector3 m_characterPositionOffset;

	public bool m_isVault;

	public bool m_adjustAnimStartState;

	public bool m_snapToFloor = true;

	public bool m_endClimbOnExit = true;

	public float m_lerpPositionDuration;

	public Vector2 m_lerpStartOffset = Vector2.zero;

	protected CharacterLedgeGrab m_ledgeGrab;

	protected CharacterDirection m_direction;

	private SpriteAnim m_spriteAnim;

	private float m_lerpPositionTimer;

	private Vector3 m_startPosition;

	private static readonly float m_startHeightOffset = 1.75f;

	private static readonly float m_endHeightOffset = 0.8f;

	public override void Awake()
	{
		base.Awake();
		if (!(base.Owner == null))
		{
			m_ledgeGrab = base.Owner.GetComponent<CharacterLedgeGrab>();
			m_direction = base.Owner.GetComponent<CharacterDirection>();
			m_spriteAnim = base.Owner.GetComponent<SpriteAnim>();
		}
	}

	private Vector3 GetPlayerPivotPosition()
	{
		bool num = m_direction.CurrentDirection == CharacterDirection.Facing.Right;
		Vector3 characterPositionOffset = m_characterPositionOffset;
		if (num)
		{
			characterPositionOffset.x *= -1f;
		}
		Vector3 position = m_ledgeGrab.ActiveLedgeTransform.position;
		position.z = base.Owner.transform.position.z;
		return position + characterPositionOffset;
	}

	public override void OnEnter()
	{
		EnterStopMovement();
		m_lerpPositionTimer = 0f;
		if (m_lerpPositionDuration <= 0f)
		{
			base.Owner.transform.position = GetPlayerPivotPosition();
		}
		else
		{
			bool num = m_direction.CurrentDirection == CharacterDirection.Facing.Right;
			Vector3 vector = m_lerpStartOffset;
			if (num)
			{
				vector.x *= -1f;
			}
			Vector3 position = base.Owner.transform.position;
			position += vector;
			base.Owner.transform.position = position;
		}
		m_startPosition = base.Owner.transform.position;
		if (m_adjustAnimStartState)
		{
			float ledgeHeightOffset = m_ledgeGrab.ActiveLedge.m_ledgeHeightOffset;
			float value = Mathf.InverseLerp(m_startHeightOffset, m_endHeightOffset, ledgeHeightOffset);
			value = Mathf.Clamp01(value);
			if (CharacterLedgeGrab.LEDGE_DEBUG)
			{
				Debug.Log("Ledge height offset: " + ledgeHeightOffset + " - anim time:" + value);
			}
			m_spriteAnim.SetNormalizedTime(value);
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (m_lerpPositionTimer >= m_lerpPositionDuration)
		{
			base.Owner.transform.position = GetPlayerPivotPosition();
		}
		else
		{
			Vector3 playerPivotPosition = GetPlayerPivotPosition();
			base.Owner.transform.position = Vector3.Lerp(m_startPosition, playerPivotPosition, m_lerpPositionTimer / m_lerpPositionDuration);
		}
		m_lerpPositionTimer += Time.deltaTime;
	}

	public override void OnExit()
	{
		base.OnExit();
		if (m_endClimbOnExit)
		{
			m_ledgeGrab.ExitLedgeGrab();
			m_movement.ApplyMovementFromAnimRootNode();
			if (m_snapToFloor && !m_movement.SnapToGround())
			{
				m_movement.SetOffGround();
			}
		}
	}
}
