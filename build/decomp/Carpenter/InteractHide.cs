using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Settings;

public class InteractHide : BaseInteractable
{
	public enum IntereactHideType
	{
		Bush,
		Locker
	}

	[SerializeField]
	private IntereactHideType m_hideType;

	[SerializeField]
	private UnityEvent m_onEnterHide;

	[SerializeField]
	private UnityEvent m_onExitHide;

	[SerializeField]
	private SpriteRenderer[] m_fadeSprites;

	public IntereactHideType HideType => m_hideType;

	public override string InteractString => LocalizationSettings.StringDatabase.GetLocalizedString("InteractPrompts", "Hide", null, FallbackBehavior.UseProjectSettings);

	public override InteractType GetInteractType()
	{
		return InteractType.Hide;
	}

	public override void Interact(BaseInteractor interactor)
	{
	}

	public void EnterHide()
	{
		m_onEnterHide.Invoke();
		SpriteRenderer[] fadeSprites = m_fadeSprites;
		foreach (SpriteRenderer obj in fadeSprites)
		{
			DOTween.Kill(obj);
			obj.DOFade(0.5f, 0.5f);
		}
	}

	public void ExitHide()
	{
		m_onExitHide.Invoke();
		SpriteRenderer[] fadeSprites = m_fadeSprites;
		foreach (SpriteRenderer obj in fadeSprites)
		{
			DOTween.Kill(obj);
			obj.DOFade(1f, 0.5f);
		}
	}
}
