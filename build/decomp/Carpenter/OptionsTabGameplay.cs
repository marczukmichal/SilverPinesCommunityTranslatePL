using System.Collections.Generic;
using UnityEngine.Localization;

public class OptionsTabGameplay : OptionsMenuTab
{
	private CycleButton m_difficultyDropdown;

	private DifficultySettingEnum m_targetDifficulty;

	private bool m_difficultyChanged;

	private List<DifficultySettingEnum> m_difficulties;

	protected override void PopulateOptionsTab()
	{
		base.PopulateOptionsTab();
		UserPreferences userPreferences = GlobalReferences.Instance.UserPreferences;
		AddToggleOption("Run Toggle", userPreferences.RunToggle, delegate(bool x)
		{
			userPreferences.RunToggle = x;
		});
		AddToggleOption("Crouch Toggle", userPreferences.CrouchToggle, delegate(bool x)
		{
			userPreferences.CrouchToggle = x;
		});
		AddToggleOption("Quick Map Toggle", userPreferences.QuickMapToggle, delegate(bool x)
		{
			userPreferences.QuickMapToggle = x;
		});
		AddToggleOption("Hints", userPreferences.ShowHints, delegate(bool x)
		{
			userPreferences.ShowHints = x;
		});
		m_difficulties = GlobalReferences.Instance.GameDifficultySettings.GetDifficultySettingsEnums();
		List<LocalizedString> list = new List<LocalizedString>();
		foreach (DifficultySettingEnum difficulty in m_difficulties)
		{
			DifficultySettings difficultyModeConfiguration = GlobalReferences.Instance.GameDifficultySettings.GetDifficultyModeConfiguration(difficulty);
			list.Add(difficultyModeConfiguration.DifficultyNameString);
		}
		m_difficultyDropdown = AddCycleButtonOption("DifficultySelect", list, 0, OnDifficultyChanged);
		AddGroupHeaderOption("Mouse", PlatformUtils.PCPlatforms);
		AddSliderOption("Generic Aim Sensitivity", 0.1f, 1f, userPreferences.MouseAimingSensitivity, 18, delegate(float x)
		{
			userPreferences.MouseAimingSensitivity = x;
		}, SliderValueType.Normal, PlatformUtils.PCPlatforms);
		AddGroupHeaderOption("Controller", PlatformUtils.PCPlatforms);
		AddSliderOption("Generic Aim Sensitivity", 0.1f, 1f, userPreferences.GamepadAimingSensitivity, 18, delegate(float x)
		{
			userPreferences.GamepadAimingSensitivity = x;
		}, SliderValueType.Normal);
		string label = ((!PlatformUtils.HasPlatformFlag(MaskUtils.ConvertToMask(new PlatformUtils.Platforms[2]
		{
			PlatformUtils.Platforms.Switch,
			PlatformUtils.Platforms.Switch2
		}))) ? "Controller Vibration" : "Controller Rumble");
		AddToggleOption(label, userPreferences.ControllerRumble, delegate(bool x)
		{
			userPreferences.ControllerRumble = x;
		});
	}

	private void OnEnable()
	{
		m_difficultyChanged = false;
		bool flag = GlobalReferences.Instance.GameState.IsInState(GameState.GameStateFlag.GameActive);
		m_difficultyDropdown.gameObject.SetActive(flag);
		if (flag)
		{
			m_difficultyDropdown.Value = m_difficulties.IndexOf(GlobalReferences.Instance.DataStore.Data.CurrentDifficulty);
		}
	}

	protected override void OnApplyChanges()
	{
		base.OnApplyChanges();
		if (m_difficultyChanged)
		{
			GlobalReferences.Instance.DataStore.ChangeDifficulty(m_targetDifficulty);
		}
	}

	private void OnDifficultyChanged(int newDifficulty)
	{
		m_difficultyChanged = true;
		m_targetDifficulty = m_difficulties[newDifficulty];
	}
}
