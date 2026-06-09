using UnityEngine;
using UnityEngine.Localization.Settings;

public class JumpTargetInteract : BaseInteractable
{
	public enum JumpType
	{
		Vertical,
		Horizontal,
		OnlyLeft,
		OnlyRight
	}

	[SerializeField]
	private JumpType m_jumptype;

	public override string InteractString => LocalizationSettings.StringDatabase.GetLocalizedString("InteractPrompts", "Jump", null, FallbackBehavior.UseProjectSettings);

	public override bool UseFSMInteractFlow => false;

	public override void Interact(BaseInteractor interactor)
	{
		PlayMakerFSM component = interactor.GetComponent<PlayMakerFSM>();
		if (!(component == null))
		{
			if (m_jumptype == JumpType.Vertical)
			{
				component.SendEvent("Movement/Jump/Vertical");
			}
			else
			{
				component.SendEvent("Movement/Jump");
			}
		}
	}

	public override bool CanInteract(BaseInteractor interactor)
	{
		if (m_jumptype == JumpType.OnlyRight || m_jumptype == JumpType.OnlyLeft)
		{
			CharacterDirection component = interactor.GetComponent<CharacterDirection>();
			if (!(component != null))
			{
				return false;
			}
			if (m_jumptype == JumpType.OnlyLeft)
			{
				return component.CurrentDirection == CharacterDirection.Facing.Left;
			}
			if (m_jumptype == JumpType.OnlyRight)
			{
				return component.CurrentDirection == CharacterDirection.Facing.Right;
			}
		}
		return base.CanInteract(interactor);
	}
}
