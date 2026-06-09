using UnityEngine;
using UnityEngine.Events;

public class BaseCharacterInput : MonoBehaviour
{
	public UnityAction<bool> OnFireAction;

	public UnityAction<bool> OnSecondaryFireAction;

	public UnityAction OnInteractAction;

	public UnityAction OnInteractUpAction;

	public UnityAction OnInteractDownAction;

	public UnityAction OnReloadAction;

	public UnityAction OnDodgeAction;

	public UnityAction OnStompAction;

	public virtual bool IsJumping => false;

	public virtual bool IsSprinting => false;

	public virtual bool IsCrouching => false;

	public virtual bool IsFiring => false;

	public virtual bool IsSecondaryFiring => false;

	public virtual bool IsAiming => false;

	public virtual bool IsThrowAiming => false;

	public virtual Vector2 MovementInput => Vector2.zero;

	public virtual Vector2 AimDirectionInput => Vector2.zero;

	public virtual bool TurnDisabled => false;

	public virtual bool IsUsingQuickMap => false;

	public virtual CharacterDirection.Facing FacingDirectionInput => CharacterDirection.Facing.None;

	public virtual bool IsDoingAIAbility(AIAbility attack)
	{
		return false;
	}
}
