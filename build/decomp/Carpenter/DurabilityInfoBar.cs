using System.Collections.Generic;
using UnityEngine;

public class DurabilityInfoBar : MonoBehaviour
{
	public enum DurabilityBarState
	{
		Normal,
		LastPip,
		Broken
	}

	[SerializeField]
	private DurabilityPip m_pipTemplate;

	[SerializeField]
	private AudioEvent m_pipDurabilityAudioEvent;

	private List<DurabilityPip> m_activePips = new List<DurabilityPip>();

	private void Start()
	{
		m_pipTemplate.gameObject.SetActive(value: false);
	}

	private void SetDurabilityInfo(float currentDurability, int maxDurability, int durabilityPerPip, bool animateStateChange = false, bool isLoss = false)
	{
		int num = Mathf.CeilToInt((float)maxDurability / (float)durabilityPerPip);
		int num2 = durabilityPerPip * num % maxDurability;
		SetNumberOfPips(num);
		int num3 = -num2;
		DurabilityBarState barState = DurabilityBarState.Normal;
		if (currentDurability <= 0f)
		{
			barState = DurabilityBarState.Broken;
		}
		else if (currentDurability <= (float)durabilityPerPip)
		{
			barState = DurabilityBarState.LastPip;
		}
		for (int i = 0; i < num; i++)
		{
			DurabilityPip.PipFilledState pipFilledState = DurabilityPip.PipFilledState.Empty;
			if ((float)num3 < currentDurability)
			{
				pipFilledState = ((!((float)(num3 + durabilityPerPip / 2) > currentDurability)) ? DurabilityPip.PipFilledState.Filled : DurabilityPip.PipFilledState.HalfFilled);
			}
			if (m_pipDurabilityAudioEvent != null && isLoss && m_activePips[i].State != pipFilledState && m_activePips[i].State != 0)
			{
				float parameterValue = currentDurability / (float)maxDurability;
				m_pipDurabilityAudioEvent.PlayWithParameteter(Vector3.zero, "DurabilityLeft", parameterValue, useOcclusion: false);
			}
			m_activePips[i].SetFilledState(pipFilledState, animateStateChange);
			m_activePips[i].SetBarState(barState);
			num3 += durabilityPerPip;
		}
	}

	public void SetDurabilityInfo(MeleeWeaponItemInstance itemInstance, bool animateStateChange, bool isLoss)
	{
		SetDurabilityInfo(itemInstance.CurrentDurability, itemInstance.WeaponDefinition.MaxDurability, itemInstance.WeaponDefinition.DurabilityPerPip, animateStateChange, isLoss);
	}

	public void AnimateTakeDamage(MeleeWeaponItemInstance itemInstance, float startingDurabilityProportion, float endingDurabilityProportion)
	{
		int durabilityPerPip = itemInstance.WeaponDefinition.DurabilityPerPip;
		int num = Mathf.RoundToInt((float)itemInstance.WeaponDefinition.MaxDurability * startingDurabilityProportion);
		int num2 = Mathf.RoundToInt((float)itemInstance.WeaponDefinition.MaxDurability * endingDurabilityProportion);
		for (int i = 0; i < m_activePips.Count; i++)
		{
			int num3 = i * durabilityPerPip;
			int num4 = num3 + durabilityPerPip;
			if (num3 <= num2 && num4 >= num)
			{
				m_activePips[i].AnimateSmallImpact();
			}
		}
	}

	private void SetNumberOfPips(int pips)
	{
		if (pips > m_activePips.Count)
		{
			for (int i = m_activePips.Count; i < pips; i++)
			{
				DurabilityPip item = Object.Instantiate(m_pipTemplate, m_pipTemplate.transform.parent);
				m_activePips.Add(item);
			}
		}
		for (int j = 0; j < m_activePips.Count; j++)
		{
			m_activePips[j].gameObject.SetActive(j < pips);
		}
	}
}
