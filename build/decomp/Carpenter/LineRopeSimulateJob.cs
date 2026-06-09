using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct LineRopeSimulateJob : IJob
{
	public struct RopeSegmentJobData
	{
		public Vector3 m_posNow;

		public Vector3 m_posOld;

		public Vector3 m_velocityToAdd;
	}

	public int m_segmentCount;

	public bool m_useEndPoint;

	public float m_dragScalar;

	public float m_timeValue;

	public float m_updateRate;

	public float m_timeOffset;

	public float m_ambientWaveSpeed;

	public float m_ambientWaveFrequency;

	public float m_ambientWaveAmplitude;

	public float m_passiveMovementTimeScale;

	public float m_passiveMovementMagnitude;

	public float m_ropeSegmentLength;

	public float m_deltaTime;

	public Vector3 m_startAnchorPosition;

	public Vector3 m_endAnchorPosition;

	public int m_stepsToSimulate;

	public int m_timesToApplyConstraints;

	public NativeArray<RopeSegmentJobData> m_ropeSegments;

	private void Simulate(float deltaTime, int constraintApplicationSteps)
	{
		Vector3 vector = new Vector3(0f, -1.5f);
		int num = m_segmentCount;
		if (m_useEndPoint)
		{
			num--;
		}
		for (int i = 1; i < num; i++)
		{
			RopeSegmentJobData value = m_ropeSegments[i];
			Vector3 vector2 = value.m_posNow - value.m_posOld;
			Vector3 vector3 = vector2 * m_dragScalar;
			vector2 -= vector3 * deltaTime;
			float x = Mathf.Sin((m_timeOffset + m_timeValue) * m_ambientWaveSpeed + (float)i * m_ambientWaveFrequency) * m_ambientWaveAmplitude;
			Vector3 vector4 = new Vector3(x, 0f, 0f);
			value.m_posOld = value.m_posNow;
			value.m_posNow += vector2 + vector4;
			value.m_posNow += vector * deltaTime;
			value.m_posNow += value.m_velocityToAdd * deltaTime;
			value.m_velocityToAdd = Vector3.zero;
			if (i == m_segmentCount - 1)
			{
				float num2 = Mathf.Sin(m_timeValue * m_passiveMovementTimeScale) * m_passiveMovementMagnitude;
				if (num2 > 0f)
				{
					value.m_posNow.x += num2;
				}
			}
			m_ropeSegments[i] = value;
		}
		for (int j = 0; j < constraintApplicationSteps; j++)
		{
			ApplyConstraint();
		}
		if (m_useEndPoint)
		{
			RopeSegmentJobData ropeSegmentJobData = m_ropeSegments[m_ropeSegments.Length - 1];
			ropeSegmentJobData.m_posNow = m_endAnchorPosition;
		}
	}

	private void ApplyConstraint()
	{
		RopeSegmentJobData value = m_ropeSegments[0];
		value.m_posNow = m_startAnchorPosition;
		m_ropeSegments[0] = value;
		if (m_useEndPoint)
		{
			RopeSegmentJobData value2 = m_ropeSegments[m_ropeSegments.Length - 1];
			value2.m_posNow = m_endAnchorPosition;
			m_ropeSegments[m_ropeSegments.Length - 1] = value2;
		}
		for (int i = 0; i < m_segmentCount - 1; i++)
		{
			RopeSegmentJobData value3 = m_ropeSegments[i];
			RopeSegmentJobData value4 = m_ropeSegments[i + 1];
			float magnitude = (value3.m_posNow - value4.m_posNow).magnitude;
			float num = Mathf.Abs(magnitude - m_ropeSegmentLength);
			Vector3 vector = Vector3.zero;
			if (magnitude > m_ropeSegmentLength)
			{
				vector = (value3.m_posNow - value4.m_posNow).normalized;
			}
			else if (magnitude < m_ropeSegmentLength)
			{
				vector = (value4.m_posNow - value3.m_posNow).normalized;
			}
			Vector3 vector2 = vector * num;
			if (i != 0)
			{
				value3.m_posNow -= vector2 * 0.5f;
				value4.m_posNow += vector2 * 0.5f;
			}
			else
			{
				value4.m_posNow += vector2;
			}
			m_ropeSegments[i] = value3;
			m_ropeSegments[i + 1] = value4;
		}
	}

	public void Execute()
	{
		for (int i = 0; i < m_stepsToSimulate; i++)
		{
			Simulate(m_deltaTime, m_timesToApplyConstraints);
		}
	}
}
