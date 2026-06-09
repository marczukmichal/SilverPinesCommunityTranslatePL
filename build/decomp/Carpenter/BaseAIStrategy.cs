using UnityEngine;
using UnityEngine.Events;

public abstract class BaseAIStrategy : MonoBehaviour
{
	[SerializeField]
	private UnityEvent m_onEnterStrategy;

	public abstract Vector2 MoveDirection { get; }

	public virtual CharacterDirection.Facing FacingDirectionInput => CharacterDirection.Facing.None;

	public virtual bool ShouldAttack => false;

	public virtual bool ShouldSprint => false;

	public virtual bool TurnDisabled => false;

	public virtual bool ShouldJump => false;

	public virtual void EnableStrategy()
	{
		m_onEnterStrategy?.Invoke();
	}

	public virtual void DisableStrategy()
	{
	}

	public virtual void UpdateStrategy()
	{
	}

	public virtual AIAbility GetRequestedAbility()
	{
		return AIAbility.None;
	}

	public virtual float DrawDebugInfo(Rect boxRect, float yPos)
	{
		return yPos;
	}
}
