using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class SideDoor : MonoBehaviour, IPersistentComponent, IDamageable
{
	[Serializable]
	public struct DoorEvents
	{
		public UnityEvent m_onDoorOpenEvent;

		public UnityEvent m_onDoorCloseEvent;

		public UnityEvent m_onDoorBrokenOpenEvent;
	}

	public enum SideDoorState
	{
		Closed,
		OpenLeft,
		OpenRight
	}

	[Serializable]
	private class PersistentData
	{
		public SideDoorState m_doorState;
	}

	[SerializeField]
	private Transform m_doorTransform;

	[SerializeField]
	private Vector3 m_openLeftRotation = new Vector3(0f, 90f, 0f);

	[SerializeField]
	private Vector3 m_openRightRotation = new Vector3(0f, -90f, 0f);

	[SerializeField]
	private AnimationCurve m_openAnimCurve;

	[SerializeField]
	private AnimationCurve m_closeAnimCurve;

	[SerializeField]
	private float m_doorAnimSpeed = 1f;

	[SerializeField]
	private int m_doorHealth = -1;

	[SerializeField]
	private DoorEvents m_doorEvents;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_doorOpenAudio;

	[SerializeField]
	private AudioEvent m_doorCloseAudio;

	[SerializeField]
	private AudioEvent m_doorTakeDamageAudio;

	[SerializeField]
	private AudioEvent m_doorBreakOpenAudio;

	[Header("Door Break")]
	[SerializeField]
	private AssetReference m_breakDoorFX;

	[SerializeField]
	private DamageCollider m_doorBreakDamageCollider;

	private static readonly float s_updateRate = 1f / 24f;

	private Tween m_activeTween;

	private Coroutine m_coroutine;

	private int m_currentHealth;

	private bool m_forceInvincible;

	private Vector3 m_doorTransformStartLocalPosition;

	private SideDoorState m_doorState;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	private int Health
	{
		get
		{
			return m_currentHealth;
		}
		set
		{
			if (m_currentHealth != value)
			{
				m_currentHealth = value;
			}
		}
	}

	public SideDoorState State => m_doorState;

	public void SetForceInvincible(bool invincible)
	{
		m_forceInvincible = invincible;
	}

	public bool IsClosed()
	{
		return m_doorState == SideDoorState.Closed;
	}

	private void Start()
	{
		m_doorTransformStartLocalPosition = m_doorTransform.localPosition;
		Health = m_doorHealth;
		Debug.LogError("Old SideDoor component is used on GameObject: " + base.gameObject.name + "- Please replace with new door!");
	}

	public void InteractWithDoor(BaseInteractor interactor, bool activated)
	{
		if (activated)
		{
			bool flag = interactor.transform.position.x < m_doorTransform.position.x;
			SetDoorState((!flag) ? SideDoorState.OpenLeft : SideDoorState.OpenRight);
		}
		else
		{
			SetDoorState(SideDoorState.Closed);
		}
	}

	public void SetDoorState(SideDoorState newState, bool instant = false)
	{
		if (m_doorState != newState)
		{
			if (newState == SideDoorState.Closed)
			{
				Health = m_doorHealth;
				m_doorEvents.m_onDoorCloseEvent?.Invoke();
				m_doorCloseAudio?.Play(base.transform.position);
			}
			else
			{
				m_doorEvents.m_onDoorOpenEvent?.Invoke();
				m_doorOpenAudio?.Play(base.transform.position);
			}
			if (m_persistentData != null)
			{
				m_persistentData.m_doorState = newState;
			}
			m_doorState = newState;
			Vector3 vector = Vector3.zero;
			switch (newState)
			{
			case SideDoorState.OpenLeft:
				vector = m_openLeftRotation;
				break;
			case SideDoorState.OpenRight:
				vector = m_openRightRotation;
				break;
			}
			if (m_activeTween != null)
			{
				DOTween.Kill(m_activeTween, complete: true);
			}
			if (m_coroutine != null)
			{
				StopCoroutine(m_coroutine);
			}
			if (instant)
			{
				m_doorTransform.localRotation = Quaternion.Euler(vector);
				return;
			}
			AnimationCurve animCurve = ((newState == SideDoorState.Closed) ? m_closeAnimCurve : m_openAnimCurve);
			m_activeTween = m_doorTransform.DOLocalRotate(vector, m_doorAnimSpeed).SetUpdate(UpdateType.Manual).SetEase(animCurve);
			m_coroutine = StartCoroutine(UpdateCoroutine());
		}
	}

	private IEnumerator UpdateCoroutine()
	{
		while (m_activeTween != null && m_activeTween.IsActive())
		{
			m_activeTween.ManualUpdate(s_updateRate, s_updateRate);
			yield return new WaitForSeconds(s_updateRate);
		}
	}

	public void ApplyDamageInstance(DamageInstance instance)
	{
		bool flag = false;
		if (m_doorHealth > 0 && !m_forceInvincible)
		{
			m_currentHealth -= instance.HealthDamageAmount;
			if (m_persistentData != null)
			{
				Health = m_currentHealth;
			}
			flag = Health <= 0;
		}
		if (flag)
		{
			m_doorBreakOpenAudio?.Play(base.transform.position);
			m_doorEvents.m_onDoorBrokenOpenEvent?.Invoke();
			if (m_activeTween != null)
			{
				DOTween.Kill(m_activeTween, complete: true);
				m_activeTween = null;
			}
			if (instance.Direction.x < 0f)
			{
				SetDoorState(SideDoorState.OpenLeft);
			}
			else
			{
				SetDoorState(SideDoorState.OpenRight);
			}
			if (m_breakDoorFX.HasAsset())
			{
				Quaternion impactEffectRotationForDamageInstance = CharacterHitReact.GetImpactEffectRotationForDamageInstance(instance);
				Quaternion quaternion = Quaternion.Euler(0f, 180f, 0f);
				impactEffectRotationForDamageInstance *= quaternion;
				DynamicallySpawnedObject.Spawn(m_breakDoorFX, persistent: false, instance.Position, impactEffectRotationForDamageInstance);
			}
			if (m_doorBreakDamageCollider != null)
			{
				StartCoroutine(DoBreakDamageCollider(instance.DamageSource));
			}
		}
		else
		{
			Vector3 zero = Vector3.zero;
			zero.x += instance.Direction.x;
			zero = zero.normalized * 0.025f;
			Vector3 localPosition = m_doorTransform.localPosition;
			localPosition += zero;
			if (m_activeTween != null)
			{
				DOTween.Kill(m_activeTween, complete: true);
			}
			m_activeTween = m_doorTransform.DOPunchPosition(localPosition, 0.5f, 30).OnComplete(ResetDoorPosition);
			m_doorTakeDamageAudio?.Play(base.transform.position);
		}
	}

	private void ResetDoorPosition()
	{
		m_doorTransform.localPosition = m_doorTransformStartLocalPosition;
	}

	private IEnumerator DoBreakDamageCollider(GameObject source)
	{
		m_doorBreakDamageCollider.Source = source;
		m_doorBreakDamageCollider.gameObject.SetActive(value: true);
		m_doorBreakDamageCollider.SetDamageEnabled(enabled: true);
		yield return new WaitForSeconds(0.5f);
		m_doorBreakDamageCollider.gameObject.SetActive(value: false);
		m_doorBreakDamageCollider.SetDamageEnabled(enabled: false);
	}

	public bool RequiresPersistentData()
	{
		return true;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData != null)
		{
			SetDoorState(m_persistentData.m_doorState, instant: true);
			return;
		}
		m_persistentData = new PersistentData();
		m_persistentDataObject.Data = m_persistentData;
	}
}
