using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MinigameGearCogManager : MonoBehaviour
{
	[SerializeField]
	private MinigameGearCog m_poweredCog;

	[SerializeField]
	private MinigameGearCog[] m_cogs;

	[Header("Events")]
	[SerializeField]
	private UnityEvent m_powerStartEvent;

	[SerializeField]
	private UnityEvent m_powerEndEvent;

	[SerializeField]
	private UnityEvent m_failedEvent;

	private bool m_isPowered;

	public void EnablePowered(bool powered)
	{
		if (powered == m_isPowered)
		{
			return;
		}
		StopAllCoroutines();
		m_isPowered = powered;
		if (powered)
		{
			m_poweredCog.SetPowered(powered: true);
			StartCoroutine(PowerFromCog(m_poweredCog));
			m_powerStartEvent?.Invoke();
			return;
		}
		MinigameGearCog[] cogs = m_cogs;
		for (int i = 0; i < cogs.Length; i++)
		{
			cogs[i].SetPowered(powered: false);
		}
		m_powerEndEvent?.Invoke();
	}

	public void CogFail()
	{
		EnablePowered(powered: false);
		m_failedEvent?.Invoke();
	}

	public IEnumerator PowerFromCog(MinigameGearCog cog)
	{
		yield return new WaitForSecondsRealtime(0.1f);
		List<MinigameGearCog> overlappingCogs = cog.GetOverlappingCogs();
		MinigameGearCog.RotationDirection rotationDirection = ((cog.Direction == MinigameGearCog.RotationDirection.Clockwise) ? MinigameGearCog.RotationDirection.AntiClockwise : MinigameGearCog.RotationDirection.Clockwise);
		foreach (MinigameGearCog item in overlappingCogs)
		{
			if (!item.IsPowered && !(item == cog))
			{
				if (!item.CanTurnInDirection(rotationDirection))
				{
					item.PlayFailTurnAnimation(rotationDirection);
					CogFail();
					break;
				}
				item.SetPowered(powered: true);
				item.SetRotationDirection(rotationDirection);
				StartCoroutine(PowerFromCog(item));
			}
		}
	}
}
