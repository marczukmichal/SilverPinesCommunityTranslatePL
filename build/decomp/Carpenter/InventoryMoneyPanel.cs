using TMPro;
using UnityEngine;

public class InventoryMoneyPanel : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_text;

	private void OnEnable()
	{
		UpdateMoneyAmount(GlobalReferences.Instance.MainInventory.Money);
		GlobalReferences.Instance.EventChannels.Inventory.OnPlayerMoneyUpdated.Register(UpdateMoneyAmount);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Inventory.OnPlayerMoneyUpdated.Register(UpdateMoneyAmount);
	}

	private void UpdateMoneyAmount(int newMoney)
	{
		m_text.text = "$" + newMoney;
	}
}
