using UnityEngine;

public class AudioSourceVisualizer : MonoBehaviour
{
	[SerializeField]
	private RectTransform[] m_bars;

	[SerializeField]
	private AudioSource m_audioSource;

	[SerializeField]
	private float m_maxBarHeight;

	[SerializeField]
	private FFTWindow m_fftWindowMode;

	private void Update()
	{
		if (m_audioSource.isPlaying)
		{
			float[] array = new float[64];
			m_audioSource.GetSpectrumData(array, 0, m_fftWindowMode);
			float[] array2 = new float[m_bars.Length];
			int num = array.Length / m_bars.Length + 1;
			int num2 = 0;
			int num3 = 0;
			for (int i = 0; i < array.Length; i++)
			{
				array2[num3] += array[i];
				num2++;
				if (num2 >= num)
				{
					num3++;
					num2 = 0;
				}
			}
			float num4 = 0f;
			for (int j = 1; j < array2.Length; j++)
			{
				if (num4 < array2[j])
				{
					num4 = array2[j];
				}
			}
			float num5 = 1f / num4;
			for (int k = 1; k < array2.Length; k++)
			{
				array2[k] *= num5;
				m_bars[k].sizeDelta = new Vector2(m_bars[k].sizeDelta.x, array2[k] * m_maxBarHeight);
			}
		}
		else
		{
			for (int l = 1; l < m_bars.Length; l++)
			{
				m_bars[l].sizeDelta = new Vector2(m_bars[l].sizeDelta.x, Mathf.MoveTowards(m_bars[l].sizeDelta.y, 0f, Time.deltaTime * 10f));
			}
		}
	}
}
