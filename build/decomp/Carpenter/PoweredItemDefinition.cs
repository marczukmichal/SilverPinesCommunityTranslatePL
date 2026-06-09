using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Items/Powered Item Definition")]
public class PoweredItemDefinition : ItemDefinition
{
	[SerializeField]
	private float m_maxCharge;

	[SerializeField]
	private float m_minCharge;

	[SerializeField]
	private float m_activatedChargeDrainRate;

	public float MaxCharge => m_maxCharge;

	public float MinCharge => m_minCharge;

	public float ActivatedChargeDrainRate => m_activatedChargeDrainRate;

	public bool UsesCharge => MaxCharge > 0f;

	public override bool CanBeCombinedWithAnything()
	{
		return true;
	}
}
