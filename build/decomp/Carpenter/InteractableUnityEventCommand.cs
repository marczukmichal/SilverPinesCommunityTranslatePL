using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[Serializable]
public class InteractableUnityEventCommand : BaseInteractableCommand
{
	[FormerlySerializedAs("m_onInteracted")]
	[SerializeField]
	private UnityEvent m_onInteractedPersistent;

	[SerializeField]
	private UnityEvent m_onInteractedTransition;

	[SerializeField]
	private UnityEvent m_onInteractedReloadActiveOnly;

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		m_onInteractedPersistent?.Invoke();
		m_onInteractedTransition?.Invoke();
		yield return null;
	}

	public override void ApplyPersistentEffects(BaseInteractable interactable)
	{
		m_onInteractedPersistent?.Invoke();
		m_onInteractedReloadActiveOnly?.Invoke();
	}

	public override bool RequiresPersistentData()
	{
		if (m_onInteractedPersistent.GetPersistentEventCount() > 0 || m_onInteractedReloadActiveOnly.GetPersistentEventCount() > 0)
		{
			return true;
		}
		return base.RequiresPersistentData();
	}
}
