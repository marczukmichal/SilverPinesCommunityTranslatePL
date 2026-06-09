using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Settings/User Selectable Post Processing Settings")]
public class UserSelectablePostProcessingSettings : ScriptableObject
{
	[Serializable]
	public class PostProcessingOption
	{
		[SerializeField]
		private VolumeProfile m_volumeProfile;

		[SerializeField]
		private LocalizedString m_modeName;

		[SerializeField]
		private string m_id;

		public VolumeProfile PostProcessingProfile => m_volumeProfile;

		public string ModeName => m_modeName.GetLocalizedString();

		public string ID => m_id;
	}

	[SerializeField]
	private PostProcessingOption[] m_options;

	public VolumeProfile GetProfile(string id)
	{
		PostProcessingOption[] options = m_options;
		foreach (PostProcessingOption postProcessingOption in options)
		{
			if (postProcessingOption.ID.Equals(id))
			{
				return postProcessingOption.PostProcessingProfile;
			}
		}
		return null;
	}

	public PostProcessingOption GetOptionForIndex(int index)
	{
		return m_options[index];
	}

	public int GetIndexForID(string id)
	{
		for (int i = 0; i < m_options.Length; i++)
		{
			if (m_options[i].ID.Equals(id))
			{
				return i;
			}
		}
		return 0;
	}

	public List<string> GetNames()
	{
		List<string> list = new List<string>();
		PostProcessingOption[] options = m_options;
		foreach (PostProcessingOption postProcessingOption in options)
		{
			list.Add(postProcessingOption.ModeName);
		}
		return list;
	}
}
