public class StaminaInfoData
{
	public bool m_isExhausted;

	public float m_baseCurrentStamina;

	public float m_availableStamina;

	public float m_maxStamina;

	public float m_totalMaxStamina;

	public float m_exhaustionRecoveryStamina;

	public float m_staminaReserve;

	public bool m_forceShow;

	public float StaminaPercentage => m_availableStamina / m_totalMaxStamina;
}
