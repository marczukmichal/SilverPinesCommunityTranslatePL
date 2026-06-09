using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Serialization;

[Serializable]
public class DifficultySettings
{
	[Serializable]
	public struct ResourceLevelThresholds
	{
		public int m_badThreshold;

		public int m_direThreshold;
	}

	[FormerlySerializedAs("m_playerDamageTakenScalar")]
	[SerializeField]
	private float m_enemyDamageDealtScalar;

	[FormerlySerializedAs("m_playerDamageDealtScalar")]
	[SerializeField]
	private float m_playerDamageDealtScalar;

	[SerializeField]
	private float m_durabilityLossScalar;

	[SerializeField]
	private ResourceLevelThresholds m_healthResourceThresholds;

	[SerializeField]
	private ResourceLevelThresholds m_ammoResourceThresholds;

	[SerializeField]
	private ResourceLevelThresholds m_meleeResourceThresholds;

	[SerializeField]
	private ResourceLevelThresholds m_generalResourceThresholds;

	[SerializeField]
	private ResourceLevelThresholds m_networthResourceThresholds;

	[Header("Info")]
	[SerializeField]
	private LocalizedString m_difficultyNameString;

	[SerializeField]
	private LocalizedString m_difficultyDescriptionString;

	[SerializeField]
	private Sprite m_difficultySelectSprite;

	public float EnemyDamageDealtScalar => m_enemyDamageDealtScalar;

	public float PlayerDamageDealtScalar => m_playerDamageDealtScalar;

	public float DurabilityLossScalar => m_durabilityLossScalar;

	public LocalizedString DifficultyNameString => m_difficultyNameString;

	public LocalizedString DifficultyDescriptionString => m_difficultyDescriptionString;

	public Sprite DifficultySelectSprite => m_difficultySelectSprite;

	public void DifficultyGUI()
	{
		GUILayout.Label("EnemyDamageScalar: " + m_enemyDamageDealtScalar);
		GUILayout.Label("PlayerDamageDealtScalar: " + m_playerDamageDealtScalar);
		GUILayout.Label("DurabilityLossScalar: " + m_durabilityLossScalar);
	}

	public GameDifficultyResourceLevel GetResourceLevel(int resourceValue, GameDifficultyResourceScoreType type)
	{
		ResourceLevelThresholds resourceLevelThresholds;
		switch (type)
		{
		case GameDifficultyResourceScoreType.General:
			resourceLevelThresholds = m_generalResourceThresholds;
			break;
		case GameDifficultyResourceScoreType.Health:
			resourceLevelThresholds = m_healthResourceThresholds;
			break;
		case GameDifficultyResourceScoreType.Ammo:
			resourceLevelThresholds = m_ammoResourceThresholds;
			break;
		case GameDifficultyResourceScoreType.Melee:
			resourceLevelThresholds = m_meleeResourceThresholds;
			break;
		case GameDifficultyResourceScoreType.NetWorth:
			resourceLevelThresholds = m_networthResourceThresholds;
			break;
		default:
			return GameDifficultyResourceLevel.None;
		}
		if (resourceValue <= resourceLevelThresholds.m_direThreshold)
		{
			return GameDifficultyResourceLevel.Dire;
		}
		if (resourceValue <= resourceLevelThresholds.m_badThreshold)
		{
			return GameDifficultyResourceLevel.Bad;
		}
		return GameDifficultyResourceLevel.Normal;
	}
}
