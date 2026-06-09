using UnityEngine;

public class CharacterDamageRage : MonoBehaviour, IDamageable, ICharacterFSMUtilitiesHitReactOverrider
{
	private enum RageState
	{
		None,
		RageQueued,
		RageActive
	}

	[SerializeField]
	private float m_ragePoint = 100f;

	[SerializeField]
	private float m_rageRangedDamageScalar = 2.5f;

	[SerializeField]
	private float m_rageMeleeDmaageScalar = 5f;

	[SerializeField]
	private float m_rageReductionRate = 10f;

	[SerializeField]
	private float m_rageReductionFadeDelay = 1f;

	[SerializeField]
	private float m_rageDuration = 4f;

	[SerializeField]
	private float m_rageActivationDelay = 0.75f;

	[SerializeField]
	private float m_cooldownReductionRateWhileRaging = 4f;

	private float m_rageBuildup;

	private RageState m_rageState;

	private float m_timer;

	public float CooldownReducationRateWhileRaging => m_cooldownReductionRateWhileRaging;

	public bool IsRaging => m_rageState == RageState.RageActive;

	public float RageMeter
	{
		get
		{
			return m_rageBuildup;
		}
		set
		{
			m_rageBuildup = value;
			if (m_rageBuildup >= m_ragePoint)
			{
				m_rageBuildup = m_ragePoint;
				m_rageState = RageState.RageQueued;
				m_timer = 0f;
			}
		}
	}

	public void ApplyDamageInstance(DamageInstance instance)
	{
		if (m_rageState == RageState.None)
		{
			if (instance.DamageCategory == DamageCategory.DamageCollider)
			{
				RageMeter += (float)instance.HealthDamageAmount * m_rageMeleeDmaageScalar;
			}
			else
			{
				RageMeter += (float)instance.HealthDamageAmount * m_rageRangedDamageScalar;
			}
			m_timer = 0f;
		}
	}

	private void Start()
	{
		GetComponent<CharacterFSMUtilities>().SetOverrideClass(this);
	}

	public bool HandleHitReact()
	{
		return m_rageState == RageState.RageActive;
	}

	private void Update()
	{
		switch (m_rageState)
		{
		case RageState.None:
			if (m_timer >= m_rageReductionFadeDelay)
			{
				m_rageBuildup -= Time.deltaTime * m_rageReductionRate;
				m_rageBuildup = Mathf.Max(0f, m_rageBuildup);
			}
			else
			{
				m_timer += Time.deltaTime;
			}
			break;
		case RageState.RageQueued:
			m_timer += Time.deltaTime;
			if (m_timer >= m_rageActivationDelay)
			{
				m_rageState = RageState.RageActive;
				m_timer = 0f;
			}
			break;
		case RageState.RageActive:
			m_timer += Time.deltaTime;
			if (m_timer >= m_rageDuration)
			{
				m_rageState = RageState.None;
				m_rageBuildup = 0f;
			}
			break;
		}
	}
}
