using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class InteractableFastTravelBoatCommand : BaseInteractableCommand
{
	public Transform m_boatTransform;

	public PlayMakerFSM m_boatPlaymakerFSM;

	private bool m_shouldContinue;

	private LevelMetadata m_targetLevel;

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		m_shouldContinue = false;
		GlobalReferences.Instance.EventChannels.InGameMenu.ShowMapMenuTab.Raise();
		GlobalReferences.Instance.EventChannels.LevelTransition.BoatLevelSelected.Register(OnLevelSelected);
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Combine(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
		yield return new WaitUntil(() => m_shouldContinue);
		GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.InGameMenu);
		Unregister();
		if (m_targetLevel != null)
		{
			DamageBlock component = interactor.GetComponent<DamageBlock>();
			if (component != null)
			{
				component.ActivateDamageBlock(touchOnly: false);
			}
			interactor.transform.SetParent(m_boatTransform, worldPositionStays: true);
			GlobalReferences.Instance.EventChannels.Camera.PauseCameraFollow.Raise(value: true);
			if (m_boatPlaymakerFSM != null)
			{
				m_boatPlaymakerFSM.SendEvent("Trigger");
				yield return new WaitForSeconds(2f);
			}
			LevelTransitionEventData value = new LevelTransitionEventData
			{
				m_levelMetadata = m_targetLevel,
				m_targetSceneName = m_targetLevel.SceneFileName,
				m_targetSceneAssetReference = m_targetLevel.TargetSceneAssetReference,
				m_targetEntryPoint = "Boat_Interact",
				m_type = LevelTransitionEventType.Transition,
				m_canEnemiesMigrate = false
			};
			GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransition.Raise(value);
			yield return new WaitForSeconds(10f);
		}
	}

	private void Unregister()
	{
		GlobalReferences.Instance.EventChannels.LevelTransition.BoatLevelSelected.Unregister(OnLevelSelected);
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Remove(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
	}

	public override void Cleanup()
	{
		base.Cleanup();
		Unregister();
	}

	private void OnGameMenuChanged(GameMenuState.GameMenu gameMenu)
	{
		if (!gameMenu.HasFlag(GameMenuState.GameMenu.InGameMenu))
		{
			m_shouldContinue = true;
		}
	}

	private void OnLevelSelected(LevelMetadata level)
	{
		m_shouldContinue = true;
		if (GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Item != level)
		{
			m_targetLevel = level;
		}
	}
}
