using System;
using UnityEngine;
using UnityEngine.Events;

public class StudioSpotlightMinigame : MonoBehaviour
{
	[SerializeField]
	private StudioSpotlightLensSlot m_cyanLens;

	[SerializeField]
	private StudioSpotlightLensSlot m_magentaLens;

	[SerializeField]
	private StudioSpotlightLensSlot m_yellowLens;

	[SerializeField]
	private ProgressionVariableInt m_lightFlagsIntValue;

	[SerializeField]
	private ProgressionVariable m_lightFlagsGoalVariable;

	public void Start()
	{
		StudioSpotlightLensSlot cyanLens = m_cyanLens;
		cyanLens.OnLensStateChanged = (UnityAction)Delegate.Combine(cyanLens.OnLensStateChanged, new UnityAction(OnLensChanged));
		StudioSpotlightLensSlot magentaLens = m_magentaLens;
		magentaLens.OnLensStateChanged = (UnityAction)Delegate.Combine(magentaLens.OnLensStateChanged, new UnityAction(OnLensChanged));
		StudioSpotlightLensSlot yellowLens = m_yellowLens;
		yellowLens.OnLensStateChanged = (UnityAction)Delegate.Combine(yellowLens.OnLensStateChanged, new UnityAction(OnLensChanged));
		OnLensChanged();
	}

	private void OnLensChanged()
	{
		StudioSpotlightCYMLensFlags studioSpotlightCYMLensFlags = StudioSpotlightCYMLensFlags.White;
		if (m_cyanLens.IsFilteringLight())
		{
			studioSpotlightCYMLensFlags |= StudioSpotlightCYMLensFlags.Cyan;
		}
		if (m_magentaLens.IsFilteringLight())
		{
			studioSpotlightCYMLensFlags |= StudioSpotlightCYMLensFlags.Magenta;
		}
		if (m_yellowLens.IsFilteringLight())
		{
			studioSpotlightCYMLensFlags |= StudioSpotlightCYMLensFlags.Yellow;
		}
		m_lightFlagsIntValue.SetValue((int)studioSpotlightCYMLensFlags);
		if (studioSpotlightCYMLensFlags == (StudioSpotlightCYMLensFlags.Magenta | StudioSpotlightCYMLensFlags.Yellow))
		{
			Debug.Log("Red Achieved");
			m_lightFlagsGoalVariable.SetValue(value: true);
		}
		else
		{
			m_lightFlagsGoalVariable.SetValue(value: false);
		}
	}
}
