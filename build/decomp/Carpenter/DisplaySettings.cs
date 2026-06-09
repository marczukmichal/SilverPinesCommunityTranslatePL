using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public class DisplaySettings : BaseSettingsGroup
{
	[SerializeField]
	private TargetFPSMode m_targetFps;

	[SerializeField]
	private VSyncMode m_verticalSync;

	[SerializeField]
	private float m_uiScale = 1f;

	[SerializeField]
	private float m_gammaAdjustment;

	[SerializeField]
	private string m_activePostProcessing = "";

	private Resolution m_resolution;

	private FullScreenMode m_fullScreenMode;

	[SerializeField]
	private bool m_filmGrainNoise = true;

	public TargetFPSMode TargetFPS
	{
		get
		{
			return m_targetFps;
		}
		set
		{
			SetValue(ref m_targetFps, value);
		}
	}

	public VSyncMode VerticalSync
	{
		get
		{
			return m_verticalSync;
		}
		set
		{
			SetValue(ref m_verticalSync, value);
		}
	}

	public float UIScale
	{
		get
		{
			return m_uiScale;
		}
		set
		{
			SetValue(ref m_uiScale, value);
		}
	}

	public float GammaAdjustment
	{
		get
		{
			return m_gammaAdjustment;
		}
		set
		{
			SetValue(ref m_gammaAdjustment, value);
		}
	}

	public string ActivePostProcessing
	{
		get
		{
			return m_activePostProcessing;
		}
		set
		{
			SetValue(ref m_activePostProcessing, value);
		}
	}

	public Resolution Resolution
	{
		get
		{
			return m_resolution;
		}
		set
		{
			SetValue(ref m_resolution, value);
		}
	}

	public FullScreenMode FullScreenMode
	{
		get
		{
			return m_fullScreenMode;
		}
		set
		{
			SetValue(ref m_fullScreenMode, value);
		}
	}

	public bool FilmGrainNoise
	{
		get
		{
			return m_filmGrainNoise;
		}
		set
		{
			SetValue(ref m_filmGrainNoise, value);
		}
	}

	public void SetResolutionToCurrentResolution()
	{
		m_resolution = Screen.currentResolution;
		m_fullScreenMode = Screen.fullScreenMode;
	}

	public override void ApplySettings()
	{
		_ = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
		int targetFrameRate = -1;
		switch (m_targetFps)
		{
		case TargetFPSMode.TargetFPS_30:
			targetFrameRate = 30;
			break;
		case TargetFPSMode.TargetFPS_40:
			targetFrameRate = 40;
			break;
		case TargetFPSMode.TargetFPS_60:
			targetFrameRate = 60;
			break;
		case TargetFPSMode.TargetFPS_120:
			targetFrameRate = 120;
			break;
		case TargetFPSMode.TargetFPS_144:
			targetFrameRate = 144;
			break;
		}
		Application.targetFrameRate = targetFrameRate;
		int vSyncCount = 0;
		switch (m_verticalSync)
		{
		case VSyncMode.None:
			vSyncCount = 0;
			break;
		case VSyncMode.EveryVBlank:
			vSyncCount = 1;
			break;
		}
		QualitySettings.vSyncCount = vSyncCount;
		if (m_resolution.width <= 0 || m_resolution.width <= 0)
		{
			SetResolutionToCurrentResolution();
		}
		else
		{
			Screen.SetResolution(m_resolution.width, m_resolution.height, m_fullScreenMode);
		}
		m_isDirty = false;
		GlobalReferences.Instance.EventChannels.UserPreferences.GraphicsQualityChanged.Raise();
		GlobalReferences.Instance.EventChannels.UserPreferences.UIScaleChanged.Raise();
	}

	public override void SetDefaults()
	{
		m_verticalSync = VSyncMode.EveryVBlank;
		m_targetFps = TargetFPSMode.Unlimited;
		m_gammaAdjustment = 0f;
		m_resolution = Screen.currentResolution;
		m_fullScreenMode = Screen.fullScreenMode;
		m_activePostProcessing = "";
		m_uiScale = 1f;
		m_filmGrainNoise = true;
	}
}
