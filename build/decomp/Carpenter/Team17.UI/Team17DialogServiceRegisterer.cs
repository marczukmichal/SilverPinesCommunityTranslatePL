using UnityEngine;
using UnityEngine.Localization;

namespace Team17.UI;

public class Team17DialogServiceRegisterer : BaseServicesRegisterer
{
	[SerializeField]
	private GameObject _popupPrefab;

	[SerializeField]
	private LocalizedString _loadSaveErrorTitle;

	[SerializeField]
	private LocalizedString _loadSaveFileTooNewError;

	[SerializeField]
	private LocalizedString _loadSaveFileCorruptError;

	[SerializeField]
	private LocalizedString _saveErrorTitle;

	[SerializeField]
	private LocalizedString _saveErrorDefaultMessage;

	[SerializeField]
	private LocalizedString _deletePromptText;

	[SerializeField]
	private LocalizedString _options;

	protected override void RegisterServicesInternal()
	{
		Services.Register<Team17DialogService>(new Team17DialogService(_popupPrefab));
		Services.Register<Team17DialogLocalizationService>(new Team17DialogLocalizationService(_loadSaveErrorTitle, _loadSaveFileTooNewError, _loadSaveFileCorruptError, _deletePromptText, _options, _saveErrorTitle, _saveErrorDefaultMessage));
	}
}
