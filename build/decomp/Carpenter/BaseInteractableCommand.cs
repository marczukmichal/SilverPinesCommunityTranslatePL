using System;
using System.Collections;

[Serializable]
public abstract class BaseInteractableCommand
{
	public virtual void SetupInteraction(BaseInteractable interactable, BaseInteractor interactor)
	{
	}

	public abstract IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result);

	public virtual void ApplyPersistentEffects(BaseInteractable interactable)
	{
	}

	public virtual void FinishInteraction(BaseInteractable interactable, BaseInteractor interactor)
	{
	}

	public virtual void Cancel()
	{
	}

	public virtual void Cleanup()
	{
	}

	public virtual void Reset()
	{
	}

	public virtual bool RequiresPersistentData()
	{
		return false;
	}
}
