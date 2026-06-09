using System;
using UnityEngine;
using UnityEngine.Events;

public class CharacterWetnessEffect : MonoBehaviour
{
	[Header("Particle Systems")]
	[SerializeField]
	private ParticleSystem m_bodyWetDripsEffect;

	[SerializeField]
	private ParticleSystem m_feetWetDripsEffect;

	[Header("Drying")]
	[SerializeField]
	private float m_dryingRate = 1f;

	private LegacyCharacterMovement m_characterMovement;

	private SurfaceType m_activeWaterSurface;

	private float m_bodyWetness;

	private float m_feetWetness;

	public float MaterialWetness => Mathf.Max(m_feetWetness * 0.25f, m_bodyWetness);

	private void OnEnable()
	{
		m_characterMovement = GetComponent<LegacyCharacterMovement>();
		if (m_characterMovement != null)
		{
			LegacyCharacterMovement characterMovement = m_characterMovement;
			characterMovement.m_onWaterSurfaceChanged = (UnityAction<SurfaceType>)Delegate.Combine(characterMovement.m_onWaterSurfaceChanged, new UnityAction<SurfaceType>(OnWaterSurfaceChanged));
		}
	}

	private void OnDisable()
	{
		if (m_characterMovement != null)
		{
			LegacyCharacterMovement characterMovement = m_characterMovement;
			characterMovement.m_onWaterSurfaceChanged = (UnityAction<SurfaceType>)Delegate.Remove(characterMovement.m_onWaterSurfaceChanged, new UnityAction<SurfaceType>(OnWaterSurfaceChanged));
		}
	}

	private void OnWaterSurfaceChanged(SurfaceType waterSurface)
	{
		if (waterSurface == null && m_activeWaterSurface != null)
		{
			m_feetWetness = 1f;
			m_feetWetDripsEffect.Play();
		}
		m_activeWaterSurface = waterSurface;
	}

	private void Update()
	{
		if (m_feetWetness > 0f)
		{
			m_feetWetness -= Time.deltaTime * m_dryingRate;
			if (m_feetWetness <= 0f)
			{
				m_feetWetDripsEffect.Stop();
			}
		}
		if (m_bodyWetness > 0f)
		{
			m_bodyWetness -= Time.deltaTime * m_dryingRate;
			if (m_bodyWetness <= 0f)
			{
				m_bodyWetDripsEffect.Stop();
			}
		}
	}

	public void PlayLandEffectEvent()
	{
		if (m_activeWaterSurface != null)
		{
			m_bodyWetness = 1f;
			m_bodyWetDripsEffect.Play();
		}
	}
}
