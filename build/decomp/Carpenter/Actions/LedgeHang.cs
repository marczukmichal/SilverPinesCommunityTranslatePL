using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class LedgeHang : StopAllMovement
{
	[SerializeField]
	public Vector3 m_characterPositionOffset;

	protected CharacterLedgeGrab m_ledgeGrab;

	protected CharacterDirection m_direction;

	public override void Awake()
	{
		base.Awake();
		if (!(base.Owner == null))
		{
			m_ledgeGrab = base.Owner.GetComponent<CharacterLedgeGrab>();
			m_direction = base.Owner.GetComponent<CharacterDirection>();
		}
	}

	public override void OnEnter()
	{
		EnterStopMovement();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		Vector3 position = m_ledgeGrab.ActiveLedgeTransform.position;
		position.y += m_characterPositionOffset.y;
		if (m_direction.CurrentDirection == CharacterDirection.Facing.Right)
		{
			position.x -= m_characterPositionOffset.x;
		}
		else
		{
			position.x += m_characterPositionOffset.x;
		}
		position.z = base.Owner.transform.position.z;
		base.Owner.transform.position = position;
	}

	public override void OnExit()
	{
		base.OnExit();
		m_ledgeGrab.ExitLedgeGrab();
		m_movement.SetOffGround();
	}
}
