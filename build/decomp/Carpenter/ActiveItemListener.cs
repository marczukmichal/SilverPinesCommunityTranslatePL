using UnityEngine;
using UnityEngine.Events;

public class ActiveItemListener : MonoBehaviour
{
	[Header("Item")]
	[SerializeField]
	private ItemDefinition m_itemDefinition;

	[Header("Event Channels")]
	[SerializeField]
	private ItemInstanceGameEventChannel m_actvatedEventChannel;

	[Header("Events")]
	[SerializeField]
	private UnityEvent m_onActiveEvent;

	[SerializeField]
	private UnityEvent m_onDeactiveEvent;

	[SerializeField]
	private UnityEvent m_onTransiitonActiveEvent;

	[Header("Audio Events")]
	[SerializeField]
	private AudioEvent m_activateAudioEvent;

	[SerializeField]
	private AudioEvent m_deactiveAudioEvent;

	private ItemInstance m_activeItemInstance;

	public UnityAction<bool> OnActiveStateChangedListener;

	public ItemInstance ActiveItemInstance => m_activeItemInstance;

	public bool ItemActive
	{
		get
		{
			if (m_activeItemInstance == null)
			{
				return false;
			}
			return m_activeItemInstance.Activated;
		}
	}

	private void OnEnable()
	{
		m_actvatedEventChannel.Register(OnItemActivatedStateChagned);
		ItemInstance itemOfType = GetComponentInParent<CharacterInventory>().Inventory.GetItemOfType(m_itemDefinition);
		if (itemOfType != null)
		{
			UpdateForItem(itemOfType, isTransition: false);
		}
		else
		{
			m_onDeactiveEvent.Invoke();
		}
	}

	private void OnDisable()
	{
		m_actvatedEventChannel.Unregister(OnItemActivatedStateChagned);
	}

	private void OnItemActivatedStateChagned(ItemInstance item)
	{
		if (item.ItemDefinition == m_itemDefinition)
		{
			UpdateForItem(item, isTransition: true);
		}
	}

	private void UpdateForItem(ItemInstance item, bool isTransition)
	{
		if (item.Activated)
		{
			m_activeItemInstance = item;
			m_onActiveEvent.Invoke();
			if (isTransition)
			{
				m_onTransiitonActiveEvent.Invoke();
			}
			if (isTransition && m_activateAudioEvent != null)
			{
				GlobalReferences.Instance.EventChannels.Audio.PlayAudio.Raise(new PlayAudioEventData(m_activateAudioEvent, base.transform.position, useOcclusion: false));
			}
		}
		else
		{
			m_activeItemInstance = null;
			m_onDeactiveEvent.Invoke();
			if (isTransition && m_deactiveAudioEvent != null)
			{
				GlobalReferences.Instance.EventChannels.Audio.PlayAudio.Raise(new PlayAudioEventData(m_deactiveAudioEvent, base.transform.position, useOcclusion: false));
			}
		}
		OnActiveStateChangedListener?.Invoke(item.Activated);
	}
}
