using UnityEngine;
using UnityEngine.Localization;

public class KeyRingItemDefinition : ItemDefinition
{
	[Header("Event Channels")]
	[SerializeField]
	private ItemInstanceGameEventChannel m_showKeyRingEventChannel;

	[Header("Key Ring Settings")]
	[SerializeField]
	private LocalizedString m_specialActionStringReference;

	public override bool HasSpecialAction()
	{
		return true;
	}

	public override LocalizedString GetSpecialActionText()
	{
		return m_specialActionStringReference;
	}

	public void SendShowKeyRingEventChannel(ItemInstance item)
	{
		m_showKeyRingEventChannel.Raise(item);
	}

	public override bool CanBeCombinedWithAnything()
	{
		return true;
	}
}
