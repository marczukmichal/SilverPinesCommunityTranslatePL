using UnityEngine;

public class ObjectVisibilityLevelSettings : ScriptableObject
{
	[SerializeField]
	private bool m_isDark;

	public bool IsDark => m_isDark;
}
