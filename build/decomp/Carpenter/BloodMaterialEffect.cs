using System;
using UnityEngine;
using UnityEngine.Events;

public class BloodMaterialEffect : MonoBehaviour, IDamageable
{
	[SerializeField]
	private float m_bloodAmount;

	[SerializeField]
	private float m_maxBloodAmount = 1f;

	[SerializeField]
	private float m_bloodFadeRate;

	[SerializeField]
	private string m_bloodPropertyName = "_BloodAmount";

	private int m_bloodPropertyID;

	private SpriteRenderer[] m_spriteRenderers;

	private CharacterHealth m_characterHealth;

	private bool m_isDead;

	private float m_healthPercent;

	private float MinBloodAmount
	{
		get
		{
			if (m_isDead)
			{
				return 0.75f;
			}
			if (m_healthPercent < 0.5f)
			{
				return 0.35f;
			}
			return 0f;
		}
	}

	private void Start()
	{
		m_healthPercent = 1f;
		m_isDead = false;
		m_bloodPropertyID = Shader.PropertyToID(m_bloodPropertyName);
		m_spriteRenderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
	}

	private void OnEnable()
	{
		m_characterHealth = GetComponent<CharacterHealth>();
		if (m_characterHealth != null)
		{
			m_isDead = m_characterHealth.IsDead;
			m_healthPercent = m_characterHealth.HealthPercentage;
			m_characterHealth.OnDead.AddListener(OnDead);
			CharacterHealth characterHealth = m_characterHealth;
			characterHealth.OnTakenDamage = (UnityAction<int>)Delegate.Combine(characterHealth.OnTakenDamage, new UnityAction<int>(OnTakenDamage));
		}
	}

	private void OnDisable()
	{
		if (m_characterHealth != null)
		{
			m_characterHealth.OnDead.RemoveListener(OnDead);
			CharacterHealth characterHealth = m_characterHealth;
			characterHealth.OnTakenDamage = (UnityAction<int>)Delegate.Remove(characterHealth.OnTakenDamage, new UnityAction<int>(OnTakenDamage));
		}
	}

	private void OnDead()
	{
		m_isDead = true;
	}

	private void OnTakenDamage(int damage)
	{
		CharacterHealth component = GetComponent<CharacterHealth>();
		m_healthPercent = component.HealthPercentage;
	}

	private void Update()
	{
		if (m_bloodAmount > 0f)
		{
			m_bloodAmount -= Time.deltaTime * m_bloodFadeRate;
			m_bloodAmount = Mathf.Clamp(m_bloodAmount, MinBloodAmount, m_maxBloodAmount);
		}
		SpriteRenderer[] spriteRenderers = m_spriteRenderers;
		foreach (SpriteRenderer spriteRenderer in spriteRenderers)
		{
			if (spriteRenderer != null)
			{
				spriteRenderer.material.SetFloat(m_bloodPropertyID, m_bloodAmount);
			}
		}
	}

	public void ApplyBloodAmount(float amount)
	{
		m_bloodAmount += amount;
		m_bloodAmount = Mathf.Clamp(m_bloodAmount, MinBloodAmount, m_maxBloodAmount);
	}

	public void ApplyDamageInstance(DamageInstance instance)
	{
		if ((float)instance.HealthDamageAmount > 0f)
		{
			ApplyBloodAmount(m_maxBloodAmount * 0.5f);
		}
	}
}
