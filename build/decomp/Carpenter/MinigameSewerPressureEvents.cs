using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

public class MinigameSewerPressureEvents : MonoBehaviour
{
	[SerializeField]
	private EventReference m_steamPressAudioEventReference;

	[SerializeField]
	private AnimationCurve m_volumeCurve;

	private float m_pressure;

	private EventInstance m_steamPressureAudioInstance;

	private void OnEnable()
	{
		m_steamPressureAudioInstance = RuntimeManager.CreateInstance(m_steamPressAudioEventReference);
		RuntimeManager.AttachInstanceToGameObject(m_steamPressureAudioInstance, base.gameObject);
		m_steamPressureAudioInstance.setVolume(0f);
		m_steamPressureAudioInstance.start();
	}

	private void OnDisable()
	{
		m_steamPressureAudioInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		m_steamPressureAudioInstance.clearHandle();
	}

	public void RegisterEvents(MinigameSewerPressureMain main)
	{
		main.m_pressureLevelChanged = (UnityAction<float>)Delegate.Combine(main.m_pressureLevelChanged, new UnityAction<float>(PressureValueChanged));
	}

	public void UnregisterEvents(MinigameSewerPressureMain main)
	{
		main.m_pressureLevelChanged = (UnityAction<float>)Delegate.Remove(main.m_pressureLevelChanged, new UnityAction<float>(PressureValueChanged));
	}

	private void PressureValueChanged(float steamPercent)
	{
		m_pressure = steamPercent;
		m_steamPressureAudioInstance.setVolume(m_volumeCurve.Evaluate(steamPercent));
	}
}
