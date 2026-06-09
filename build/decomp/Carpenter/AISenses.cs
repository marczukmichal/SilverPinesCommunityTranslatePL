using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AISenses : MonoBehaviour
{
	public enum TargetState
	{
		None,
		Suspicious,
		Detected
	}

	public class DetectableData
	{
		public Detectable m_detectable;

		public float m_detectionRate;
	}

	public class DetectableResults
	{
		public List<DetectableData> m_detectables = new List<DetectableData>();

		public void AddDetectable(Detectable detectable, float detectionRate)
		{
			foreach (DetectableData detectable2 in m_detectables)
			{
				if (detectable2.m_detectable == detectable)
				{
					detectable2.m_detectionRate += detectionRate;
					return;
				}
			}
			m_detectables.Add(new DetectableData
			{
				m_detectable = detectable,
				m_detectionRate = detectionRate
			});
		}
	}

	[Tooltip("How long until we forget a target that we can no longer sense")]
	[SerializeField]
	private float m_targetMemoryDuration = 10f;

	[SerializeField]
	private float m_detectTime = 0.2f;

	[SerializeField]
	private float m_targetLostDetectionReductionRate = 0.1f;

	[Header("Alert Nearby")]
	[SerializeField]
	private float m_nearbyAlertDistance;

	[SerializeField]
	private float m_nearbyAlertTime = 5f;

	[ShowInDesignerInspector]
	[SerializeField]
	private bool m_alwaysDetectPlayer;

	[Header("Misc")]
	[SerializeField]
	private bool m_onlyDangerSense;

	[DebugCommand("ai_oblivious", "AI is oblivious and can't detect anything", "ai_oblivious <true/false>", typeof(bool), false)]
	private static bool s_aiOblivious;

	private IAISense[] m_senses;

	private Detectable m_possibleTarget;

	private float m_targetLostTimer;

	private bool m_canSenseTarget;

	private float m_activeTargetDetectionTimer;

	private float m_currentDetectionRate;

	private Detectable m_forcedDetectable;

	private float m_forceDetectableTimer;

	private CharacterDirection m_characterDirection;

	public float TargetMemoryDuration => m_targetMemoryDuration;

	private IAISense[] Senses
	{
		get
		{
			if (m_senses == null)
			{
				m_senses = GetComponentsInChildren<IAISense>();
			}
			return m_senses;
		}
	}

	public Detectable CurrentTarget
	{
		get
		{
			if (CurrentTargetState != TargetState.Detected)
			{
				return null;
			}
			return m_possibleTarget;
		}
	}

	public float TargetLostTimer => m_targetLostTimer;

	public bool CanSenseTarget => m_canSenseTarget;

	public float DetectionPercent => Mathf.Clamp01(m_activeTargetDetectionTimer / m_detectTime);

	public TargetState CurrentTargetState
	{
		get
		{
			if (m_possibleTarget == null)
			{
				return TargetState.None;
			}
			if (m_activeTargetDetectionTimer >= m_detectTime)
			{
				return TargetState.Detected;
			}
			if (m_activeTargetDetectionTimer >= m_detectTime * 0.5f)
			{
				return TargetState.Suspicious;
			}
			return TargetState.None;
		}
	}

	public void SetOnlyDangerSense(bool onlyDangerSense)
	{
		m_onlyDangerSense = onlyDangerSense;
	}

	private void Awake()
	{
		m_senses = GetComponentsInChildren<IAISense>();
		m_characterDirection = GetComponent<CharacterDirection>();
	}

	private void Start()
	{
		CharacterHealth component = GetComponent<CharacterHealth>();
		if (component != null)
		{
			if (component.IsDead)
			{
				OnDead();
			}
			component.OnDead.AddListener(OnDead);
			component.OnRevive = (UnityAction)Delegate.Combine(component.OnRevive, new UnityAction(OnRevive));
		}
	}

	private void OnDead()
	{
		base.enabled = false;
	}

	private void OnRevive()
	{
		base.enabled = true;
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.Sets.Generic.ActiveAISensesSet.Add(this);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Sets.Generic.ActiveAISensesSet.Remove(this);
		m_possibleTarget = null;
		m_activeTargetDetectionTimer = 0f;
	}

	public void Update()
	{
		m_canSenseTarget = false;
		DetectableResults detectableResults = new DetectableResults();
		if (m_alwaysDetectPlayer)
		{
			GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
			if (item != null)
			{
				Detectable component = item.GetComponent<Detectable>();
				if (component != null)
				{
					m_forcedDetectable = component;
				}
			}
		}
		else if (m_forceDetectableTimer > 0f)
		{
			m_forceDetectableTimer -= Time.deltaTime;
			if (m_forceDetectableTimer <= 0f)
			{
				m_forcedDetectable = null;
			}
		}
		if (!s_aiOblivious)
		{
			if (m_forcedDetectable != null)
			{
				detectableResults.AddDetectable(m_forcedDetectable, 1000f);
			}
			IAISense[] senses = Senses;
			for (int i = 0; i < senses.Length; i++)
			{
				senses[i].PopulateTargets(detectableResults, m_onlyDangerSense);
			}
		}
		m_currentDetectionRate = 0f;
		foreach (DetectableData detectable in detectableResults.m_detectables)
		{
			if (!(detectable.m_detectable == null))
			{
				CharacterHealth component2 = detectable.m_detectable.GetComponent<CharacterHealth>();
				if (!component2 || !component2.IsDead)
				{
					m_canSenseTarget = true;
					m_possibleTarget = detectable.m_detectable;
					m_currentDetectionRate = detectable.m_detectionRate;
					m_targetLostTimer = 0f;
					break;
				}
			}
		}
		if (!m_possibleTarget)
		{
			return;
		}
		bool flag = false;
		CharacterHealth component3 = m_possibleTarget.GetComponent<CharacterHealth>();
		if ((bool)component3 && component3.IsDead)
		{
			flag = true;
		}
		AICollisionTag.AICollisionTagFlag collisionTags = GetCollisionTags(base.transform.position, m_possibleTarget);
		if (collisionTags.HasFlag(AICollisionTag.AICollisionTagFlag.DetectionBarrier))
		{
			flag = true;
		}
		if (m_canSenseTarget && !flag)
		{
			float activeTargetDetectionTimer = m_activeTargetDetectionTimer;
			m_activeTargetDetectionTimer += Time.deltaTime * m_currentDetectionRate;
			if (m_activeTargetDetectionTimer > m_detectTime && activeTargetDetectionTimer <= m_detectTime && m_nearbyAlertDistance > 0f)
			{
				AlertNeraby(m_nearbyAlertDistance, m_nearbyAlertTime);
			}
		}
		else if (CurrentTargetState == TargetState.Detected)
		{
			m_targetLostTimer += Time.deltaTime;
			if (m_targetLostTimer >= m_targetMemoryDuration || flag)
			{
				m_activeTargetDetectionTimer = 0f;
				m_possibleTarget = null;
			}
		}
		else
		{
			m_activeTargetDetectionTimer -= Time.deltaTime * m_targetLostDetectionReductionRate;
			if (m_activeTargetDetectionTimer <= 0f || flag)
			{
				m_activeTargetDetectionTimer = 0f;
				m_possibleTarget = null;
			}
		}
	}

	private AICollisionTag.AICollisionTagFlag GetCollisionTags(Vector3 myPosition, Detectable detectable)
	{
		AICollisionTag.AICollisionTagFlag aICollisionTagFlag = AICollisionTag.AICollisionTagFlag.None;
		foreach (Transform detectableTransform in detectable.DetectableTransforms)
		{
			RaycastHit2D raycastHit2D = Physics2D.Linecast(myPosition, detectableTransform.position, GameLayers.AIToolsMask);
			if (raycastHit2D.collider != null)
			{
				AICollisionTag component = raycastHit2D.collider.GetComponent<AICollisionTag>();
				aICollisionTagFlag |= component.Flags;
			}
		}
		return aICollisionTagFlag;
	}

	public void ForgetTarget()
	{
		m_possibleTarget = null;
		m_activeTargetDetectionTimer = 0f;
	}

	public float DrawDebugInfo(Rect boxRect, float yPos)
	{
		Rect position = new Rect(boxRect);
		position.height = 20f;
		position.y = yPos;
		GUI.Label(position, "Sense State: " + CurrentTargetState);
		position.y += position.height;
		GUI.Label(position, "Detect : " + m_activeTargetDetectionTimer + " / " + m_detectTime);
		position.y += position.height;
		GUI.Label(position, "Detection Rate: " + m_currentDetectionRate);
		position.y += position.height;
		return position.y;
	}

	public void ForceTarget(Detectable target, float timer)
	{
		m_forcedDetectable = target;
		m_forceDetectableTimer = timer;
	}

	public void ForceTarget(GameObjectAnchor target, float timer)
	{
		if (target.Item != null)
		{
			Detectable component = target.Item.GetComponent<Detectable>();
			if ((bool)component)
			{
				m_forcedDetectable = component;
				m_forceDetectableTimer = timer;
			}
		}
	}

	public void ForcePlayerAsTarget()
	{
		ForceTarget(GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor, 10f);
	}

	public void AlertNeraby(float alertDistance, float forceTargetTime)
	{
		foreach (AISenses item in GlobalReferences.Instance.Sets.Generic.ActiveAISensesSet)
		{
			if (!(item == this) && !item.m_onlyDangerSense && Vector2.Distance(item.transform.position, base.transform.position) < alertDistance)
			{
				item.ForceTarget(CurrentTarget, forceTargetTime);
			}
		}
	}

	public bool IsLookingAtTargetSuspicious()
	{
		if (m_possibleTarget != null && CurrentTargetState != 0 && m_characterDirection != null && m_characterDirection.IsFacingPoint(m_possibleTarget.transform.position))
		{
			return true;
		}
		return false;
	}
}
