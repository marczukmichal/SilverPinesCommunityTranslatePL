using UnityEngine.Localization;

namespace Team17.UI;

public class Team17DialogLocalizationService
{
	public LocalizedString LoadSaveErrorTitle { get; }

	public LocalizedString LoadSaveFileTooNewError { get; }

	public LocalizedString LoadSaveFileCorruptError { get; }

	public LocalizedString DeletePromptText { get; }

	public LocalizedString Options { get; }

	public LocalizedString SaveErrorTitle { get; }

	public LocalizedString SaveFileError { get; }

	public Team17DialogLocalizationService(LocalizedString loadSaveErrorTitle, LocalizedString loadSaveFileTooNewError, LocalizedString loadSaveFileCorruptError, LocalizedString deletePromptText, LocalizedString options, LocalizedString saveErrorTitle, LocalizedString saveFileError)
	{
		LoadSaveErrorTitle = loadSaveErrorTitle;
		LoadSaveFileTooNewError = loadSaveFileTooNewError;
		LoadSaveFileCorruptError = loadSaveFileCorruptError;
		DeletePromptText = deletePromptText;
		Options = options;
		SaveErrorTitle = saveErrorTitle;
		SaveFileError = saveFileError;
	}
}
