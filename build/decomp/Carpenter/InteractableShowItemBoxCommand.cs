using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class InteractableShowItemBoxCommand : BaseInteractableCommand
{
	private bool m_menuClosed;

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		m_menuClosed = false;
		GlobalReferences.Instance.EventChannels.Hints.DynamicHint.Raise(DynamicHintEvent.UseStorage);
		GlobalReferences.Instance.Anchors.Inventory.ItemBoxInteractableAnchor.Set(interactable);
		GlobalReferences.Instance.EventChannels.InGameMenu.ShowInventoryMenuTab.Raise();
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Combine(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
		yield return new WaitUntil(() => m_menuClosed);
		OnExit();
	}

	private void OnGameMenuChanged(GameMenuState.GameMenu menu)
	{
		if (!menu.HasFlag(GameMenuState.GameMenu.InGameMenu))
		{
			m_menuClosed = true;
		}
	}

	private void OnExit()
	{
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Remove(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
		GlobalReferences.Instance.Anchors.Inventory.ItemBoxInteractableAnchor.Set(null);
		GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.InGameMenu);
	}

	public override void Cancel()
	{
		base.Cancel();
		OnExit();
	}
}
