using System;
using UnityEngine;
using UnityEngine.Events;

public class CodeCombinationMinigame : MonoBehaviour, IMinigameComponent
{
	[SerializeField]
	private BaseCodeCombinationValue[] m_combinationWheels;

	[SerializeField]
	private string m_targetCode;

	[SerializeField]
	private UnityEvent m_onCodeCorrect;

	[SerializeField]
	private AudioEvent m_unlockAudioEvent;

	private CodeCombinationMinigameData m_minigameData;

	private void Start()
	{
		BaseCodeCombinationValue[] combinationWheels = m_combinationWheels;
		foreach (BaseCodeCombinationValue obj in combinationWheels)
		{
			obj.OnValueChanged = (UnityAction<int>)Delegate.Combine(obj.OnValueChanged, new UnityAction<int>(OnValueChanged));
		}
	}

	private void OnValueChanged(int wheelValue)
	{
		CheckForCompletion();
		if (m_minigameData != null)
		{
			for (int i = 0; i < m_combinationWheels.Length; i++)
			{
				m_minigameData.SetValueAtIndex(i, m_combinationWheels[i].CurrentIndex);
			}
		}
	}

	private string GetCode()
	{
		string text = "";
		bool flag = true;
		BaseCodeCombinationValue[] combinationWheels = m_combinationWheels;
		foreach (BaseCodeCombinationValue baseCodeCombinationValue in combinationWheels)
		{
			if (!flag && m_targetCode.Contains("/"))
			{
				text += "/";
			}
			flag = false;
			text = (baseCodeCombinationValue.isActiveAndEnabled ? (text + baseCodeCombinationValue.CurrentValue.ToString()) : (text + "-"));
		}
		return text;
	}

	private void CheckForCompletion()
	{
		if (GetCode().Equals(m_targetCode))
		{
			if (m_unlockAudioEvent != null)
			{
				m_unlockAudioEvent.Play2D();
			}
			m_onCodeCorrect.Invoke();
		}
	}

	public void Setup(GameObject parent)
	{
		m_minigameData = parent.GetComponent<CodeCombinationMinigameData>();
		if (m_minigameData != null)
		{
			m_targetCode = m_minigameData.Code;
			for (int i = 0; i < m_combinationWheels.Length; i++)
			{
				m_combinationWheels[i].SetValueToIndex(m_minigameData.GetValueAtIndex(i));
			}
		}
	}
}
