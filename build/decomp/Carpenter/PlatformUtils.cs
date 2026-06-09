using UnityEngine;

public class PlatformUtils : MonoBehaviour
{
	public enum Platforms
	{
		Steam,
		Winstore,
		EGS,
		XBSeriesX,
		PS5,
		Switch,
		Switch2
	}

	public static readonly int s_PlatformCount = 7;

	public static Platforms[] PCPlatforms = new Platforms[3]
	{
		Platforms.Steam,
		Platforms.Winstore,
		Platforms.EGS
	};

	public static Platforms[] ConsolePlatforms = new Platforms[4]
	{
		Platforms.XBSeriesX,
		Platforms.PS5,
		Platforms.Switch,
		Platforms.Switch2
	};

	public static bool HasPlatformFlag(int _mask)
	{
		Debug.LogWarning("Warning: Current Platform Configuration not supported, defaulting to Steam");
		return MaskUtils.HasFlag(_mask, Platforms.Steam);
	}
}
