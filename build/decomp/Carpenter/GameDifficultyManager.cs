using UnityEngine;

public class GameDifficultyManager : MonoBehaviour
{
	[SerializeField]
	private GameDifficultySettings m_difficultySettings;

	[DebugCommand("difficulty_debug", "Enable difficulty debug info overlay", "difficulty_debug <true/false>", typeof(bool), false)]
	private static bool m_difficultyDebug;

	[SerializeField]
	private Inventory m_playerInventory;

	[SerializeField]
	private Inventory m_stashInventory;

	public static DifficultySettings CurrentDifficulty => GlobalReferences.Instance.Anchors.Gameplay.GameDifficultyManagerAnchor.Item.GetCurrentDifficulty();

	private void OnGUI()
	{
		if (m_difficultyDebug)
		{
			GUILayout.Label("Difficulty Mode: " + GetCurrentDifficultyEnum());
			GetCurrentDifficulty().DifficultyGUI();
			GUILayout.Label("Calculated Player Scores:");
			GUILayout.Label("General: " + CalculateResourceScore(GameDifficultyResourceScoreType.General) + " - " + GetResourceLevel(GameDifficultyResourceScoreType.General));
			GUILayout.Label("Health: " + CalculateResourceScore(GameDifficultyResourceScoreType.Health) + " - " + GetResourceLevel(GameDifficultyResourceScoreType.Health));
			GUILayout.Label("Ammo: " + CalculateResourceScore(GameDifficultyResourceScoreType.Ammo) + " - " + GetResourceLevel(GameDifficultyResourceScoreType.Ammo));
			GUILayout.Label("Melee: " + CalculateResourceScore(GameDifficultyResourceScoreType.Melee) + " - " + GetResourceLevel(GameDifficultyResourceScoreType.Melee));
			GUILayout.Label("Net Worth: " + CalculateResourceScore(GameDifficultyResourceScoreType.NetWorth) + " - " + GetResourceLevel(GameDifficultyResourceScoreType.NetWorth));
		}
	}

	private GameDifficultyResourceLevel GetResourceLevel(GameDifficultyResourceScoreType type)
	{
		int resourceValue = CalculateResourceScore(type);
		return GetCurrentDifficulty().GetResourceLevel(resourceValue, type);
	}

	public static GameDifficultyResourceLevel GetCurrentResourceLevel(GameDifficultyResourceScoreType type)
	{
		return GlobalReferences.Instance.Anchors.Gameplay.GameDifficultyManagerAnchor.Item.GetResourceLevel(type);
	}

	private int CalculateResourceScore(GameDifficultyResourceScoreType type)
	{
		switch (type)
		{
		case GameDifficultyResourceScoreType.None:
			return 0;
		case GameDifficultyResourceScoreType.General:
			return CalculateResourceScore(GameDifficultyResourceScoreType.Health) + CalculateResourceScore(GameDifficultyResourceScoreType.Ammo) + CalculateResourceScore(GameDifficultyResourceScoreType.Melee);
		case GameDifficultyResourceScoreType.Health:
		{
			GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
			int num = 50;
			if (item != null)
			{
				num = item.GetComponent<CharacterHealth>().Health;
			}
			return num + m_playerInventory.GetResourceScore(type) + m_stashInventory.GetResourceScore(type);
		}
		default:
			return m_playerInventory.GetResourceScore(type) + m_stashInventory.GetResourceScore(type);
		}
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.Anchors.Gameplay.GameDifficultyManagerAnchor.Set(this);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Anchors.Gameplay.GameDifficultyManagerAnchor.Set(null);
	}

	private DifficultySettingEnum GetCurrentDifficultyEnum()
	{
		DifficultySettingEnum result = DifficultySettingEnum.Normal;
		if (GlobalReferences.Instance.DataStore != null)
		{
			result = GlobalReferences.Instance.DataStore.Data.CurrentDifficulty;
		}
		return result;
	}

	private DifficultySettings GetCurrentDifficulty()
	{
		return m_difficultySettings.GetDifficultyModeConfiguration(GetCurrentDifficultyEnum());
	}

	public static int ScaleDamageValue(int damageAmount, DifficultyDamageScalingMode difficultyScaling)
	{
		switch (difficultyScaling)
		{
		case DifficultyDamageScalingMode.PlayerDamageDealt:
			damageAmount = Mathf.RoundToInt((float)damageAmount * CurrentDifficulty.PlayerDamageDealtScalar);
			break;
		case DifficultyDamageScalingMode.EnemyDamageDealt:
			damageAmount = Mathf.RoundToInt((float)damageAmount * CurrentDifficulty.EnemyDamageDealtScalar);
			break;
		}
		return damageAmount;
	}
}
