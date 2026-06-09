using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

[Serializable]
public class InteractableMinigameCommand : BaseInteractableCommand, IPersistentComponent
{
	[Serializable]
	private class PersistentData
	{
		public bool m_minigameComplete;
	}

	[SerializeField]
	private MinigameMetadata m_minigame;

	[SerializeField]
	private bool m_minigameReplayable;

	[SerializeField]
	private MinigameOutputEvent[] m_minigameOutputEvents;

	[SerializeField]
	private bool m_alwaysContinue;

	private bool m_minigameComplete;

	private AsyncOperationHandle<SceneInstance> m_loadedSceneHandle;

	private MinigameScene m_activeMinigameScene;

	private bool m_shouldExit;

	private Scene m_loadedMinigameScene;

	private bool m_sceneLoaded;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public bool MinigameComplete => m_minigameComplete;

	public bool MinigameActive => m_activeMinigameScene != null;

	public static InteractableMinigameCommand CreateInstance(MinigameMetadata minigame)
	{
		return new InteractableMinigameCommand
		{
			m_minigame = minigame,
			m_minigameReplayable = false,
			m_alwaysContinue = false
		};
	}

	public override bool RequiresPersistentData()
	{
		return true;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData != null)
		{
			m_minigameComplete = m_persistentData.m_minigameComplete;
			return;
		}
		m_persistentData = new PersistentData();
		m_persistentDataObject.Data = m_persistentData;
	}

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		m_sceneLoaded = false;
		bool flag = false;
		if (GameDebugCommands.CHEAT_MASTER_KEY && !m_minigameReplayable)
		{
			flag = true;
		}
		if (m_persistentData != null && m_persistentData.m_minigameComplete)
		{
			flag = true;
		}
		if (!flag)
		{
			m_shouldExit = false;
			if (m_minigame.DoFullscreenFade)
			{
				GlobalReferences.Instance.EventChannels.Generic.ScreenFadeOfType.Raise(FadeType.Minigame);
				yield return new WaitForSeconds(0.75f);
			}
			GlobalReferences.Instance.Anchors.Minigame.ActiveMinigameInteractableAnchor.Set(interactable.gameObject);
			GlobalReferences.Instance.Anchors.Minigame.ActiveMinigameMetadataAnchor.Set(m_minigame);
			StartMinigame(interactable);
			yield return new WaitUntil(() => m_sceneLoaded);
			ConfigureMinigameScene(interactable, m_loadedMinigameScene);
			yield return new WaitUntil(() => m_shouldExit);
			if (m_minigame.DoFullscreenFade)
			{
				GlobalReferences.Instance.EventChannels.Generic.ScreenFadeOfType.Raise(FadeType.None);
			}
			GlobalReferences.Instance.Anchors.Minigame.ActiveMinigameInteractableAnchor.Set(null);
			GlobalReferences.Instance.Anchors.Minigame.ActiveMinigameMetadataAnchor.Set(null);
			if (!m_minigameComplete && !m_alwaysContinue)
			{
				result.m_cancel = true;
			}
		}
	}

	public void CompleteMinigame()
	{
		if (m_minigameOutputEvents != null)
		{
			MinigameOutputEvent[] minigameOutputEvents = m_minigameOutputEvents;
			foreach (MinigameOutputEvent minigameOutputEvent in minigameOutputEvents)
			{
				if (minigameOutputEvent.m_value == m_activeMinigameScene.CustomData)
				{
					minigameOutputEvent.m_event.Invoke();
				}
			}
		}
		UnloadMinigame();
		m_minigameComplete = true;
		m_shouldExit = true;
		if (m_persistentData != null && !m_minigameReplayable)
		{
			m_persistentData.m_minigameComplete = m_minigameComplete;
		}
	}

	private void StartMinigame(BaseInteractable newInteractable)
	{
		MinigameManager.OnMinigameSceneLoaded = (UnityAction<Scene>)Delegate.Combine(MinigameManager.OnMinigameSceneLoaded, new UnityAction<Scene>(OnMinigameLoaded));
		MinigameManager.SetActivateMinigameMetadata(m_minigame);
	}

	private void OnMinigameLoaded(Scene scene)
	{
		m_loadedMinigameScene = scene;
		m_sceneLoaded = true;
	}

	private void ConfigureMinigameScene(BaseInteractable interactable, Scene scene)
	{
		GameObject[] rootGameObjects = scene.GetRootGameObjects();
		for (int i = 0; i < rootGameObjects.Length; i++)
		{
			if ((bool)rootGameObjects[i].GetComponent<MinigameScene>())
			{
				m_activeMinigameScene = rootGameObjects[i].GetComponent<MinigameScene>();
			}
			IMinigameComponent[] componentsInChildren = rootGameObjects[i].GetComponentsInChildren<IMinigameComponent>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].Setup(interactable.gameObject);
			}
		}
		MinigameScene activeMinigameScene = m_activeMinigameScene;
		activeMinigameScene.m_onMinigameComplete = (UnityAction)Delegate.Combine(activeMinigameScene.m_onMinigameComplete, new UnityAction(CompleteMinigame));
		MinigameScene activeMinigameScene2 = m_activeMinigameScene;
		activeMinigameScene2.m_onMinigameClose = (UnityAction)Delegate.Combine(activeMinigameScene2.m_onMinigameClose, new UnityAction(CancelMinigame));
	}

	private void UnloadMinigame()
	{
		if (m_activeMinigameScene != null)
		{
			MinigameScene activeMinigameScene = m_activeMinigameScene;
			activeMinigameScene.m_onMinigameComplete = (UnityAction)Delegate.Remove(activeMinigameScene.m_onMinigameComplete, new UnityAction(CompleteMinigame));
			MinigameScene activeMinigameScene2 = m_activeMinigameScene;
			activeMinigameScene2.m_onMinigameClose = (UnityAction)Delegate.Remove(activeMinigameScene2.m_onMinigameClose, new UnityAction(CancelMinigame));
			m_activeMinigameScene = null;
		}
		MinigameManager.SetActivateMinigameMetadata(null);
	}

	private void CancelMinigame()
	{
		if (m_minigame.DoFullscreenFade)
		{
			GlobalReferences.Instance.EventChannels.Generic.ScreenFadeOfType.Raise(FadeType.None);
		}
		UnloadMinigame();
		m_shouldExit = true;
	}

	public override void Cancel()
	{
		base.Cancel();
		CancelMinigame();
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (m_loadedSceneHandle.IsValid())
		{
			Addressables.Release(m_loadedSceneHandle);
		}
	}
}
