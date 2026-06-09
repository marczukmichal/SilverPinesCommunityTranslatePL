using UnityEngine;

public class MeatballDamageListener : MonoBehaviour, IDamageable, ICharacterFSMUtilitiesHitReactOverrider
{
	private enum ReactState
	{
		None,
		Normal,
		Pushback
	}

	[SerializeField]
	private int m_damageThreshold;

	[SerializeField]
	private PlayMakerFSM m_fsm;

	[SerializeField]
	private float m_weakpointDamageMultiplier = 8f;

	[SerializeField]
	private float m_meleeDamageMultiplier = 1f;

	[SerializeField]
	private float m_normalMultiplier;

	[SerializeField]
	private float m_xPosForMaximumStrength;

	[SerializeField]
	private float m_xPosForMinimumStrength;

	[SerializeField]
	private AnimationCurve m_pushbackCurve;

	private float m_startingXPos;

	private int m_damageReceived;

	private bool m_triggered;

	private ReactState m_reactState;

	private void Start()
	{
		m_startingXPos = base.transform.position.x;
		GetComponent<CharacterFSMUtilities>().SetOverrideClass(this);
	}

	public void ResetDamageReceived()
	{
		m_damageReceived = 0;
		m_triggered = false;
	}

	public void ResetReactState()
	{
		m_reactState = ReactState.None;
	}

	public void ApplyDamageInstance(DamageInstance instance)
	{
		float num = (instance.IsWeakpointDamage ? m_weakpointDamageMultiplier : ((instance.DamageCategory != DamageCategory.DamageCollider) ? m_normalMultiplier : m_meleeDamageMultiplier));
		float num2 = m_pushbackCurve.Evaluate(Mathf.Clamp01(Mathf.InverseLerp(m_xPosForMinimumStrength + m_startingXPos, m_xPosForMaximumStrength + m_startingXPos, base.transform.position.x)));
		num *= num2;
		m_damageReceived += Mathf.RoundToInt((float)instance.HealthDamageAmount * num);
		if (!m_triggered && m_damageReceived > m_damageThreshold)
		{
			m_triggered = true;
		}
		if (m_damageReceived > m_damageThreshold)
		{
			m_reactState = ReactState.Pushback;
			m_fsm.SendEvent("Boss/HitReact/Pushback");
		}
	}

	public bool HandleHitReact()
	{
		return m_reactState == ReactState.Pushback;
	}

	private void OnDrawGizmos()
	{
		Vector3 from;
		Vector3 position;
		if (Application.isPlaying)
		{
			from = (position = base.transform.position);
			from.x = m_startingXPos + m_xPosForMinimumStrength;
			position.x = m_startingXPos + m_xPosForMaximumStrength;
		}
		else
		{
			from = (position = base.transform.position);
			from.x = base.transform.position.x + m_xPosForMinimumStrength;
			position.x = base.transform.position.x + m_xPosForMaximumStrength;
		}
		Gizmos.color = Color.red;
		Gizmos.DrawLine(from, position);
		Gizmos.color = Color.white;
	}
}
