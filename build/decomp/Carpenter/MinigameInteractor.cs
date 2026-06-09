public class MinigameInteractor : BaseInteractor, IMinigameBackInputHandler
{
	private Interactable m_activeInteractable;

	public Interactable ActiveInteractable => m_activeInteractable;

	public void RequestInteracat(Interactable interactable)
	{
		if (!(m_activeInteractable != null) && interactable.CanInteract(this))
		{
			m_activeInteractable = interactable;
			interactable.Interact(this);
			GlobalReferences.Instance.EventChannels.Generic.SetGamepadCursorAllowed.Raise(value: false);
			GlobalReferences.Instance.EventChannels.Minigames.MinigameHighlightInteract.Raise(interactable.gameObject);
		}
	}

	private void Update()
	{
		if (m_activeInteractable != null && m_activeInteractable.InteractionDone)
		{
			m_activeInteractable = null;
			GlobalReferences.Instance.EventChannels.Generic.SetGamepadCursorAllowed.Raise(value: true);
			GlobalReferences.Instance.EventChannels.Minigames.MinigameHighlightInteract.Raise(null);
		}
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.Generic.SetGamepadCursorAllowed.Raise(value: true);
	}

	private void OnDestroy()
	{
		if (m_activeInteractable != null)
		{
			GlobalReferences.Instance.EventChannels.Generic.SetGamepadCursorAllowed.Raise(value: true);
		}
	}

	public bool TryBackInput()
	{
		if (m_activeInteractable != null)
		{
			return true;
		}
		return false;
	}
}
