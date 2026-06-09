using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Inventory")]
public class UseQueuedItem : FsmStateAction
{
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
		m_inventory.UseQueuedItem();
		Finish();
	}
}
