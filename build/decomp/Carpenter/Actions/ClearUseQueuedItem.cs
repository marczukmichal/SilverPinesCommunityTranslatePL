using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Inventory")]
public class ClearUseQueuedItem : FsmStateAction
{
	public bool m_onEnter;

	public bool m_onExit;

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
		if (m_onEnter)
		{
			m_inventory.ClearQueuedItem();
			Finish();
		}
	}

	public override void OnExit()
	{
		if (m_onExit)
		{
			m_inventory.ClearQueuedItem();
		}
	}
}
