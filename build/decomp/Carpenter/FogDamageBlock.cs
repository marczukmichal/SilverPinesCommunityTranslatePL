using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.VFX;

[RequireComponent(typeof(DamageBlock))]
public class FogDamageBlock : MonoBehaviour
{
	[SerializeField]
	private UnityEvent m_onEnterFog;

	[SerializeField]
	private UnityEvent m_onExitFog;

	[SerializeField]
	private SpriteRenderer m_spriteRenderer;

	[SerializeField]
	private float m_boundsExpandAmount = 0.2f;

	[Header("Effects")]
	[SerializeField]
	private Transform m_fogTransform;

	[SerializeField]
	private VisualEffect m_constantFogEffect;

	[SerializeField]
	private VisualEffect m_revealedFogEffect;

	[SerializeField]
	private float m_fogScaleSpeed = 1f;

	private DamageBlock m_damageBlock;

	private CharacterHealth m_health;

	private Vector3 m_startingFogTransformScale;

	private bool m_isInFog;

	private bool InFog
	{
		get
		{
			return m_isInFog;
		}
		set
		{
			if (m_isInFog != value)
			{
				m_isInFog = value;
				if (value)
				{
					m_damageBlock.ActivateDamageBlock();
					m_onEnterFog.Invoke();
				}
				else
				{
					m_damageBlock.DeactivateDamageBlock();
					m_onExitFog.Invoke();
				}
				if (m_fogTransform != null)
				{
					DOTween.Kill(m_fogTransform);
					m_fogTransform.DOScale(value ? m_startingFogTransformScale : Vector3.zero, m_fogScaleSpeed);
				}
				if (m_constantFogEffect != null)
				{
					m_constantFogEffect.SetBool("Spawning", value);
				}
				if (m_revealedFogEffect != null && !value)
				{
					m_revealedFogEffect.Play();
				}
				if (m_spriteRenderer != null)
				{
					m_spriteRenderer.shadowCastingMode = ((!value) ? ShadowCastingMode.On : ShadowCastingMode.Off);
				}
			}
		}
	}

	private void Start()
	{
		m_startingFogTransformScale = m_fogTransform.localScale;
		m_damageBlock = GetComponent<DamageBlock>();
		m_health = GetComponent<CharacterHealth>();
		InFog = false;
		if (m_health != null)
		{
			if (m_health.IsDead)
			{
				OnCharacterDead();
			}
			else
			{
				m_health.OnDead.AddListener(OnCharacterDead);
			}
		}
	}

	private void OnCharacterDead()
	{
		InFog = false;
		base.enabled = false;
	}

	private void Update()
	{
		Bounds bounds = m_spriteRenderer.bounds;
		bounds.Expand(m_boundsExpandAmount);
		InFog = GlobalReferences.Instance.Sets.Generic.GameplayFogSet.IsInFog(bounds);
	}
}
