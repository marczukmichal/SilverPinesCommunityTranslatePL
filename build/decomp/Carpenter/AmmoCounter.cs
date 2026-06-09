using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoCounter : MonoBehaviour
{
	[SerializeField]
	private Image m_ammoImage;

	[SerializeField]
	private TextMeshProUGUI m_text;

	[Header("Action")]
	[SerializeField]
	private GameObject m_actionIcon;

	[SerializeField]
	private Color m_actionTextColor;

	[SerializeField]
	private Color m_actionAmmoColor;

	[Header("Text")]
	[SerializeField]
	private bool m_adjustFontSize;

	[SerializeField]
	private float m_singleDigitFontSize = 18f;

	[SerializeField]
	private float m_doubleDigitFontSize = 16f;

	private AmmunitionItemDefinition m_ammunitionItemDefinition;

	private bool m_gotInitialColors;

	private Color m_normalTextColor;

	private Color m_normalAmmoColor;

	private void SaveInitialColors()
	{
		if (!m_gotInitialColors)
		{
			m_normalTextColor = m_text.color;
			m_normalAmmoColor = m_ammoImage.color;
			if (m_actionIcon != null)
			{
				m_actionIcon.SetActive(value: false);
			}
			m_gotInitialColors = true;
		}
	}

	private void SetAmmunitionItemDefinition(AmmunitionItemDefinition definition)
	{
		if (m_ammunitionItemDefinition != definition)
		{
			m_ammunitionItemDefinition = definition;
			m_ammoImage.sprite = m_ammunitionItemDefinition.AmmoCountSprite;
		}
	}

	private void SetCount(int capacity, int amount)
	{
		if (capacity >= 10)
		{
			m_text.text = amount.ToString("00") + "/" + capacity;
		}
		else
		{
			m_text.text = amount + "/" + capacity;
		}
		if (m_adjustFontSize)
		{
			m_text.enableAutoSizing = false;
			m_text.fontSize = ((capacity >= 10) ? m_doubleDigitFontSize : m_singleDigitFontSize);
		}
	}

	public void PopulateFromWeaponInstance(ProjectileWeaponItemInstance weaponInstance)
	{
		SaveInitialColors();
		SetAmmunitionItemDefinition(weaponInstance.LoadedAmmoType);
		SetCount(weaponInstance.GetMaxAmmoCapacity(), weaponInstance.AmmoCount);
		if (m_actionIcon != null)
		{
			m_ammoImage.color = (weaponInstance.RequiresAction ? m_actionAmmoColor : m_normalAmmoColor);
			m_text.color = (weaponInstance.RequiresAction ? m_actionTextColor : m_normalTextColor);
			m_actionIcon.SetActive(weaponInstance.RequiresAction);
		}
	}
}
