using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FuseTripGroup : MonoBehaviour
{
	[SerializeField]
	private FuseTripSwitch[] m_switches;

	[SerializeField]
	private ProgressionVariable m_progressionVariable;

	[SerializeField]
	private Image m_stateLightImage;

	[SerializeField]
	private Color m_stateLightOnColor;

	[SerializeField]
	private Color m_stateLightOffColor;

	private void Start()
	{
		UpateLight(m_progressionVariable.Value);
		FuseTripSwitch[] switches = m_switches;
		foreach (FuseTripSwitch obj in switches)
		{
			obj.OnStateChanged = (UnityAction<bool>)Delegate.Combine(obj.OnStateChanged, new UnityAction<bool>(OnStateChanged));
		}
	}

	private void OnStateChanged(bool newValue)
	{
		CheckState();
	}

	private void CheckState()
	{
		bool flag = true;
		FuseTripSwitch[] switches = m_switches;
		for (int i = 0; i < switches.Length; i++)
		{
			if (!switches[i].IsTripSwitchActive())
			{
				flag = false;
			}
		}
		m_progressionVariable.Value = flag;
		UpateLight(flag);
	}

	private void UpateLight(bool active)
	{
		if (m_stateLightImage != null)
		{
			m_stateLightImage.color = (active ? m_stateLightOnColor : m_stateLightOffColor);
		}
	}
}
