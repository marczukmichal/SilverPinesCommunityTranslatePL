using System;
using UnityEngine;
using UnityEngine.Localization.Settings;

[CreateAssetMenu(menuName = "Misc/User Preferences")]
public class UserPreferences : ScriptableObject
{
	[NonSerialized]
	private bool m_isDirty;

	[SerializeField]
	private float m_masterVolume = 1f;

	[SerializeField]
	private float m_effectsVolume = 1f;

	[SerializeField]
	private float m_ambientVolume = 0.7f;

	[SerializeField]
	private float m_musicVolume = 0.8f;

	[SerializeField]
	private float m_voiceVolume = 1f;

	[SerializeField]
	private bool m_runToggle;

	[SerializeField]
	private bool m_crouchToggle = true;

	[SerializeField]
	private bool m_quickMapToggle = true;

	[SerializeField]
	private float m_mouseAimingSensitivity = 0.4f;

	[SerializeField]
	private float m_gamepadAimingSensitivity = 0.5f;

	[SerializeField]
	private bool m_controllerRumble = true;

	[SerializeField]
	private InputPromptGamepadDetectionMode m_inputPromptGamepadDetectionMode;

	[SerializeField]
	private bool m_showHints;

	[SerializeField]
	private bool m_waterReflections = true;

	[SerializeField]
	private GraphicsQualitySettings m_graphicsQualitySettings;

	[SerializeField]
	private DisplaySettings m_displaySettings;

	[SerializeField]
	private string m_inputJson;

	[SerializeField]
	private bool m_aimCrosshair;

	[SerializeField]
	private bool m_aimAssist;

	[SerializeField]
	private bool m_automateItemInteractMinigame;

	[SerializeField]
	private bool m_skipButtonMashEvents;

	[SerializeField]
	private bool m_voSubtitles = true;

	[SerializeField]
	private string m_language;

	public bool IsDirty => m_isDirty;

	public float MasterVolume
	{
		get
		{
			return m_masterVolume;
		}
		set
		{
			if (SetValue(ref m_masterVolume, value))
			{
				GlobalReferences.Instance.EventChannels.Audio.UpdateAudioVolumeLevelsFromUserPreferences.Raise();
			}
		}
	}

	public float EffectsVolume
	{
		get
		{
			return m_effectsVolume;
		}
		set
		{
			if (SetValue(ref m_effectsVolume, value))
			{
				GlobalReferences.Instance.EventChannels.Audio.UpdateAudioVolumeLevelsFromUserPreferences.Raise();
			}
		}
	}

	public float AmbientVolume
	{
		get
		{
			return m_ambientVolume;
		}
		set
		{
			if (SetValue(ref m_ambientVolume, value))
			{
				GlobalReferences.Instance.EventChannels.Audio.UpdateAudioVolumeLevelsFromUserPreferences.Raise();
			}
		}
	}

	public float MusicVolume
	{
		get
		{
			return m_musicVolume;
		}
		set
		{
			if (SetValue(ref m_musicVolume, value))
			{
				GlobalReferences.Instance.EventChannels.Audio.UpdateAudioVolumeLevelsFromUserPreferences.Raise();
			}
		}
	}

	public float VoiceVolume
	{
		get
		{
			return m_voiceVolume;
		}
		set
		{
			if (SetValue(ref m_voiceVolume, value))
			{
				GlobalReferences.Instance.EventChannels.Audio.UpdateAudioVolumeLevelsFromUserPreferences.Raise();
			}
		}
	}

	public bool AlwaysRun => false;

	public bool RunToggle
	{
		get
		{
			return m_runToggle;
		}
		set
		{
			SetValue(ref m_runToggle, value);
		}
	}

	public bool CrouchToggle
	{
		get
		{
			return m_crouchToggle;
		}
		set
		{
			SetValue(ref m_crouchToggle, value);
		}
	}

	public bool QuickMapToggle
	{
		get
		{
			return m_quickMapToggle;
		}
		set
		{
			SetValue(ref m_quickMapToggle, value);
		}
	}

	public float MouseAimingSensitivity
	{
		get
		{
			return m_mouseAimingSensitivity;
		}
		set
		{
			SetValue(ref m_mouseAimingSensitivity, value);
		}
	}

	public float GamepadAimingSensitivity
	{
		get
		{
			return m_gamepadAimingSensitivity;
		}
		set
		{
			SetValue(ref m_gamepadAimingSensitivity, value);
		}
	}

	public bool ControllerRumble
	{
		get
		{
			return m_controllerRumble;
		}
		set
		{
			SetValue(ref m_controllerRumble, value);
		}
	}

	public InputPromptGamepadDetectionMode InputPromptGamepadDetectionMode
	{
		get
		{
			return m_inputPromptGamepadDetectionMode;
		}
		set
		{
			SetValue(ref m_inputPromptGamepadDetectionMode, value);
		}
	}

	public bool ShowHints
	{
		get
		{
			return m_showHints;
		}
		set
		{
			SetValue(ref m_showHints, value);
		}
	}

	public bool WaterReflections
	{
		get
		{
			return m_waterReflections;
		}
		set
		{
			if (SetValue(ref m_waterReflections, value))
			{
				GlobalReferences.Instance.EventChannels.UserPreferences.GraphicsQualityChanged.Raise();
			}
		}
	}

	public GraphicsQualitySettings GraphicsQualitySettings => m_graphicsQualitySettings;

	public DisplaySettings DisplaySettings => m_displaySettings;

	public string InputJson
	{
		get
		{
			return m_inputJson;
		}
		set
		{
			m_inputJson = value;
		}
	}

	public bool AimCrosshair
	{
		get
		{
			return m_aimCrosshair;
		}
		set
		{
			SetValue(ref m_aimCrosshair, value);
		}
	}

	public bool AimAssist
	{
		get
		{
			return m_aimAssist;
		}
		set
		{
			SetValue(ref m_aimAssist, value);
		}
	}

	public bool AutomateItemInteractMinigame
	{
		get
		{
			return m_automateItemInteractMinigame;
		}
		set
		{
			SetValue(ref m_automateItemInteractMinigame, value);
		}
	}

	public bool SkipButtonMashEvents
	{
		get
		{
			return m_skipButtonMashEvents;
		}
		set
		{
			SetValue(ref m_skipButtonMashEvents, value);
		}
	}

	public bool VOSubtitles
	{
		get
		{
			return m_voSubtitles;
		}
		set
		{
			SetValue(ref m_voSubtitles, value);
		}
	}

	public string Language
	{
		get
		{
			return m_language;
		}
		set
		{
			SetValue(ref m_language, value);
		}
	}

	private bool SetValue<T>(ref T variable, T value)
	{
		if (!variable.Equals(value))
		{
			variable = value;
			m_isDirty = true;
			return true;
		}
		return false;
	}

	public void SetDefaults()
	{
		MasterVolume = 0.5f;
		EffectsVolume = 1f;
		AmbientVolume = 0.7f;
		MusicVolume = 1f;
		RunToggle = false;
		CrouchToggle = true;
		QuickMapToggle = true;
		AimAssist = false;
		AimCrosshair = false;
		ControllerRumble = true;
		ShowHints = ((!Application.isEditor) ? true : false);
		VOSubtitles = true;
		SkipButtonMashEvents = false;
		AutomateItemInteractMinigame = false;
		InputPromptGamepadDetectionMode = InputPromptGamepadDetectionMode.AutoDetect;
		MouseAimingSensitivity = 0.5f;
		GamepadAimingSensitivity = 0.5f;
		m_graphicsQualitySettings.SetDefaults();
		m_displaySettings.SetDefaults();
		if (LocalizationSettings.SelectedLocale != null)
		{
			Language = LocalizationSettings.SelectedLocale.Identifier.Code;
		}
		else
		{
			Language = "en";
		}
	}

	public void Validate()
	{
	}

	public void ClearDirtyFlag()
	{
		m_isDirty = false;
	}

	public void SetDirtyFlag()
	{
		m_isDirty = true;
	}

	public void ApplyGraphicsQualitySettings(GraphicsQualitySettings settings)
	{
		m_graphicsQualitySettings = settings;
		m_graphicsQualitySettings.ApplySettings();
	}

	public void ApplyDisplaySettings(DisplaySettings settings)
	{
		m_displaySettings = settings;
		m_displaySettings.ApplySettings();
	}
}
