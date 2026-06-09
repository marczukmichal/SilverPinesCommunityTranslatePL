using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "Misc/UI HealthStatusSettings")]
public class HealthStatusSettings : ScriptableObject
{
	[Serializable]
	public class HealthStatusGroup
	{
		public float m_maxHealth;

		public LocalizedString m_statusLabel;

		public Sprite m_image;

		public Color m_color;

		public bool m_alwaysShowBarPopup;
	}

	[SerializeField]
	private List<HealthStatusGroup> m_statuses;

	public HealthStatusGroup GetStatusFromHealthPercent(float healthPercentage)
	{
		for (int i = 0; i < m_statuses.Count - 1; i++)
		{
			if (healthPercentage < m_statuses[i].m_maxHealth)
			{
				return m_statuses[i];
			}
		}
		return m_statuses[m_statuses.Count - 1];
	}
}
