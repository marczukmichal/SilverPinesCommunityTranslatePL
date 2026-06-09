using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Interactable")]
public class SetDoorState : FsmStateAction
{
	public SideDoor m_sideDoor;

	public SideDoor.SideDoorState m_targetState;

	public bool m_instant;

	public override void OnEnter()
	{
		if (m_sideDoor.State != m_targetState)
		{
			Interactable component = m_sideDoor.GetComponent<Interactable>();
			if (component != null)
			{
				component.ForceSetInteractDone(m_targetState != SideDoor.SideDoorState.Closed);
			}
			m_sideDoor.SetDoorState(m_targetState, m_instant);
		}
		Finish();
	}
}
