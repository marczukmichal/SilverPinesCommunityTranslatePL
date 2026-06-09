using UnityEngine;
using UnityEngine.Events;

public class SpendCoinsMinigame : MonoBehaviour
{
	[SerializeField]
	private ItemDefinition m_coinItemDefinition;

	[SerializeField]
	private UIMinigameHighlight m_coinSlotHighlight;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_coinAddAudioEvent;

	[SerializeField]
	private AudioEvent m_coinReturnAudioEvent;

	private bool m_requiresCoin = true;

	private bool m_insertedCoin;

	[SerializeField]
	private UnityEvent m_onCoinAdded;

	public bool InsertedCoin => m_insertedCoin;

	public bool RequiresCoin
	{
		get
		{
			return m_requiresCoin;
		}
		set
		{
			if (value != m_requiresCoin)
			{
				if (m_coinSlotHighlight != null)
				{
					m_coinSlotHighlight.enabled = false;
				}
				m_requiresCoin = value;
			}
		}
	}

	public void TryAddCoin()
	{
		if (RequiresCoin && GlobalReferences.Instance.MainInventory.Money > 0)
		{
			m_coinAddAudioEvent.Play2D();
			GlobalReferences.Instance.MainInventory.RemoveMoney(1);
			RequiresCoin = false;
			m_insertedCoin = true;
			m_onCoinAdded.Invoke();
		}
	}

	public void ReturnCoin()
	{
		if (m_insertedCoin)
		{
			m_coinReturnAudioEvent.Play2D();
			GlobalReferences.Instance.MainInventory.AddMoney(1);
			m_insertedCoin = false;
		}
	}
}
