using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(FPSCounter))]
public class FPSDisplay : MonoBehaviour
{
	[Serializable]
	private struct FPSColor
	{
		public Color m_color;

		public int m_minimumFPS;
	}

	[SerializeField]
	private Text m_highestFPSLabel;

	[SerializeField]
	private Text m_averageFPSLabel;

	[SerializeField]
	private Text m_lowestFPSLAbel;

	[SerializeField]
	private FPSColor[] m_coloring;

	private FPSCounter m_fpsCounter;

	private void Awake()
	{
		m_fpsCounter = GetComponent<FPSCounter>();
	}

	private void Update()
	{
		Display(m_highestFPSLabel, m_fpsCounter.HighestFPS);
		Display(m_averageFPSLabel, m_fpsCounter.AverageFPS);
		Display(m_lowestFPSLAbel, m_fpsCounter.LowestFPS);
	}

	private void Display(Text label, int fps)
	{
		label.text = fps.ToString();
		for (int i = 0; i < m_coloring.Length; i++)
		{
			if (fps >= m_coloring[i].m_minimumFPS)
			{
				label.color = m_coloring[i].m_color;
				break;
			}
		}
	}
}
