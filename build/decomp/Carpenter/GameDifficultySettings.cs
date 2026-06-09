using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameDifficultySettings", menuName = "Misc/Game Difficulty Settings")]
public class GameDifficultySettings : ScriptableObject
{
	[SerializeField]
	private DifficultySettings m_difficultyNormal;

	[SerializeField]
	private DifficultySettings m_difficultyStory;

	public DifficultySettings GetDifficultyModeConfiguration(DifficultySettingEnum difficultyMode)
	{
		return difficultyMode switch
		{
			DifficultySettingEnum.Normal => m_difficultyNormal, 
			DifficultySettingEnum.Story => m_difficultyStory, 
			_ => m_difficultyNormal, 
		};
	}

	public List<DifficultySettingEnum> GetDifficultySettingsEnums()
	{
		return new List<DifficultySettingEnum>
		{
			DifficultySettingEnum.Story,
			DifficultySettingEnum.Normal
		};
	}
}
