public class OptionsTabAccessibility : OptionsMenuTab
{
	protected override void PopulateOptionsTab()
	{
		base.PopulateOptionsTab();
		UserPreferences userPreferences = GlobalReferences.Instance.UserPreferences;
		AddToggleOption("VOSubtitles", userPreferences.VOSubtitles, delegate(bool x)
		{
			userPreferences.VOSubtitles = x;
		});
		AddToggleOption("AutomateItemInteractMinigame", userPreferences.AutomateItemInteractMinigame, delegate(bool x)
		{
			userPreferences.AutomateItemInteractMinigame = x;
		});
		AddToggleOption("SkipButtonMashEvents", userPreferences.SkipButtonMashEvents, delegate(bool x)
		{
			userPreferences.SkipButtonMashEvents = x;
		});
	}
}
