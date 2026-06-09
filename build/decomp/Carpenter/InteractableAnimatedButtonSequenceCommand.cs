using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class InteractableAnimatedButtonSequenceCommand : BaseInteractableCommand
{
	[SerializeField]
	private string m_characterStartFSMEvent;

	[SerializeField]
	private BaseAnimatedButtonSequence m_sequence;

	[SerializeField]
	private bool m_resetProgress = true;

	private PlayMakerFSM m_fsm;

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		CharacterInteractor characterInteractor = interactor as CharacterInteractor;
		if (characterInteractor != null)
		{
			m_fsm = characterInteractor.GetComponent<PlayMakerFSM>();
			characterInteractor.InteractableCommandEventToSend = m_characterStartFSMEvent;
			characterInteractor.InteractableCommandRequestFlag = CharacterInteractor.InteractableCommandFlag.RequestEvent;
			m_fsm.SendEvent(m_characterStartFSMEvent);
		}
		if (m_resetProgress)
		{
			m_sequence.ResetProgress();
		}
		GlobalReferences.Instance.EventChannels.AnimatedButtonSequence.AnimatedButtonSequenceSuccess.Register(ABSSuccess);
		yield return new WaitUntil(() => characterInteractor.InteractableCommandRequestFlag == CharacterInteractor.InteractableCommandFlag.EventDone);
		if (m_sequence.DoneSequences < m_sequence.SequenceCount)
		{
			result.m_cancel = true;
			if (m_sequence != null)
			{
				m_sequence.OnCancel();
			}
		}
		else if (m_sequence != null)
		{
			m_sequence.OnComplete();
		}
		GlobalReferences.Instance.EventChannels.AnimatedButtonSequence.AnimatedButtonSequenceSuccess.Unregister(ABSSuccess);
		characterInteractor.InteractableCommandRequestFlag = CharacterInteractor.InteractableCommandFlag.None;
	}

	public override void FinishInteraction(BaseInteractable interactable, BaseInteractor interactor)
	{
		base.FinishInteraction(interactable, interactor);
		GlobalReferences.Instance.EventChannels.AnimatedButtonSequence.AnimatedButtonSequenceSuccess.Unregister(ABSSuccess);
	}

	public override void Cancel()
	{
		base.Cancel();
		GlobalReferences.Instance.EventChannels.AnimatedButtonSequence.AnimatedButtonSequenceSuccess.Unregister(ABSSuccess);
	}

	public override void ApplyPersistentEffects(BaseInteractable interactable)
	{
		base.ApplyPersistentEffects(interactable);
		if (m_sequence != null)
		{
			m_sequence.OnComplete();
		}
	}

	public override bool RequiresPersistentData()
	{
		return true;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		GlobalReferences.Instance.EventChannels.AnimatedButtonSequence.AnimatedButtonSequenceSuccess.Unregister(ABSSuccess);
	}

	private void ABSSuccess()
	{
		if (m_sequence != null)
		{
			m_sequence.OnProgress();
		}
		if (m_sequence.DoneSequences >= m_sequence.SequenceCount)
		{
			m_fsm.SendEvent("Interact/ABS/Complete");
		}
		else
		{
			m_fsm.SendEvent("Interact/ABS/Continue");
		}
	}
}
