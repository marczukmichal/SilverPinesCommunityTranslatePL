using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "Misc/Persistent Data Store")]
public class PersistentDataStore : ScriptableObject
{
	[SerializeField]
	private PersistentData m_data;

	[SerializeField]
	private List<FloatVariable> m_persistentFloatVariables;

	[SerializeField]
	private List<IntVariable> m_persistentIntVariables;

	[SerializeField]
	private List<BoolVariable> m_persistentBoolVariables;

	[SerializeField]
	private GameObjectAnchor m_playerAnchor;

	[SerializeField]
	private PlayerMainInventory m_characterInventory;

	[SerializeField]
	private Inventory m_stashInventory;

	[SerializeField]
	private LoreInventory m_loreInventory;

	[SerializeField]
	private CollectableInventory m_collectableInventory;

	[SerializeField]
	private UserMapData m_userMapData;

	[SerializeField]
	private TimeOfDay m_timeOfDay;

	[SerializeField]
	private Weather m_weather;

	[SerializeField]
	private MapDynamicData m_mapCompletionData;

	[SerializeField]
	private ActiveObjective m_activeObjective;

	[SerializeField]
	private PhoneMemory m_phoneMemory;

	[SerializeField]
	private StatusEffectsVariable m_statusEffectsVariable;

	[SerializeField]
	private ComicMemory m_comicMemory;

	public PersistentData Data => m_data;

	public IEnumerable<BoolVariable> GetPersistentBoolVariables()
	{
		return m_persistentBoolVariables.AsReadOnly();
	}

	public IEnumerable<IntVariable> GetPersistentIntVariables()
	{
		return m_persistentIntVariables.AsReadOnly();
	}

	public void ClearDataStore()
	{
		m_data.Clear();
		m_characterInventory.Clear();
		m_activeObjective.Clear();
		m_stashInventory.Clear();
		m_loreInventory.Clear();
		m_collectableInventory.Clear();
		m_userMapData.Clear();
		m_mapCompletionData.Clear();
		m_phoneMemory.Clear();
		m_comicMemory.Clear();
		m_timeOfDay.SetTimeOfDay(TimeOfDay.TimePeriod.Night);
		foreach (FloatVariable persistentFloatVariable in m_persistentFloatVariables)
		{
			if (!(persistentFloatVariable == null))
			{
				try
				{
					persistentFloatVariable.Reset();
				}
				catch (Exception ex)
				{
					Debug.LogError(ex.ToString() + " - on variable " + persistentFloatVariable.name);
				}
			}
		}
		foreach (IntVariable persistentIntVariable in m_persistentIntVariables)
		{
			if (!(persistentIntVariable == null))
			{
				try
				{
					persistentIntVariable.Reset();
				}
				catch (Exception ex2)
				{
					Debug.LogError(ex2.ToString() + " - on variable " + persistentIntVariable.name);
				}
			}
		}
		foreach (BoolVariable persistentBoolVariable in m_persistentBoolVariables)
		{
			if (!(persistentBoolVariable == null))
			{
				try
				{
					persistentBoolVariable.Reset();
				}
				catch (Exception ex3)
				{
					Debug.LogError(ex3.ToString() + " - on variable " + persistentBoolVariable.name);
				}
			}
		}
		m_statusEffectsVariable.Reset();
	}

	private void SavePersistentVariables()
	{
		for (int i = 0; i < m_persistentFloatVariables.Count; i++)
		{
			if (!(m_persistentFloatVariables[i] == null))
			{
				m_data.GetFloatVariableEntry(m_persistentFloatVariables[i].GetPersistentID()).Data = m_persistentFloatVariables[i].Value;
			}
		}
		for (int j = 0; j < m_persistentIntVariables.Count; j++)
		{
			if (!(m_persistentIntVariables[j] == null))
			{
				m_data.GetIntVariableEntry(m_persistentIntVariables[j].GetPersistentID()).Data = m_persistentIntVariables[j].Value;
			}
		}
		for (int k = 0; k < m_persistentBoolVariables.Count; k++)
		{
			if (!(m_persistentBoolVariables[k] == null))
			{
				m_data.GetBoolVariableEntry(m_persistentBoolVariables[k].GetPersistentID()).Data = m_persistentBoolVariables[k].Value;
			}
		}
	}

	private void LoadPersistentVariables()
	{
		for (int i = 0; i < m_persistentFloatVariables.Count; i++)
		{
			if (!(m_persistentFloatVariables[i] == null))
			{
				PersistentDataFloat floatVariableEntry = m_data.GetFloatVariableEntry(m_persistentFloatVariables[i].GetPersistentID());
				m_persistentFloatVariables[i].Value = floatVariableEntry.Data;
			}
		}
		for (int j = 0; j < m_persistentIntVariables.Count; j++)
		{
			if (!(m_persistentIntVariables[j] == null))
			{
				PersistentDataInt intVariableEntry = m_data.GetIntVariableEntry(m_persistentIntVariables[j].GetPersistentID());
				m_persistentIntVariables[j].Value = intVariableEntry.Data;
			}
		}
		for (int k = 0; k < m_persistentBoolVariables.Count; k++)
		{
			if (!(m_persistentBoolVariables[k] == null))
			{
				PersistentDataBool boolVariableEntry = m_data.GetBoolVariableEntry(m_persistentBoolVariables[k].GetPersistentID());
				m_persistentBoolVariables[k].Value = boolVariableEntry.Data;
			}
		}
	}

	public PersistentData GetPopulatedPersistentData()
	{
		SavePersistentVariables();
		m_data.UpdateSaveVersion();
		m_data.IncrementSaveCount();
		m_data.SetTimeStampToNow();
		GlobalReferences.Instance.EventChannels.SaveLoad.PersistentDataPopulateForSave.Raise(m_data);
		m_data.ActiveScene = SceneManager.GetActiveScene().path;
		m_data.ActiveLevelMetadata = GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Item;
		m_data.PlayerPosition = m_playerAnchor.Item.transform.position;
		m_data.CharacterInventory = m_characterInventory.GetPersistentData() as PersistentDataMainInventory;
		m_data.StashInventory = m_stashInventory.GetPersistentData();
		m_data.LoreInventory = m_loreInventory.GetPersistentData();
		m_data.CollectableInventory = m_collectableInventory.GetPersistentData();
		m_data.UserMapData = m_userMapData.GetPersistentData();
		m_data.TimeOfDay = m_timeOfDay.CurrentTimeOfDay;
		m_data.ActiveWeatherSettings = m_weather.ActiveWeatherSettings;
		m_data.MapStateData = m_mapCompletionData.GetPersistentData();
		m_data.KnownPhoneNumbers = m_phoneMemory.KnownPhoneNumbers;
		m_data.CalledNumberIDs = m_phoneMemory.CalledNumberIDs;
		m_data.PreviouslyDonePhoneEvents = m_phoneMemory.PreviouslyDonePhoneEvents;
		m_data.MostRecentPhoneEvent = m_phoneMemory.SavePhoneEvent;
		m_data.StatusEffects = m_statusEffectsVariable.Value;
		m_data.ObjectiveState = m_activeObjective.ActiveObjectives;
		m_data.SeenComicIDs = m_comicMemory.GetPersistentData();
		return m_data;
	}

	public void PopulateFromLoadedPersistentData(PersistentData loadedData)
	{
		m_data = loadedData;
		GlobalReferences.Instance.Anchors.Inventory.QueuedUseItemInstanceAnchor.Set(null);
		m_characterInventory.ReadFromPersistentData(m_data.CharacterInventory);
		m_stashInventory.ReadFromPersistentData(m_data.StashInventory);
		m_loreInventory.ReadFromPersistentData(m_data.LoreInventory);
		m_collectableInventory.ReadFromPersistentData(m_data.CollectableInventory);
		m_userMapData.ReadFromPersistentData(m_data.UserMapData);
		m_timeOfDay.SetTimeOfDay(m_data.TimeOfDay);
		m_weather.SetActiveWeatherSettings(m_data.ActiveWeatherSettings, forceInstant: true);
		m_mapCompletionData.ReadFromPersistentData(m_data.MapStateData);
		m_phoneMemory.KnownPhoneNumbers = m_data.KnownPhoneNumbers;
		m_phoneMemory.CalledNumberIDs = m_data.CalledNumberIDs;
		m_phoneMemory.PreviouslyDonePhoneEvents = m_data.PreviouslyDonePhoneEvents;
		m_phoneMemory.SavePhoneEvent = m_data.MostRecentPhoneEvent;
		m_statusEffectsVariable.SetValue(m_data.StatusEffects);
		m_activeObjective.ReadFromPersistentData(m_data.ObjectiveState);
		m_comicMemory.ReadFromPersistentData(m_data.SeenComicIDs);
		LoadPersistentVariables();
		GlobalReferences.Instance.EventChannels.SaveLoad.PersistentDataOnLoaded.Raise(m_data);
		GlobalReferences.Instance.EventChannels.Datastore.PersistentDataRefresh.Raise();
	}

	public void StartNewGame(NewGameStartConfiguration startConfiguration)
	{
		GlobalReferences.Instance.EventChannels.Inventory.SetMaxNumberOfActiveArtifacts.Raise(startConfiguration.StartingArtifactSlots);
		int amountRemaining;
		foreach (NewGameStartConfiguration.StartingItem startingItem in startConfiguration.StartingItems)
		{
			if (startingItem.m_itemType == null)
			{
				Debug.LogError("Starting item type is null. unable to add to inventory");
				continue;
			}
			if (startingItem.m_itemType is ArtifactItemDefinition)
			{
				(m_characterInventory.AddItemToInventory(startingItem.m_itemType, startingItem.m_amount, new Vector2Int(-1, -1), rotated: false, autoEquip: false, out amountRemaining) as ArtifactItemInstance).PowerUp();
				continue;
			}
			ItemInstance itemInstance = m_characterInventory.AddItemToInventory(startingItem.m_itemType, startingItem.m_amount, new Vector2Int(-1, -1), rotated: false, autoEquip: false, out amountRemaining);
			if (startingItem.m_startEquipped)
			{
				if (itemInstance.ItemDefinition is ProjectileWeaponItemDefinition)
				{
					m_characterInventory.EquipRangedItem(itemInstance, toggle: false);
				}
				else if (itemInstance.ItemDefinition is MeleeWeaponItemDefinition)
				{
					m_characterInventory.EquipMeleeItem(itemInstance, toggle: false);
				}
				else if (itemInstance.ItemDefinition.CanBeActivated())
				{
					itemInstance.Activated = true;
				}
			}
		}
		foreach (NewGameStartConfiguration.StartingVariable startingVariable in startConfiguration.StartingVariables)
		{
			startingVariable.m_variable.SetValue(startingVariable.m_setValue);
		}
		m_characterInventory.AddMoney(startConfiguration.Money);
		foreach (NewGameStartConfiguration.StartingItem stashItem in startConfiguration.StashItems)
		{
			if (stashItem.m_itemType == null)
			{
				Debug.LogError("Starting stash item type is null. unable to add to inventory");
			}
			else
			{
				m_stashInventory.AddItemToInventory(stashItem.m_itemType, stashItem.m_amount, new Vector2Int(-1, -1), rotated: false, autoEquip: false, out amountRemaining);
			}
		}
		m_timeOfDay.SetTimeOfDay(startConfiguration.StartTimePeriod);
		m_weather.SetActiveWeatherSettings(startConfiguration.Weather, forceInstant: true);
		if (startConfiguration.KnownPhonesIDs != null)
		{
			string[] knownPhonesIDs = startConfiguration.KnownPhonesIDs;
			foreach (string phoneID in knownPhonesIDs)
			{
				m_phoneMemory.AddPhoneNumber(phoneID);
			}
		}
		GlobalReferences.Instance.EventChannels.Datastore.PersistentDataRefresh.Raise();
	}

	public void IncreasePlayTime(float timeStep)
	{
		m_data.IncreasePlayTime(timeStep);
	}

	public void SetStartingDifficulty(DifficultySettingEnum difficulty)
	{
		m_data.SetStartingDifficulty(difficulty);
	}

	public void ChangeDifficulty(DifficultySettingEnum difficulty)
	{
		m_data.ChangeDifficulty(difficulty);
	}
}
