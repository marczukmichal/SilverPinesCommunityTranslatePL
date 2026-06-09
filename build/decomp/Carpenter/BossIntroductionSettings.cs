using System;
using UnityEngine;
using UnityEngine.Localization;

[Serializable]
[CreateAssetMenu(menuName = "Settings/Boss Introduction")]
public class BossIntroductionSettings : ScriptableObject
{
	[SerializeField]
	private LocalizedString m_mainNameString;

	[SerializeField]
	private LocalizedString m_subtitleString;

	public string MainName
	{
		get
		{
			if (!m_mainNameString.IsEmpty)
			{
				return m_mainNameString.GetLocalizedString();
			}
			return "";
		}
	}

	public string SubTitle
	{
		get
		{
			if (!m_subtitleString.IsEmpty)
			{
				return m_subtitleString.GetLocalizedString();
			}
			return "";
		}
	}
}
