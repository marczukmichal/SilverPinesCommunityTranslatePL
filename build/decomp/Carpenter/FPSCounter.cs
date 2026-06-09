using UnityEngine;

public class FPSCounter : MonoBehaviour
{
	public int m_frameRange = 60;

	private int[] m_fpsBuffer;

	private int m_fpsBufferIndex;

	public int AverageFPS { get; private set; }

	public int HighestFPS { get; private set; }

	public int LowestFPS { get; private set; }

	private void Update()
	{
		if (m_fpsBuffer == null || m_fpsBuffer.Length != m_frameRange)
		{
			InitializeBuffer();
		}
		UpdateBuffer();
		CalculateFPS();
	}

	private void InitializeBuffer()
	{
		if (m_frameRange <= 0)
		{
			m_frameRange = 1;
		}
		m_fpsBuffer = new int[m_frameRange];
		m_fpsBufferIndex = 0;
	}

	private void UpdateBuffer()
	{
		m_fpsBuffer[m_fpsBufferIndex++] = (int)(1f / Time.unscaledDeltaTime);
		if (m_fpsBufferIndex >= m_frameRange)
		{
			m_fpsBufferIndex = 0;
		}
	}

	private void CalculateFPS()
	{
		int num = 0;
		int num2 = 0;
		int num3 = int.MaxValue;
		for (int i = 0; i < m_frameRange; i++)
		{
			int num4 = m_fpsBuffer[i];
			num += num4;
			if (num4 > num2)
			{
				num2 = num4;
			}
			if (num4 < num3)
			{
				num3 = num4;
			}
		}
		AverageFPS = (int)((float)num / (float)m_frameRange);
		HighestFPS = num2;
		LowestFPS = num3;
	}
}
