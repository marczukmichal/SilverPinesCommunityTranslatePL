using UnityEngine;

public interface IAIBrainControlAction
{
	Vector2 MoveDirection { get; }

	CharacterDirection.Facing FacingDirectionInput => CharacterDirection.Facing.None;

	bool ShouldAttack => false;

	bool ShouldSprint => false;

	bool ShouldWalkBackwards => false;

	bool ShouldJump => false;

	bool IsDoingAIAbility(AIAbility attack)
	{
		return false;
	}
}
