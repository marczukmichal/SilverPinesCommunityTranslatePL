using System;
using System.Collections.Generic;
using UnityEngine;

public class BirdFlockController : MonoBehaviour, IPersistentComponent
{
	private enum FlockState
	{
		Waiting,
		HasTarget,
		Dissipate
	}

	[Serializable]
	private class PersistentData
	{
		public bool m_hasDissipated;
	}

	[SerializeField]
	private AISenses m_senses;

	[SerializeField]
	private Transform m_targetTransform;

	[SerializeField]
	private Vector2 m_attackTime;

	[SerializeField]
	private int m_birdCountForDissipate = 3;

	[SerializeField]
	private float m_targetLookahead = 0.5f;

	[SerializeField]
	private float m_targetOffsetHeightNormal = 1f;

	[SerializeField]
	private float m_targetHeightRemovalWhenCrouched = 0.5f;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_activeAudio;

	private CharacterBirdMovement[] m_birds;

	private float m_activeAttackTimer;

	private FlockState m_flockState;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	private FlockState State
	{
		get
		{
			return m_flockState;
		}
		set
		{
			if (m_flockState == value)
			{
				return;
			}
			m_flockState = value;
			switch (value)
			{
			case FlockState.HasTarget:
				if (GetAliveBirdCount() >= 2)
				{
					m_activeAudio.Play(base.transform.position);
				}
				SetAttackTimer();
				SendEventToAllBirds("EnableFlock");
				break;
			case FlockState.Dissipate:
				DissipateBirds();
				if (m_persistentData != null)
				{
					m_persistentData.m_hasDissipated = true;
				}
				break;
			}
		}
	}

	private void Awake()
	{
		m_birds = GetComponentsInChildren<CharacterBirdMovement>(includeInactive: true);
	}

	private void SetAttackTimer()
	{
		m_activeAttackTimer = m_attackTime.GetRandom();
	}

	private void Update()
	{
		int aliveBirdCount = GetAliveBirdCount();
		if (aliveBirdCount == 0)
		{
			base.enabled = false;
			return;
		}
		switch (m_flockState)
		{
		case FlockState.Waiting:
			if (m_senses != null && m_senses.CurrentTarget != null)
			{
				State = FlockState.HasTarget;
			}
			break;
		case FlockState.HasTarget:
		{
			if (aliveBirdCount <= m_birdCountForDissipate)
			{
				State = FlockState.Dissipate;
				break;
			}
			if (m_senses.CurrentTarget == null)
			{
				State = FlockState.Waiting;
				break;
			}
			m_activeAttackTimer -= Time.deltaTime;
			if (m_activeAttackTimer <= 0f && !AreAnyBirdsAttacking())
			{
				SendAttackToABird();
				SetAttackTimer();
			}
			Vector3 position = m_senses.CurrentTarget.transform.position;
			position.y += m_targetOffsetHeightNormal;
			CharacterMovement componentInParent = m_senses.CurrentTarget.GetComponentInParent<CharacterMovement>();
			if ((object)componentInParent != null)
			{
				position.x += componentInParent.PreviousVelocity.x * m_targetLookahead;
			}
			CharacterStance componentInParent2 = m_senses.CurrentTarget.GetComponentInParent<CharacterStance>();
			if ((object)componentInParent2 != null && componentInParent2.CurrentStance == CharacterStance.Stance.Crouching)
			{
				position.y -= m_targetHeightRemovalWhenCrouched;
			}
			m_targetTransform.transform.position = position;
			break;
		}
		}
	}

	private void SendEventToAllBirds(string eventName)
	{
		CharacterBirdMovement[] birds = m_birds;
		foreach (CharacterBirdMovement characterBirdMovement in birds)
		{
			if (!characterBirdMovement.GetComponent<CharacterHealth>().IsDead)
			{
				characterBirdMovement.GetComponent<PlayMakerFSM>().SendEvent(eventName);
			}
		}
	}

	private bool AreAnyBirdsAttacking()
	{
		CharacterBirdMovement[] birds = m_birds;
		foreach (CharacterBirdMovement characterBirdMovement in birds)
		{
			if (!(characterBirdMovement == null) && !characterBirdMovement.GetComponent<CharacterHealth>().IsDead && characterBirdMovement.IsAttacking())
			{
				return true;
			}
		}
		return false;
	}

	private void SendAttackToABird()
	{
		List<CharacterBirdMovement> list = new List<CharacterBirdMovement>();
		CharacterBirdMovement[] birds = m_birds;
		foreach (CharacterBirdMovement characterBirdMovement in birds)
		{
			if (!(characterBirdMovement == null) && !characterBirdMovement.GetComponent<CharacterHealth>().IsDead && !characterBirdMovement.IsAttacking())
			{
				list.Add(characterBirdMovement);
			}
		}
		if (list.Count > 0)
		{
			list[UnityEngine.Random.Range(0, list.Count)].SetAttackFlag();
		}
	}

	private void DissipateBirds()
	{
		CharacterBirdMovement[] birds = m_birds;
		foreach (CharacterBirdMovement characterBirdMovement in birds)
		{
			if (!(characterBirdMovement == null) && !characterBirdMovement.GetComponent<CharacterHealth>().IsDead)
			{
				characterBirdMovement.SetDissipate();
			}
		}
	}

	private int GetAliveBirdCount()
	{
		List<CharacterBirdMovement> list = new List<CharacterBirdMovement>();
		CharacterBirdMovement[] birds = m_birds;
		foreach (CharacterBirdMovement characterBirdMovement in birds)
		{
			if (!(characterBirdMovement == null) && !characterBirdMovement.GetComponent<CharacterHealth>().IsDead)
			{
				list.Add(characterBirdMovement);
			}
		}
		return list.Count;
	}

	private void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawSphere(m_targetTransform.position, 0.5f);
			Gizmos.color = Color.white;
		}
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData != null)
		{
			if (m_persistentData.m_hasDissipated)
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else
		{
			m_persistentData = new PersistentData();
			m_persistentDataObject.Data = m_persistentData;
		}
	}

	public bool RequiresPersistentData()
	{
		return true;
	}
}
