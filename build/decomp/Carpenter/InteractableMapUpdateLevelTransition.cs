using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class InteractableMapUpdateLevelTransition : BaseInteractableCommand
{
	[SerializeField]
	private LevelTransition m_levelTransition;

	[SerializeField]
	private MapConnectionStatus m_status;

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		if (m_levelTransition == null)
		{
			Debug.LogWarning("InteractableUpdateLevelTransitionMapCommand is missing a LevelTransition, don't do anything");
			yield return null;
		}
		m_levelTransition.SetMapConnectionStatus(m_status);
		yield return null;
	}
}
