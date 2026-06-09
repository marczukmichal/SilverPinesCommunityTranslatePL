using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;

public class OptionsTabDisplay : OptionsMenuTab
{
	[Header("Display")]
	[SerializeField]
	private UserSelectablePostProcessingSettings m_postProcessingSettings;

	private DisplaySettings m_tempDisplaySettings;

	private CycleButton m_targetFPSButton;

	private SliderExtended m_brightnessSlider;

	private List<Resolution> m_resolutions;

	private static List<Resolution> GetUniqueResolutionsWithBestRefreshRate()
	{
		return (from res in Screen.resolutions
			group res by new Vector2Int(res.width, res.height) into @group
			select @group.OrderByDescending((Resolution r) => r.refreshRateRatio).First()).ToList();
	}

	protected override void PopulateOptionsTab()
	{
		base.PopulateOptionsTab();
		UserPreferences userPreferences = GlobalReferences.Instance.UserPreferences;
		userPreferences.DisplaySettings.SetResolutionToCurrentResolution();
		m_tempDisplaySettings = userPreferences.DisplaySettings.Clone() as DisplaySettings;
		m_resolutions = GetUniqueResolutionsWithBestRefreshRate();
		List<LocalizedString> localisedNames = GetLocalisedNames(Enum.GetNames(typeof(FullScreenMode)));
		int fullScreenMode = (int)m_tempDisplaySettings.FullScreenMode;
		AddCycleButtonOption("Display Mode", localisedNames, fullScreenMode, delegate(int x)
		{
			m_tempDisplaySettings.FullScreenMode = (FullScreenMode)x;
		}, PlatformUtils.PCPlatforms);
		List<string> list = new List<string>();
		int startingIndex = 0;
		int num = 0;
		foreach (Resolution resolution in m_resolutions)
		{
			list.Add(resolution.width + " x " + resolution.height);
			if (resolution.height == Screen.currentResolution.height && resolution.width == Screen.currentResolution.width)
			{
				startingIndex = num;
			}
			num++;
		}
		AddCycleButtonOption("Resolution", list, startingIndex, delegate(int x)
		{
			m_tempDisplaySettings.Resolution = m_resolutions[x];
		}, PlatformUtils.PCPlatforms);
		CycleButton cycleButton = AddCycleButtonOption("Vertical Sync", GetLocalisedNames(Enum.GetNames(typeof(VSyncMode))), (int)m_tempDisplaySettings.VerticalSync, delegate(int x)
		{
			m_tempDisplaySettings.VerticalSync = (VSyncMode)x;
		}, PlatformUtils.PCPlatforms);
		if (cycleButton != null)
		{
			cycleButton.m_onValueChanged = (UnityAction<int>)Delegate.Combine(cycleButton.m_onValueChanged, new UnityAction<int>(OnVsyncOptionChanged));
		}
		m_targetFPSButton = AddCycleButtonOption("Max FPS", GetLocalisedNames(Enum.GetNames(typeof(TargetFPSMode))), (int)m_tempDisplaySettings.TargetFPS, delegate(int x)
		{
			m_tempDisplaySettings.TargetFPS = (TargetFPSMode)x;
		}, PlatformUtils.PCPlatforms);
		AddSliderOption("UIScale", 0.75f, 1f, m_tempDisplaySettings.UIScale, 5, delegate(float x)
		{
			m_tempDisplaySettings.UIScale = x;
		}, SliderValueType.Percentilex100);
		AddToggleOption("FilmGrainNoise", m_tempDisplaySettings.FilmGrainNoise, delegate(bool x)
		{
			m_tempDisplaySettings.FilmGrainNoise = x;
		});
		m_brightnessSlider = AddSliderOption("Brightness", -1f, 1f, m_tempDisplaySettings.GammaAdjustment, 40, ApplyTemporaryGammaSettings, SliderValueType.Percentilex100);
		AddApplyButton();
		OnVsyncOptionChanged((int)m_tempDisplaySettings.VerticalSync);
	}

	private void OnVsyncOptionChanged(int vsyncOption)
	{
		if (m_targetFPSButton != null)
		{
			m_targetFPSButton.interactable = vsyncOption == 0;
		}
	}

	private void OnPostProcessingChanges(int newIndex)
	{
		m_tempDisplaySettings.ActivePostProcessing = m_postProcessingSettings.GetOptionForIndex(newIndex).ID;
	}

	private void ApplyTemporaryGammaSettings(float newGamma)
	{
		m_tempDisplaySettings.GammaAdjustment = newGamma;
		GlobalReferences.Instance.EventChannels.UserPreferences.SetTemporaryGamma.Raise(newGamma);
	}

	protected override void OnApplyChanges()
	{
		base.OnApplyChanges();
		if (m_tempDisplaySettings.IsDirty)
		{
			UserPreferences userPreferences = GlobalReferences.Instance.UserPreferences;
			userPreferences.SetDirtyFlag();
			userPreferences.ApplyDisplaySettings(m_tempDisplaySettings);
			m_tempDisplaySettings = m_tempDisplaySettings.Clone() as DisplaySettings;
		}
	}
}
