using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[ShowInDesignerInspector]
public class InteractableSetProgressionVariableCommand : BaseInteractableCommand
{
	[SerializeField]
	private ProgressionVariable m_progressionVariable;

	[SerializeField]
	private bool m_value;

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		if (m_progressionVariable != null)
		{
			m_progressionVariable.Value = m_value;
		}
		else
		{
			Debug.LogWarning("InteractableSetProgressionVariableCommand has missing variable, won't do anything!");
		}
		yield return null;
	}
}
