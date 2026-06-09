using System;

[Serializable]
public class MeleeChargeState
{
	public float[] m_staminaRemaining;

	public MeleeChargeState()
	{
		m_staminaRemaining = new float[3];
	}
}
