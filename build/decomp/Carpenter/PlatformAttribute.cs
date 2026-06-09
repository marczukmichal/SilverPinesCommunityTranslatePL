using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class PlatformAttribute : PropertyAttribute
{
	public Type EnumType;

	public PlatformAttribute(Type _enumType)
	{
		EnumType = _enumType;
	}
}
