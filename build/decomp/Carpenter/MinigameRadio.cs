using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MinigameRadio : MonoBehaviour, IMinigameComponent
{
	public enum RadioMode
	{
		FM,
		AM,
		SSB
	}

	[SerializeField]
	private TextMeshProUGUI m_frequencyText;

	[SerializeField]
	private MinigameInteractiveDial m_frequencyDial;

	[SerializeField]
	private float m_startingValue = 1150f;

	[SerializeField]
	private float m_minimumValue = 500f;

	[SerializeField]
	private float m_maximumValue = 2000f;

	[SerializeField]
	private TextMeshProUGUI m_modeText;

	[SerializeField]
	private RectTransform m_alignmentBar;

	private MinigameRadioData m_radioData;

	private RadioMode m_radioMode;

	private float m_frequencyValue;

	public void SetMode(int mode)
	{
		m_radioMode = (RadioMode)mode;
		if (m_radioData != null)
		{
			m_radioData.RadioMode = m_radioMode;
		}
		UpdateModeText();
	}

	private void Start()
	{
		m_frequencyValue = m_startingValue;
		MinigameInteractiveDial frequencyDial = m_frequencyDial;
		frequencyDial.OnDialRotated = (UnityAction<float>)Delegate.Combine(frequencyDial.OnDialRotated, new UnityAction<float>(OnFrequencyChanged));
		UpdateFrequencyText();
		UpdateModeText();
	}

	private void OnFrequencyChanged(float delta)
	{
		m_frequencyValue += delta;
		m_frequencyValue = Mathf.Clamp(m_frequencyValue, m_minimumValue, m_maximumValue);
		if (m_radioData != null)
		{
			m_radioData.FrequencyValue = m_frequencyValue;
		}
		UpdateFrequencyText();
	}

	private void UpdateFrequencyText()
	{
		if ((bool)m_frequencyText)
		{
			m_frequencyText.text = m_frequencyValue.ToString(".0") + "MHz";
		}
		if (m_alignmentBar != null)
		{
			Vector2 anchorMax = m_alignmentBar.anchorMax;
			Vector2 anchorMin = m_alignmentBar.anchorMin;
			anchorMin.x = (anchorMax.x = Mathf.InverseLerp(m_minimumValue, m_maximumValue, m_frequencyValue));
			m_alignmentBar.anchorMin = anchorMin;
			m_alignmentBar.anchorMax = anchorMax;
		}
	}

	private void UpdateModeText()
	{
		if (m_modeText != null)
		{
			m_modeText.text = m_radioMode.ToString();
		}
	}

	public void BroadcastButtonPressed()
	{
		if (m_radioData != null && m_radioData.TryBroadcast())
		{
			MinigameScene minigameScene = UnityEngine.Object.FindFirstObjectByType<MinigameScene>();
			if (minigameScene != null)
			{
				minigameScene.SetMinigameCompleted();
			}
		}
	}

	public void Setup(GameObject parent)
	{
		m_radioData = parent.GetComponent<MinigameRadioData>();
		if (m_radioData != null)
		{
			m_radioMode = m_radioData.RadioMode;
			m_frequencyValue = m_radioData.FrequencyValue;
		}
		UpdateFrequencyText();
		UpdateModeText();
	}
}
