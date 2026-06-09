using UnityEngine;
using UnityEngine.Events;

public class DamageBlock : MonoBehaviour, IDamageable
{
	[SerializeField]
	private float m_postHitInvincibilityDuration;

	private bool m_damageBlockActive;

	private bool m_damageBlockActiveTouchOnly;

	[DebugCommand("show_damage_block", "Show Dmaage Block on player", "show_damage_block <true/false>", typeof(bool), false)]
	private static bool s_show_damage_block;

	private float m_postHitInvincibilityTimer;

	private bool m_postHitInvincibilityIsTouchDamageOnly;

	public UnityAction<DamageBlock, bool> m_onDamageBlockEnabled;

	public void ActivateDamageBlock(bool touchOnly)
	{
		if (touchOnly)
		{
			m_damageBlockActiveTouchOnly = true;
		}
		else
		{
			m_damageBlockActive = true;
		}
		m_onDamageBlockEnabled?.Invoke(this, arg1: true);
	}

	public void ActivateDamageBlock()
	{
		ActivateDamageBlock(touchOnly: false);
	}

	public void DeactivateDamageBlock(bool touchOnly)
	{
		if (touchOnly)
		{
			m_damageBlockActiveTouchOnly = false;
		}
		else
		{
			m_damageBlockActive = false;
		}
		m_onDamageBlockEnabled?.Invoke(this, arg1: false);
	}

	public void DeactivateDamageBlock()
	{
		DeactivateDamageBlock(touchOnly: false);
	}

	public bool CanTakeDamage(DamageInstance damageInstance)
	{
		if (GameDebugCommands.CHEAT_INVINCIBLE && GameUtils.IsPlayer(base.gameObject))
		{
			return false;
		}
		if (damageInstance != null && damageInstance.HitFlags.HasFlag(HitFlags.IgnoreDamageBlock))
		{
			return true;
		}
		if (LevelManager.LevelTransitionActive && GameUtils.IsPlayer(base.gameObject))
		{
			return false;
		}
		if (GameCutsceneManager.CutsceneActive && GameUtils.IsPlayer(base.gameObject))
		{
			return false;
		}
		bool result = !m_damageBlockActive;
		if (damageInstance != null && damageInstance.IsTouchDamage && m_damageBlockActiveTouchOnly)
		{
			result = false;
		}
		if (m_postHitInvincibilityTimer > 0f && (!m_postHitInvincibilityIsTouchDamageOnly || (damageInstance != null && damageInstance.IsTouchDamage)))
		{
			result = false;
		}
		return result;
	}

	private void Update()
	{
		if (m_postHitInvincibilityTimer > 0f)
		{
			m_postHitInvincibilityTimer -= Time.deltaTime;
			if (m_postHitInvincibilityTimer <= 0f)
			{
				m_onDamageBlockEnabled?.Invoke(this, m_damageBlockActive);
			}
		}
		if (s_show_damage_block && GameUtils.IsPlayer(base.gameObject))
		{
			SpriteRenderer componentInChildren = base.gameObject.GetComponentInChildren<SpriteRenderer>();
			Color color = Color.white;
			if (m_postHitInvincibilityTimer > 0f)
			{
				color = (m_postHitInvincibilityIsTouchDamageOnly ? Color.magenta : Color.green);
			}
			else if (m_damageBlockActive)
			{
				color = Color.blue;
			}
			else if (m_damageBlockActiveTouchOnly)
			{
				color = Color.red;
			}
			componentInChildren.color = color;
		}
	}

	public void ApplyDamageInstance(DamageInstance instance)
	{
		if (!instance.HitFlags.HasFlag(HitFlags.DontTriggerDamageBlock) && instance.HealthDamageAmount > 0)
		{
			m_postHitInvincibilityTimer = m_postHitInvincibilityDuration;
			m_postHitInvincibilityIsTouchDamageOnly = (instance.IsTouchDamage ? true : false);
		}
	}
}
