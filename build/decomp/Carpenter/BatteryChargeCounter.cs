using UnityEngine;
using UnityEngine.UI;

public class BatteryChargeCounter : MonoBehaviour
{
	[SerializeField]
	private Image[] m_chargeBlocks;

	[SerializeField]
	private Image m_frame;

	[SerializeField]
	private Color m_emptyColor;

	[SerializeField]
	private Color m_normalColor;

	[SerializeField]
	private Color m_lowColor;

	[SerializeField]
	private float m_lowPercent;

	private void SetCount(float capacity, float amount)
	{
		int num = Mathf.CeilToInt(amount / capacity * (float)m_chargeBlocks.Length);
		Color color = ((amount <= 0f) ? m_emptyColor : ((!(amount / capacity <= m_lowPercent)) ? m_normalColor : m_lowColor));
		for (int i = 0; i < m_chargeBlocks.Length; i++)
		{
			m_chargeBlocks[i].color = ((i < num) ? color : m_emptyColor);
		}
		m_frame.color = color;
	}

	public void PopulateFromPoweredItemInstance(PoweredItemInstance poweredInstance)
	{
		SetCount(poweredInstance.PoweredItemDefinition.MaxCharge, poweredInstance.ChargeAmount);
	}
}
