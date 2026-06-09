using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class DialogueSettings
{
	[SerializeField]
	public UnityEvent m_onDialogueStarted;

	[SerializeField]
	public UnityEvent m_onDialogueFinished;

	[SerializeField]
	public bool m_forceContinue;
}
