using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class MeleeWeaponItemInstance : ItemInstance
{
	[SerializeField]
	private float m_currentDurability;

	public UnityAction OnWeaponBroken;

	public float CurrentDurability => m_currentDurability;

	public float DurabilityProportion
	{
		get
		{
			if (WeaponDefinition.MaxDurability <= 0)
			{
				return 1f;
			}
			return m_currentDurability / (float)WeaponDefinition.MaxDurability;
		}
	}

	public int DurabilityPercent => Mathf.RoundToInt(DurabilityProportion * 100f);

	public MeleeWeaponItemDefinition WeaponDefinition => base.ItemDefinition as MeleeWeaponItemDefinition;

	public MeleeWeaponItemInstance(MeleeWeaponItemDefinition definition, int stackSize, Vector2Int position, bool rotated, int durability)
		: base(definition, stackSize, position, rotated)
	{
		m_currentDurability = durability;
	}

	public MeleeWeaponItemInstance(MeleeWeaponItemDefinition definition, int stackSize, Vector2Int position, bool rotated)
		: this(definition, stackSize, position, rotated, definition.MaxDurability)
	{
	}

	public bool RemoveDurability(float amount)
	{
		if (!WeaponDefinition.HasDurability)
		{
			return false;
		}
		if (m_currentDurability <= 0f)
		{
			return false;
		}
		m_currentDurability -= amount;
		m_currentDurability = Mathf.Max(m_currentDurability, 0f);
		if (m_currentDurability <= 0f)
		{
			OnWeaponBroken?.Invoke();
			return true;
		}
		return false;
	}

	public void SetDurabilityProportion(float proportion)
	{
		m_currentDurability = proportion * (float)WeaponDefinition.MaxDurability;
	}

	public void RepairDurability(int durability)
	{
		m_currentDurability = Mathf.Min(m_currentDurability + (float)durability, WeaponDefinition.MaxDurability);
	}

	public bool IsBroken()
	{
		if (WeaponDefinition.HasDurability)
		{
			return m_currentDurability <= 0f;
		}
		return false;
	}

	public override int GetDifficultyResourceValue(GameDifficultyResourceScoreType type)
	{
		return Mathf.RoundToInt((float)base.GetDifficultyResourceValue(type) * DurabilityProportion);
	}
}
