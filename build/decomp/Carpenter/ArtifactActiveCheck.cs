using HutongGames.PlayMaker;

[ActionCategory("Artifacts")]
public class ArtifactActiveCheck : FsmStateAction
{
	public FsmEvent m_activeEvent;

	public FsmEvent m_notActiveEvent;

	public ArtifactEffectDefinition m_artifactDefinition;

	private CharacterInventory m_inventory;

	public override void Awake()
	{
		base.Awake();
		if (base.Owner != null)
		{
			m_inventory = base.Owner.GetComponent<CharacterInventory>();
		}
	}

	public override void OnEnter()
	{
		if (m_inventory.ArtifactInventory.HasActiveArtifactEffect(m_artifactDefinition))
		{
			base.Fsm.Event(m_activeEvent);
		}
		else
		{
			base.Fsm.Event(m_notActiveEvent);
		}
		Finish();
	}
}
