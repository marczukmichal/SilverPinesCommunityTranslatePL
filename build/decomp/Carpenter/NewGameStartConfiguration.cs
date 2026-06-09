using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "NewGameStartConfiguration", menuName = "Settings/New Game Start Configuration")]
public class NewGameStartConfiguration : ScriptableObject
{
	[Serializable]
	public class StartingItem
	{
		public ItemDefinition m_itemType;

		public int m_amount;

		public bool m_startEquipped;
	}

	[Serializable]
	public class StartingVariable
	{
		public ProgressionVariable m_variable;

		public bool m_setValue;
	}

	[Header("Character Inventory")]
	[SerializeField]
	private List<StartingItem> m_startingItems;

	[SerializeField]
	private int m_money;

	[Header("Stash")]
	[SerializeField]
	private List<StartingItem> m_stashItems;

	[Header("Time of Day")]
	[SerializeField]
	private TimeOfDay.TimePeriod m_startingTimePeriod;

	[Header("Starting Scene")]
	[SerializeField]
	private AssetReference m_startingScene;

	[Header("Weather")]
	[SerializeField]
	private WeatherSettings m_startingWeather;

	[Header("Variables")]
	[SerializeField]
	private List<StartingVariable> m_startingVariables;

	[SerializeField]
	private int m_startingArtifactSlots;

	[SerializeField]
	private string[] m_knownPhoneIDs;

	public IEnumerable<StartingItem> StartingItems => m_startingItems.AsReadOnly();

	public int Money => m_money;

	public IEnumerable<StartingItem> StashItems => m_stashItems.AsReadOnly();

	public TimeOfDay.TimePeriod StartTimePeriod => m_startingTimePeriod;

	public AssetReference StartingScene => m_startingScene;

	public WeatherSettings Weather => m_startingWeather;

	public IEnumerable<StartingVariable> StartingVariables => m_startingVariables.AsReadOnly();

	public int StartingArtifactSlots => m_startingArtifactSlots;

	public string[] KnownPhonesIDs => m_knownPhoneIDs;
}
