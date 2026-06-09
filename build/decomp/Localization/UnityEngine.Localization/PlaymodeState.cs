namespace UnityEngine.Localization;

internal static class PlaymodeState
{
	public static bool IsChangingPlayMode
	{
		get
		{
			if (IsPlayingOrWillChangePlaymode)
			{
				return !IsPlaying;
			}
			return false;
		}
	}

	public static bool IsPlayingOrWillChangePlaymode => true;

	public static bool IsPlaying => Application.isPlaying;
}
