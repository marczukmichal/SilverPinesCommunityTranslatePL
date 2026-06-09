using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerRumbleManager
{
	private class ActiveRumble
	{
		public ControllerRumbleSettings m_rumbleSettings;

		public Vector2 m_sourcePosition;

		public float m_startTime;
	}

	private List<ActiveRumble> m_activeRumbles = new List<ActiveRumble>();

	private bool m_rumblePaused;

	public void Rumble(ControllerRumbleSettings rumbleSettings, Vector2 position)
	{
		if (GlobalReferences.Instance.InputState.InputMode == InputState.Mode.Gamepad)
		{
			m_activeRumbles.Add(new ActiveRumble
			{
				m_rumbleSettings = rumbleSettings,
				m_sourcePosition = position,
				m_startTime = Time.time
			});
		}
	}

	public void ResetRumble()
	{
		m_activeRumbles.Clear();
		InputSystem.ResetHaptics();
	}

	public void PauseRumble()
	{
		m_rumblePaused = true;
		InputSystem.PauseHaptics();
	}

	public void ResumeRumble()
	{
		m_rumblePaused = false;
		InputSystem.ResumeHaptics();
	}

	private bool IsRumbleActive()
	{
		if (!GlobalReferences.Instance.UserPreferences.ControllerRumble)
		{
			return false;
		}
		if (Time.timeScale <= 0f)
		{
			return false;
		}
		if (GlobalReferences.Instance.InputState.InputMode == InputState.Mode.Gamepad)
		{
			return !m_rumblePaused;
		}
		return false;
	}

	public void Update()
	{
		if (IsRumbleActive())
		{
			float num = 0f;
			float num2 = 0f;
			bool flag = false;
			Vector2 b = Vector2.zero;
			if (GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item != null)
			{
				b = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item.transform.position;
			}
			foreach (ActiveRumble activeRumble in m_activeRumbles)
			{
				if (activeRumble.m_startTime + activeRumble.m_rumbleSettings.m_duration < Time.time)
				{
					flag = true;
					continue;
				}
				float distance = activeRumble.m_rumbleSettings.m_distance;
				float num3 = Vector2.Distance(activeRumble.m_sourcePosition, b);
				if (num3 < distance)
				{
					float time = (Time.time - activeRumble.m_startTime) / activeRumble.m_rumbleSettings.m_duration;
					activeRumble.m_rumbleSettings.m_distanceFalloff.Evaluate((distance - num3) / distance);
					num = Mathf.Max(activeRumble.m_rumbleSettings.m_lowFrequencyCurve.Evaluate(time), num);
					num2 = Mathf.Max(activeRumble.m_rumbleSettings.m_highFrequencyCurve.Evaluate(time), num2);
				}
			}
			if (flag)
			{
				m_activeRumbles.RemoveAll((ActiveRumble x) => x.m_startTime + x.m_rumbleSettings.m_duration < Time.time);
			}
			if (Gamepad.current != null)
			{
				Gamepad.current.SetMotorSpeeds(num, num2);
			}
		}
		else if (Gamepad.current != null)
		{
			Gamepad.current.SetMotorSpeeds(0f, 0f);
		}
	}
}
