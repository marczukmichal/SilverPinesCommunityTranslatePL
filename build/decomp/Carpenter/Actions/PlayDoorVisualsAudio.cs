using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Interactable")]
public class PlayDoorVisualsAudio : FsmStateAction
{
	public DoorVisuals.DoorAudioEvent m_doorAudioEventType;

	public float m_delay;

	public override void OnEnter()
	{
		DoorVisuals componentInChildren = base.Owner.GetComponentInChildren<DoorVisuals>();
		if (componentInChildren != null)
		{
			componentInChildren.PlayDoorAudioEvent(m_doorAudioEventType, m_delay);
		}
		Finish();
	}
}
