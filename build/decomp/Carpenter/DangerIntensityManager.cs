using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DangerIntensityManager : MonoBehaviour
{
	[SerializeField]
	private FloatVariable m_enemyPresence;

	[SerializeField]
	private DangerIntensityVariable m_dangerIntensityVariable;

	[Header("Suddeny Enemy Change Check")]
	[SerializeField]
	private float m_enemyPresenceCheckRate = 0.1f;

	[SerializeField]
	private int m_enemyPresenceValuesToStore = 30;

	[SerializeField]
	private float m_suddenChangeThreshold = 0.5f;

	[SerializeField]
	private float m_suddenChangeRepeatBlockTime = 3f;

	[SerializeField]
	private float m_recentlyInCombatTime = 3f;

	private Queue<float> m_recentEnemyPresenceValues;

	private float m_recentEnemyPresenceValuesTotal;

	private float m_enemyPresenceCheckTimer;

	private float m_lastSuddenChangeTime;

	private float m_lastTimeNotInCombat;

	private DangerIntensity m_dangerIntensity;

	[DebugCommand("danger_intensity_debug", "Show danger intensity debug", "danger_intensity_debug <true/false>", typeof(bool), false)]
	private static bool s_showDangerIntensityDebug;

	private bool WasRecentlyNotInCombat => Time.time - m_lastTimeNotInCombat < m_recentlyInCombatTime;

	public DangerIntensity DangerIntensity
	{
		get
		{
			return m_dangerIntensity;
		}
		private set
		{
			if (m_dangerIntensity != value)
			{
				m_dangerIntensity = value;
				m_dangerIntensityVariable.SetEnumValue(value);
			}
		}
	}

	private bool IsInCombat()
	{
		foreach (AISenses item in GlobalReferences.Instance.Sets.Generic.ActiveAISensesSet.Items)
		{
			CharacterIdentifier component = item.GetComponent<CharacterIdentifier>();
			if (!(component == null) && component.Faction == CharacterIdentifier.CharacterFaction.Monsters)
			{
				EnemyPresence component2 = item.GetComponent<EnemyPresence>();
				if ((!(component2 != null) || component2.IsKnown()) && item.CurrentTargetState == AISenses.TargetState.Detected)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool IsNearEnemy()
	{
		return m_enemyPresence.Value > 0f;
	}

	private void Start()
	{
		m_recentEnemyPresenceValues = new Queue<float>();
		m_enemyPresence.Value = 0f;
	}

	private void OnEnable()
	{
		m_dangerIntensityVariable.SetEnumValue(m_dangerIntensity);
		GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Register(OnActivePlayerChanged);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Unregister(OnActivePlayerChanged);
	}

	private void OnActivePlayerChanged(GameObject player)
	{
		if (player != null)
		{
			CharacterHealth component = player.GetComponent<CharacterHealth>();
			if (component != null)
			{
				component.OnTakenDamage = (UnityAction<int>)Delegate.Combine(component.OnTakenDamage, new UnityAction<int>(OnPlayerTakeDamage));
			}
		}
	}

	private void OnPlayerTakeDamage(int damage)
	{
	}

	private float GetEnemyPresenceRecentAverage()
	{
		return m_recentEnemyPresenceValuesTotal / (float)m_recentEnemyPresenceValues.Count;
	}

	private void Update()
	{
		m_enemyPresenceCheckTimer -= Time.deltaTime;
		if (m_enemyPresenceCheckTimer <= 0f)
		{
			m_enemyPresenceCheckTimer = m_enemyPresenceCheckRate;
			m_recentEnemyPresenceValues.Enqueue(m_enemyPresence.Value);
			m_recentEnemyPresenceValuesTotal += m_enemyPresence.Value;
			if (m_recentEnemyPresenceValues.Count > m_enemyPresenceValuesToStore)
			{
				float num = m_recentEnemyPresenceValues.Dequeue();
				m_recentEnemyPresenceValuesTotal -= num;
			}
			if (Time.time - m_lastSuddenChangeTime >= m_suddenChangeRepeatBlockTime && WasRecentlyNotInCombat && m_enemyPresence.Value - GetEnemyPresenceRecentAverage() > m_suddenChangeThreshold)
			{
				m_lastSuddenChangeTime = Time.time;
				GlobalReferences.Instance.EventChannels.Gameplay.SuddenEnemyAppeared.Raise();
			}
		}
		DangerIntensity dangerIntensity = DangerIntensity.None;
		if (IsInCombat())
		{
			dangerIntensity = DangerIntensity.InCombat;
		}
		else if (IsNearEnemy())
		{
			dangerIntensity = DangerIntensity.NearbyEnemy;
		}
		if (dangerIntensity != DangerIntensity.InCombat)
		{
			m_lastTimeNotInCombat = Time.time;
		}
		DangerIntensity = dangerIntensity;
	}

	private void OnGUI()
	{
		if (s_showDangerIntensityDebug)
		{
			GUILayout.Label("Enemy Presnce: " + m_enemyPresence.Value);
			GUILayout.Label("Danger Intensity: " + m_dangerIntensity);
			GUILayout.Label("Was Recently Not In Combat: " + WasRecentlyNotInCombat);
			GUILayout.Label("Recent Enemy Presence Danger Intensity: " + GetEnemyPresenceRecentAverage());
		}
	}
}
