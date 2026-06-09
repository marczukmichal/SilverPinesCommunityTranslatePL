public class OptionsTabAudio : OptionsMenuTab
{
	protected override void PopulateOptionsTab()
	{
		base.PopulateOptionsTab();
		UserPreferences userPreferences = GlobalReferences.Instance.UserPreferences;
		AddSliderOption("Master Volume", 0f, 1f, userPreferences.MasterVolume, 10, delegate(float x)
		{
			userPreferences.MasterVolume = x;
		}, SliderValueType.Percentilex100);
		AddSliderOption("Ambience Volume", 0f, 1f, userPreferences.AmbientVolume, 10, delegate(float x)
		{
			userPreferences.AmbientVolume = x;
		}, SliderValueType.Percentilex100);
		AddSliderOption("Music Volume", 0f, 1f, userPreferences.MusicVolume, 10, delegate(float x)
		{
			userPreferences.MusicVolume = x;
		}, SliderValueType.Percentilex100);
		AddSliderOption("Effects Volume", 0f, 1f, userPreferences.EffectsVolume, 10, delegate(float x)
		{
			userPreferences.EffectsVolume = x;
		}, SliderValueType.Percentilex100);
		AddSliderOption("Voice Volume", 0f, 1f, userPreferences.VoiceVolume, 10, delegate(float x)
		{
			userPreferences.VoiceVolume = x;
		}, SliderValueType.Percentilex100);
	}
}
