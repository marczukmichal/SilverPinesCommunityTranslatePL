using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Serialization;

public class LineRope : MonoBehaviour
{
	public class RopeSegment
	{
		public Vector3 m_posNow;

		public Vector3 m_posOld;

		public Vector3 m_velocityToAdd;

		public Vector3 m_targetVisualOffset;

		public Vector3 m_visualOffset;

		public Transform m_colliderTransform;

		public RopeSegment(Vector3 pos)
		{
			m_posNow = pos;
			m_posOld = pos;
			m_velocityToAdd = Vector3.zero;
			m_colliderTransform = null;
		}

		public void AddVelocityScaledByDistance(Vector3 velocityAmount, Vector3 fromPosition)
		{
			float num = Vector3.Distance(fromPosition, m_posNow);
			if (num < 1f)
			{
				m_velocityToAdd = velocityAmount * (1f - num);
			}
		}

		public void AddVelocity(Vector3 velocityAmount)
		{
			m_velocityToAdd = velocityAmount;
		}

		public void GenerateCollider(Transform parent, int collisionLayer, bool isTrigger)
		{
			GameObject gameObject = new GameObject("LineRope Collider");
			CircleCollider2D circleCollider2D = gameObject.AddComponent<CircleCollider2D>();
			circleCollider2D.isTrigger = isTrigger;
			circleCollider2D.radius = 0.1f;
			gameObject.layer = collisionLayer;
			gameObject.AddComponent<LineRopeCollider>();
			gameObject.transform.SetParent(parent);
			m_colliderTransform = gameObject.transform;
		}

		public void UpdateColliderPosition()
		{
			if (m_colliderTransform != null)
			{
				m_colliderTransform.localPosition = m_posNow;
			}
		}
	}

	[SerializeField]
	private bool m_overrideLineWidth = true;

	[SerializeField]
	private float m_lineWidth = 0.1f;

	[FormerlySerializedAs("m_ropeSegLen")]
	[SerializeField]
	[Range(0.05f, 10f)]
	private float m_ropeSegmentLength = 0.25f;

	[SerializeField]
	[Range(5f, 50f)]
	private int m_segmentCount = 35;

	[SerializeField]
	private float m_passiveMovementMagnitude = 0.0005f;

	[SerializeField]
	private float m_passiveMovementTimeScale = 2.5f;

	[SerializeField]
	private float m_dragScalar = 2f;

	[SerializeField]
	private Vector3 m_startAnchorPosition;

	[SerializeField]
	private bool m_useEndPoint;

	[SerializeField]
	private Vector3 m_endAnchorPosition;

	[Header("Collision")]
	[SerializeField]
	private bool m_generateCollision;

	[SerializeField]
	private int m_collidersEveryNSegments = 1;

	[SerializeField]
	private float m_collisionMovementScalar = 1f;

	[SerializeField]
	private int m_generatedCollisionLayer;

	[SerializeField]
	private bool m_colliderIsTrigger = true;

	[SerializeField]
	private AudioEvent m_collisionAudio;

	[Header("Misc")]
	[SerializeField]
	private Transform m_endTransform;

	[SerializeField]
	private float m_targetRopeLengthScalar = 1f;

	[SerializeField]
	private float m_ropeLengthScaleChangeRate = 1f;

	[Header("Ambient motion")]
	[SerializeField]
	private float m_ambientWaveFrequency;

	[SerializeField]
	private float m_ambientWaveAmplitude;

	[SerializeField]
	private float m_ambientWaveSpeed;

	[Header("Update Rate")]
	[Tooltip("This is how often this rope should actually update. Time between updates in seconds.")]
	[SerializeField]
	private float m_ropeSimulateFrameTime = 0.0333f;

	private float m_updateTimer;

	private float m_ropeLengthScalar = 1f;

	private LineRenderer m_lineRenderer;

	private List<RopeSegment> m_ropeSegments = new List<RopeSegment>();

	private static int s_constraintApplicationSteps = 50;

	private float m_timeOffset;

	private JobHandle m_jobHandle;

	private bool m_waitingForJob;

	private NativeArray<LineRopeSimulateJob.RopeSegmentJobData> m_jobRopeSegments;

	private float RopeSegmentLength => m_ropeSegmentLength * m_ropeLengthScalar;

	public Vector3 StartAnchorLocalPosition
	{
		get
		{
			return m_startAnchorPosition;
		}
		set
		{
			m_startAnchorPosition = value;
		}
	}

	public Vector3 StartAnchorWorldPosition
	{
		get
		{
			return base.transform.TransformPoint(m_startAnchorPosition);
		}
		set
		{
			m_startAnchorPosition = base.transform.InverseTransformPoint(value);
		}
	}

	public bool UseEndPoint
	{
		get
		{
			return m_useEndPoint;
		}
		set
		{
			m_useEndPoint = value;
		}
	}

	public Vector3 EndAnchorLocalPosition
	{
		get
		{
			return m_endAnchorPosition;
		}
		set
		{
			m_endAnchorPosition = value;
		}
	}

	public Vector3 EndAnchorWorldPosition
	{
		get
		{
			return base.transform.TransformPoint(m_endAnchorPosition);
		}
		set
		{
			m_endAnchorPosition = base.transform.InverseTransformPoint(value);
		}
	}

	public float TargetRopeLengthScale
	{
		get
		{
			return m_targetRopeLengthScalar;
		}
		set
		{
			m_targetRopeLengthScalar = value;
		}
	}

	public float CurrentRopeLengthScalar => m_ropeLengthScalar;

	public Vector3 EndOfRopeWorldPosition
	{
		get
		{
			RopeSegment ropeSegment = m_ropeSegments[m_ropeSegments.Count - 1];
			return base.transform.TransformPoint(ropeSegment.m_posNow);
		}
	}

	public float RopeLength => (float)m_segmentCount * m_ropeSegmentLength * m_ropeLengthScalar;

	public List<Vector2> GetRopeSegmentPositionsLocal()
	{
		List<Vector2> list = new List<Vector2>(m_ropeSegments.Count);
		foreach (RopeSegment ropeSegment in m_ropeSegments)
		{
			list.Add(ropeSegment.m_posNow);
		}
		return list;
	}

	public void AddCollisionVelocityFromPosition(Vector3 velocity, Vector3 position)
	{
		velocity *= m_collisionMovementScalar;
		Vector3 fromPosition = base.transform.InverseTransformPoint(position);
		for (int i = 1; i < m_segmentCount; i++)
		{
			m_ropeSegments[i].AddVelocityScaledByDistance(velocity, fromPosition);
		}
	}

	public void AddVelocityToRopeEnd(Vector3 velocity)
	{
		m_ropeSegments[m_ropeSegments.Count - 1].AddVelocity(velocity);
	}

	private void Start()
	{
		InitaliseRope();
		if (m_generateCollision)
		{
			GenerateColliders();
		}
	}

	private void OnDisable()
	{
		if (m_waitingForJob)
		{
			m_jobHandle.Complete();
			m_jobRopeSegments.Dispose();
		}
	}

	private void GenerateColliders()
	{
		for (int num = m_segmentCount - 1; num >= 1; num -= m_collidersEveryNSegments)
		{
			m_ropeSegments[num].GenerateCollider(base.transform, m_generatedCollisionLayer, m_colliderIsTrigger);
		}
	}

	public void InitaliseRope()
	{
		m_timeOffset = Random.Range(0f, 100f);
		m_lineRenderer = GetComponent<LineRenderer>();
		Vector3 startAnchorPosition = m_startAnchorPosition;
		m_ropeSegments.Clear();
		for (int i = 0; i < m_segmentCount; i++)
		{
			m_ropeSegments.Add(new RopeSegment(startAnchorPosition));
			startAnchorPosition.y -= RopeSegmentLength;
		}
		m_ropeLengthScalar = m_targetRopeLengthScalar;
		ScheduleSimulateJob(0.05f, 10, 100);
		if (!Application.isPlaying)
		{
			DrawRope();
		}
	}

	private void OnValidate()
	{
		if (!Application.isPlaying)
		{
			InitaliseRope();
		}
	}

	private void ScheduleSimulateJob(float deltaTime, int constraintApplicationSteps, int simulateSteps = 1)
	{
		m_jobRopeSegments = new NativeArray<LineRopeSimulateJob.RopeSegmentJobData>(m_ropeSegments.Count, Allocator.TempJob);
		for (int i = 0; i < m_ropeSegments.Count; i++)
		{
			m_jobRopeSegments[i] = new LineRopeSimulateJob.RopeSegmentJobData
			{
				m_posNow = m_ropeSegments[i].m_posNow,
				m_posOld = m_ropeSegments[i].m_posOld,
				m_velocityToAdd = m_ropeSegments[i].m_velocityToAdd
			};
		}
		LineRopeSimulateJob lineRopeSimulateJob = default(LineRopeSimulateJob);
		lineRopeSimulateJob.m_segmentCount = m_segmentCount;
		lineRopeSimulateJob.m_useEndPoint = m_useEndPoint;
		lineRopeSimulateJob.m_dragScalar = m_dragScalar;
		lineRopeSimulateJob.m_timeValue = Time.time;
		lineRopeSimulateJob.m_updateRate = m_ropeSimulateFrameTime;
		lineRopeSimulateJob.m_timeOffset = m_timeOffset;
		lineRopeSimulateJob.m_ambientWaveSpeed = m_ambientWaveSpeed;
		lineRopeSimulateJob.m_ambientWaveFrequency = m_ambientWaveFrequency;
		lineRopeSimulateJob.m_ambientWaveAmplitude = m_ambientWaveAmplitude;
		lineRopeSimulateJob.m_passiveMovementTimeScale = m_passiveMovementTimeScale;
		lineRopeSimulateJob.m_passiveMovementMagnitude = m_passiveMovementMagnitude;
		lineRopeSimulateJob.m_ropeSegmentLength = RopeSegmentLength;
		lineRopeSimulateJob.m_deltaTime = deltaTime;
		lineRopeSimulateJob.m_startAnchorPosition = m_startAnchorPosition;
		lineRopeSimulateJob.m_endAnchorPosition = m_endAnchorPosition;
		lineRopeSimulateJob.m_stepsToSimulate = simulateSteps;
		lineRopeSimulateJob.m_timesToApplyConstraints = constraintApplicationSteps;
		lineRopeSimulateJob.m_ropeSegments = m_jobRopeSegments;
		LineRopeSimulateJob jobData = lineRopeSimulateJob;
		if (Application.isPlaying)
		{
			m_jobHandle = jobData.Schedule();
			m_waitingForJob = true;
		}
		else
		{
			jobData.Run();
			UpdateRopeDataFromJobAndDispose();
		}
	}

	private void Update()
	{
		if (m_targetRopeLengthScalar != m_ropeLengthScalar)
		{
			m_ropeLengthScalar = Mathf.MoveTowards(m_ropeLengthScalar, m_targetRopeLengthScalar, Time.deltaTime * m_ropeLengthScaleChangeRate);
		}
		m_updateTimer += Time.deltaTime;
		if (!(m_updateTimer < m_ropeSimulateFrameTime))
		{
			m_updateTimer -= m_ropeSimulateFrameTime;
			ScheduleSimulateJob(m_ropeSimulateFrameTime, s_constraintApplicationSteps);
		}
	}

	private void UpdateRopeDataFromJobAndDispose()
	{
		for (int i = 0; i < m_ropeSegments.Count; i++)
		{
			m_ropeSegments[i].m_posNow = m_jobRopeSegments[i].m_posNow;
			m_ropeSegments[i].m_posOld = m_jobRopeSegments[i].m_posOld;
			m_ropeSegments[i].m_velocityToAdd = m_jobRopeSegments[i].m_velocityToAdd;
		}
		m_jobRopeSegments.Dispose();
	}

	private void LateUpdate()
	{
		if (!m_waitingForJob)
		{
			return;
		}
		m_waitingForJob = false;
		m_jobHandle.Complete();
		UpdateRopeDataFromJobAndDispose();
		if (m_endTransform != null)
		{
			RopeSegment ropeSegment = m_ropeSegments[m_ropeSegments.Count - 1];
			m_endTransform.localPosition = ropeSegment.m_posNow;
			Quaternion localRotation = Quaternion.identity;
			if (m_ropeSegments.Count > 2)
			{
				RopeSegment ropeSegment2 = m_ropeSegments[m_ropeSegments.Count - 2];
				localRotation = Quaternion.FromToRotation(Vector3.down, (ropeSegment.m_posNow - ropeSegment2.m_posNow).normalized);
			}
			m_endTransform.SetLocalPositionAndRotation(ropeSegment.m_posNow, localRotation);
		}
		for (int i = 0; i < m_segmentCount; i++)
		{
			m_ropeSegments[i].UpdateColliderPosition();
		}
		UpdateRopeVisualOffset();
		DrawRope();
	}

	private void UpdateRopeVisualOffset()
	{
		for (int i = 0; i < m_segmentCount; i++)
		{
			m_ropeSegments[i].m_visualOffset = Vector3.MoveTowards(m_ropeSegments[i].m_visualOffset, m_ropeSegments[i].m_targetVisualOffset, Time.deltaTime);
		}
	}

	private void DrawRope()
	{
		if (m_overrideLineWidth)
		{
			float lineWidth = m_lineWidth;
			m_lineRenderer.startWidth = lineWidth;
			m_lineRenderer.endWidth = lineWidth;
		}
		Vector3[] array = new Vector3[m_segmentCount];
		for (int i = 0; i < m_segmentCount; i++)
		{
			array[i] = m_ropeSegments[i].m_posNow + m_ropeSegments[i].m_visualOffset;
		}
		m_lineRenderer.positionCount = array.Length;
		m_lineRenderer.SetPositions(array);
	}

	public void ApplyRopeSnapToLegsPosition(Vector3 legPosition)
	{
		Vector3 localPosition = base.transform.InverseTransformPoint(legPosition);
		int num = GetRopeSegmentIndexClosestToPosition(localPosition);
		if (localPosition.y - m_ropeSegments[num].m_posNow.y < -0.5f)
		{
			num = -1;
		}
		bool flag = false;
		float x = 0f;
		for (int i = 0; i < m_ropeSegments.Count; i++)
		{
			if (i == num)
			{
				flag = true;
				x = localPosition.x - m_ropeSegments[i].m_posNow.x;
				m_ropeSegments[i].m_targetVisualOffset = new Vector3(x, 0f, 0f);
			}
			else if (flag)
			{
				m_ropeSegments[i].m_targetVisualOffset = new Vector3(x, 0f, 0f);
			}
			else
			{
				m_ropeSegments[i].m_targetVisualOffset = Vector3.zero;
			}
		}
	}

	public void ClearRopeSnapToLegsPosition()
	{
		for (int i = 0; i < m_ropeSegments.Count; i++)
		{
			m_ropeSegments[i].m_targetVisualOffset = Vector3.zero;
		}
	}

	public Vector3 GetClosestWorldPositionOnRope(Vector3 worldPosition)
	{
		Vector3 localPosition = base.transform.InverseTransformPoint(worldPosition);
		return base.transform.TransformPoint(m_ropeSegments[GetRopeSegmentIndexClosestToPosition(localPosition)].m_posNow);
	}

	public int GetRopeSegmentIndexClosestToPosition(Vector3 localPosition)
	{
		if (m_ropeSegments == null || m_ropeSegments.Count == 0)
		{
			Debug.LogError("No points provided to defined for rope line.");
			return -1;
		}
		if (m_ropeSegments.Count == 1)
		{
			return 0;
		}
		int result = 0;
		float num = float.PositiveInfinity;
		for (int i = 0; i < m_ropeSegments.Count - 1; i++)
		{
			Vector3 posNow = m_ropeSegments[i].m_posNow;
			Vector3 posNow2 = m_ropeSegments[i + 1].m_posNow;
			Vector3 closestPointOnSegment = GetClosestPointOnSegment(posNow, posNow2, localPosition);
			float num2 = Vector3.Distance(localPosition, closestPointOnSegment);
			if (num2 < num)
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	public void OnRopeCollisionEnter()
	{
		if (m_collisionAudio != null)
		{
			m_collisionAudio.Play(EndOfRopeWorldPosition);
		}
	}

	private Vector3 GetClosestPointOnSegment(Vector3 p1, Vector3 p2, Vector3 position)
	{
		Vector3 vector = p2 - p1;
		float magnitude = vector.magnitude;
		if (magnitude == 0f)
		{
			return p1;
		}
		float num = Mathf.Clamp01(Vector3.Dot(position - p1, vector) / magnitude);
		return p1 + num * vector;
	}
}
