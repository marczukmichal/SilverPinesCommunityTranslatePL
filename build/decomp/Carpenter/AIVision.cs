using UnityEngine;

public class AIVision : MonoBehaviour, IAISense
{
	[SerializeField]
	private Transform m_eyesTransform;

	[SerializeField]
	private GameObjectAnchor m_playerAnchor;

	[SerializeField]
	private float m_visionUpdateRate = 0.2f;

	[SerializeField]
	private float m_visionDistance = 6f;

	[SerializeField]
	private float m_visionDetectionScalar = 1f;

	[Tooltip("Dot product from the eyes to the target > required to count as having vision")]
	[SerializeField]
	private float m_visionDirectionRequirement = 0.25f;

	private bool m_canSeeTarget;

	private bool m_canSeeFlashlight;

	private float m_updateTimer;

	private LightBeamSensor m_lightBeamSensor;

	private float m_activeDetectionScalar = 1f;

	private float m_detectionDistanceScalar = 1f;

	private bool m_ignoreLookDirection;

	public bool CanSeeTarget => m_canSeeTarget;

	public float ActiveDetectionScalar
	{
		get
		{
			return m_activeDetectionScalar;
		}
		set
		{
			m_activeDetectionScalar = value;
		}
	}

	public float DetectionDistanceScalar
	{
		get
		{
			return m_detectionDistanceScalar;
		}
		set
		{
			m_detectionDistanceScalar = value;
		}
	}

	private float VisionDistance => m_visionDistance * DetectionDistanceScalar;

	public bool IgnoreLookDirection
	{
		get
		{
			return m_ignoreLookDirection;
		}
		set
		{
			m_ignoreLookDirection = value;
		}
	}

	private void Awake()
	{
		m_lightBeamSensor = base.gameObject.AddComponent<LightBeamSensor>();
		m_updateTimer = Random.Range(0f, m_visionUpdateRate);
	}

	private void Update()
	{
		if (m_updateTimer > 0f)
		{
			m_updateTimer -= Time.deltaTime;
			if (m_updateTimer <= 0f)
			{
				PerformVisionCheck();
			}
		}
	}

	private bool CheckVisionForPosition(Vector3 targetPosition, GameObject targetGameObject, bool ignoreDistance = false, bool ignoreDirection = false)
	{
		float num = Vector2.Distance(m_eyesTransform.position, targetPosition);
		if (num < VisionDistance || ignoreDistance)
		{
			Vector3 vector = targetPosition - m_eyesTransform.position;
			vector.Normalize();
			if (Vector3.Dot(vector, m_eyesTransform.up) > m_visionDirectionRequirement || ignoreDirection)
			{
				RaycastHit2D[] array = Physics2D.RaycastAll(m_eyesTransform.position, vector, num, GameLayers.EnvironmentMask);
				bool flag = true;
				RaycastHit2D[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					RaycastHit2D raycastHit2D = array2[i];
					if (!(raycastHit2D.transform.gameObject == targetGameObject) && !(raycastHit2D.transform.gameObject == base.gameObject))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void PerformVisionCheck()
	{
		m_canSeeTarget = false;
		m_canSeeFlashlight = false;
		if (m_eyesTransform != null && m_playerAnchor != null)
		{
			GameObject item = m_playerAnchor.Item;
			if (item != null)
			{
				Detectable component = item.GetComponent<Detectable>();
				if (component != null)
				{
					foreach (Transform detectableTransform in component.DetectableTransforms)
					{
						if (CheckVisionForPosition(detectableTransform.position, item, ignoreDistance: false, IgnoreLookDirection))
						{
							m_canSeeTarget = true;
							break;
						}
					}
					if ((bool)m_lightBeamSensor.DetectedLightBeam && CheckVisionForPosition(m_lightBeamSensor.DetectedLightBeam.RootPosition, item, ignoreDistance: true, ignoreDirection: true))
					{
						m_canSeeFlashlight = true;
					}
				}
			}
		}
		m_updateTimer = m_visionUpdateRate;
	}

	private void OnDrawGizmosSelected()
	{
		if (!(m_eyesTransform != null) || !(m_playerAnchor != null))
		{
			return;
		}
		GameObject item = m_playerAnchor.Item;
		if (!(item != null))
		{
			return;
		}
		Detectable component = item.GetComponent<Detectable>();
		if (component != null)
		{
			foreach (Transform detectableTransform in component.DetectableTransforms)
			{
				Gizmos.color = (CheckVisionForPosition(detectableTransform.position, item) ? Color.red : Color.green);
				GizmoExtensions.DrawToFromArrow(m_eyesTransform.position, detectableTransform.position, 0.25f, 0.25f);
			}
		}
		Gizmos.color = Color.white;
	}

	public void PopulateTargets(AISenses.DetectableResults detectableResults, bool onlyDangerSense)
	{
		if (onlyDangerSense || (!m_canSeeTarget && !m_canSeeFlashlight) || !(m_playerAnchor.Item != null))
		{
			return;
		}
		Detectable component = m_playerAnchor.Item.GetComponent<Detectable>();
		if (!(component != null))
		{
			return;
		}
		float num = 0f;
		if (m_canSeeTarget)
		{
			num = ((component.CurrentVisibility == Detectable.Visibility.Reduced) ? 0.5f : ((component.CurrentVisibility == Detectable.Visibility.Hidden) ? 0.1f : ((!m_canSeeFlashlight) ? 1f : 2f)));
			CharacterMovement component2 = component.GetComponent<CharacterMovement>();
			if ((object)component2 != null)
			{
				if (component2.PreviousVelocity.magnitude > 4f)
				{
					num *= 2f;
				}
				else if (component2.PreviousVelocity.magnitude < 0.5f)
				{
					num *= 0.4f;
				}
			}
		}
		else if (m_canSeeFlashlight)
		{
			num = 0.5f;
		}
		float num2 = float.MaxValue;
		foreach (Transform detectableTransform in component.DetectableTransforms)
		{
			num2 = Mathf.Min(num2, Vector2.Distance(m_eyesTransform.position, detectableTransform.position));
		}
		float time = 1f - Mathf.Clamp01(num2 / VisionDistance);
		num *= GlobalReferences.Instance.AISharedSettings.m_aiVisionDetectionCurve.Evaluate(time);
		num *= m_visionDetectionScalar * m_activeDetectionScalar;
		detectableResults.AddDetectable(component, num);
	}
}
