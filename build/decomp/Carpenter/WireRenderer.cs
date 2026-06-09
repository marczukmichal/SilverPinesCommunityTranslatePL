using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[ExecuteAlways]
public class WireRenderer : MonoBehaviour
{
	[SerializeField]
	public Vector3[] m_anchorPoints;

	[SerializeField]
	private int m_pointCount = 5;

	[SerializeField]
	private float m_slack = 1f;

	[SerializeField]
	private float m_windAmplitude = 0.5f;

	[SerializeField]
	private float m_windFrequency = 1f;

	private LineRenderer m_lineRenderer;

	private Vector3[] m_pointsArray;

	private void Start()
	{
		GenerateLine();
		CreateArray();
	}

	private void CreateArray()
	{
		int num = 1;
		for (int i = 0; i < m_anchorPoints.Length - 1; i++)
		{
			for (int j = 1; j < m_pointCount - 1; j++)
			{
				num++;
			}
			num++;
		}
		m_pointsArray = new Vector3[num];
	}

	private void OnValidate()
	{
		CreateArray();
		GenerateLine();
	}

	[ContextMenu("Generate Lines")]
	public void GenerateLine()
	{
		if (m_lineRenderer == null)
		{
			m_lineRenderer = GetComponent<LineRenderer>();
		}
		if (m_pointCount < 2)
		{
			m_pointCount = 2;
		}
		if (m_pointsArray == null)
		{
			CreateArray();
		}
		int num = 0;
		m_pointsArray[num++] = m_anchorPoints[0];
		float num2 = Mathf.Sin(Time.time * m_windFrequency) * m_windAmplitude;
		for (int i = 0; i < m_anchorPoints.Length - 1; i++)
		{
			for (int j = 1; j < m_pointCount - 1; j++)
			{
				float num3 = (float)j / (float)(m_pointCount - 1);
				Vector3 vector = Vector3.Lerp(m_anchorPoints[i], m_anchorPoints[i + 1], num3);
				float num4 = (math.cosh(num3 * 2f - 1f) - 1.54f) * 2f;
				vector.y += num4 * m_slack;
				vector.z += num4 * num2;
				m_pointsArray[num++] = vector;
			}
			m_pointsArray[num++] = m_anchorPoints[i + 1];
		}
		m_lineRenderer.positionCount = m_pointsArray.Length;
		m_lineRenderer.SetPositions(m_pointsArray);
	}

	private void Update()
	{
		GenerateLine();
	}
}
