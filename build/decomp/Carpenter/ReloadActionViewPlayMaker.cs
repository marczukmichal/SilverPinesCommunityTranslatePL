using UnityEngine;

public class ReloadActionViewPlayMaker : ActionUseStateViewBase
{
	[SerializeField]
	private PlayMakerFSM m_playmakerFsm;

	[SerializeField]
	private string m_onAddAmmoEvent;

	[SerializeField]
	private string m_onLastEvent;

	private int m_currentAmmoCount;

	public override void Setup(int currentCount, int maxCount)
	{
		m_currentAmmoCount = currentCount;
		m_playmakerFsm.SendEvent("Reset");
	}

	public override void UpdateAmmoCount(int newAmmoAmount, bool isLast)
	{
		if (newAmmoAmount > m_currentAmmoCount)
		{
			m_playmakerFsm.SendEvent(isLast ? m_onLastEvent : m_onAddAmmoEvent);
		}
		m_currentAmmoCount = newAmmoAmount;
	}
}
