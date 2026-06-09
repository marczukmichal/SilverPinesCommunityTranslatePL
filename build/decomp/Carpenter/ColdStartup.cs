using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class ColdStartup : MonoBehaviour
{
	[Serializable]
	private class StartingItem
	{
		public ItemDefinition m_itemType;

		public int m_amount;

		public bool m_startEquipped;
	}

	[SerializeField]
	private PersistentDataStore m_persistentDataStore;

	[Header("Character Inventory")]
	[SerializeField]
	private List<StartingItem> m_startingItems;

	[SerializeField]
	private int m_startingMoney;

	[Header("Stash")]
	[SerializeField]
	private Inventory m_stashInventory;

	[SerializeField]
	private List<StartingItem> m_stashItems;

	[Header("Progression Variables")]
	[SerializeField]
	private List<ProgressionVariable> m_variablesToSet;

	[Header("Event Channels")]
	[SerializeField]
	private VoidGameEventChannel m_coldStartupSetupScene;

	[Header("Time Of Day")]
	[SerializeField]
	private TimeOfDay m_timeOfDay;

	[SerializeField]
	private TimeOfDay.TimePeriod m_startingTimeOfDay;

	[Header("Weather")]
	[SerializeField]
	private Weather m_activeWeather;

	[SerializeField]
	private WeatherSettings m_startingWeatherSettings;

	[Header("Phone")]
	[SerializeField]
	private PhoneMemory m_phoneMemory;

	[SerializeField]
	private string[] m_phoneIdsToAdd;

	[Header("Upgrades")]
	[SerializeField]
	private int m_artifactSlots;

	[SerializeField]
	private int m_staminaUpgrades;

	[SerializeField]
	private StartupStep[] m_preServicesSteupSteps;

	[SerializeField]
	private StartupStep[] m_preStartGameSteps;

	private static bool s_coldStartupOngoing;

	private static bool s_doneColdStartup;

	[UsedImplicitly]
	public bool IsColdStartupOngoing => s_coldStartupOngoing;

	public static void ClearStartupFlag()
	{
		s_doneColdStartup = false;
	}

	private void Awake()
	{
		if (!Startup.DoneRegularStartup && !s_doneColdStartup)
		{
			s_doneColdStartup = true;
			StartCoroutine(PerformColdStartup());
		}
	}

	public void PerformColdGameStartup()
	{
		Debug.Log("Performing Cold Startup... ");
		if ((bool)m_persistentDataStore)
		{
			m_persistentDataStore.ClearDataStore();
		}
		PrePerformColdGameStartup();
		PostPerformanceColdGameStartup();
	}

	private void PrePerformColdGameStartup()
	{
		if ((bool)GlobalReferences.Instance.MainInventory)
		{
			GlobalReferences.Instance.MainInventory.Clear();
			int amountRemaining;
			foreach (StartingItem startingItem in m_startingItems)
			{
				if (startingItem == null || startingItem.m_itemType == null || startingItem.m_amount == 0)
				{
					continue;
				}
				ItemInstance itemInstance = GlobalReferences.Instance.MainInventory.AddItemToInventory(startingItem.m_itemType, startingItem.m_amount, new Vector2Int(-1, -1), rotated: false, autoEquip: false, out amountRemaining, autoAddShortcut: true);
				if (itemInstance == null)
				{
					Debug.LogError("Inventory is full - can't add any more items from ColdStartup!");
					continue;
				}
				if (startingItem.m_startEquipped)
				{
					if (itemInstance.ItemDefinition is ProjectileWeaponItemDefinition)
					{
						GlobalReferences.Instance.MainInventory.EquipRangedItem(itemInstance, toggle: false);
					}
					else if (itemInstance.ItemDefinition is MeleeWeaponItemDefinition)
					{
						GlobalReferences.Instance.MainInventory.EquipMeleeItem(itemInstance, toggle: false);
					}
					else if (itemInstance.ItemDefinition is ArtifactItemDefinition)
					{
						GlobalReferences.Instance.MainInventory.EquipArtifactItem(itemInstance);
					}
					else if (itemInstance.CanBeActivated())
					{
						itemInstance.Activated = true;
					}
				}
				if (itemInstance is ArtifactItemInstance artifactItemInstance)
				{
					artifactItemInstance.PowerUp();
				}
			}
			GlobalReferences.Instance.MainInventory.AddMoney(m_startingMoney);
			foreach (StartingItem stashItem in m_stashItems)
			{
				if (stashItem != null && !(stashItem.m_itemType == null) && stashItem.m_amount != 0)
				{
					m_stashInventory.AddItemToInventory(stashItem.m_itemType, stashItem.m_amount, new Vector2Int(-1, -1), rotated: false, autoEquip: false, out amountRemaining);
				}
			}
		}
		if ((bool)m_activeWeather)
		{
			m_activeWeather.SetActiveWeatherSettings(m_startingWeatherSettings, forceInstant: true);
		}
		if ((bool)m_phoneMemory)
		{
			string[] phoneIdsToAdd = m_phoneIdsToAdd;
			foreach (string phoneID in phoneIdsToAdd)
			{
				m_phoneMemory.AddPhoneNumber(phoneID);
			}
		}
		GlobalReferences.Instance.EventChannels.Inventory.SetMaxNumberOfActiveArtifacts.Raise(m_artifactSlots);
		foreach (ProgressionVariable item in m_variablesToSet)
		{
			if (item == null)
			{
				Debug.LogError("Variable to set is null. skipping...");
			}
			else
			{
				item.SetValue(value: true);
			}
		}
		GlobalReferences.Instance.GameState.SetStateFlag(GameState.GameStateFlag.GameActive);
		GlobalReferences.Instance.Variables.Generic.MaxStaminaUpgradeCount.Value = m_staminaUpgrades;
		if (Application.isEditor)
		{
			LevelManager.LoadDebugConsole();
		}
		LevelManager.LoadUtilityScenes();
	}

	public void PostPerformanceColdGameStartup()
	{
		StartCoroutine(SetupScene());
	}

	private IEnumerator PerformColdStartup()
	{
		Debug.Log("Performing Cold Startup... ");
		s_coldStartupOngoing = true;
		if ((bool)m_persistentDataStore)
		{
			m_persistentDataStore.ClearDataStore();
		}
		yield return InitServices();
		PrePerformColdGameStartup();
		yield return InitPreStart();
		PostPerformanceColdGameStartup();
	}

	private IEnumerator SetupScene()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		m_coldStartupSetupScene.Raise();
		if (m_timeOfDay != null)
		{
			m_timeOfDay.SetTimeOfDay(m_startingTimeOfDay);
		}
		s_coldStartupOngoing = false;
		Debug.Log("Completed Cold Startup.");
	}

	private IEnumerator InitServices()
	{
		yield return InitPcPlatform();
		StartupStep[] preServicesSteupSteps = m_preServicesSteupSteps;
		foreach (StartupStep startupStep in preServicesSteupSteps)
		{
			yield return startupStep.Execute(null);
		}
	}

	private IEnumerator InitPreStart()
	{
		StartupStep[] preStartGameSteps = m_preStartGameSteps;
		foreach (StartupStep startupStep in preStartGameSteps)
		{
			yield return startupStep.Execute(null);
		}
	}

	private IEnumerator InitPcPlatform()
	{
		new GameObject("SteamManager").AddComponent<SteamManager>();
		yield return new WaitForEndOfFrame();
		yield return new WaitUntil(() => SteamManager.Initialized);
		yield return new WaitForEndOfFrame();
	}
}
