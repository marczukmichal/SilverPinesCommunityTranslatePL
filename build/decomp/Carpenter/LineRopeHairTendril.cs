using System;
using UnityEngine;
using UnityEngine.Events;

public class LineRopeHairTendril : MonoBehaviour, IDamageable
{
	public enum HairMode
	{
		Idle,
		Seeking,
		Attached,
		Retreat
	}

	[SerializeField]
	private float m_forceMultiplier;

	[SerializeField]
	private float m_maxDistance;

	[SerializeField]
	private LineRope m_lineRope;

	private HairMode m_hairMode;

	[Header("Gameplay")]
	[SerializeField]
	private bool m_isActive;

	[SerializeField]
	private float m_startSeekDistance = 15f;

	[SerializeField]
	private float m_attachDistance = 0.5f;

	[SerializeField]
	private float m_attachOffsetHeight;

	[SerializeField]
	private Vector2 m_randomOffsetRange;

	[SerializeField]
	private float m_attachDepth = 0.25f;

	[SerializeField]
	private float m_lifeTime;

	[Header("Damageable")]
	[SerializeField]
	private EdgeCollider2D m_edgeCollider;

	[SerializeField]
	private float m_retreatRate = 1f;

	[Header("Status Effect")]
	[SerializeField]
	private StatusEffectDefinition m_statusEffectDefinition;

	[SerializeField]
	private int m_statusEffectAmount = 1;

	[SerializeField]
	private float m_statusEffectApplyRate = 0.1f;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_onDeadAudioEvent;

	[SerializeField]
	private AudioEvent m_onAttachAudioEvent;

	private StatusEffectReceiver m_targetStatusEffectReceiver;

	private Vector3 m_attachOffset;

	private Transform m_cachedTargetTransform;

	private float m_statusEffectApplyTimer;

	private float m_aliveTimer;

	private HairMode CurrentHairMode
	{
		get
		{
			return m_hairMode;
		}
		set
		{
			if (m_hairMode == value)
			{
				return;
			}
			if (m_hairMode == HairMode.Attached)
			{
				GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
				m_targetStatusEffectReceiver = item.GetComponent<StatusEffectReceiver>();
				StatusEffectReceiver targetStatusEffectReceiver = m_targetStatusEffectReceiver;
				targetStatusEffectReceiver.m_onStatusEffectActiveChanged = (UnityAction<StatusEffectInstance, bool>)Delegate.Remove(targetStatusEffectReceiver.m_onStatusEffectActiveChanged, new UnityAction<StatusEffectInstance, bool>(OnStatusEffectActiveChanged));
			}
			m_hairMode = value;
			if (m_hairMode == HairMode.Attached)
			{
				GameObject item2 = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
				m_targetStatusEffectReceiver = item2.GetComponent<StatusEffectReceiver>();
				StatusEffectReceiver targetStatusEffectReceiver2 = m_targetStatusEffectReceiver;
				targetStatusEffectReceiver2.m_onStatusEffectActiveChanged = (UnityAction<StatusEffectInstance, bool>)Delegate.Combine(targetStatusEffectReceiver2.m_onStatusEffectActiveChanged, new UnityAction<StatusEffectInstance, bool>(OnStatusEffectActiveChanged));
				if (m_onAttachAudioEvent != null)
				{
					m_onAttachAudioEvent.Play(item2.transform.position);
				}
			}
			else if (m_hairMode == HairMode.Retreat)
			{
				m_lineRope.TargetRopeLengthScale = 0f;
			}
		}
	}

	private bool GetTargetPosition(out Vector3 position)
	{
		if (m_cachedTargetTransform == null)
		{
			GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
			if (item != null)
			{
				SpriteAnimNodeFollower[] componentsInChildren = item.GetComponentsInChildren<SpriteAnimNodeFollower>();
				foreach (SpriteAnimNodeFollower spriteAnimNodeFollower in componentsInChildren)
				{
					if (spriteAnimNodeFollower.NodeType == SpriteAnimNodeType.Flashlight)
					{
						m_cachedTargetTransform = spriteAnimNodeFollower.transform;
						break;
					}
				}
			}
		}
		if (m_cachedTargetTransform != null)
		{
			position = m_cachedTargetTransform.position + m_attachOffset;
			return true;
		}
		position = Vector3.zero;
		return false;
	}

	private void Start()
	{
		m_aliveTimer = 0f;
		m_attachOffset = new Vector3(UnityEngine.Random.Range(0f - m_randomOffsetRange.x, m_randomOffsetRange.x), UnityEngine.Random.Range(0f - m_randomOffsetRange.y, m_randomOffsetRange.y), m_attachDepth);
		m_attachOffset.y += m_attachOffsetHeight;
	}

	private void Update()
	{
		HairMode hairMode = CurrentHairMode;
		if (!m_isActive)
		{
			hairMode = HairMode.Idle;
		}
		else if (CurrentHairMode == HairMode.Retreat)
		{
			if (m_lineRope.CurrentRopeLengthScalar <= 0.01f)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		else
		{
			if (GetTargetPosition(out var position))
			{
				Vector3 startAnchorWorldPosition = m_lineRope.StartAnchorWorldPosition;
				float num = Vector2.Distance(position, startAnchorWorldPosition);
				float num2 = Vector2.Distance(position, m_lineRope.EndOfRopeWorldPosition);
				if (CurrentHairMode == HairMode.Attached)
				{
					if (num > m_maxDistance)
					{
						hairMode = HairMode.Idle;
					}
				}
				else if (CurrentHairMode == HairMode.Idle)
				{
					if (num < m_startSeekDistance)
					{
						hairMode = HairMode.Seeking;
					}
				}
				else if (num > m_maxDistance)
				{
					hairMode = HairMode.Idle;
				}
				else if (num2 < m_attachDistance)
				{
					hairMode = HairMode.Attached;
				}
			}
			if (hairMode != HairMode.Attached)
			{
				m_aliveTimer += Time.deltaTime;
				if (m_aliveTimer > m_lifeTime && m_lifeTime > 0f)
				{
					hairMode = HairMode.Retreat;
				}
			}
		}
		if (m_hairMode == HairMode.Attached)
		{
			m_statusEffectApplyTimer += Time.deltaTime;
			if (m_statusEffectApplyTimer > m_statusEffectApplyRate)
			{
				m_statusEffectApplyTimer -= m_statusEffectApplyRate;
				m_targetStatusEffectReceiver.ApplyStatusEffect(m_statusEffectDefinition, m_statusEffectAmount);
			}
		}
		CurrentHairMode = hairMode;
	}

	private void MatchColliderToRope()
	{
		m_edgeCollider.SetPoints(m_lineRope.GetRopeSegmentPositionsLocal());
	}

	private void FixedUpdate()
	{
		Vector3 position;
		bool targetPosition = GetTargetPosition(out position);
		bool flag = false;
		if (!targetPosition)
		{
			CurrentHairMode = HairMode.Idle;
		}
		m_lineRope.UseEndPoint = CurrentHairMode == HairMode.Attached;
		switch (CurrentHairMode)
		{
		case HairMode.Idle:
			if (m_lineRope.TargetRopeLengthScale > 1f)
			{
				m_lineRope.TargetRopeLengthScale -= Time.deltaTime * m_retreatRate;
			}
			break;
		case HairMode.Seeking:
		{
			Vector3 startAnchorWorldPosition = m_lineRope.StartAnchorWorldPosition;
			Vector3 vector = position - startAnchorWorldPosition;
			float num = Vector2.Distance(position, startAnchorWorldPosition);
			if (num < m_maxDistance)
			{
				Vector2 vector2 = vector * m_forceMultiplier;
				m_lineRope.AddVelocityToRopeEnd(vector2);
			}
			if (num > m_lineRope.RopeLength + 0.5f)
			{
				m_lineRope.TargetRopeLengthScale += Time.deltaTime * 0.2f;
			}
			else
			{
				m_lineRope.TargetRopeLengthScale -= Time.deltaTime * 2f;
			}
			m_lineRope.TargetRopeLengthScale = Mathf.Max(m_lineRope.TargetRopeLengthScale, 0.25f);
			flag = true;
			break;
		}
		case HairMode.Attached:
			m_lineRope.EndAnchorWorldPosition = position;
			flag = true;
			break;
		}
		if (flag)
		{
			MatchColliderToRope();
			m_edgeCollider.enabled = true;
		}
		else
		{
			m_edgeCollider.enabled = false;
		}
	}

	private void OnStatusEffectActiveChanged(StatusEffectInstance statusEffect, bool enable)
	{
		if (statusEffect.Definition == m_statusEffectDefinition && m_hairMode == HairMode.Attached)
		{
			CurrentHairMode = HairMode.Retreat;
		}
	}

	public bool AllowPassThroughProjectile()
	{
		return true;
	}

	public bool ShouldConsumeMeleeHit()
	{
		return true;
	}

	public void ApplyDamageInstance(DamageInstance instance)
	{
		if (instance.HealthDamageAmount > 0)
		{
			CurrentHairMode = HairMode.Retreat;
			if (m_onDeadAudioEvent != null)
			{
				Vector3 position = instance.Position;
				position.z = base.transform.position.z;
				m_onDeadAudioEvent.Play(position);
			}
		}
	}

	public ConsumeHitType GetConsumeHitType()
	{
		return ConsumeHitType.Always;
	}
}
