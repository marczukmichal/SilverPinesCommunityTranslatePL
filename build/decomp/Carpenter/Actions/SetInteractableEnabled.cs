using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Interactable")]
public class SetInteractableEnabled : FsmStateAction
{
	public Interactable m_interactable;

	public bool m_interactEnabled;

	public override void OnEnter()
	{
		m_interactable.SetInteractEnabled(m_interactEnabled);
		Finish();
	}
}
