using UnityEngine;

public class DamageColliderChild : BaseDamageCollider
{
	[SerializeField]
	private DamageCollider m_parent;

	public override bool CanRebound => false;

	protected override HitResultType PerformHit(Collider2D damageCollider, Collider2D otherCollider, CharacterIdentifier.CharacterFaction otherFaction, CharacterIdentifier characterIdentifier)
	{
		return m_parent.CheckColliderOverlapForDamage(m_collider, otherCollider);
	}

	protected override void TriggerRebound(GameObject impactGO, Vector3 impactPosition)
	{
	}

	protected override bool CheckForRebound()
	{
		return false;
	}

	public override void OnTriggerExit2D(Collider2D collider)
	{
		base.OnTriggerExit2D(collider);
		m_parent.OnTriggerExit2D(collider);
	}
}
