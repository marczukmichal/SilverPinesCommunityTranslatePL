using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class InteractablePickupLoreCommand : BaseInteractableCommand
{
	[SerializeField]
	private LorePickup m_lorePickup;

	[SerializeField]
	private bool m_waitForRead;

	private bool m_finishedReading;

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		if (m_lorePickup == null)
		{
			Debug.LogWarning("InteractablePickupLoreCommand is missing a lore pickup, don't do anything");
			yield return null;
		}
		m_lorePickup.PickupItem();
		m_finishedReading = false;
		if (m_waitForRead)
		{
			GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
			gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Combine(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(GameMenuChanged));
			yield return new WaitUntil(FinishedReading);
		}
		OnExit();
		yield return null;
	}

	public override void Cancel()
	{
		base.Cancel();
		OnExit();
	}

	private bool FinishedReading()
	{
		return m_finishedReading;
	}

	private void OnExit()
	{
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Remove(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(GameMenuChanged));
	}

	private void GameMenuChanged(GameMenuState.GameMenu menuState)
	{
		m_finishedReading = !menuState.HasFlag(GameMenuState.GameMenu.GameNotesReader);
	}
}
