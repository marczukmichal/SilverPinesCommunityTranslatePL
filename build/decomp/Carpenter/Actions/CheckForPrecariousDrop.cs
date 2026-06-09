using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class CheckForPrecariousDrop : FsmStateAction
{
	public FsmEvent m_dropDetectedEvent;

	protected CharacterPrecariousDropDetection m_dropDetection;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_dropDetection = base.Owner.GetComponent<CharacterPrecariousDropDetection>();
		}
	}

	public override void Reset()
	{
		base.Reset();
		m_dropDetectedEvent = new FsmEvent("Movement/DropDetected");
	}

	public override void OnUpdate()
	{
		Check();
	}

	private void Check()
	{
		if (m_dropDetection.DropDetected)
		{
			base.Fsm.Event(m_dropDetectedEvent);
		}
	}
}
