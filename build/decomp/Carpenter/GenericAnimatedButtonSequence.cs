using UnityEngine;
using UnityEngine.Events;

public class GenericAnimatedButtonSequence : BaseAnimatedButtonSequence
{
	[SerializeField]
	private UnityEvent m_onProgressEvent;

	[SerializeField]
	private UnityEvent m_onCompleteEvent;

	[SerializeField]
	private UnityEvent m_onCancelEvent;

	public override void OnProgress()
	{
		base.OnProgress();
		m_onProgressEvent.Invoke();
	}

	public override void OnComplete()
	{
		base.OnComplete();
		m_onCompleteEvent.Invoke();
	}

	public override void OnCancel()
	{
		m_onCancelEvent.Invoke();
	}
}
