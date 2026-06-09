using System;
using UnityEngine;

[Serializable]
public struct PersistentDataStats
{
	public int m_damageTaken;

	public int m_kills;

	public int m_killsWithGuns;

	public int m_killsWithMelee;

	public int m_healingItemsUsed;

	public int m_foodAndDrinkItemsUsed;

	public int m_totalMoneySpent;

	public float m_distanceTravelled;

	public void Clear()
	{
		m_damageTaken = 0;
		m_kills = 0;
		m_killsWithGuns = 0;
		m_killsWithMelee = 0;
		m_healingItemsUsed = 0;
		m_foodAndDrinkItemsUsed = 0;
		m_totalMoneySpent = 0;
		m_distanceTravelled = 0f;
	}

	public void DrawDebugGUI()
	{
		GUILayout.Label("Stats GUI");
		GUILayout.Label("Damage Taken: " + m_damageTaken);
		GUILayout.Label("Total Kills: " + m_kills);
		GUILayout.Label("Kills with Guns: " + m_killsWithGuns);
		GUILayout.Label("Kills with Melee: " + m_killsWithMelee);
		GUILayout.Label("Healing Items Used: " + m_healingItemsUsed);
		GUILayout.Label("Food and Drink Items Used: " + m_foodAndDrinkItemsUsed);
		GUILayout.Label("Total Money Spent: " + m_totalMoneySpent);
		GUILayout.Label("Distance Travelled: " + m_distanceTravelled);
	}
}
