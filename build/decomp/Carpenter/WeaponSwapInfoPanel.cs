using UnityEngine;
using UnityEngine.UI;

public class WeaponSwapInfoPanel : MonoBehaviour
{
	[Header("Current Weapon")]
	[SerializeField]
	private Image m_currentWeapon;

	[Header("Up Weapon")]
	[SerializeField]
	private Image m_upArrow;

	[SerializeField]
	private Image m_upWeaponImage;

	[Header("Down Weapon")]
	[SerializeField]
	private Image m_downArrow;

	[SerializeField]
	private Image m_downWeaponImage;

	public void Populate(PlayerMainInventory inventory)
	{
		bool active;
		bool num = (active = inventory.CanCycleProjectileWeapon());
		bool active2 = false;
		if (num)
		{
			ItemInstance nextWeaponToCycleTo = inventory.GetNextWeaponToCycleTo(next: false);
			ItemInstance nextWeaponToCycleTo2 = inventory.GetNextWeaponToCycleTo(next: true);
			active2 = nextWeaponToCycleTo != nextWeaponToCycleTo2;
			m_downWeaponImage.sprite = nextWeaponToCycleTo.ItemSprite;
			m_upWeaponImage.sprite = nextWeaponToCycleTo2.ItemSprite;
		}
		if (GlobalReferences.Instance.InputState.InputMode == InputState.Mode.Gamepad)
		{
			active2 = false;
		}
		m_downArrow.gameObject.SetActive(active2);
		m_downWeaponImage.gameObject.SetActive(active2);
		m_upArrow.gameObject.SetActive(active);
		m_upWeaponImage.gameObject.SetActive(active);
		ItemInstance itemInstance = ((inventory.QueuedPrimaryItem != null) ? inventory.QueuedPrimaryItem : inventory.EquippedPrimaryItem);
		if (itemInstance != null)
		{
			m_currentWeapon.sprite = itemInstance.ItemSprite;
		}
	}
}
