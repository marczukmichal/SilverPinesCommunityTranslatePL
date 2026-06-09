using System.Collections.Generic;
using UnityEngine;

public class CycleButtonSubButtons : CycleButton
{
	[Header("Sub Buttons")]
	[SerializeField]
	private CycleButtonSubButtonInstance m_prefab;

	[SerializeField]
	private RectTransform m_buttonParent;

	private List<CycleButtonSubButtonInstance> m_buttons = new List<CycleButtonSubButtonInstance>();

	protected override void UpdateDisplay()
	{
		base.UpdateDisplay();
		for (int i = 0; i < base.OptionsCount && i < m_buttons.Count; i++)
		{
			m_buttons[i].SetHighlighted(i == base.Value);
		}
	}

	protected override void RefreshOption()
	{
		base.RefreshOption();
		foreach (CycleButtonSubButtonInstance button in m_buttons)
		{
			Object.Destroy(button.gameObject);
		}
		m_buttons.Clear();
		foreach (string rawStringOption in m_rawStringOptions)
		{
			CycleButtonSubButtonInstance cycleButtonSubButtonInstance = Object.Instantiate(m_prefab, m_buttonParent);
			cycleButtonSubButtonInstance.Setup(rawStringOption);
			m_buttons.Add(cycleButtonSubButtonInstance);
		}
	}
}
