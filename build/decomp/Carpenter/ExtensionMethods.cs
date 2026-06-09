using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class ExtensionMethods
{
	public static float ClampToVector2(this float value, Vector2 minMax)
	{
		return Mathf.Clamp(value, minMax.x, minMax.y);
	}

	public static float SnapToGrid(this float value, float gridSize)
	{
		value /= gridSize;
		value = Mathf.Round(value);
		value *= gridSize;
		return value;
	}

	public static float GetRandom(this Vector2 minMax)
	{
		return UnityEngine.Random.Range(minMax.x, minMax.y);
	}

	public static int GetRandom(this Vector2Int minMax)
	{
		return UnityEngine.Random.Range(minMax.x, minMax.y);
	}

	public static float Remap(this float value, float from1, float to1, float from2, float to2)
	{
		return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
	}

	public static Vector2 Rotate(this Vector2 v, float degrees)
	{
		float num = Mathf.Sin(degrees * (MathF.PI / 180f));
		float num2 = Mathf.Cos(degrees * (MathF.PI / 180f));
		float x = v.x;
		float y = v.y;
		v.x = num2 * x - num * y;
		v.y = num * x + num2 * y;
		return v;
	}

	public static BaseCharacterInput GetCharacterInputComponent(this GameObject gameObject)
	{
		BaseCharacterInput[] components = gameObject.GetComponents<BaseCharacterInput>();
		BaseCharacterInput[] array = components;
		foreach (BaseCharacterInput baseCharacterInput in array)
		{
			if (baseCharacterInput is CharacterInputCompanion)
			{
				return baseCharacterInput;
			}
		}
		if (components.Length == 0)
		{
			return null;
		}
		return components[0];
	}

	public static bool HasAsset(this AssetReference assetReference)
	{
		if (assetReference != null)
		{
			return !string.IsNullOrEmpty(assetReference.AssetGUID);
		}
		return false;
	}

	public static Vector2 RadianToVector2(float radian)
	{
		return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
	}

	public static Vector2 DegreeToVector2(float degree)
	{
		return RadianToVector2(degree * (MathF.PI / 180f));
	}
}
