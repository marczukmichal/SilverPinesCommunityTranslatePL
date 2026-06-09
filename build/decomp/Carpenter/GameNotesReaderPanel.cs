using DG.Tweening;
using UnityEngine;

public class GameNotesReaderPanel : MonoBehaviour, IInputBackHandler
{
	[SerializeField]
	private LoreReader m_loreReader;

	[SerializeField]
	private GameObject m_container;

	[SerializeField]
	private MenuInputPrompts m_menuInputPrompts;

	[Header("Fade")]
	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private float m_fadeInAnimTime;

	[SerializeField]
	private float m_fadeOutAnimTime;

	private bool m_isActive;

	private void Awake()
	{
		m_container.gameObject.SetActive(value: false);
		m_canvasGroup.alpha = 0f;
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.Lore.LorePickup.Register(OnFindLore);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Lore.LorePickup.Unregister(OnFindLore);
	}

	private void OnFindLore(LoreEntry loreEntry)
	{
		m_loreReader.ShowLore(loreEntry);
		SetActive(active: true);
		m_menuInputPrompts.SetInputs(m_loreReader.AvailableInputs);
		m_loreReader.DoReadFromGameAnimationFlow();
	}

	private void SetActive(bool active)
	{
		if (m_isActive == active)
		{
			return;
		}
		m_isActive = active;
		DOTween.Kill(m_canvasGroup);
		if (active)
		{
			m_container.gameObject.SetActive(active);
			m_canvasGroup.DOFade(1f, m_fadeInAnimTime);
			GameInputManager.PushBackInputHandler(this);
			GlobalReferences.Instance.GameMenuState.SetInMenu(GameMenuState.GameMenu.GameNotesReader);
		}
		else
		{
			m_canvasGroup.DOFade(0f, m_fadeOutAnimTime).OnComplete(delegate
			{
				m_container.gameObject.SetActive(value: false);
			});
			GameInputManager.RemoveBackInputHandler(this);
			GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.GameNotesReader);
		}
	}

	public bool OnInputBack()
	{
		SetActive(active: false);
		return false;
	}
}
