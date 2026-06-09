using System;
using UnityEngine;
using UnityEngine.Events;

public class MinigameSafeExternalDial : MonoBehaviour
{
	private enum SafeTurnDirection
	{
		ClockwiseRight,
		AntiClockwiseLeft
	}

	[Serializable]
	private struct CodeInput
	{
		public SafeTurnDirection m_direction;

		public int m_number;
	}

	[SerializeField]
	private MinigameInteractiveDial m_dial;

	[SerializeField]
	private RectTransform m_numberSpinner;

	[SerializeField]
	private int m_segmentCount = 8;

	private float m_currentRotationValue;

	[SerializeField]
	private CodeInput[] m_inputs;

	[SerializeField]
	private AudioEvent m_valueChangedAudio;

	[SerializeField]
	private AudioEvent m_safeUnlockAudio;

	[SerializeField]
	private float m_scale = 1f;

	private int m_currentCodeIndex;

	private int m_currentHighlightedNumber;

	private bool m_complete;

	private void Start()
	{
		m_complete = false;
		m_currentCodeIndex = 0;
		m_currentRotationValue = UnityEngine.Random.Range(0f, 1f);
		m_currentHighlightedNumber = GetCurrentHighlightedNumber();
		MinigameInteractiveDial dial = m_dial;
		dial.OnDialRotated = (UnityAction<float>)Delegate.Combine(dial.OnDialRotated, new UnityAction<float>(OnDialRotated));
		UpdateSpinnerVisuals();
	}

	private void Update()
	{
		if (m_complete)
		{
			float num = 1f / (float)m_segmentCount;
			float target = (float)(m_inputs[m_inputs.Length - 1].m_number - 1) * num + num * 0.5f;
			m_currentRotationValue = Mathf.MoveTowards(m_currentRotationValue, target, Time.deltaTime);
			UpdateSpinnerVisuals();
		}
	}

	private void OnDialRotated(float delta)
	{
		if (!m_complete)
		{
			m_currentRotationValue += delta * m_scale;
			if (m_currentRotationValue < 0f)
			{
				m_currentRotationValue = 1f - m_currentRotationValue;
			}
			else
			{
				m_currentRotationValue %= 1f;
			}
			UpdateSpinnerVisuals();
			CheckForUpdatedNumber();
		}
	}

	private void CheckForUpdatedNumber()
	{
		int currentHighlightedNumber = GetCurrentHighlightedNumber();
		if (currentHighlightedNumber != m_currentHighlightedNumber)
		{
			SafeTurnDirection safeTurnDirection = ((Mathf.Abs(currentHighlightedNumber - m_currentHighlightedNumber) != 1) ? ((currentHighlightedNumber > m_currentHighlightedNumber) ? SafeTurnDirection.AntiClockwiseLeft : SafeTurnDirection.ClockwiseRight) : ((currentHighlightedNumber <= m_currentHighlightedNumber) ? SafeTurnDirection.AntiClockwiseLeft : SafeTurnDirection.ClockwiseRight));
			if (m_currentCodeIndex > 0 && safeTurnDirection != m_inputs[m_currentCodeIndex].m_direction)
			{
				ResetProgress();
			}
			else if (currentHighlightedNumber == m_inputs[m_currentCodeIndex].m_number)
			{
				AddProgress();
			}
			m_currentHighlightedNumber = currentHighlightedNumber;
			if (m_valueChangedAudio != null)
			{
				m_valueChangedAudio.Play2D();
			}
		}
	}

	private void ResetProgress()
	{
		m_currentCodeIndex = 0;
	}

	private void AddProgress()
	{
		m_currentCodeIndex++;
		if (m_currentCodeIndex >= m_inputs.Length)
		{
			m_complete = true;
			UnityEngine.Object.FindFirstObjectByType<MinigameScene>().SetMinigameCompleted();
			if (m_safeUnlockAudio != null)
			{
				m_safeUnlockAudio.Play2D();
			}
		}
	}

	private void UpdateSpinnerVisuals()
	{
		m_numberSpinner.localRotation = Quaternion.Euler(0f, 0f, m_currentRotationValue * 360f);
	}

	private int GetCurrentHighlightedNumber()
	{
		float num = 1f / (float)m_segmentCount;
		for (int i = 1; i < m_segmentCount; i++)
		{
			if (m_currentRotationValue > (float)i * num && m_currentRotationValue <= num * (float)(i + 1))
			{
				return m_segmentCount - i;
			}
		}
		return m_segmentCount;
	}
}
