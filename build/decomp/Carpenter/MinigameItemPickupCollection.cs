using UnityEngine;

public class MinigameItemPickupCollection : MonoBehaviour
{
	[SerializeField]
	private MinigameScene m_minigame;

	private ItemPickup[] m_items;

	private LorePickup[] m_lore;

	private void Start()
	{
		m_items = GetComponentsInChildren<ItemPickup>();
		m_lore = GetComponentsInChildren<LorePickup>();
	}

	private void Update()
	{
		bool flag = true;
		ItemPickup[] items = m_items;
		for (int i = 0; i < items.Length; i++)
		{
			if (!items[i].ItemPickedup)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			LorePickup[] lore = m_lore;
			for (int i = 0; i < lore.Length; i++)
			{
				if (!lore[i].LorePickedup)
				{
					flag = false;
					break;
				}
			}
		}
		if (flag)
		{
			m_minigame.SetMinigameCompleted();
			base.enabled = false;
		}
	}
}
