using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
[VolumeComponentMenu("Custom/Carpenter Danger")]
public class CarpenterDangerVolumeComponent : VolumeComponent, IPostProcessComponent
{
	public ClampedFloatParameter m_intensity = new ClampedFloatParameter(0f, 0f, 1f, overrideState: true);

	public ClampedFloatParameter m_warpScale = new ClampedFloatParameter(-0.004f, -1f, 1f);

	public ClampedFloatParameter m_warpPower = new ClampedFloatParameter(6f, 0f, 10f);

	public ClampedFloatParameter m_timeAmplitude = new ClampedFloatParameter(0.04f, 0f, 1f);

	public ClampedFloatParameter m_timeFrequency = new ClampedFloatParameter(5f, 0f, 20f);

	public Vector2Parameter m_bleedOffset = new Vector2Parameter(new Vector2(0.005f, 0.002f));

	public bool IsActive()
	{
		return m_intensity.value > 0f;
	}
}
