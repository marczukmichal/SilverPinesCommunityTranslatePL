using System;
using UnityEngine;

[Serializable]
public class EndLoopSettings
{
	[SerializeField]
	private PlayableDialogue[] m_endLoopDialogues;

	[SerializeField]
	private PlayableDialogue m_specialDialogue;

	[SerializeField]
	private int m_loopCountForSpecialDialogue;

	public bool HasEndLoopDialogue()
	{
		if (m_endLoopDialogues != null)
		{
			return m_endLoopDialogues.Length != 0;
		}
		return false;
	}

	public PlayableDialogue GetDialogue(int endLoopIndex)
	{
		if (m_specialDialogue != null && m_specialDialogue.m_dialogue != null && endLoopIndex == m_loopCountForSpecialDialogue)
		{
			return m_specialDialogue;
		}
		return m_endLoopDialogues[endLoopIndex % m_endLoopDialogues.Length];
	}
}
