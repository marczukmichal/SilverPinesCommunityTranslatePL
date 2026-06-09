using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class HorizontalMove : FsmStateAction
{
	public enum LockedDirection
	{
		None,
		MaintainCurrentForceMove,
		ReadInputForceMove,
		FreeMoveInCurrentDirection,
		ForceOppositeCurrentDirection,
		ForceFullMovementAnyDirection
	}

	[SerializeField]
	public LockedDirection m_lockedDirection;

	[SerializeField]
	public bool m_hasTurnAnimation;

	[SerializeField]
	public bool m_onlyOnce;

	[SerializeField]
	public bool m_disableDirectionChanges;

	[SerializeField]
	public bool m_applyCurrentInputDirectionOnEnter;

	[SerializeField]
	public bool m_instantFullSpeed;

	[SerializeField]
	public bool m_onlyTurnIfSprinting;

	[SerializeField]
	public bool m_canWalkBackwards;

	[HutongGames.PlayMaker.Tooltip("If true then while in this state the character can become stationary and disable the physics on the character")]
	[SerializeField]
	public bool m_canBeKinematic;

	[HutongGames.PlayMaker.Tooltip("Event to trigger when turn animation is needed")]
	public FsmEvent m_turnAnimation;

	private BaseCharacterInput m_input;

	private LegacyCharacterMovement m_movement;

	private CharacterDirection m_characterDirection;

	private Vector2 m_lockedInput;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_input = base.Owner.GetCharacterInputComponent();
			m_movement = base.Owner.GetComponent<LegacyCharacterMovement>();
			m_characterDirection = base.Owner.GetComponent<CharacterDirection>();
		}
	}

	public override void OnEnter()
	{
		Debug.LogWarning(base.State.Name + " is using Horizontal Move");
		Finish();
	}

	private void ApplyMovement()
	{
		if (m_lockedDirection != 0)
		{
			if (m_lockedDirection == LockedDirection.FreeMoveInCurrentDirection)
			{
				Vector2 movementInput = m_input.MovementInput;
				if (m_characterDirection.CurrentDirection == CharacterDirection.Facing.Right)
				{
					movementInput.x = Mathf.Max(movementInput.x, 0f);
				}
				else
				{
					movementInput.x = Mathf.Min(movementInput.x, 0f);
				}
				m_movement.MovementInput = movementInput;
			}
			else if (m_lockedDirection == LockedDirection.ForceFullMovementAnyDirection)
			{
				Vector2 movementInput2 = m_input.MovementInput;
				if (movementInput2.magnitude <= float.Epsilon)
				{
					if (m_characterDirection.CurrentDirection == CharacterDirection.Facing.Right)
					{
						movementInput2.x = 1f;
					}
					else
					{
						movementInput2.x = -1f;
					}
				}
				m_movement.MovementInput = movementInput2.normalized;
			}
			else
			{
				m_movement.MovementInput = m_lockedInput;
			}
		}
		else
		{
			m_movement.MovementInput = m_input.MovementInput;
		}
		if (!m_disableDirectionChanges && !m_input.TurnDisabled && (!m_onlyTurnIfSprinting || m_input.IsSprinting))
		{
			if (m_movement.MovementInput.x > GameUtils.Constants.s_inputMoveDeadzoneMinValue)
			{
				m_characterDirection.DesiredDirection = CharacterDirection.Facing.Right;
			}
			else if (m_movement.MovementInput.x < 0f - GameUtils.Constants.s_inputMoveDeadzoneMinValue)
			{
				m_characterDirection.DesiredDirection = CharacterDirection.Facing.Left;
			}
			else if (m_input.FacingDirectionInput != 0)
			{
				m_characterDirection.DesiredDirection = m_input.FacingDirectionInput;
			}
		}
		if (m_instantFullSpeed)
		{
			m_movement.InstantFullSpeed(m_movement.MovementInput);
		}
		if (m_characterDirection.HasTurnAnimation && m_characterDirection.DesiredDirection != m_characterDirection.CurrentDirection && (!m_onlyTurnIfSprinting || m_input.IsSprinting))
		{
			base.Fsm.Event(m_turnAnimation);
		}
		if (m_lockedDirection == LockedDirection.ForceFullMovementAnyDirection)
		{
			Vector2 movementInput3 = m_input.MovementInput;
			if (m_characterDirection.CurrentDirection == CharacterDirection.Facing.Right)
			{
				movementInput3.x = 1f;
			}
			else
			{
				movementInput3.x = -1f;
			}
			m_movement.MovementInput = movementInput3.normalized;
		}
	}

	public override void OnUpdate()
	{
	}

	public override void OnExit()
	{
	}
}
