using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class InteractableSideDoorCommand : BaseInteractableCommand
{
	[SerializeField]
	private SideDoor m_sideDoor;

	[SerializeField]
	private bool m_open;

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		if (m_sideDoor != null)
		{
			m_sideDoor.InteractWithDoor(interactor, m_open);
		}
		else
		{
			Debug.LogWarning("InteractableSideDoorCommand has missing door reference, won't do anything!");
		}
		yield return null;
	}
}
