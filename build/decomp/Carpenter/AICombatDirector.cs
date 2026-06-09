using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AICombatDirector : MonoBehaviour
{
	public enum Side
	{
		Left,
		Right
	}

	[Serializable]
	public class CombatSpot
	{
		public float m_distance;

		public Side m_side;

		public IAICombatPawn m_occupant;

		public Vector2 m_calculatedPosition;

		public bool m_isValid;
	}

	[SerializeField]
	private float m_combatSpotBaseDistance;

	[SerializeField]
	private float m_combatSpotDistance;

	[SerializeField]
	private int m_combatSpotCount;

	private CombatSpot[] m_combatSpots;

	private float m_activeAttackCooldown;

	private bool m_attackCooldownBlocked;

	private IAICombatPawn m_lastAttackRequestPawn;

	private LegacyCharacterMovement m_characterMovement;

	[SerializeField]
	private float m_attackCooldownTime = 1f;

	[DebugCommand("combat_debug", "AI Combat Director debug gui", "combat_debug <true/false>", typeof(bool), false)]
	private static bool s_combat_director_debug = false;

	private static readonly float s_allowedAICombatPawnOverlap = 0.5f;

	private void Awake()
	{
		m_combatSpots = new CombatSpot[m_combatSpotCount * 2];
		for (int i = 0; i < m_combatSpotCount; i++)
		{
			float distance = m_combatSpotBaseDistance + m_combatSpotDistance * (float)i;
			m_combatSpots[i * 2] = new CombatSpot
			{
				m_distance = distance,
				m_side = Side.Right
			};
			m_combatSpots[i * 2 + 1] = new CombatSpot
			{
				m_distance = distance,
				m_side = Side.Left
			};
		}
		m_characterMovement = GetComponent<LegacyCharacterMovement>();
		CharacterSyncGrab component = GetComponent<CharacterSyncGrab>();
		if (component != null)
		{
			component.m_onGrabbed = (UnityAction<bool>)Delegate.Combine(component.m_onGrabbed, new UnityAction<bool>(OnSyncGrabbed));
		}
	}

	private void OnSyncGrabbed(bool isGrabbed)
	{
		m_attackCooldownBlocked = isGrabbed;
	}

	public Vector3 GetCombatSpotPosition(CombatSpot combatSpot)
	{
		return combatSpot.m_calculatedPosition;
	}

	private bool BlockedByOtherCharacterOverlap(IAICombatPawn combatPawn)
	{
		Vector2 vector = combatPawn.GetNavigationPosition();
		AICombatPawnType aICombatPawnType = combatPawn.GetAICombatPawnType();
		Vector2 a = base.transform.position;
		float num = Vector2.Distance(a, vector);
		for (int i = 0; i < m_combatSpots.Length; i++)
		{
			if (m_combatSpots[i].m_occupant == null)
			{
				continue;
			}
			IAICombatPawn occupant = m_combatSpots[i].m_occupant;
			if (occupant != combatPawn && occupant.GetAICombatPawnType() == aICombatPawnType)
			{
				Vector2 b = occupant.GetNavigationPosition();
				if (Vector2.Distance(vector, b) < s_allowedAICombatPawnOverlap && Vector2.Distance(a, b) < num)
				{
					return true;
				}
			}
		}
		return false;
	}

	public Vector3 GetGoalPosition(IAICombatPawn combatPawn, int startCombatSpotIndex)
	{
		Unregister(combatPawn);
		int num = -1;
		float num2 = float.MaxValue;
		Vector3 navigationPosition = combatPawn.GetNavigationPosition();
		if (startCombatSpotIndex != -1 && !BlockedByOtherCharacterOverlap(combatPawn))
		{
			for (int i = startCombatSpotIndex * 2; i < m_combatSpots.Length; i += 2)
			{
				for (int j = 0; j < 2; j++)
				{
					int num3 = i + j;
					if ((m_combatSpots[num3].m_occupant == null || m_combatSpots[num3].m_occupant == combatPawn) && m_combatSpots[num3].m_isValid)
					{
						float num4 = Vector2.Distance(navigationPosition, GetCombatSpotPosition(m_combatSpots[num3]));
						if (num4 < num2)
						{
							num2 = num4;
							num = num3;
						}
					}
				}
				if (num != -1)
				{
					break;
				}
			}
			if (num != -1)
			{
				m_combatSpots[num].m_occupant = combatPawn;
				return GetCombatSpotPosition(m_combatSpots[num]);
			}
		}
		return navigationPosition;
	}

	public void Unregister(IAICombatPawn combatPawn)
	{
		CombatSpot[] combatSpots = m_combatSpots;
		foreach (CombatSpot combatSpot in combatSpots)
		{
			if (combatSpot.m_occupant == combatPawn)
			{
				combatSpot.m_occupant = null;
			}
		}
	}

	private void Update()
	{
		UpdateCombatSpotPositions();
		if (m_attackCooldownBlocked)
		{
			m_activeAttackCooldown = m_attackCooldownTime;
		}
		else if (m_activeAttackCooldown > 0f)
		{
			m_activeAttackCooldown -= Time.deltaTime;
		}
		else
		{
			CheckForAIAttacks();
		}
		if (m_lastAttackRequestPawn != null)
		{
			AttackRequestResult attackRequestResult = m_lastAttackRequestPawn.GetAttackRequestResult();
			if (attackRequestResult == AttackRequestResult.Failed)
			{
				m_activeAttackCooldown = 0f;
			}
			if (attackRequestResult != 0)
			{
				m_lastAttackRequestPawn = null;
			}
		}
	}

	private void UpdateCombatSpotPositions()
	{
		CombatSpot[] combatSpots = m_combatSpots;
		foreach (CombatSpot combatSpot in combatSpots)
		{
			Vector2 vector = base.transform.position;
			float num = ((combatSpot.m_side == Side.Right) ? combatSpot.m_distance : (0f - combatSpot.m_distance));
			vector.x += num;
			bool flag = false;
			bool flag2 = false;
			Vector2 origin = vector;
			origin.y += 1f;
			RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, Vector2.down, 2f, GameLayers.EnvironmentMask);
			if (raycastHit2D.collider != null)
			{
				combatSpot.m_calculatedPosition = raycastHit2D.point;
				flag = true;
			}
			else
			{
				combatSpot.m_calculatedPosition = vector;
			}
			Vector2 size = new Vector2(0.5f, 1f);
			Vector2 calculatedPosition = combatSpot.m_calculatedPosition;
			calculatedPosition.y += size.y * 0.5f + 0.1f;
			if (Physics2D.OverlapBox(calculatedPosition, size, 0f, GameLayers.EnvironmentMask) != null)
			{
				flag2 = true;
			}
			combatSpot.m_isValid = flag && !flag2;
		}
	}

	private void CheckForAIAttacks()
	{
		List<IAICombatPawn> list = new List<IAICombatPawn>();
		CombatSpot[] combatSpots = m_combatSpots;
		foreach (CombatSpot combatSpot in combatSpots)
		{
			if (combatSpot.m_occupant != null && combatSpot.m_occupant.IsRequestingAttack())
			{
				list.Add(combatSpot.m_occupant);
			}
		}
		while (list.Count > 0)
		{
			int index = UnityEngine.Random.Range(0, list.Count);
			if (list[index].PermitAttack())
			{
				m_lastAttackRequestPawn = list[index];
				m_activeAttackCooldown = m_attackCooldownTime;
				break;
			}
			list.Remove(list[index]);
		}
	}

	public void OnDrawGizmos()
	{
		if (m_combatSpots == null)
		{
			return;
		}
		Vector3 size = new Vector3(0.5f, 0.25f, 0.1f);
		CombatSpot[] combatSpots = m_combatSpots;
		foreach (CombatSpot combatSpot in combatSpots)
		{
			Vector3 combatSpotPosition = GetCombatSpotPosition(combatSpot);
			if (combatSpot.m_occupant != null)
			{
				Gizmos.color = new Color(0.7f, 0.1f, 1f);
				Gizmos.DrawLine(combatSpotPosition, combatSpot.m_occupant.GetNavigationPosition());
			}
			else if (!combatSpot.m_isValid)
			{
				Gizmos.color = Color.red;
			}
			else
			{
				Gizmos.color = Color.green;
			}
			Gizmos.DrawWireCube(combatSpotPosition, size);
		}
		Gizmos.color = Color.white;
	}

	private void OnGUI()
	{
		if (s_combat_director_debug)
		{
			GUILayout.Label("m_activeAttackCooldown: " + m_activeAttackCooldown);
			GUILayout.Label("m_attackCooldownBlocked: " + m_attackCooldownBlocked);
		}
	}
}
