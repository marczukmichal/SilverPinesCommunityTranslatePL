using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class TranslateCharacter : FsmStateAction
{
	[SerializeField]
	public float m_xMoveAmount;

	protected CharacterDirection m_characterDirection;

	protected CharacterMovement m_movement;

	protected Transform m_transform;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterDirection = base.Owner.GetComponent<CharacterDirection>();
			m_movement = base.Owner.GetComponent<CharacterMovement>();
			m_transform = base.Owner.GetComponent<Transform>();
		}
	}

	public override void OnEnter()
	{
		Collider2D collider = m_movement.Collider;
		Vector2 direction = m_characterDirection.GetForwardVector();
		Vector2 origin = collider.bounds.center;
		origin.y += 0.1f;
		Vector2 vector = collider.bounds.size;
		vector.y -= 0.05f;
		RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, direction, m_xMoveAmount, GameLayers.EnvironmentMask);
		Vector3 position;
		if (raycastHit2D.collider != null)
		{
			position = raycastHit2D.point;
			position.x -= direction.x * vector.x * 0.5f;
		}
		else
		{
			position = m_transform.position;
			position.x += direction.x * m_xMoveAmount;
		}
		position.y = m_transform.position.y;
		m_movement.PositionAndUndoAnimRootNode(position);
		Finish();
	}
}
