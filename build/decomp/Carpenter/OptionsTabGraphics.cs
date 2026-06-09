using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Localization;

public class OptionsTabGraphics : OptionsMenuTab
{
	private GraphicsQualitySettings m_tempQualitySettings;

	private CycleButton m_graphicsPresetButton;

	private Dictionary<CycleButton, Func<int>> m_callbackDictionary;

	private SliderExtended m_renderScaleSlider;

	protected override void PopulateOptionsTab()
	{
		base.PopulateOptionsTab();
		m_callbackDictionary = new Dictionary<CycleButton, Func<int>>();
		UserPreferences userPreferences = GlobalReferences.Instance.UserPreferences;
		m_tempQualitySettings = userPreferences.GraphicsQualitySettings.Clone() as GraphicsQualitySettings;
		m_graphicsPresetButton = AddCycleButtonOption("GraphicsPreset", GetLocalisedNames(Enum.GetNames(typeof(GraphicsPresetLevel))), (int)GetCurrentGraphicsQualityLevel(), OnSetGraphicsQualityLevel, PlatformUtils.PCPlatforms);
		m_renderScaleSlider = AddSliderOption("RenderScale", GraphicsQualitySettings.MinRenderScale, GraphicsQualitySettings.MaxRenderScale, m_tempQualitySettings.RenderScale, 30, delegate(float x)
		{
			m_tempQualitySettings.RenderScale = x;
		}, SliderValueType.Percentilex100, PlatformUtils.PCPlatforms);
		AddGraphicsButtonOption("AntiAliasing", GetLocalisedNames(Enum.GetNames(typeof(AntiAliasingOption))), () => (int)m_tempQualitySettings.AntiAliasing, delegate(int x)
		{
			m_tempQualitySettings.AntiAliasing = (AntiAliasingOption)x;
		}, PlatformUtils.PCPlatforms);
		AddGraphicsButtonOption("Shadows", GetLocalisedNames(Enum.GetNames(typeof(GraphicsQualityLevel))), () => (int)m_tempQualitySettings.Shadow, delegate(int x)
		{
			m_tempQualitySettings.Shadow = (GraphicsQualityLevel)x;
		}, PlatformUtils.PCPlatforms);
		AddGraphicsButtonOption("WorldDetail", GetLocalisedNames(Enum.GetNames(typeof(GraphicsQualityLevel))), () => (int)m_tempQualitySettings.WorldDetail, delegate(int x)
		{
			m_tempQualitySettings.WorldDetail = (GraphicsQualityLevel)x;
		}, PlatformUtils.PCPlatforms);
		AddGraphicsButtonOption("Texture", GetLocalisedNames(Enum.GetNames(typeof(TextureQualityLevel))), () => (int)m_tempQualitySettings.TextureQuality, delegate(int x)
		{
			m_tempQualitySettings.TextureQuality = (TextureQualityLevel)x;
		}, PlatformUtils.PCPlatforms);
		AddGraphicsButtonOption("DepthOfField", GetLocalisedNames(Enum.GetNames(typeof(PostProcessingLevel))), () => (int)m_tempQualitySettings.DepthOfField, delegate(int x)
		{
			m_tempQualitySettings.DepthOfField = (PostProcessingLevel)x;
		}, PlatformUtils.PCPlatforms);
		AddGraphicsButtonOption("Bloom", GetLocalisedNames(Enum.GetNames(typeof(PostProcessingLevel))), () => (int)m_tempQualitySettings.Bloom, delegate(int x)
		{
			m_tempQualitySettings.Bloom = (PostProcessingLevel)x;
		});
		AddGraphicsButtonOption("Fog", GetLocalisedNames(Enum.GetNames(typeof(GraphicsQualityLevel))), () => (int)m_tempQualitySettings.Fog, delegate(int x)
		{
			m_tempQualitySettings.Fog = (GraphicsQualityLevel)x;
		}, PlatformUtils.PCPlatforms);
		AddGraphicsButtonOption("Water", GetLocalisedNames(Enum.GetNames(typeof(GraphicsQualityLevel))), () => (int)m_tempQualitySettings.Water, delegate(int x)
		{
			m_tempQualitySettings.Water = (GraphicsQualityLevel)x;
		}, PlatformUtils.PCPlatforms);
		AddGraphicsButtonOption("CameraRain", GetLocalisedNames(Enum.GetNames(typeof(PostProcessingLevel))), () => (int)m_tempQualitySettings.CameraRain, delegate(int x)
		{
			m_tempQualitySettings.CameraRain = (PostProcessingLevel)x;
		});
		AddApplyButton();
	}

	protected override void OnApplyChanges()
	{
		base.OnApplyChanges();
		if (m_tempQualitySettings.IsDirty)
		{
			UserPreferences userPreferences = GlobalReferences.Instance.UserPreferences;
			userPreferences.SetDirtyFlag();
			userPreferences.ApplyGraphicsQualitySettings(m_tempQualitySettings);
			m_tempQualitySettings = m_tempQualitySettings.Clone() as GraphicsQualitySettings;
		}
	}

	private void AddGraphicsButtonOption(string label, List<LocalizedString> options, Func<int> currentIndexFunction, UnityAction<int> action, PlatformUtils.Platforms[] platforms = null)
	{
		CycleButton cycleButton = AddCycleButtonOption(label, options, currentIndexFunction(), action, platforms);
		if (cycleButton != null)
		{
			cycleButton.m_onValueChanged = (UnityAction<int>)Delegate.Combine(cycleButton.m_onValueChanged, new UnityAction<int>(OnGraphicsValueChanged));
			m_callbackDictionary.Add(cycleButton, currentIndexFunction);
		}
	}

	private void OnGraphicsValueChanged(int changed)
	{
		RefreshGraphicsPresetState();
	}

	private void RefreshGraphicsPresetState()
	{
		m_graphicsPresetButton.Value = (int)GetCurrentGraphicsQualityLevel();
	}

	private GraphicsPresetLevel GetCurrentGraphicsQualityLevel()
	{
		if (m_tempQualitySettings.MatchesPreset(GlobalReferences.Instance.GraphicsSettings.HighQualityPreset.m_settings))
		{
			return GraphicsPresetLevel.High;
		}
		if (m_tempQualitySettings.MatchesPreset(GlobalReferences.Instance.GraphicsSettings.MediumQualityPreset.m_settings))
		{
			return GraphicsPresetLevel.Medium;
		}
		if (m_tempQualitySettings.MatchesPreset(GlobalReferences.Instance.GraphicsSettings.LowQualityPreset.m_settings))
		{
			return GraphicsPresetLevel.Low;
		}
		return GraphicsPresetLevel.Custom;
	}

	private void OnSetGraphicsQualityLevel(int value)
	{
		switch (value)
		{
		case 2:
			m_tempQualitySettings.CopyValues(GlobalReferences.Instance.GraphicsSettings.HighQualityPreset.m_settings);
			break;
		case 1:
			m_tempQualitySettings.CopyValues(GlobalReferences.Instance.GraphicsSettings.MediumQualityPreset.m_settings);
			break;
		case 0:
			m_tempQualitySettings.CopyValues(GlobalReferences.Instance.GraphicsSettings.LowQualityPreset.m_settings);
			break;
		}
		foreach (KeyValuePair<CycleButton, Func<int>> item in m_callbackDictionary)
		{
			item.Key.Value = item.Value();
		}
		if (m_renderScaleSlider != null)
		{
			m_renderScaleSlider.SetValue(m_tempQualitySettings.RenderScale);
		}
	}
}
