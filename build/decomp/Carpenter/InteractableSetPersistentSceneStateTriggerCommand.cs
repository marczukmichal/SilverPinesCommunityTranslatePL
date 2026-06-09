using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[ShowInDesignerInspector]
public class InteractableSetPersistentSceneStateTriggerCommand : BaseInteractableCommand
{
	[SerializeField]
	private PersistentSceneTriggerEvents m_sceneState;

	[SerializeField]
	private bool m_value;

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		if (m_sceneState != null)
		{
			m_sceneState.SetState(m_value);
		}
		else
		{
			Debug.LogWarning("InteractableSetPersistentSceneStateTriggerCommand has missing trigger events, won't do anything!");
		}
		yield return null;
	}
}
