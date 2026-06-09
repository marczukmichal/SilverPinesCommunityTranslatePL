using System;
using UnityEngine;

[Serializable]
public class PoweredItemInstance : ItemInstance
{
	[SerializeField]
	private float m_chargeAmount;

	public PoweredItemDefinition PoweredItemDefinition => base.ItemDefinition as PoweredItemDefinition;

	public float ChargeAmount
	{
		get
		{
			return m_chargeAmount;
		}
		set
		{
			if (m_chargeAmount != value)
			{
				m_chargeAmount = Mathf.Clamp(value, PoweredItemDefinition.MinCharge, PoweredItemDefinition.MaxCharge);
			}
		}
	}

	public PoweredItemInstance(PoweredItemDefinition definition, int stackSize, Vector2Int position, bool rotated)
		: base(definition, 1, position, rotated)
	{
		m_chargeAmount = stackSize;
	}

	public override bool CanBeActivated()
	{
		if (base.CanBeActivated())
		{
			if (PoweredItemDefinition.UsesCharge)
			{
				return ChargeAmount > 0f;
			}
			return true;
		}
		return false;
	}
}
