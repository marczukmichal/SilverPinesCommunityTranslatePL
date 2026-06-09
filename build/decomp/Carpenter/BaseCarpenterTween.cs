using HutongGames.PlayMaker;

public abstract class BaseCarpenterTween : FsmStateAction
{
	public FsmOwnerDefault m_gameObject;

	public FsmEvent m_onDoneEvent;

	public FsmAnimationCurve m_curve;

	public float m_duration;

	public float m_delay;

	public void OnTweenComplete()
	{
		base.Fsm.Event(m_onDoneEvent);
		Finish();
	}
}
