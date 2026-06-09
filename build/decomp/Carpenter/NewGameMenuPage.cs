using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NewGameMenuPage : MenuPage
{
	[SerializeField]
	private CycleButton m_difficultyDropdown;

	[SerializeField]
	private Image m_difficultyImage;

	[SerializeField]
	private TextMeshProUGUI m_difficultyDescriptionText;

	[SerializeField]
	private AudioEvent m_selectedStoryAudioEvent;

	[SerializeField]
	private AudioEvent m_selectedNormalAudioEvent;

	private DifficultySettingEnum m_currentlySelectedDifficulty;

	private List<DifficultySettingEnum> m_difficulties;

	public DifficultySettingEnum SelectedDifficulty => m_currentlySelectedDifficulty;

	protected override void Awake()
	{
		base.Awake();
		CycleButton difficultyDropdown = m_difficultyDropdown;
		difficultyDropdown.m_onValueChanged = (UnityAction<int>)Delegate.Combine(difficultyDropdown.m_onValueChanged, new UnityAction<int>(OnDifficultyValueChanged));
	}

	private void OnEnable()
	{
		m_difficultyDropdown.ClearOptions();
		m_difficulties = GlobalReferences.Instance.GameDifficultySettings.GetDifficultySettingsEnums();
		List<string> list = new List<string>();
		foreach (DifficultySettingEnum difficulty in m_difficulties)
		{
			DifficultySettings difficultyModeConfiguration = GlobalReferences.Instance.GameDifficultySettings.GetDifficultyModeConfiguration(difficulty);
			list.Add(difficultyModeConfiguration.DifficultyNameString.GetLocalizedString());
		}
		m_difficultyDropdown.AddOptions(list);
		m_difficultyDropdown.Value = m_difficulties.IndexOf(m_currentlySelectedDifficulty);
		UpdateForDifficultyOption(m_currentlySelectedDifficulty);
	}

	private void OnDifficultyValueChanged(int newValue)
	{
		m_currentlySelectedDifficulty = m_difficulties[newValue];
		switch (m_currentlySelectedDifficulty)
		{
		case DifficultySettingEnum.Normal:
			m_selectedNormalAudioEvent?.Play2D();
			break;
		case DifficultySettingEnum.Story:
			m_selectedStoryAudioEvent?.Play2D();
			break;
		}
		UpdateForDifficultyOption(m_currentlySelectedDifficulty);
	}

	private void UpdateForDifficultyOption(DifficultySettingEnum difficulty)
	{
		DifficultySettings difficultyModeConfiguration = GlobalReferences.Instance.GameDifficultySettings.GetDifficultyModeConfiguration(difficulty);
		m_difficultyImage.sprite = difficultyModeConfiguration.DifficultySelectSprite;
		m_difficultyDescriptionText.text = difficultyModeConfiguration.DifficultyDescriptionString.GetLocalizedString();
	}
}
