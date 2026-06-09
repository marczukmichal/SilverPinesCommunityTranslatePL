using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class ShootingRangeTarget : MonoBehaviour, IDamageable
{
	[SerializeField]
	private Transform m_targetTransform;

	[SerializeField]
	private float m_downedRotation;

	[SerializeField]
	private float m_upRotation;

	[SerializeField]
	private Collider2D m_collider;

	[SerializeField]
	private int m_baseScore = 1;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_raiseAudio;

	[Header("Looping Movement")]
	[SerializeField]
	private Vector3 m_moveAmount;

	[SerializeField]
	private float m_moveSpeed = 1f;

	[SerializeField]
	private Vector2 m_randomDelayRange;

	[SerializeField]
	private AudioEvent m_creakAudio;

	private bool m_loopingMovementActive;

	private bool m_endLoopingMovement;

	private float m_loopingMovementTimer;

	private float m_delayTimer;

	private bool m_isUp;

	private Vector3 m_baseRotation;

	private Vector3 m_basePosition;

	public UnityAction<int> OnShot;

	public bool IsUp => m_isUp;

	private void Awake()
	{
		m_baseRotation = m_targetTransform.rotation.eulerAngles;
		m_basePosition = base.transform.position;
		m_isUp = false;
		Vector3 baseRotation = m_baseRotation;
		baseRotation.x = m_downedRotation;
		Quaternion localRotation = Quaternion.Euler(baseRotation);
		m_targetTransform.localRotation = localRotation;
		m_collider.enabled = false;
	}

	public void ApplyDamageInstance(DamageInstance instance)
	{
		if (instance.DamageCategory == DamageCategory.Projectile && m_isUp)
		{
			int baseScore = m_baseScore;
			LowerTarget();
			OnShot?.Invoke(baseScore);
		}
	}

	public void RaiseTarget(float timer)
	{
		if (!m_isUp)
		{
			StartCoroutine(RaiseAndCloseDelayed(timer));
		}
	}

	public void RaiseTarget()
	{
		if (!m_isUp)
		{
			SetStateUp();
		}
	}

	public IEnumerator RaiseAndCloseDelayed(float timer)
	{
		SetStateUp();
		yield return new WaitForSeconds(timer);
		SetStateDown();
	}

	public void LowerTarget()
	{
		if (m_isUp)
		{
			StopAllCoroutines();
			SetStateDown();
		}
	}

	private void SetStateUp()
	{
		Vector3 baseRotation = m_baseRotation;
		baseRotation.x = m_upRotation;
		Quaternion.Euler(baseRotation);
		m_targetTransform.DOLocalRotate(baseRotation, 0.5f).SetEase(Ease.OutBounce);
		m_isUp = true;
		m_collider.enabled = true;
		m_loopingMovementActive = true;
		m_endLoopingMovement = false;
		m_delayTimer = UnityEngine.Random.Range(m_randomDelayRange.x, m_randomDelayRange.y);
		m_raiseAudio.Play(base.transform.position);
	}

	private void SetStateDown()
	{
		Vector3 baseRotation = m_baseRotation;
		baseRotation.x = m_downedRotation;
		Quaternion.Euler(baseRotation);
		m_targetTransform.DOLocalRotate(baseRotation, 0.5f).SetEase(Ease.OutBounce);
		m_isUp = false;
		m_collider.enabled = false;
		m_endLoopingMovement = true;
	}

	private void Update()
	{
		if (!m_loopingMovementActive)
		{
			return;
		}
		if (m_delayTimer > 0f)
		{
			m_delayTimer -= Time.deltaTime;
			if (m_endLoopingMovement)
			{
				m_loopingMovementActive = false;
				m_endLoopingMovement = false;
			}
		}
		else
		{
			float loopingMovementTimer = m_loopingMovementTimer;
			m_loopingMovementTimer += Time.deltaTime * m_moveSpeed;
			if (loopingMovementTimer < MathF.PI && m_loopingMovementTimer >= MathF.PI)
			{
				m_creakAudio.Play(base.transform.position);
			}
			if (loopingMovementTimer < MathF.PI / 2f && m_loopingMovementTimer >= MathF.PI / 2f)
			{
				m_creakAudio.Play(base.transform.position);
			}
			if (m_loopingMovementTimer >= MathF.PI * 2f)
			{
				if (m_endLoopingMovement)
				{
					m_loopingMovementTimer = 0f;
					m_loopingMovementActive = false;
					m_endLoopingMovement = false;
				}
				else
				{
					m_loopingMovementTimer = 0f;
					m_delayTimer = UnityEngine.Random.Range(m_randomDelayRange.x, m_randomDelayRange.y);
				}
			}
		}
		Vector3 basePosition = m_basePosition;
		basePosition += (Mathf.Cos(m_loopingMovementTimer) * -1f + 1f) * 0.5f * m_moveAmount;
		base.transform.position = basePosition;
	}

	private void OnDrawGizmos()
	{
		Vector3 position = base.transform.position;
		position += m_moveAmount;
		Gizmos.color = Color.yellow;
		Gizmos.DrawLine(base.transform.position, position);
		Gizmos.color = Color.white;
	}
}
