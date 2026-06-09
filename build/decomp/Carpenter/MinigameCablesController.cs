using System;
using UnityEngine;

public class MinigameCablesController : MonoBehaviour
{
	[Serializable]
	public class CableSettings
	{
		public MinigameCableHead m_cable;

		public int m_targetIndex;
	}

	[SerializeField]
	private CableSettings[] m_cables;

	private bool m_completed;

	private void Start()
	{
		m_completed = false;
	}

	private void Update()
	{
		if (!m_completed && CheckForCompletion())
		{
			m_completed = true;
			CableSettings[] cables = m_cables;
			for (int i = 0; i < cables.Length; i++)
			{
				cables[i].m_cable.DisableInteract();
				GetComponentInParent<MinigameScene>().SetMinigameCompleted();
			}
		}
	}

	private bool CheckForCompletion()
	{
		bool result = true;
		CableSettings[] cables = m_cables;
		foreach (CableSettings cableSettings in cables)
		{
			bool flag = false;
			if (cableSettings.m_cable.isActiveAndEnabled && !cableSettings.m_cable.IsHeld && cableSettings.m_cable.PlugIndex == cableSettings.m_targetIndex)
			{
				flag = true;
			}
			if (!flag)
			{
				result = false;
			}
		}
		return result;
	}

	public bool CanPlugInto(int index)
	{
		CableSettings[] cables = m_cables;
		for (int i = 0; i < cables.Length; i++)
		{
			if (cables[i].m_cable.PlugIndex == index)
			{
				return false;
			}
		}
		return true;
	}
}
