using UnityEngine;
using UnityEngine.Localization.Settings;

public class StairsPortal : BaseInteractable
{
	public enum EntryType
	{
		StairsDown,
		StairsUp
	}

	[SerializeField]
	private EntryType m_entryType;

	[SerializeField]
	private StairsPortal m_pairedStairPortal;

	public EntryType StairsEntryType => m_entryType;

	public StairsPortal PairedStairsPortal => m_pairedStairPortal;

	public override string InteractString => LocalizationSettings.StringDatabase.GetLocalizedString("InteractPrompts", "Use", null, FallbackBehavior.UseProjectSettings);

	public override InteractButtonType GetInteractButonType()
	{
		if (m_entryType == EntryType.StairsDown)
		{
			return InteractButtonType.TransitionDown;
		}
		return InteractButtonType.TransitionUp;
	}

	public override InteractType GetInteractType()
	{
		if (m_entryType == EntryType.StairsDown)
		{
			return InteractType.StairsPortalDown;
		}
		return InteractType.StairsPortalUp;
	}

	public override void Interact(BaseInteractor interactor)
	{
		interactor.GetComponent<StairsPortalUser>().SetActiveStairsPortal(this);
	}
}
