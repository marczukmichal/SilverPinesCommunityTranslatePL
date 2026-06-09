using System;
using UnityEngine;

[Serializable]
public class ControllerRumbleSettings
{
	public AnimationCurve m_lowFrequencyCurve;

	public AnimationCurve m_highFrequencyCurve;

	public float m_distance;

	public float m_duration;

	public AnimationCurve m_distanceFalloff;
}
