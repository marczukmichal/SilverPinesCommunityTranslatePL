using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CableStationBellsMinigame : MonoBehaviour, IMinigameBackInputHandler
{
	[SerializeField]
	private float m_initialDelay = 1f;

	[SerializeField]
	private CableStationBellInstance[] m_bellCallSequence;

	[SerializeField]
	private float[] m_completionSequenceTimings;

	private bool m_completeSequenceActive;

	private int m_sequenceIndex;

	private void Start()
	{
		CableStationBellInstance[] componentsInChildren = GetComponentsInChildren<CableStationBellInstance>(includeInactive: true);
		foreach (CableStationBellInstance obj in componentsInChildren)
		{
			obj.OnRing = (UnityAction<CableStationBellInstance>)Delegate.Combine(obj.OnRing, new UnityAction<CableStationBellInstance>(OnBellRing));
		}
	}

	private void OnBellRing(CableStationBellInstance bell)
	{
		if (m_completeSequenceActive)
		{
			return;
		}
		if (m_bellCallSequence[m_sequenceIndex] == bell)
		{
			m_sequenceIndex++;
			if (m_sequenceIndex >= m_bellCallSequence.Length)
			{
				StartCoroutine(OnBellCompleteSequence());
			}
		}
		else
		{
			m_sequenceIndex = 0;
		}
	}

	public void OnButtonPressed()
	{
		if (!m_completeSequenceActive)
		{
			StartCoroutine(OnBellCompleteSequence());
		}
	}

	public bool TryBackInput()
	{
		if (m_completeSequenceActive)
		{
			return true;
		}
		return false;
	}

	private IEnumerator OnBellCompleteSequence()
	{
		bool success = true;
		m_completeSequenceActive = true;
		CableStationBellInstance[] componentsInChildren = GetComponentsInChildren<CableStationBellInstance>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].DisableInput();
		}
		yield return new WaitForSeconds(m_initialDelay);
		int index = 0;
		CableStationBellInstance[] bellCallSequence = m_bellCallSequence;
		for (int j = 0; j < bellCallSequence.Length; j++)
		{
			if (!bellCallSequence[j].Ring())
			{
				success = false;
			}
			yield return new WaitForSecondsRealtime(m_completionSequenceTimings[index]);
			index++;
		}
		if (success)
		{
			OnSuccess();
		}
		else
		{
			m_completeSequenceActive = false;
		}
	}

	private void OnSuccess()
	{
		GetComponentInParent<MinigameScene>().SetMinigameCompleted();
	}
}
