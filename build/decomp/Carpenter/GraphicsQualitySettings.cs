using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[Serializable]
public class GraphicsQualitySettings : BaseSettingsGroup
{
	[SerializeField]
	private GraphicsQualityLevel m_lightingQuality;

	[SerializeField]
	private GraphicsQualityLevel m_shadowQuality;

	[SerializeField]
	private GraphicsQualityLevel m_worldDetail;

	[SerializeField]
	private GraphicsQualityLevel m_effects;

	[SerializeField]
	private TextureQualityLevel m_textureQuality;

	[SerializeField]
	private PostProcessingLevel m_depthOfField;

	[SerializeField]
	private PostProcessingLevel m_bloom;

	[SerializeField]
	private GraphicsQualityLevel m_fog;

	[SerializeField]
	private GraphicsQualityLevel m_water;

	[SerializeField]
	private AntiAliasingOption m_antiAliasing;

	[SerializeField]
	private float m_renderScale;

	public static float MaxRenderScale = 2f;

	public static float MinRenderScale = 0.5f;

	[SerializeField]
	private bool m_ssao;

	[SerializeField]
	private PostProcessingLevel m_cameraRain;

	private UniversalRenderPipelineAsset m_tempURPAsset;

	public GraphicsQualityLevel Lighting
	{
		get
		{
			return m_lightingQuality;
		}
		set
		{
			SetValue(ref m_lightingQuality, value);
		}
	}

	public GraphicsQualityLevel Shadow
	{
		get
		{
			return m_shadowQuality;
		}
		set
		{
			SetValue(ref m_shadowQuality, value);
		}
	}

	public GraphicsQualityLevel WorldDetail
	{
		get
		{
			return m_worldDetail;
		}
		set
		{
			SetValue(ref m_worldDetail, value);
		}
	}

	public GraphicsQualityLevel Effects
	{
		get
		{
			return m_effects;
		}
		set
		{
			SetValue(ref m_effects, value);
		}
	}

	public TextureQualityLevel TextureQuality
	{
		get
		{
			return m_textureQuality;
		}
		set
		{
			SetValue(ref m_textureQuality, value);
		}
	}

	public PostProcessingLevel DepthOfField
	{
		get
		{
			return m_depthOfField;
		}
		set
		{
			SetValue(ref m_depthOfField, value);
		}
	}

	public PostProcessingLevel Bloom
	{
		get
		{
			return m_bloom;
		}
		set
		{
			SetValue(ref m_bloom, value);
		}
	}

	public GraphicsQualityLevel Fog
	{
		get
		{
			return m_fog;
		}
		set
		{
			SetValue(ref m_fog, value);
		}
	}

	public GraphicsQualityLevel Water
	{
		get
		{
			return m_water;
		}
		set
		{
			SetValue(ref m_water, value);
		}
	}

	public AntiAliasingOption AntiAliasing
	{
		get
		{
			return m_antiAliasing;
		}
		set
		{
			SetValue(ref m_antiAliasing, value);
		}
	}

	public float RenderScale
	{
		get
		{
			return m_renderScale;
		}
		set
		{
			SetValue(ref m_renderScale, value);
		}
	}

	public bool SSAO
	{
		get
		{
			return m_ssao;
		}
		set
		{
			SetValue(ref m_ssao, value);
		}
	}

	public PostProcessingLevel CameraRain
	{
		get
		{
			return m_cameraRain;
		}
		set
		{
			SetValue(ref m_cameraRain, value);
		}
	}

	public override void ApplySettings()
	{
		UniversalRenderPipelineAsset universalRenderPipelineAsset = null;
		universalRenderPipelineAsset = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;
		switch (m_shadowQuality)
		{
		case GraphicsQualityLevel.High:
			universalRenderPipelineAsset = GlobalReferences.Instance.GraphicsSettings.HighQualityShadowsPipelineAsset;
			break;
		case GraphicsQualityLevel.Medium:
			universalRenderPipelineAsset = GlobalReferences.Instance.GraphicsSettings.MediumQualityShadowsPipelineAsset;
			break;
		case GraphicsQualityLevel.Low:
			universalRenderPipelineAsset = GlobalReferences.Instance.GraphicsSettings.LowQualityShadowsPipelineAsset;
			break;
		}
		UniversalRenderPipelineAsset tempURPAsset = m_tempURPAsset;
		m_tempURPAsset = UnityEngine.Object.Instantiate(universalRenderPipelineAsset);
		m_tempURPAsset.renderScale = Mathf.Clamp(m_renderScale, MinRenderScale, MaxRenderScale);
		QualitySettings.renderPipeline = m_tempURPAsset;
		if (tempURPAsset != null)
		{
			UnityEngine.Object.Destroy(tempURPAsset);
		}
		switch (m_textureQuality)
		{
		case TextureQualityLevel.High:
			QualitySettings.globalTextureMipmapLimit = 0;
			break;
		case TextureQualityLevel.Medium:
			QualitySettings.globalTextureMipmapLimit = 1;
			break;
		case TextureQualityLevel.Low:
			QualitySettings.globalTextureMipmapLimit = 2;
			break;
		case TextureQualityLevel.VeryLow:
			QualitySettings.globalTextureMipmapLimit = 3;
			break;
		}
		if (GlobalReferences.Instance.ScreenSpaceAmbientOcclusionRenderFeature != null)
		{
			GlobalReferences.Instance.ScreenSpaceAmbientOcclusionRenderFeature.SetActive(m_ssao);
		}
		m_isDirty = false;
		GlobalReferences.Instance.EventChannels.UserPreferences.GraphicsQualityChanged.Raise();
	}

	public bool MatchesPreset(GraphicsQualitySettings presetLevel)
	{
		if (!Mathf.Approximately(m_renderScale, presetLevel.m_renderScale))
		{
			return false;
		}
		if (m_antiAliasing != presetLevel.m_antiAliasing)
		{
			return false;
		}
		if (m_fog != presetLevel.m_fog)
		{
			return false;
		}
		if (m_effects != presetLevel.m_effects)
		{
			return false;
		}
		if (m_depthOfField != presetLevel.m_depthOfField)
		{
			return false;
		}
		if (m_bloom != presetLevel.m_bloom)
		{
			return false;
		}
		if (m_worldDetail != presetLevel.m_worldDetail)
		{
			return false;
		}
		if (m_water != presetLevel.m_water)
		{
			return false;
		}
		if (m_textureQuality != presetLevel.m_textureQuality)
		{
			return false;
		}
		if (m_shadowQuality != presetLevel.m_shadowQuality)
		{
			return false;
		}
		if (m_lightingQuality != presetLevel.m_lightingQuality)
		{
			return false;
		}
		if (m_ssao != presetLevel.m_ssao)
		{
			return false;
		}
		if (m_cameraRain != presetLevel.m_cameraRain)
		{
			return false;
		}
		return true;
	}

	public void CopyValues(GraphicsQualitySettings presetLevel, bool setDirty = true)
	{
		m_lightingQuality = presetLevel.m_lightingQuality;
		m_shadowQuality = presetLevel.m_shadowQuality;
		m_textureQuality = presetLevel.m_textureQuality;
		m_water = presetLevel.m_water;
		m_worldDetail = presetLevel.m_worldDetail;
		m_antiAliasing = presetLevel.m_antiAliasing;
		m_bloom = presetLevel.m_bloom;
		m_depthOfField = presetLevel.m_depthOfField;
		m_effects = presetLevel.m_effects;
		m_fog = presetLevel.m_fog;
		m_renderScale = presetLevel.m_renderScale;
		m_ssao = presetLevel.m_ssao;
		m_cameraRain = presetLevel.m_cameraRain;
		if (setDirty)
		{
			m_isDirty = true;
		}
	}

	public override void SetDefaults()
	{
		int graphicsMemorySize = SystemInfo.graphicsMemorySize;
		int processorCount = SystemInfo.processorCount;
		int systemMemorySize = SystemInfo.systemMemorySize;
		int num = 3;
		if (graphicsMemorySize < 2000)
		{
			num = 1;
		}
		else if (graphicsMemorySize < 4000)
		{
			num = 2;
		}
		int num2 = 3;
		if (processorCount <= 2)
		{
			num2 = 1;
		}
		else if (processorCount <= 4)
		{
			num2 = 2;
		}
		int num3 = 3;
		if (systemMemorySize <= 3500)
		{
			num3 = 1;
		}
		else if (systemMemorySize <= 7000)
		{
			num3 = 2;
		}
		int num4 = Mathf.Min(num, num2, num3);
		GraphicsPresetLevel graphicsPresetLevel = GraphicsPresetLevel.High;
		switch (num4)
		{
		case 3:
			graphicsPresetLevel = GraphicsPresetLevel.High;
			break;
		case 2:
			graphicsPresetLevel = GraphicsPresetLevel.Medium;
			break;
		case 1:
			graphicsPresetLevel = GraphicsPresetLevel.Low;
			break;
		}
		Debug.Log("Setting defaults based on the following values: VRAM: " + graphicsMemorySize + " CPU Count:" + processorCount + " System Memory: " + systemMemorySize + "- Selected graphics quality is: " + graphicsPresetLevel);
		switch (graphicsPresetLevel)
		{
		case GraphicsPresetLevel.High:
			CopyValues(GlobalReferences.Instance.GraphicsSettings.HighQualityPreset.m_settings, setDirty: false);
			break;
		case GraphicsPresetLevel.Medium:
			CopyValues(GlobalReferences.Instance.GraphicsSettings.MediumQualityPreset.m_settings, setDirty: false);
			break;
		case GraphicsPresetLevel.Low:
			CopyValues(GlobalReferences.Instance.GraphicsSettings.LowQualityPreset.m_settings, setDirty: false);
			break;
		}
	}
}
