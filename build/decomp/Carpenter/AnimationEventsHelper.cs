using System.Collections.Generic;
using PowerTools;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEventsHelper : MonoBehaviour
{
	[SerializeField]
	private SpriteAnimNodes m_spriteAnimNodes;

	[SerializeField]
	private float m_fallBackwardsCheckDistance = 1.5f;

	private List<DamageCollider> m_colliders = new List<DamageCollider>();

	private bool m_rootMovementEnabled;

	private Vector3 m_previousMovementNodePos;

	private float m_nextAnimStartTime;

	private AnimationClip m_nextAnimStartTimeClip;

	private CharacterDirection m_charDirection;

	private CharacterMovement m_charMovement;

	private CharacterAiming m_charAiming;

	private DamageBlock m_damageBlock;

	private CharacterHitReact m_charHitReact;

	private bool m_isAnimationInputBlocked;

	public UnityEvent<string> OnAnimationEventStringEvent;

	public bool IsAnimationInputBlocked => m_isAnimationInputBlocked;

	public float GetNextAnimStartTime(AnimationClip clip)
	{
		if (clip == m_nextAnimStartTimeClip)
		{
			return m_nextAnimStartTime;
		}
		return 0f;
	}

	public void SetNextAnimStartTime(AnimationClip clip, float time)
	{
		m_nextAnimStartTimeClip = clip;
		m_nextAnimStartTime = time;
	}

	private void Start()
	{
		m_charDirection = GetComponent<CharacterDirection>();
		m_charMovement = GetComponent<CharacterMovement>();
		m_charAiming = GetComponent<CharacterAiming>();
		m_charHitReact = GetComponent<CharacterHitReact>();
		m_damageBlock = GetComponent<DamageBlock>();
	}

	private void OnEnable()
	{
		m_colliders = new List<DamageCollider>();
	}

	private void LateUpdate()
	{
		if (m_rootMovementEnabled)
		{
			Vector3 localPosition = m_spriteAnimNodes.GetLocalPosition(3);
			if (localPosition != m_previousMovementNodePos)
			{
				base.transform.Translate(localPosition - m_previousMovementNodePos);
				m_previousMovementNodePos = localPosition;
			}
		}
	}

	public void EnableMovementFromNode()
	{
		m_rootMovementEnabled = true;
		m_previousMovementNodePos = m_spriteAnimNodes.GetLocalPosition(3);
	}

	public void DisableMovementFromNode()
	{
		LateUpdate();
		m_rootMovementEnabled = false;
	}

	public void TranslateRootVertically(float amount)
	{
		SpriteRenderer component = m_spriteAnimNodes.GetComponent<SpriteRenderer>();
		float y = amount * (1f / component.sprite.pixelsPerUnit);
		base.transform.Translate(new Vector3(0f, y, 0f));
	}

	public void TranslateRootHorizontally(float amount)
	{
		SpriteRenderer component = m_spriteAnimNodes.GetComponent<SpriteRenderer>();
		float num = amount * (1f / component.sprite.pixelsPerUnit);
		if (m_charDirection != null && m_charDirection.CurrentDirection == CharacterDirection.Facing.Left)
		{
			num *= -1f;
		}
		base.transform.Translate(new Vector3(num, 0f, 0f));
	}

	public void EnableDamageCollider(string colliderName)
	{
		DamageCollider[] componentsInChildren = GetComponentsInChildren<DamageCollider>(includeInactive: true);
		foreach (DamageCollider damageCollider in componentsInChildren)
		{
			if (damageCollider.name.Equals(colliderName))
			{
				damageCollider.SetDamageEnabled(enabled: true);
				damageCollider.gameObject.SetActive(value: true);
				m_colliders.Add(damageCollider);
			}
		}
	}

	public void DisableDamageCollider(string colliderName)
	{
		DamageCollider[] componentsInChildren = GetComponentsInChildren<DamageCollider>(includeInactive: true);
		foreach (DamageCollider damageCollider in componentsInChildren)
		{
			if (damageCollider.name.Equals(colliderName))
			{
				damageCollider.gameObject.SetActive(value: false);
				damageCollider.SetDamageEnabled(enabled: false);
				m_colliders.Remove(damageCollider);
			}
		}
	}

	public void EnableSoloDamageColliderChild(string colliderName)
	{
		DamageColliderChild[] componentsInChildren = GetComponentsInChildren<DamageColliderChild>(includeInactive: true);
		foreach (DamageColliderChild damageColliderChild in componentsInChildren)
		{
			if (damageColliderChild.name.Equals(colliderName))
			{
				damageColliderChild.SetDamageEnabled(enabled: true);
				damageColliderChild.gameObject.SetActive(value: true);
			}
			else
			{
				damageColliderChild.SetDamageEnabled(enabled: false);
				damageColliderChild.gameObject.SetActive(value: false);
			}
		}
	}

	public void EnableDamageColliderChild(string colliderName)
	{
		DamageColliderChild[] componentsInChildren = GetComponentsInChildren<DamageColliderChild>(includeInactive: true);
		foreach (DamageColliderChild damageColliderChild in componentsInChildren)
		{
			if (damageColliderChild.name.Equals(colliderName))
			{
				damageColliderChild.SetDamageEnabled(enabled: true);
				damageColliderChild.gameObject.SetActive(value: true);
			}
		}
	}

	public void DisableDamageColliderChild(string colliderName)
	{
		DamageColliderChild[] componentsInChildren = GetComponentsInChildren<DamageColliderChild>(includeInactive: true);
		foreach (DamageColliderChild damageColliderChild in componentsInChildren)
		{
			if (damageColliderChild.name.Equals(colliderName))
			{
				damageColliderChild.gameObject.SetActive(value: false);
				damageColliderChild.SetDamageEnabled(enabled: false);
			}
		}
	}

	public void DisableAllDamageColliders()
	{
		foreach (DamageCollider collider in m_colliders)
		{
			collider.SetDamageEnabled(enabled: false);
			collider.gameObject.SetActive(value: false);
		}
		m_colliders.Clear();
		DamageColliderChild[] componentsInChildren = GetComponentsInChildren<DamageColliderChild>();
		foreach (DamageColliderChild obj in componentsInChildren)
		{
			obj.SetDamageEnabled(enabled: false);
			obj.gameObject.SetActive(value: false);
		}
	}

	public void PerformSyncGrabHit()
	{
		GetComponent<CharacterSyncGrab>().ApplyHit();
	}

	public void EnableInvincibilityFrames()
	{
		m_damageBlock.ActivateDamageBlock();
	}

	public void DisableInvincibilityFrames()
	{
		m_damageBlock.DeactivateDamageBlock();
	}

	public void HitReactDisable()
	{
		m_charHitReact.StatelessHitReactEnabled = true;
		m_charHitReact.HitReactsEventsBlocked = true;
	}

	public void HitReactEnable()
	{
		m_charHitReact.StatelessHitReactEnabled = false;
		m_charHitReact.HitReactsEventsBlocked = false;
	}

	public void EnableInputBlock()
	{
		m_isAnimationInputBlocked = true;
	}

	public void DisableInputBlock()
	{
		m_isAnimationInputBlocked = false;
	}

	public void EnableFixedDepth()
	{
		m_charMovement.FixedDepthMovement = true;
	}

	public void DisableFixedDepth()
	{
		m_charMovement.FixedDepthMovement = false;
	}

	public void PlayAudioEvent(AudioEvent audioEvent)
	{
		if (audioEvent == null)
		{
			Debug.Log("Tried to play a null audio event on gameobject " + base.gameObject);
			return;
		}
		bool flag = GameUtils.IsPlayer(base.gameObject);
		GlobalReferences.Instance.EventChannels.Audio.PlayAudio.Raise(new PlayAudioEventData(audioEvent, base.transform.position, !flag));
	}

	public void PerformWeaponPump()
	{
		m_charAiming.PerformPumpFromAnimation(ejectShellCasing: true);
	}

	public void PlayCameraShake(CameraShakeSettings cameraShake)
	{
		Vector3 direction;
		if (cameraShake.m_matchCharacterDirection && m_charDirection != null)
		{
			direction = m_charDirection.GetForwardVector();
			switch (cameraShake.m_yAxisDirectionMode)
			{
			case CameraShakeSettings.CameraShakeYMode.SetValue:
				direction.y = cameraShake.m_matchCharacterDirectionYValue;
				break;
			case CameraShakeSettings.CameraShakeYMode.RandomRange:
				direction.y = Random.Range(0f - cameraShake.m_matchCharacterDirectionYValue, cameraShake.m_matchCharacterDirectionYValue);
				break;
			}
			direction.x *= cameraShake.m_characterDirectionXScalar;
			direction.Normalize();
		}
		else
		{
			direction = Random.onUnitSphere;
		}
		CameraShakeEventData value = new CameraShakeEventData
		{
			m_cameraShakeSettings = cameraShake,
			m_position = base.transform.position,
			m_direction = direction
		};
		GlobalReferences.Instance.EventChannels.Generic.CameraShake.Raise(value);
	}

	public void CustomAnimationEvent(string message)
	{
		OnAnimationEventStringEvent?.Invoke(message);
	}

	public void FallOverFlipCheck()
	{
		CharacterDirection component = GetComponent<CharacterDirection>();
		if (!(component != null))
		{
			return;
		}
		Vector2 direction = component.GetForwardVector();
		direction.x *= -1f;
		Vector2 origin = base.transform.position;
		origin.y += 1f;
		RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, direction, m_fallBackwardsCheckDistance, GameLayers.CharacterNavigationMask);
		if ((bool)raycastHit2D)
		{
			Vector3 position = base.transform.position;
			position.x = raycastHit2D.point.x - direction.x * m_fallBackwardsCheckDistance * 0.25f;
			position.y += 0.25f;
			base.transform.position = position;
			CharacterMovement component2 = GetComponent<CharacterMovement>();
			if (component2 != null)
			{
				component2.SnapToGround();
			}
			component.CurrentDirection = CharacterDirection.GetOppositeDirection(component.CurrentDirection);
		}
	}

	public void FlashlightNormal()
	{
		PlayerFlashlight componentInChildren = GetComponentInChildren<PlayerFlashlight>();
		if (componentInChildren != null)
		{
			componentInChildren.SetFlashlightPosition(PlayerFlashlight.FlashlightPosition.Normal, 0.2f);
		}
	}

	public void FlashlightRotatedAway()
	{
		PlayerFlashlight componentInChildren = GetComponentInChildren<PlayerFlashlight>();
		if (componentInChildren != null)
		{
			componentInChildren.SetFlashlightPosition(PlayerFlashlight.FlashlightPosition.RotatedAway, 0.2f);
		}
	}

	public void FlashlightRotatedTowards()
	{
		PlayerFlashlight componentInChildren = GetComponentInChildren<PlayerFlashlight>();
		if (componentInChildren != null)
		{
			componentInChildren.SetFlashlightPosition(PlayerFlashlight.FlashlightPosition.RotatedTowards, 0.2f);
		}
	}
}
