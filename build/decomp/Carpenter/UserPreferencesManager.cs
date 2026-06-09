using Team17;
using Team17.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class UserPreferencesManager : MonoBehaviour
{
	private static UserPreferencesManager _instance;

	public static UserPreferencesManager Instance => _instance;

	private void Awake()
	{
		_instance = this;
	}

	private void OnDestroy()
	{
		if (_instance == this)
		{
			_instance = null;
		}
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.UserPreferences.SaveUserPreferences.Register(CheckForSave);
		GlobalReferences.Instance.EventChannels.UserPreferences.UpdateLocalization.Register(UpdateLocalization);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.UserPreferences.SaveUserPreferences.Unregister(CheckForSave);
		GlobalReferences.Instance.EventChannels.UserPreferences.UpdateLocalization.Unregister(UpdateLocalization);
	}

	public void OverwritePreferences()
	{
		GlobalReferences.Instance.UserPreferences.InputJson = GameInputManager.GameInputActions.SaveBindingOverridesAsJson();
	}

	public void ApplyFromPreferences()
	{
		if (GameInputManager.GameInputActions != null && !string.IsNullOrEmpty(GlobalReferences.Instance.UserPreferences.InputJson))
		{
			GameInputManager.GameInputActions.LoadBindingOverridesFromJson(GlobalReferences.Instance.UserPreferences.InputJson);
		}
		GlobalReferences.Instance.UserPreferences.GraphicsQualitySettings.ApplySettings();
		GlobalReferences.Instance.UserPreferences.DisplaySettings.ApplySettings();
		GlobalReferences.Instance.EventChannels.Audio.UpdateAudioVolumeLevelsFromUserPreferences.Raise();
		UpdateLocalization();
	}

	private void SaveToFile()
	{
		SaveDataManager.Instance.SaveUserPreferences(GlobalReferences.Instance.UserPreferences, OnSavedCallback);
	}

	private Awaitable<ResultCode> SaveToFileAsync()
	{
		AwaitableCompletionSource<ResultCode> awaitableCompletionSource = new AwaitableCompletionSource<ResultCode>();
		SaveDataManager.Instance.SaveUserPreferences(GlobalReferences.Instance.UserPreferences, delegate(DataSavedCallbackEvent e)
		{
			awaitableCompletionSource.SetResult(in e.m_resultCode);
		});
		return awaitableCompletionSource.Awaitable;
	}

	private void OnSavedCallback(DataSavedCallbackEvent result)
	{
		Debug.Log("Save User Preferences Result: " + result.m_resultCode);
		if (result.m_resultCode != ResultCode.Success)
		{
			Services.Get<Team17DialogService>().PromptUserFailedToSaveAsync();
		}
	}

	public async Awaitable<bool> TryLoadAsync()
	{
		DataLoadedCallbackEvent<UserPreferences> dataLoadedCallbackEvent = await SaveDataManager.Instance.LoadUserPreferencesAwaitable(GlobalReferences.Instance.UserPreferences);
		Debug.Log("Load User Preferences Result: " + dataLoadedCallbackEvent.m_resultCode);
		if (dataLoadedCallbackEvent.m_resultCode == ResultCode.Success)
		{
			Debug.Log("Load user preferences from file");
			GlobalReferences.Instance.UserPreferences.Validate();
			ApplyFromPreferences();
			return true;
		}
		if (await TryResolveLoadErrorAsync(dataLoadedCallbackEvent))
		{
			ApplyFromPreferences();
			return true;
		}
		return false;
	}

	private async Awaitable<bool> TryResolveLoadErrorAsync(DataLoadedCallbackEvent<UserPreferences> result)
	{
		switch (result.m_resultCode)
		{
		case ResultCode.FileDoesNotExist:
			GlobalReferences.Instance.UserPreferences.SetDefaults();
			return true;
		case ResultCode.FileVersionTooHigh:
			return await PromptUserToDeleteOrCancelLoadingAsync(Services.Get<Team17DialogLocalizationService>().LoadSaveFileTooNewError);
		default:
			return await PromptUserToDeleteOrCancelLoadingAsync();
		}
	}

	private async Awaitable<bool> PromptUserToDeleteOrCancelLoadingAsync(LocalizedString messageOverride = null)
	{
		Team17DialogLocalizationService team17DialogLocalizationService = Services.Get<Team17DialogLocalizationService>();
		string titleOverride = team17DialogLocalizationService.LoadSaveErrorTitle.GetLocalizedString() + " - " + team17DialogLocalizationService.Options.GetLocalizedString();
		if (!(await Services.Get<Team17DialogService>().PromptUserLoadErrorDeleteOrCancelAsync(messageOverride, titleOverride)))
		{
			return false;
		}
		GlobalReferences.Instance.UserPreferences.SetDefaults();
		if (await SaveToFileAsync() != ResultCode.Success)
		{
			await Services.Get<Team17DialogService>().PromptUserFailedToSaveAsync();
			return false;
		}
		return true;
	}

	private void CheckForSave()
	{
		if (GlobalReferences.Instance.UserPreferences.IsDirty)
		{
			Save();
			GlobalReferences.Instance.UserPreferences.ClearDirtyFlag();
		}
	}

	private void Save()
	{
		OverwritePreferences();
		SaveToFile();
	}

	private void UpdateLocalization()
	{
		string language = GlobalReferences.Instance.UserPreferences.Language;
		UnityEngine.Localization.Locale locale = null;
		foreach (UnityEngine.Localization.Locale locale2 in LocalizationSettings.AvailableLocales.Locales)
		{
			if (locale2.Identifier.Code.Equals(language))
			{
				locale = locale2;
			}
		}
		if (locale != null)
		{
			LocalizationSettings.SelectedLocale = locale;
		}
	}
}
