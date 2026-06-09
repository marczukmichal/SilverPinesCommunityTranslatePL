public class ArtifactScaleableCharacterMovementAction : CharacterMovementAction
{
	public enum ArtifactScalingMode
	{
		None,
		Sprinting
	}

	public ArtifactScalingMode m_artifactScalingMode;

	protected CharacterInventory m_inventory;

	public override void Awake()
	{
		base.Awake();
		if (base.Owner != null)
		{
			m_inventory = base.Owner.GetComponent<CharacterInventory>();
		}
	}

	protected override float GetMoveSpeed()
	{
		float num = base.GetMoveSpeed();
		if (m_artifactScalingMode == ArtifactScalingMode.Sprinting && m_inventory != null && m_inventory.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.FasterSprint, out float floatValue))
		{
			num *= 1f + floatValue;
		}
		return num;
	}
}
