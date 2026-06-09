using UnityEngine;
using UnityEngine.UI;

public class ArtifactsSlotsStatusInfo : MonoBehaviour
{
	[SerializeField]
	private Image[] m_slots;

	[SerializeField]
	private Sprite m_filledSlotSprite;

	[SerializeField]
	private Sprite m_emptySlotSprite;

	public void SetSlotsStatus(int totalSlots, int usedSlots)
	{
		if (totalSlots > m_slots.Length)
		{
			Debug.LogError("ArtifactsSlotsStatusInfo - trying to show too many artifact slots, max is " + m_slots.Length + " - trying to show " + totalSlots);
		}
		for (int i = 0; i < m_slots.Length; i++)
		{
			if (i >= totalSlots)
			{
				m_slots[i].gameObject.SetActive(value: false);
				continue;
			}
			m_slots[i].gameObject.SetActive(value: true);
			m_slots[i].sprite = ((i >= usedSlots) ? m_emptySlotSprite : m_filledSlotSprite);
		}
	}
}
