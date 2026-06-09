using System;
using UnityEngine;
using UnityEngine.Localization;

[Serializable]
[CreateAssetMenu(menuName = "Settings/Level Region Settings")]
public class LevelRegionSettings : ScriptableObject
{
	[SerializeField]
	private LocalizedString m_displayNameStringReference;

	[SerializeField]
	private bool m_showLevelIntro;

	public LocalizedString DisplayNameLocalizedString => m_displayNameStringReference;

	public string DisplayName
	{
		get
		{
			if (m_displayNameStringReference.TableReference.ReferenceType != 0)
			{
				return m_displayNameStringReference.GetLocalizedString();
			}
			return string.Empty;
		}
	}

	public bool ShowLevelIntro => m_showLevelIntro;
}
