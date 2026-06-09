using Steamworks;
using UnityEngine;

public class SteamAchievements : IPlatformAchievements
{
	private CGameID m_GameID;

	protected Callback<UserStatsReceived_t> m_UserStatsReceived;

	protected Callback<UserStatsStored_t> m_UserStatsStored;

	protected Callback<UserAchievementStored_t> m_UserAchievementStored;

	private bool m_bRequestedStats;

	private bool m_bStatsValid;

	private bool m_bStoreStats;

	public void Initialise()
	{
		if (SteamManager.Initialized)
		{
			m_GameID = new CGameID(SteamUtils.GetAppID());
			m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
			m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
			m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);
			m_bRequestedStats = false;
			m_bStatsValid = false;
		}
	}

	public void Destroy()
	{
		m_UserStatsReceived?.Dispose();
		m_UserStatsStored?.Dispose();
		m_UserAchievementStored?.Dispose();
	}

	public void Update()
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		if (!m_bRequestedStats)
		{
			if (!SteamManager.Initialized)
			{
				m_bRequestedStats = true;
				return;
			}
			bool flag = (m_bRequestedStats = SteamUserStats.RequestCurrentStats());
		}
		if (m_bStatsValid && m_bStoreStats)
		{
			bool flag2 = SteamUserStats.StoreStats();
			m_bStoreStats = !flag2;
		}
	}

	public void UnlockAchievement(AchievementDefinition achievement)
	{
		SteamUserStats.SetAchievement(achievement.AchievementID);
		m_bStoreStats = true;
	}

	private void OnUserStatsReceived(UserStatsReceived_t pCallback)
	{
		if (SteamManager.Initialized && (ulong)m_GameID == pCallback.m_nGameID)
		{
			if (EResult.k_EResultOK == pCallback.m_eResult)
			{
				Debug.Log("Received stats and achievements from Steam\n");
				m_bStatsValid = true;
			}
			else
			{
				Debug.Log("RequestStats - failed, " + pCallback.m_eResult);
			}
		}
	}

	private void OnUserStatsStored(UserStatsStored_t pCallback)
	{
		if ((ulong)m_GameID == pCallback.m_nGameID)
		{
			if (EResult.k_EResultOK == pCallback.m_eResult)
			{
				Debug.Log("StoreStats - success");
			}
			else if (EResult.k_EResultInvalidParam == pCallback.m_eResult)
			{
				Debug.Log("StoreStats - some failed to validate");
				UserStatsReceived_t pCallback2 = default(UserStatsReceived_t);
				pCallback2.m_eResult = EResult.k_EResultOK;
				pCallback2.m_nGameID = (ulong)m_GameID;
				OnUserStatsReceived(pCallback2);
			}
			else
			{
				Debug.Log("StoreStats - failed, " + pCallback.m_eResult);
			}
		}
	}

	private void OnAchievementStored(UserAchievementStored_t pCallback)
	{
		if ((ulong)m_GameID == pCallback.m_nGameID)
		{
			if (pCallback.m_nMaxProgress == 0)
			{
				Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
				return;
			}
			Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
		}
	}

	public void ResetAchievements()
	{
		SteamUserStats.ResetAllStats(bAchievementsToo: true);
		SteamUserStats.RequestCurrentStats();
	}
}
