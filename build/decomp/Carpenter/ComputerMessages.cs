using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Minigames/Computer Desktop/Messages")]
public class ComputerMessages : ScriptableObject
{
	[Serializable]
	public struct ComputerMessage
	{
		public string m_subject;

		[Multiline(6)]
		public string m_content;

		public UnityEvent m_onShownMessage;
	}

	public ComputerMessage[] m_messages;
}
