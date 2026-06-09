using System.Collections;
using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class OptionsTabSystem : OptionsMenuTab
{
	private List<string> m_controllerPromptTypes = new List<string> { "Auto-Detect", "Xbox", "PlayStation", "Switch" };

	protected override void PopulateOptionsTab()
	{
		base.PopulateOptionsTab();
		UserPreferences userPreferences = GlobalReferences.Instance.UserPreferences;
		AddCycleButtonOption("ButtonPromptType", m_controllerPromptTypes, (int)userPreferences.InputPromptGamepadDetectionMode, delegate(int x)
		{
			userPreferences.InputPromptGamepadDetectionMode = (InputPromptGamepadDetectionMode)x;
		});
		StartCoroutine(LanguageSetup());
	}

	private IEnumerator LanguageSetup()
	{
		yield return LocalizationSettings.InitializationOperation;
		List<string> list = new List<string>();
		int startingIndex = 0;
		for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
		{
			UnityEngine.Localization.Locale locale = LocalizationSettings.AvailableLocales.Locales[i];
			if (LocalizationSettings.SelectedLocale == locale)
			{
				startingIndex = i;
			}
			if (locale.Identifier.Code.Equals("en"))
			{
				string item = locale.name;
				list.Add(item);
			}
		}
		AddCycleButtonOption("Language", list, startingIndex, LocaleSelected);
	}

	private void LocaleSelected(int index)
	{
		GlobalReferences.Instance.UserPreferences.Language = LocalizationSettings.AvailableLocales.Locales[index].Identifier.Code;
		GlobalReferences.Instance.EventChannels.UserPreferences.UpdateLocalization.Raise();
	}
}
