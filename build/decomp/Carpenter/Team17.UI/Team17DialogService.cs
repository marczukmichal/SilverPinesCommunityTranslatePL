using UnityEngine;
using UnityEngine.Localization;

namespace Team17.UI;

public class Team17DialogService
{
	private GameObject _prefab;

	public int _activeDialogs;

	public bool AnyActiveDialogs => _activeDialogs > 0;

	public Team17DialogService(GameObject popupPrefab)
	{
		_prefab = popupPrefab;
	}

	public async Awaitable PromptUserFailedToSaveAsync(LocalizedString messageOverride = null)
	{
		T17MenuDialogPopup t17MenuDialogPopup = Services.Get<Team17DialogService>().RequestNewDialog();
		Team17DialogLocalizationService team17DialogLocalizationService = Services.Get<Team17DialogLocalizationService>();
		await t17MenuDialogPopup.ShowCancelDialog(team17DialogLocalizationService.SaveErrorTitle, messageOverride ?? team17DialogLocalizationService.SaveFileError);
	}

	public async Awaitable<bool> PromptUserLoadErrorDeleteOrCancelAsync(LocalizedString messageOverride = null, string titleOverride = null)
	{
		T17MenuDialogPopup t17MenuDialogPopup = Services.Get<Team17DialogService>().RequestNewDialog();
		Team17DialogLocalizationService team17DialogLocalizationService = Services.Get<Team17DialogLocalizationService>();
		return await t17MenuDialogPopup.ShowCustomTwoOptionDialogAsync(titleOverride ?? team17DialogLocalizationService.LoadSaveErrorTitle.GetLocalizedString(), (messageOverride ?? team17DialogLocalizationService.LoadSaveFileCorruptError).GetLocalizedString(), team17DialogLocalizationService.DeletePromptText);
	}

	public T17MenuDialogPopup RequestNewDialog()
	{
		_activeDialogs++;
		if (_activeDialogs > 0)
		{
			GlobalReferences.Instance.GameMenuState.SetInMenu(GameMenuState.GameMenu.DialogUI);
		}
		return Object.Instantiate(_prefab, null).GetComponent<T17MenuDialogPopup>();
	}

	public void OnDialogClose()
	{
		_activeDialogs--;
		if (_activeDialogs == 0)
		{
			GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.DialogUI);
		}
	}
}
