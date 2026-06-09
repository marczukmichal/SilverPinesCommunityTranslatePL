using UnityEngine;
using UnityEngine.Localization;

public static class LocalizationUtility
{
	[DebugCommand("localization_warning", "Show big error when localization is missing", "localization_warning <true/false>", typeof(bool), false)]
	public static bool LOCALIZATION_WARNING;

	public static string GetLocalizedString(LocalizedString locString, string rawString, GameObject errorObject = null)
	{
		if (locString != null && !locString.IsEmpty)
		{
			return locString.GetLocalizedString();
		}
		if ((bool)errorObject)
		{
			Debug.LogWarning("Not localised string in [" + rawString + "] in GameObject (" + errorObject.name + " in scene " + errorObject.scene.name + ")");
		}
		else
		{
			Debug.LogWarning("Not localised string in [" + rawString + "]");
		}
		if (LOCALIZATION_WARNING && !string.IsNullOrEmpty(rawString))
		{
			return "<color=purple><b>[NOT LOCALIZED]</b>" + rawString + "</color>";
		}
		return rawString;
	}
}
