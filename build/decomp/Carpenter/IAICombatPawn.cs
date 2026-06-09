using UnityEngine;

public interface IAICombatPawn
{
	Vector3 GetNavigationPosition();

	bool IsRequestingAttack();

	bool PermitAttack();

	AttackRequestResult GetAttackRequestResult();

	AICombatPawnType GetAICombatPawnType();
}
