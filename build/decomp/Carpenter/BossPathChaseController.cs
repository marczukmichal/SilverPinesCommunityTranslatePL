using System;
using System.Collections;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using UnityEngine;

public class BossPathChaseController : BaseCharacterInput
{
	[Serializable]
	public class AttackSettings
	{
		public string m_comment;

		public AIAbility m_ability;

		public float m_minDistance;

		public float m_maxDistance;

		public float m_cooldown;

		public float m_weight = 1f;
	}

	private enum BossMode
	{
		Chase,
		FightStage1,
		FightStage2
	}

	[SerializeField]
	private BossPathChaseNode[] m_nodes;

	[SerializeField]
	private float m_doorCheckDistance;

	[SerializeField]
	private float m_attackDistance;

	[SerializeField]
	private float m_sprintDistance;

	[SerializeField]
	private int m_minimumHealthUntilEndFight = 200;

	[Header("Fight Stage 1")]
	[SerializeField]
	private float m_stage1HealthEndValue;

	[Header("Attacks")]
	[SerializeField]
	private AttackSettings[] m_attackSettings;

	[SerializeField]
	private AttackSettings m_stage1EndAttack;

	private AttackSettings m_lastUsedAttack;

	private int m_nodeIndex;

	private CharacterDirection m_direction;

	private CharacterMovement m_movement;

	private CharacterHealth m_health;

	private PlayMakerFSM m_fsm;

	private bool m_isPaused;

	private float m_attackCooldown;

	private BossMode m_mode;

	private AIAbility m_activeAIAbility;

	private Vector2 m_movementInput;

	public override Vector2 MovementInput => m_movementInput;

	public override bool IsSprinting => GetDistanceFromPlayer() > m_sprintDistance;

	public override bool IsDoingAIAbility(AIAbility attack)
	{
		return m_activeAIAbility == attack;
	}

	private void Start()
	{
		m_direction = GetComponent<CharacterDirection>();
		m_movement = GetComponent<CharacterMovement>();
		m_fsm = GetComponent<PlayMakerFSM>();
		m_health = GetComponent<CharacterHealth>();
		m_health.SetMinimumHealth(m_minimumHealthUntilEndFight);
		m_movementInput = Vector2.zero;
		m_nodeIndex = 0;
		m_mode = BossMode.Chase;
	}

	private BossPathChaseNode GetNextNode()
	{
		if (m_nodeIndex < m_nodes.Length)
		{
			return m_nodes[m_nodeIndex];
		}
		return null;
	}

	private void Update()
	{
		m_movementInput = Vector2.zero;
		m_activeAIAbility = AIAbility.None;
		if (!m_isPaused)
		{
			switch (m_mode)
			{
			case BossMode.Chase:
				UpdateChase();
				break;
			case BossMode.FightStage1:
				UpdateFight();
				break;
			case BossMode.FightStage2:
				UpdateFight();
				break;
			}
		}
	}

	private void UpdateChase()
	{
		if (IsFacingDoor(m_doorCheckDistance) || ShouldAttackPlayer())
		{
			m_activeAIAbility = AIAbility.BasicAttack;
			return;
		}
		BossPathChaseNode nextNode = GetNextNode();
		Vector2 vector = Vector2.zero;
		if (nextNode != null)
		{
			vector = nextNode.Position - base.transform.position;
			if (vector.x <= 0f)
			{
				if (nextNode.Type == BossPathChaseNode.ChaseNodeType.Climb)
				{
					Vector3 position = base.transform.position;
					Vector3 position2 = nextNode.GetClimbPosition();
					position2.z = position.z;
					position2.y += 0.1f;
					base.transform.position = position2;
					m_fsm.SendEvent("Ledge/ClimbUp");
				}
				else if (nextNode.Type == BossPathChaseNode.ChaseNodeType.StartFightStage1)
				{
					m_mode = BossMode.FightStage1;
					m_attackCooldown = 0f;
				}
				else if (nextNode.Type == BossPathChaseNode.ChaseNodeType.StartFightStage2)
				{
					m_mode = BossMode.FightStage2;
					m_attackCooldown = 0f;
					m_health.SetMinimumHealth(0);
				}
				nextNode.ReachedNodeTrigger();
				m_nodeIndex++;
				nextNode = GetNextNode();
				if (nextNode != null)
				{
					vector = nextNode.Position - base.transform.position;
				}
			}
		}
		if (nextNode != null && vector.x > 0f)
		{
			m_movementInput = new Vector2(1f, 0f);
		}
	}

	private void UpdateFight()
	{
		m_attackCooldown -= Time.deltaTime;
		if (m_attackCooldown <= 0f)
		{
			AttackSettings attackSettings = SelectAttack();
			if (attackSettings != null)
			{
				StartCoroutine(DoAttack(attackSettings));
			}
		}
	}

	public void EndStage1()
	{
		m_mode = BossMode.Chase;
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (item != null)
		{
			FsmBool fsmBool = item.GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("ForceHeavyLanding");
			if (fsmBool != null)
			{
				fsmBool.Value = true;
			}
		}
	}

	private IEnumerator DoAttack(AttackSettings attack)
	{
		m_lastUsedAttack = attack;
		m_attackCooldown = attack.m_cooldown;
		m_activeAIAbility = attack.m_ability;
		yield return new WaitForSeconds(0.5f);
		m_activeAIAbility = AIAbility.None;
	}

	private float GetAttackWeight(AttackSettings settings)
	{
		float num = settings.m_weight;
		if (m_lastUsedAttack == settings)
		{
			num *= 0.25f;
		}
		return num;
	}

	private AttackSettings SelectAttack()
	{
		if (m_mode == BossMode.FightStage1 && (float)m_health.Health <= m_stage1HealthEndValue)
		{
			return m_stage1EndAttack;
		}
		float distanceFromPlayer = GetDistanceFromPlayer();
		List<AttackSettings> list = new List<AttackSettings>();
		float num = 0f;
		AttackSettings[] attackSettings = m_attackSettings;
		foreach (AttackSettings attackSettings2 in attackSettings)
		{
			if (distanceFromPlayer >= attackSettings2.m_minDistance && distanceFromPlayer < attackSettings2.m_maxDistance)
			{
				list.Add(attackSettings2);
				num += GetAttackWeight(attackSettings2);
			}
		}
		if (list.Count > 0)
		{
			float num2 = UnityEngine.Random.Range(0f, num);
			float num3 = 0f;
			foreach (AttackSettings item in list)
			{
				num3 += GetAttackWeight(item);
				if (num3 >= num2)
				{
					return item;
				}
			}
		}
		return null;
	}

	private float GetDistanceFromPlayer()
	{
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (item != null)
		{
			return Mathf.Abs(base.transform.position.x - item.transform.position.x);
		}
		return float.MaxValue;
	}

	private bool ShouldAttackPlayer()
	{
		BossPathChaseNode nextNode = GetNextNode();
		if (nextNode != null && nextNode.Type == BossPathChaseNode.ChaseNodeType.Climb)
		{
			return false;
		}
		if (GetDistanceFromPlayer() < m_attackDistance)
		{
			return true;
		}
		return false;
	}

	private bool IsFacingDoor(float distance)
	{
		Vector2 origin = base.transform.position;
		origin.y += 0.5f;
		Vector2 direction = m_direction.GetForwardVector();
		RaycastHit2D[] array = Physics2D.RaycastAll(origin, direction, distance);
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit2D raycastHit2D = array[i];
			if (raycastHit2D.collider != null)
			{
				NewSideDoor componentInParent = raycastHit2D.collider.GetComponentInParent<NewSideDoor>();
				if (componentInParent != null && componentInParent.IsClosed())
				{
					return true;
				}
				SideDoor componentInParent2 = raycastHit2D.collider.GetComponentInParent<SideDoor>();
				if (componentInParent2 != null && componentInParent2.IsClosed())
				{
					return true;
				}
			}
		}
		return false;
	}

	public void OnPlayerHit()
	{
		StartCoroutine(PauseBehaviour(2f));
	}

	private IEnumerator PauseBehaviour(float duration)
	{
		m_isPaused = true;
		yield return new WaitForSeconds(duration);
		m_isPaused = false;
	}

	private void OnDrawGizmos()
	{
		if (m_nodes == null || m_nodes.Length == 0)
		{
			return;
		}
		Gizmos.color = Color.red;
		BossPathChaseNode nextNode = GetNextNode();
		if (nextNode != null)
		{
			GizmoExtensions.DrawToFromArrow(base.transform.position, nextNode.Position);
		}
		Gizmos.color = Color.yellow;
		for (int i = 0; i < m_nodes.Length; i++)
		{
			if (!(m_nodes[i] == null) && i + 1 < m_nodes.Length)
			{
				GizmoExtensions.DrawToFromArrow(m_nodes[i].Position, m_nodes[i + 1].Position);
			}
		}
	}
}
