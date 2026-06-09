using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering.Universal;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class GameDebugCommands : MonoBehaviour
{
	[Serializable]
	private struct AssetReferenceCommandSettings
	{
		public string m_commandName;

		public AssetReference m_assetReference;
	}

	[SerializeField]
	private List<AssetReferenceCommandSettings> m_spawnableGameObjects;

	[SerializeField]
	private List<AssetReferenceCommandSettings> m_spawnableObjectsPlayerControlled;

	[SerializeField]
	private PlayerMainInventory m_inventory;

	[SerializeField]
	private List<AssetReferenceCommandSettings> m_givableItems;

	[SerializeField]
	private LoreInventory m_loreInventory;

	[SerializeField]
	private List<AssetReferenceCommandSettings> m_loreEntries;

	[SerializeField]
	private GameObjectAnchor m_playerAnchor;

	[SerializeField]
	private IntVariable m_healthVariable;

	[SerializeField]
	private VoidGameEventChannel m_onReloadEventChannel;

	[SerializeField]
	private PersistentDataStore m_persistentDataStore;

	[SerializeField]
	private LevelTransitionGameEventChannel m_levelTransitionEventChannel;

	[SerializeField]
	private List<AssetReferenceCommandSettings> m_loadableLevels;

	[SerializeField]
	private ArtifactsGameSettings m_artifactsGameSettings;

	[SerializeField]
	private GameCompletionPercentSettings m_gameCompletionPercentSettings;

	private List<AsyncOperationHandle<GameObject>> m_loadedAssetHandles = new List<AsyncOperationHandle<GameObject>>();

	[DebugCommand("skip_story", "Skip various story elements", "skip_story <true/false>", typeof(bool), false)]
	public static bool SKIP_STORY;

	[DebugCommand("prog_var_log_changes", "Log changes to progression variables", "prog_var_log_changes <true/false>", typeof(bool), false)]
	public static bool PROG_VAR_LOG_CHANGES;

	[DebugCommand("infinite_ammo", "Enable infinite ammo", "infinite_ammo <true/false>", typeof(bool), true)]
	public static bool CHEAT_INFINITE_AMMO;

	[DebugCommand("infinite_stamina", "Enable infinite stamina", "infinite_stamina <true/false>", typeof(bool), true)]
	public static bool CHEAT_INFINITE_STAMINA;

	[DebugCommand("invincible", "Invincible (player god mode)", "invincible <true/false>", typeof(bool), true)]
	public static bool CHEAT_INVINCIBLE;

	[DebugCommand("master_key", "ignore item reuqirement for interactables, e.g. don't need keys to unlock doors", "master_key <true/false>", typeof(bool), true)]
	public static bool CHEAT_MASTER_KEY;

	[DebugCommand("map_show_everything", "If true map shown on the map even if they aren't discovered", "always_show_map_icon <true/false>", typeof(bool), false)]
	public static bool CHEAT_MAP_SHOW_EVERYTHING;

	[DebugCommand("camera_debug", "Enable camera debug info overlay", "camera_debug <true/false>", typeof(bool), false)]
	public static bool CAMERA_DEBUG;

	[DebugCommand("always_weakpoint_hit", "Force all hits to count as weakpoint", "always_weakpoint_hit <true/false>", typeof(bool), false)]
	public static bool ALWAYS_WEAKPOINT_HIT;

	private GameObject m_previouslyDisabledPlayerObject;

	[DebugCommand("character_debug", "Enable character debug info overlay", "character_debug <true/false>", typeof(bool), true)]
	private static bool m_showCharacterDebug;

	[DebugCommand("noise_debug", "Enable noise debug info overlay", "noise_debug <true/false>", typeof(bool), false)]
	private static bool m_noiseDebug;

	[DebugCommand("surface_debug", "Enable surface type debug info overlay", "surface_debug <true/false>", typeof(bool), false)]
	private static bool m_surfaceDebug;

	[DebugCommand("show_location", "Debug player scene / location info", "show_location <true/false>", typeof(bool), true)]
	public static bool m_showLocation;

	[DebugCommand("stats_debug", "Enable stats debug", "stats_debug <true/false>", typeof(bool), false)]
	private static bool m_statsDebug;

	private void Start()
	{
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelUnloaded.Register(UnloadSpawned);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelUnloaded.Unregister(UnloadSpawned);
	}

	private void UnloadSpawned()
	{
		foreach (AsyncOperationHandle<GameObject> loadedAssetHandle in m_loadedAssetHandles)
		{
			Addressables.ReleaseInstance(loadedAssetHandle);
		}
		m_loadedAssetHandles.Clear();
	}

	[DebugCommand("target_fps", "Set target FPS", "target_fps 30", typeof(int), false)]
	public void SetTargetFPS(int framerate)
	{
		Application.targetFrameRate = framerate;
	}

	[DebugCommand("kill", "Kills the player", "kill", null, false)]
	public void KillPlayer()
	{
		if (m_playerAnchor != null && (bool)m_playerAnchor.Item)
		{
			DamageInstance damageInstance = new DamageInstance().SetHealthDamage(int.MaxValue);
			DamageUtilities.ApplyDamage(m_playerAnchor.Item.GetComponentsInChildren<IDamageable>(), damageInstance);
		}
	}

	[DebugCommand("hurt", "Hurts the player", "hurt", null, false)]
	public void HurtPlayer()
	{
		if (m_playerAnchor != null && (bool)m_playerAnchor.Item)
		{
			DamageInstance damageInstance = new DamageInstance().SetHealthDamage(25);
			DamageUtilities.ApplyDamage(m_playerAnchor.Item.GetComponentsInChildren<IDamageable>(), damageInstance);
		}
	}

	[DebugCommand("hurt_large_impact", "Hurts the player with a large impact", "hurt_large_impact", null, false)]
	public void HurtPlayerLargeImpact()
	{
		if (m_playerAnchor != null && (bool)m_playerAnchor.Item)
		{
			DamageInstance damageInstance = new DamageInstance().SetHealthDamage(25).SetImpactType(ImpactType.Large).SetDirection(Vector2.right)
				.SetPosition(m_playerAnchor.Item.transform.position);
			DamageUtilities.ApplyDamage(m_playerAnchor.Item.GetComponentsInChildren<IDamageable>(), damageInstance);
		}
	}

	[DebugCommand("hurt_others", "Hurts everyone that isn't the player", "hurt_others", null, false)]
	public void HurtOthers()
	{
		CharacterHealth[] array = UnityEngine.Object.FindObjectsByType<CharacterHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		foreach (CharacterHealth characterHealth in array)
		{
			if (characterHealth.gameObject != m_playerAnchor.Item)
			{
				DamageInstance damageInstance = new DamageInstance().SetHealthDamage(25);
				DamageUtilities.ApplyDamage(characterHealth.GetComponentsInChildren<IDamageable>(), damageInstance);
			}
		}
	}

	[DebugCommand("player_set_state", "Force the player into a specific state", "player_set_state <state_name_string>", typeof(string), false)]
	public void PlayerSetState(string stateName)
	{
		if (m_playerAnchor != null && (bool)m_playerAnchor.Item)
		{
			m_playerAnchor.Item.GetComponent<PlayMakerFSM>().SetState(stateName);
		}
	}

	[DebugCommand("status_effect", "Give the player a status effect", "status_effect <status_effect_name>", typeof(string), false)]
	public void GiveStatusEffect(string statusEffect)
	{
		if (m_playerAnchor != null && (bool)m_playerAnchor.Item)
		{
			StatusEffectDefinition statusEffectDefinition = (StatusEffectDefinition)((AsyncOperationHandle)Addressables.LoadAssetAsync<StatusEffectDefinition>("Assets/ScriptableObjects/Settings/StatusEffects/" + statusEffect + ".asset")).WaitForCompletion();
			if (statusEffectDefinition != null)
			{
				m_playerAnchor.Item.GetComponent<StatusEffectReceiver>().ApplyStatusEffect(statusEffectDefinition, 100);
			}
		}
	}

	[DebugCommand("clear_status_effects", "Clear all status effects", "clear_status_effects", null, false)]
	public void ClearStatusEffects()
	{
		if (m_playerAnchor != null && (bool)m_playerAnchor.Item)
		{
			m_playerAnchor.Item.GetComponent<StatusEffectReceiver>().ClearAll();
		}
	}

	[DebugCommand("disable_player", "Disable player object", "disable_player <bool>", typeof(bool), false)]
	public void DisablePlayer(bool disable)
	{
		if (m_playerAnchor != null && (bool)m_playerAnchor.Item)
		{
			m_previouslyDisabledPlayerObject = m_playerAnchor.Item;
			m_playerAnchor.Item.gameObject.SetActive(!disable);
		}
		else if ((bool)m_previouslyDisabledPlayerObject)
		{
			m_previouslyDisabledPlayerObject.SetActive(!disable);
		}
	}

	[DebugCommand("hide_player", "Hide player renderers", "hide_player <bool>", typeof(bool), false)]
	public void HidePlayer(bool hidden)
	{
		if (m_playerAnchor != null && (bool)m_playerAnchor.Item)
		{
			Renderer[] componentsInChildren = m_playerAnchor.Item.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = !hidden;
			}
			DecalProjector[] componentsInChildren2 = m_playerAnchor.Item.GetComponentsInChildren<DecalProjector>();
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].enabled = !hidden;
			}
		}
	}

	[DebugCommand("respawn", "Respawn the player", "respawn", null, false)]
	public void Respawn()
	{
		m_healthVariable.Value = 100;
		m_onReloadEventChannel.Raise();
		SpawnCharacterAsPlayer("player");
	}

	[DebugCommand("damage_melee", "Sets the current melee weapon to 5% durability", "damage_melee", null, false)]
	public void DamageCurrentMeleeWeapon()
	{
		if (m_playerAnchor != null && (bool)m_playerAnchor.Item)
		{
			ItemInstance equippedMeleeItem = m_playerAnchor.Item.GetComponent<CharacterInventory>().Inventory.EquippedMeleeItem;
			if (equippedMeleeItem != null && equippedMeleeItem is MeleeWeaponItemInstance meleeWeaponItemInstance)
			{
				meleeWeaponItemInstance.SetDurabilityProportion(0.05f);
			}
		}
	}

	[DebugCommand("repair_melee", "Sets the current melee weapon to 100% durability", "repair_melee", null, false)]
	public void RepairCurrentMeleeWeapon()
	{
		if (m_playerAnchor != null && (bool)m_playerAnchor.Item)
		{
			ItemInstance equippedMeleeItem = m_playerAnchor.Item.GetComponent<CharacterInventory>().Inventory.EquippedMeleeItem;
			if (equippedMeleeItem != null && equippedMeleeItem is MeleeWeaponItemInstance meleeWeaponItemInstance)
			{
				meleeWeaponItemInstance.SetDurabilityProportion(1f);
			}
		}
	}

	[DebugCommand("spawn", "Spawn GameObject", "spawn <objectname>", typeof(string), false)]
	public void SpawnObject(string objectName)
	{
		if (objectName.Equals("list"))
		{
			PrintList(m_spawnableGameObjects);
			return;
		}
		AssetReference assetReference = FindObjectInList(m_spawnableGameObjects, objectName);
		if (assetReference != null)
		{
			Vector3 position = Vector3.zero;
			if (m_playerAnchor != null && (bool)m_playerAnchor.Item)
			{
				position = m_playerAnchor.Item.transform.position;
			}
			SpawnGameObject(assetReference, position);
		}
		else
		{
			Debug.Log("No object defined for type " + objectName + "... For a full list use 'list'");
		}
	}

	[DebugAutoComplete("spawn")]
	public static string[] SpawnAutoComplete(string input)
	{
		return UnityEngine.Object.FindAnyObjectByType<GameDebugCommands>().SpawnAutoComplete_Internal(input);
	}

	private string[] SpawnAutoComplete_Internal(string input)
	{
		List<string> list = new List<string>();
		foreach (AssetReferenceCommandSettings spawnableGameObject in m_spawnableGameObjects)
		{
			if (spawnableGameObject.m_commandName.Contains(input, StringComparison.OrdinalIgnoreCase))
			{
				list.Add(spawnableGameObject.m_commandName);
			}
		}
		return list.ToArray();
	}

	[DebugCommand("spawn_as_player", "Spawn Player Controlled Character", "spawn_as_player <objectname>", typeof(string), false)]
	public void SpawnCharacterAsPlayer(string objectName)
	{
		if (objectName.Equals("list"))
		{
			PrintList(m_spawnableObjectsPlayerControlled);
			return;
		}
		AssetReference assetReference = FindObjectInList(m_spawnableObjectsPlayerControlled, objectName);
		if (assetReference != null)
		{
			Vector3 position = Vector3.zero;
			if (m_playerAnchor != null && (bool)m_playerAnchor.Item)
			{
				position = m_playerAnchor.Item.transform.position;
				UnityEngine.Object.Destroy(m_playerAnchor.Item);
			}
			SpawnGameObject(assetReference, position);
		}
		else
		{
			Debug.Log("No object defined for type " + objectName + "... For a full list use 'list'");
		}
	}

	[DebugAutoComplete("spawn_as_player")]
	public static string[] SpawnAsPlayerAutoComplete(string input)
	{
		return UnityEngine.Object.FindAnyObjectByType<GameDebugCommands>().SpawnAsPlayerAutoComplete_Internal(input);
	}

	private string[] SpawnAsPlayerAutoComplete_Internal(string input)
	{
		List<string> list = new List<string>();
		foreach (AssetReferenceCommandSettings item in m_spawnableObjectsPlayerControlled)
		{
			if (item.m_commandName.Contains(input, StringComparison.OrdinalIgnoreCase))
			{
				list.Add(item.m_commandName);
			}
		}
		return list.ToArray();
	}

	private void PrintList(List<AssetReferenceCommandSettings> list)
	{
		string text = "Available items to spawn: \n";
		foreach (AssetReferenceCommandSettings item in list)
		{
			text += item.m_commandName;
			text += "\n";
		}
		Debug.Log(text);
	}

	private AssetReference FindObjectInList(List<AssetReferenceCommandSettings> list, string name)
	{
		foreach (AssetReferenceCommandSettings item in list)
		{
			if (item.m_commandName.Equals(name, StringComparison.OrdinalIgnoreCase))
			{
				return item.m_assetReference;
			}
		}
		return null;
	}

	private GameObject SpawnGameObject(AssetReference assetReference, Vector3 position)
	{
		if (m_playerAnchor != null && m_playerAnchor.Item != null)
		{
			position = m_playerAnchor.Item.transform.position;
		}
		position.x += 1f;
		position.y += 1f;
		RaycastHit2D raycastHit2D = Physics2D.Raycast(position, Vector2.down, 10f);
		if ((bool)raycastHit2D)
		{
			position.x = raycastHit2D.point.x;
			position.y = raycastHit2D.point.y;
		}
		AsyncOperationHandle<GameObject> item = Addressables.LoadAssetAsync<GameObject>(assetReference);
		item.WaitForCompletion();
		m_loadedAssetHandles.Add(item);
		GameObject obj = UnityEngine.Object.Instantiate(item.Result, position, Quaternion.identity);
		PersistentDataIdentifier component = obj.GetComponent<PersistentDataIdentifier>();
		if ((bool)component)
		{
			UnityEngine.Object.Destroy(component);
		}
		return obj;
	}

	private void PrintList<T>(List<T> list) where T : ScriptableObject
	{
		string text = "Available items to spawn: \n";
		foreach (T item in list)
		{
			if (!((UnityEngine.Object)item == (UnityEngine.Object)null))
			{
				text += item.name;
				text += "\n";
			}
		}
		Debug.Log(text);
	}

	[DebugCommand("money", "Give the player money", "money <amount>", typeof(int), false)]
	public void GiveMoney(int moneyAmount)
	{
		m_inventory.AddMoney(moneyAmount);
	}

	private string[] AutoComplete_ListInternal(string input, List<AssetReferenceCommandSettings> list)
	{
		List<string> list2 = new List<string>();
		foreach (AssetReferenceCommandSettings item in list)
		{
			if (item.m_commandName.Contains(input, StringComparison.OrdinalIgnoreCase))
			{
				list2.Add(item.m_commandName);
			}
		}
		return list2.ToArray();
	}

	[DebugCommand("artifact_slots", "How many artifact (memento) slots are available", "artifact_slots <amount>", typeof(int), false)]
	public void SetArtifactSlots(int slots)
	{
		GlobalReferences.Instance.EventChannels.Inventory.SetMaxNumberOfActiveArtifacts.Raise(slots);
	}

	[DebugCommand("give", "Give the player an item", "give <itemname>", typeof(string), false)]
	public void GiveItem(string objectName)
	{
		if (objectName.Equals("list"))
		{
			PrintList(m_givableItems);
			return;
		}
		AssetReference assetReference = FindObjectInList(m_givableItems, objectName);
		ItemDefinition itemDefinition = AddressablesContentManager.Instance.GetScriptableObjectAsset(assetReference) as ItemDefinition;
		if ((bool)itemDefinition)
		{
			int amount = itemDefinition.MaxStackSize;
			if (itemDefinition is ProjectileWeaponItemDefinition)
			{
				amount = (itemDefinition as ProjectileWeaponItemDefinition).WeaponSettings.MaxAmmo;
			}
			else if (itemDefinition is PoweredItemDefinition)
			{
				amount = (int)(itemDefinition as PoweredItemDefinition).MaxCharge;
			}
			else if (itemDefinition is RefillableItemDefinition)
			{
				amount = (itemDefinition as RefillableItemDefinition).MaxUses;
			}
			m_inventory.AddItemToInventory(itemDefinition, amount, new Vector2Int(-1, -1), rotated: false, autoEquip: true, out var _);
		}
		else
		{
			Debug.Log("No item defined for type " + objectName + "... For a full list use 'list'");
		}
	}

	[DebugAutoComplete("give")]
	public static string[] GiveAutoComplete(string input)
	{
		GameDebugCommands gameDebugCommands = UnityEngine.Object.FindAnyObjectByType<GameDebugCommands>();
		return UnityEngine.Object.FindAnyObjectByType<GameDebugCommands>().AutoComplete_ListInternal(input, gameDebugCommands.m_givableItems);
	}

	[DebugCommand("give_all_artifacts", "Give the player all artifacts", "give_all_artifacts", null, false)]
	public void GiveAllArtifacts()
	{
		ArtifactItemDefinition[] artifacts = m_artifactsGameSettings.Artifacts;
		foreach (ArtifactItemDefinition itemDefinition in artifacts)
		{
			m_inventory.AddItemToInventory(itemDefinition, 1, new Vector2Int(-1, -1), rotated: false, autoEquip: true, out var _);
		}
	}

	[DebugCommand("check_completion", "Checks game completion percentage and logs results", "check_completion", null, false)]
	public void CheckCompletionPercent()
	{
		m_gameCompletionPercentSettings.CalculatePercentage(logResults: true);
	}

	[DebugCommand("getlore", "Give the player a lore item", "getlore <lorename>", typeof(string), false)]
	public void GetLore(string objectName)
	{
		if (objectName.Equals("list"))
		{
			PrintList(m_loreEntries);
			return;
		}
		if (objectName.Equals("all"))
		{
			Debug.Log("Added all lore entries");
			{
				foreach (AssetReferenceCommandSettings loreEntry3 in m_loreEntries)
				{
					LoreEntry loreEntry = AddressablesContentManager.Instance.GetScriptableObjectAsset(loreEntry3.m_assetReference) as LoreEntry;
					if (!(loreEntry == null) && loreEntry.SaveToLoreInventory)
					{
						m_loreInventory.AddLoreEntry(loreEntry);
					}
				}
				return;
			}
		}
		AssetReference assetReference = FindObjectInList(m_givableItems, objectName);
		LoreEntry loreEntry2 = AddressablesContentManager.Instance.GetScriptableObjectAsset(assetReference) as LoreEntry;
		if ((bool)loreEntry2)
		{
			m_loreInventory.AddLoreEntry(loreEntry2);
		}
		else
		{
			Debug.Log("No lore entry defined for type " + objectName + "... For a full list use 'list'");
		}
	}

	[DebugAutoComplete("getlore")]
	public static string[] GetLoreAutoComplete(string input)
	{
		GameDebugCommands gameDebugCommands = UnityEngine.Object.FindAnyObjectByType<GameDebugCommands>();
		return UnityEngine.Object.FindAnyObjectByType<GameDebugCommands>().AutoComplete_ListInternal(input, gameDebugCommands.m_loreEntries);
	}

	[DebugCommand("level", "Switches to a level, maintaining player status", "level <levelname>", typeof(string), false)]
	public void LevelTransition(string objectName)
	{
		if (objectName.Equals("list"))
		{
			PrintList(m_loadableLevels);
			return;
		}
		AssetReference assetReference = FindObjectInList(m_loadableLevels, objectName);
		LevelMetadata levelMetadata = AddressablesContentManager.Instance.GetScriptableObjectAsset(assetReference) as LevelMetadata;
		if ((bool)levelMetadata)
		{
			LevelTransitionEventData value = new LevelTransitionEventData
			{
				m_levelMetadata = levelMetadata,
				m_targetSceneName = levelMetadata.SceneFileName,
				m_targetSceneAssetReference = levelMetadata.TargetSceneAssetReference,
				m_type = LevelTransitionEventType.Transition
			};
			m_levelTransitionEventChannel.Raise(value);
		}
		else
		{
			Debug.Log("No level found for name " + objectName + "... For a full list use 'list'");
		}
	}

	[DebugAutoComplete("level")]
	public static string[] GetLevelTransitionAutocomplete(string input)
	{
		GameDebugCommands gameDebugCommands = UnityEngine.Object.FindAnyObjectByType<GameDebugCommands>();
		return UnityEngine.Object.FindAnyObjectByType<GameDebugCommands>().AutoComplete_ListInternal(input, gameDebugCommands.m_loadableLevels);
	}

	private void SetProgressionVariableToValue(string varName, bool value)
	{
		if (varName.Equals("list"))
		{
			string text = "Available progression variables: \n";
			foreach (BoolVariable persistentBoolVariable in m_persistentDataStore.GetPersistentBoolVariables())
			{
				if (!(persistentBoolVariable == null) && persistentBoolVariable is ProgressionVariable)
				{
					text = text + persistentBoolVariable.name + " [" + persistentBoolVariable.Value + "]";
					text += "\n";
				}
			}
			Debug.Log(text);
			return;
		}
		if (varName.Equals("all"))
		{
			foreach (BoolVariable persistentBoolVariable2 in m_persistentDataStore.GetPersistentBoolVariables())
			{
				if (!(persistentBoolVariable2 == null) && persistentBoolVariable2 is ProgressionVariable)
				{
					persistentBoolVariable2.SetValue(value);
				}
			}
			Debug.Log("Set all progression valariables to " + value);
			return;
		}
		ProgressionVariable progressionVariable = null;
		foreach (BoolVariable persistentBoolVariable3 in m_persistentDataStore.GetPersistentBoolVariables())
		{
			if (!(persistentBoolVariable3 == null) && persistentBoolVariable3 is ProgressionVariable && persistentBoolVariable3.name.Equals(varName, StringComparison.OrdinalIgnoreCase))
			{
				progressionVariable = persistentBoolVariable3 as ProgressionVariable;
			}
		}
		if ((bool)progressionVariable)
		{
			progressionVariable.Value = value;
		}
		else
		{
			Debug.Log("No progression variable found for " + varName + "... For a full list use 'list'");
		}
	}

	[DebugCommand("setprogvar", "Set a progression variable to true", "setprogvar <varname>", typeof(string), false)]
	public void SetProgressionVariable(string varName)
	{
		SetProgressionVariableToValue(varName, value: true);
	}

	[DebugAutoComplete("setprogvar")]
	public static string[] SetProgressionVariableAutoComplete(string input)
	{
		return UnityEngine.Object.FindAnyObjectByType<GameDebugCommands>().SetProgressionVariableAutoComplete_Internal(input);
	}

	[DebugCommand("unsetprogvar", "Set a progression variable to false", "unsetprogvar <varname>", typeof(string), false)]
	public void UnSetProgressionVariable(string varName)
	{
		SetProgressionVariableToValue(varName, value: false);
	}

	[DebugAutoComplete("unsetprogvar")]
	public static string[] UnSetProgressionVariableAutoComplete(string input)
	{
		return UnityEngine.Object.FindAnyObjectByType<GameDebugCommands>().SetProgressionVariableAutoComplete_Internal(input);
	}

	private string[] SetProgressionVariableAutoComplete_Internal(string input)
	{
		List<string> list = new List<string>();
		foreach (BoolVariable persistentBoolVariable in m_persistentDataStore.GetPersistentBoolVariables())
		{
			if (!(persistentBoolVariable == null) && persistentBoolVariable is ProgressionVariable && persistentBoolVariable.name.Contains(input, StringComparison.OrdinalIgnoreCase))
			{
				list.Add(persistentBoolVariable.name);
			}
		}
		return list.ToArray();
	}

	[DebugCommand("setprogvarint", "Set a progression variable a specified value", "setprogvar <varname> <int>", typeof(string), false)]
	public void SetProgressionVariableInt(string args)
	{
		string[] array = args.Split(' ');
		if (array.Length == 2)
		{
			string text = array[0];
			if (int.TryParse(array[1], out var result))
			{
				ProgressionVariableInt progressionVariableInt = null;
				foreach (IntVariable persistentIntVariable in m_persistentDataStore.GetPersistentIntVariables())
				{
					if (!(persistentIntVariable == null) && persistentIntVariable is ProgressionVariableInt && persistentIntVariable.name.Equals(text, StringComparison.OrdinalIgnoreCase))
					{
						progressionVariableInt = persistentIntVariable as ProgressionVariableInt;
					}
				}
				if ((bool)progressionVariableInt)
				{
					progressionVariableInt.Value = result;
				}
				else
				{
					Debug.Log("No progression variable found for " + text + "... For a full list use 'list'");
				}
			}
			else
			{
				Debug.Log("Second token is not a valid integer!");
			}
		}
		else
		{
			Debug.Log("Not enough tokens for setprogvarint. Use is 'setprogvarint <varname> <int>'");
		}
	}

	[DebugAutoComplete("setprogvarint")]
	public static string[] SetProgressionVariableIntgerAutoComplete(string input)
	{
		return UnityEngine.Object.FindAnyObjectByType<GameDebugCommands>().SetProgressionVariableIntegerAutoComplete_Internal(input);
	}

	private string[] SetProgressionVariableIntegerAutoComplete_Internal(string input)
	{
		List<string> list = new List<string>();
		foreach (IntVariable persistentIntVariable in m_persistentDataStore.GetPersistentIntVariables())
		{
			if (!(persistentIntVariable == null) && persistentIntVariable is ProgressionVariableInt && persistentIntVariable.name.Contains(input, StringComparison.OrdinalIgnoreCase))
			{
				list.Add(persistentIntVariable.name);
			}
		}
		return list.ToArray();
	}

	private void DrawFillBar(Rect rect, Color color, float fillPercent)
	{
		GUI.color = Color.gray;
		GUI.DrawTexture(rect, Texture2D.whiteTexture);
		GUI.color = color;
		Rect position = rect;
		position.width *= fillPercent;
		GUI.DrawTexture(position, Texture2D.whiteTexture);
		GUI.color = Color.white;
	}

	private void OnGUI()
	{
		if (m_showCharacterDebug)
		{
			CharacterInfoDebugGUI();
		}
		if (m_noiseDebug)
		{
			NoiseDebugGUI();
		}
		if (m_surfaceDebug)
		{
			SurfaceDebugGUI();
		}
		if (m_showLocation)
		{
			ShowLocationDebugGUI();
		}
		if (m_statsDebug)
		{
			GlobalReferences.Instance.DataStore.Data.m_stats.DrawDebugGUI();
		}
	}

	private void CharacterInfoDebugGUI()
	{
		if (Camera.main == null)
		{
			return;
		}
		CharacterIdentifier[] array = UnityEngine.Object.FindObjectsByType<CharacterIdentifier>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		foreach (CharacterIdentifier characterIdentifier in array)
		{
			string infoString = "";
			Vector2 position = Camera.main.WorldToScreenPoint(characterIdentifier.transform.position);
			position.y = (float)Screen.height - position.y;
			Rect rect = new Rect(position, new Vector2(128f, 32f));
			rect.x -= rect.width * 0.5f;
			GUI.Box(rect, characterIdentifier.gameObject.name);
			rect.y += 24f;
			CharacterHealth component = characterIdentifier.GetComponent<CharacterHealth>();
			if (component != null)
			{
				rect.height = 4f;
				DrawFillBar(rect, Color.red, component.HealthPercentage);
				rect.y += 6f;
				infoString = infoString + "HP: " + component.Health + "/" + component.MaxHealth + " | ";
			}
			PlayMakerFSM[] componentsInChildren = characterIdentifier.GetComponentsInChildren<PlayMakerFSM>();
			foreach (PlayMakerFSM playMakerFSM in componentsInChildren)
			{
				infoString = infoString + playMakerFSM.FsmName + " State: " + playMakerFSM.ActiveStateName + "\n";
			}
			CharacterSyncGrab component2 = characterIdentifier.GetComponent<CharacterSyncGrab>();
			if (component2 != null)
			{
				component2.GetDebugInfo(ref infoString);
			}
			StatusEffectReceiver component3 = characterIdentifier.GetComponent<StatusEffectReceiver>();
			if (component3 != null)
			{
				component3.GetDebugInfo(ref infoString);
			}
			CharacterHitReact component4 = characterIdentifier.GetComponent<CharacterHitReact>();
			if (component4 != null)
			{
				infoString = infoString + "HitReact Stagger: " + component4.StaggerBuildUp;
			}
			CharacterDamageRage component5 = characterIdentifier.GetComponent<CharacterDamageRage>();
			if (component5 != null)
			{
				if (component5.IsRaging)
				{
					infoString += "\nRage Active";
				}
				else if (component5.RageMeter > 0f)
				{
					infoString = infoString + "\nRage: " + component5.RageMeter;
				}
			}
			CharacterNecromancy component6 = characterIdentifier.GetComponent<CharacterNecromancy>();
			if (component6 != null && component6.ReviveTimerActive)
			{
				infoString = infoString + "\nNecromancy: " + component6.GetDebugString();
			}
			rect.y += 8f;
			rect.width = 256f;
			rect.height = 128f;
			GUI.Label(rect, infoString);
			rect.y += rect.height;
			AIBrain component7 = characterIdentifier.GetComponent<AIBrain>();
			if (component7 != null)
			{
				component7.DrawDebugInfo(rect);
			}
		}
	}

	private static void DrawEllipse(Vector3 pos, Vector3 forward, Vector3 up, float radiusX, float radiusY, int segments, Color color, float duration = 0f)
	{
		float num = 0f;
		Quaternion quaternion = Quaternion.LookRotation(forward, up);
		Vector3 vector = Vector3.zero;
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < segments + 1; i++)
		{
			zero.x = Mathf.Sin(MathF.PI / 180f * num) * radiusX;
			zero.y = Mathf.Cos(MathF.PI / 180f * num) * radiusY;
			if (i > 0)
			{
				Debug.DrawLine(quaternion * vector + pos, quaternion * zero + pos, color, duration);
			}
			vector = zero;
			num += 360f / (float)segments;
		}
	}

	private void NoiseDebugGUI()
	{
		NoiseSource[] array = UnityEngine.Object.FindObjectsByType<NoiseSource>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		foreach (NoiseSource noiseSource in array)
		{
			Vector3 position = noiseSource.transform.position;
			position.x += noiseSource.NoiseDistance;
			DrawEllipse(noiseSource.transform.position, Vector3.forward, Vector3.up, noiseSource.NoiseDistance, noiseSource.NoiseDistance, 32, Color.red, 0.05f);
		}
	}

	private void SurfaceDebugGUI()
	{
		if (Camera.main == null)
		{
			return;
		}
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (item == null)
		{
			return;
		}
		CharacterMovement component = item.GetComponent<CharacterMovement>();
		if (component == null)
		{
			return;
		}
		string text = "";
		Vector2 position = Camera.main.WorldToScreenPoint(component.transform.position);
		position.y = (float)Screen.height - position.y;
		position.y -= 128f;
		Rect position2 = new Rect(position, new Vector2(128f, 96f));
		position2.x -= position2.width * 0.5f;
		if (component.CurrentSurfaceSettings != null)
		{
			text = text + "\nSurfaceType: " + component.CurrentSurfaceSettings.name;
		}
		if (component.CurrentSurfaceCollider != null)
		{
			text = text + "\nObject: " + component.CurrentSurfaceCollider.name;
		}
		if (component.CurrentWaterSurface != null)
		{
			text = text + "\nWater: " + component.CurrentWaterSurface.name;
		}
		position2.y += 24f;
		position2.width = 256f;
		position2.height = 128f;
		GUI.Label(position2, text);
		position2.y += position2.height;
		CharacterAiming component2 = item.GetComponent<CharacterAiming>();
		if (!component2.AimingActive)
		{
			return;
		}
		Vector3 position3 = component2.AimTransform.position;
		RaycastHit2D raycastHit2D = Physics2D.Raycast(direction: component2.PreviousAimVector, origin: position3, distance: 10f, layerMask: GameLayers.ProjectileMask);
		if (raycastHit2D.collider != null)
		{
			position = Camera.main.WorldToScreenPoint(raycastHit2D.point);
			position.y = (float)Screen.height - position.y;
			text = "";
			SurfaceType component3 = raycastHit2D.collider.GetComponent<SurfaceType>();
			SurfaceSettings surfaceSettings = GlobalReferences.Instance.DefaultSurfaceSettings;
			if (component3 != null)
			{
				surfaceSettings = component3.Surface;
			}
			text = text + "\nSurfaceType: " + surfaceSettings.name;
			text = text + "\nObject: " + raycastHit2D.collider.name;
			position2 = new Rect(position, new Vector2(128f, 96f));
			position2.x -= position2.width * 0.5f;
			GUI.Label(position2, text);
		}
	}

	private void ShowLocationDebugGUI()
	{
		GUILayout.BeginArea(new Rect((float)Screen.width * 0.5f - 256f, 0f, 512f, 256f));
		string text = "";
		text = ((LevelManager.ActiveTransition == null) ? SceneManager.GetActiveScene().name : ("Loading: " + LevelManager.ActiveTransition.m_targetSceneName));
		Vector3 vector = Vector3.zero;
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (item != null)
		{
			vector = item.transform.position;
		}
		string text2 = text;
		Vector3 vector2 = vector;
		GUILayout.Label("Scene: " + text2 + " | Position: " + vector2.ToString());
		GUILayout.EndArea();
	}
}
