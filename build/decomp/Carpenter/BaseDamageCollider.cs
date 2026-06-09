using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class BaseDamageCollider : MonoBehaviour
{
	public enum DamageMultiHitScaling
	{
		None,
		PlayerMelee
	}

	public enum HitResultType
	{
		None,
		HitDamageable,
		Rebound,
		HitDoor,
		Ignore
	}

	[SerializeField]
	protected bool m_startEnabled;

	[SerializeField]
	protected bool m_isTouchDamage;

	[SerializeField]
	protected int m_maxHitsPerActivation = 1;

	[SerializeField]
	protected DamageMultiHitScaling m_multiHitScaling;

	[SerializeField]
	private float m_allowedDepthDistance = 2f;

	[SerializeField]
	private bool m_allowFriendlyFire;

	protected int m_countedHits;

	protected MeleeWeapon m_meleeWeapon;

	protected CharacterIdentifier.CharacterFaction m_characterFaction;

	protected Collider2D m_collider;

	protected List<IDamageable> m_damaged = new List<IDamageable>();

	protected Dictionary<DamageBlock, Collider2D> m_overlappedCollidersDamageBlock = new Dictionary<DamageBlock, Collider2D>();

	protected bool m_damageEnabled;

	protected bool m_shouldEnable;

	[SerializeField]
	private bool m_canRebound;

	private CharacterDirection m_characterDirection;

	private static bool m_isFacingRight;

	public float AllowedDepthDistance
	{
		get
		{
			return m_allowedDepthDistance;
		}
		set
		{
			m_allowedDepthDistance = value;
		}
	}

	public bool DamageEnabled
	{
		get
		{
			if (m_damageEnabled)
			{
				return base.isActiveAndEnabled;
			}
			return false;
		}
	}

	public virtual bool CanRebound => m_canRebound;

	public void SetMeleeWeapon(MeleeWeapon meleeWeapon)
	{
		m_meleeWeapon = meleeWeapon;
	}

	protected abstract HitResultType PerformHit(Collider2D damageCollider, Collider2D otherCollider, CharacterIdentifier.CharacterFaction otherFaction, CharacterIdentifier characterIdentifier);

	protected abstract void TriggerRebound(GameObject impactGO, Vector3 impactPosition);

	protected abstract bool CheckForRebound();

	public void SetDamageEnabled(bool enabled)
	{
		m_shouldEnable = enabled;
	}

	protected virtual void Start()
	{
		m_characterDirection = GetComponentInParent<CharacterDirection>();
		m_meleeWeapon = GetComponentInParent<MeleeWeapon>();
		m_collider = GetComponent<Collider2D>();
		if (m_startEnabled)
		{
			m_shouldEnable = true;
		}
		if (!m_shouldEnable && !m_damageEnabled && m_collider != null)
		{
			m_collider.enabled = false;
		}
	}

	private void OnNodeMoved(Vector3 movedAmount)
	{
		if (m_damageEnabled && movedAmount.magnitude > 0.01f)
		{
			CheckForContact();
		}
	}

	private static int CompareCollidersByDistance(RaycastHit2D a, RaycastHit2D b)
	{
		if (m_isFacingRight)
		{
			if (a.collider.bounds.extents.x < 0.1f || b.collider.bounds.extents.y < 0.1f)
			{
				return a.collider.bounds.center.x.CompareTo(b.collider.bounds.center.x);
			}
			return a.collider.bounds.min.x.CompareTo(b.collider.bounds.min.x);
		}
		if (b.collider.bounds.extents.x < 0.1f || a.collider.bounds.extents.y < 0.1f)
		{
			return b.collider.bounds.center.x.CompareTo(a.collider.bounds.center.x);
		}
		return b.collider.bounds.max.x.CompareTo(a.collider.bounds.max.x);
	}

	private void CheckForContact()
	{
		if (m_characterDirection != null)
		{
			m_isFacingRight = m_characterDirection.CurrentDirection == CharacterDirection.Facing.Right;
		}
		else
		{
			m_isFacingRight = true;
		}
		Vector2 direction = (m_isFacingRight ? Vector2.right : Vector2.left);
		int num = m_collider.Cast(direction, RaycastUtils.ContactFilterMelee, RaycastUtils.SharedRaycastList, 0.01f);
		RaycastUtils.SharedRaycastList.Sort(CompareCollidersByDistance);
		HitResultType hitResultType = HitResultType.None;
		for (int i = 0; i < num; i++)
		{
			Collider2D collider = RaycastUtils.SharedRaycastList[i].collider;
			HitResultType hitResultType2 = CheckColliderOverlapForDamage(m_collider, collider);
			if (hitResultType2 == HitResultType.None)
			{
				continue;
			}
			if (hitResultType2 == HitResultType.Rebound && hitResultType != HitResultType.HitDamageable)
			{
				hitResultType = hitResultType2;
				continue;
			}
			switch (hitResultType2)
			{
			case HitResultType.HitDamageable:
				hitResultType = HitResultType.HitDamageable;
				continue;
			case HitResultType.HitDoor:
				break;
			case HitResultType.Ignore:
				if (hitResultType == HitResultType.None)
				{
					hitResultType = HitResultType.Ignore;
				}
				continue;
			default:
				continue;
			}
			hitResultType = HitResultType.HitDoor;
			SetDamageEnabled(enabled: false);
			break;
		}
		if (hitResultType == HitResultType.None && CanRebound)
		{
			CheckForRebound();
		}
	}

	private void OnEnable()
	{
		m_collider = GetComponent<Collider2D>();
		SpriteAnimNodeFollower componentInParent = GetComponentInParent<SpriteAnimNodeFollower>();
		if (componentInParent != null)
		{
			componentInParent.m_onNodeMoved = (UnityAction<Vector3>)Delegate.Combine(componentInParent.m_onNodeMoved, new UnityAction<Vector3>(OnNodeMoved));
		}
		if (DamageEnabled)
		{
			CheckForContact();
		}
	}

	private void OnDisable()
	{
		SpriteAnimNodeFollower componentInParent = GetComponentInParent<SpriteAnimNodeFollower>();
		if (componentInParent != null)
		{
			componentInParent.m_onNodeMoved = (UnityAction<Vector3>)Delegate.Remove(componentInParent.m_onNodeMoved, new UnityAction<Vector3>(OnNodeMoved));
		}
		foreach (KeyValuePair<DamageBlock, Collider2D> item in m_overlappedCollidersDamageBlock)
		{
			DamageBlock key = item.Key;
			key.m_onDamageBlockEnabled = (UnityAction<DamageBlock, bool>)Delegate.Remove(key.m_onDamageBlockEnabled, new UnityAction<DamageBlock, bool>(OnDamageBlockStateChanged));
		}
		m_overlappedCollidersDamageBlock.Clear();
		m_damageEnabled = false;
		if (m_collider != null)
		{
			m_collider.enabled = false;
		}
	}

	protected virtual CharacterIdentifier.CharacterFaction GetFaction()
	{
		CharacterIdentifier componentInParent = GetComponentInParent<CharacterIdentifier>();
		CharacterIdentifier.CharacterFaction result = CharacterIdentifier.CharacterFaction.Unknown;
		if (componentInParent != null)
		{
			result = componentInParent.Faction;
		}
		return result;
	}

	protected virtual void OnDamageEnabled()
	{
		m_characterFaction = GetFaction();
		if (m_damageEnabled && m_collider != null)
		{
			CheckForContact();
		}
	}

	private void Update()
	{
		if (m_shouldEnable == m_damageEnabled)
		{
			return;
		}
		m_damageEnabled = m_shouldEnable;
		if (m_collider != null)
		{
			m_collider.enabled = m_shouldEnable;
		}
		if (m_damageEnabled)
		{
			m_damaged.Clear();
			m_countedHits = 0;
			OnDamageEnabled();
			if (m_collider != null)
			{
				m_collider.enabled = m_damageEnabled;
			}
		}
		else
		{
			m_overlappedCollidersDamageBlock.Clear();
			if (m_collider != null)
			{
				m_collider.enabled = false;
			}
		}
	}

	public void OnTriggerEnter2D(Collider2D hitCollider)
	{
		if (m_damageEnabled)
		{
			CheckForContact();
		}
	}

	public HitResultType CheckColliderOverlapForDamage(Collider2D damageCollider, Collider2D hitCollider)
	{
		if (!DamageEnabled)
		{
			return HitResultType.None;
		}
		if (m_countedHits >= m_maxHitsPerActivation && m_maxHitsPerActivation > 0 && !m_isTouchDamage)
		{
			return HitResultType.Ignore;
		}
		float z = damageCollider.transform.position.z;
		float z2 = hitCollider.transform.position.z;
		if (Mathf.Abs(z - z2) > m_allowedDepthDistance)
		{
			return HitResultType.None;
		}
		CharacterIdentifier.CharacterFaction characterFaction = CharacterIdentifier.CharacterFaction.Unknown;
		CharacterIdentifier componentInParent = hitCollider.GetComponentInParent<CharacterIdentifier>();
		if (componentInParent != null)
		{
			characterFaction = componentInParent.Faction;
		}
		if (!m_allowFriendlyFire && m_characterFaction == characterFaction)
		{
			return HitResultType.None;
		}
		MeleeDamageTrigger component = hitCollider.GetComponent<MeleeDamageTrigger>();
		if (component != null)
		{
			component.Trigger();
			return HitResultType.None;
		}
		return PerformHit(damageCollider, hitCollider, characterFaction, componentInParent);
	}

	protected void OnDamageBlockStateChanged(DamageBlock damageBlock, bool enabled)
	{
		damageBlock.m_onDamageBlockEnabled = (UnityAction<DamageBlock, bool>)Delegate.Remove(damageBlock.m_onDamageBlockEnabled, new UnityAction<DamageBlock, bool>(OnDamageBlockStateChanged));
		if (m_overlappedCollidersDamageBlock.ContainsKey(damageBlock))
		{
			Collider2D hitCollider = m_overlappedCollidersDamageBlock[damageBlock];
			m_overlappedCollidersDamageBlock.Remove(damageBlock);
			if (m_collider != null)
			{
				CheckColliderOverlapForDamage(m_collider, hitCollider);
			}
		}
	}

	public virtual void OnTriggerExit2D(Collider2D collider)
	{
		if (m_isTouchDamage)
		{
			IDamageable[] componentsInParent = collider.transform.GetComponentsInParent<IDamageable>();
			foreach (IDamageable item in componentsInParent)
			{
				if (m_damaged.Contains(item))
				{
					m_damaged.Remove(item);
				}
			}
		}
		DamageBlock componentInParent = collider.transform.GetComponentInParent<DamageBlock>();
		if (componentInParent != null)
		{
			componentInParent.m_onDamageBlockEnabled = (UnityAction<DamageBlock, bool>)Delegate.Remove(componentInParent.m_onDamageBlockEnabled, new UnityAction<DamageBlock, bool>(OnDamageBlockStateChanged));
			m_overlappedCollidersDamageBlock.Remove(componentInParent);
		}
	}

	public void Activate()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Deactivate()
	{
		base.gameObject.SetActive(value: false);
	}

	public void SetColliderSizeAndOffset(Vector2 size, Vector2 offset)
	{
		if (m_collider is CapsuleCollider2D capsuleCollider2D)
		{
			capsuleCollider2D.direction = ((size.x > size.y) ? CapsuleDirection2D.Horizontal : CapsuleDirection2D.Vertical);
			capsuleCollider2D.size = size;
			capsuleCollider2D.transform.localPosition = offset;
		}
		else
		{
			Debug.LogError("Unsupported collider2D type for SetColliderSize", base.gameObject);
		}
	}
}
