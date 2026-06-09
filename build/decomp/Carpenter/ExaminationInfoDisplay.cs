using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ExaminationInfoDisplay : MonoBehaviour, IInputBackHandler, IMinigameBackInputHandler
{
	[Header("Events")]
	[SerializeField]
	private VoidGameEventChannel m_showExaminationEventChannel;

	[SerializeField]
	private VoidGameEventChannel m_cancelExaminationEventChannel;

	[SerializeField]
	private BoolGameEventChannel m_doneExaminationEventChannel;

	[Header("Anchors")]
	[SerializeField]
	private ExaminableAnchor m_examinableAnchor;

	[SerializeField]
	private GameMenuState m_gameMenuState;

	[Header("UI")]
	[SerializeField]
	private Image m_background;

	[SerializeField]
	private TextMeshProUGUI m_text;

	[SerializeField]
	private GameObject m_morePrompt;

	[SerializeField]
	private GameObject m_choicesContainer;

	[SerializeField]
	private TextMeshProUGUI m_confirmText;

	[SerializeField]
	private TextMeshProUGUI m_cancelText;

	[SerializeField]
	private Image m_image;

	[Header("Animation")]
	[SerializeField]
	private float m_textRevealAnimTime = 0.04f;

	[SerializeField]
	private float m_backgroundAnimTime = 0.1f;

	[Header("Mode")]
	[SerializeField]
	private bool m_isMinigame;

	private int m_entryIndex;

	private Coroutine m_textRevealCoroutine;

	private bool m_doneAnimating;

	private bool m_isShowingChoices;

	private bool m_isActive;

	private void OnEnable()
	{
		m_showExaminationEventChannel.Register(ShowExamination);
		m_cancelExaminationEventChannel.Register(HideExamination);
	}

	private void OnDisable()
	{
		m_showExaminationEventChannel.Unregister(ShowExamination);
		m_cancelExaminationEventChannel.Unregister(HideExamination);
		if (m_isActive)
		{
			HideExamination();
		}
	}

	private void OnDestroy()
	{
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.Game.Proceed.performed -= ProgressInputPressed;
			GameInputManager.GameInputActions.Game.Proceed.performed -= AcceptChoiceInput;
			GameInputManager.GameInputActions.UI.Cancel.performed -= CancelChoiceInput;
			GameInputManager.RemoveBackInputHandler(this);
		}
	}

	private void ShowExamination()
	{
		m_entryIndex = 0;
		m_isActive = true;
		DOTween.Kill(m_background);
		m_background.gameObject.SetActive(value: true);
		m_background.DOFade(0.5f, m_backgroundAnimTime).SetUpdate(isIndependentUpdate: true);
		m_choicesContainer.gameObject.SetActive(value: false);
		m_isShowingChoices = false;
		ShowEntry(m_examinableAnchor.Item, m_entryIndex);
		m_gameMenuState.SetInMenu(GameMenuState.GameMenu.Examinable);
		GameInputManager.GameInputActions.Game.Proceed.performed += ProgressInputPressed;
		if (!m_isMinigame)
		{
			GameInputManager.PushBackInputHandler(this);
		}
	}

	private void HideExamination()
	{
		if (m_textRevealCoroutine != null)
		{
			StopCoroutine(m_textRevealCoroutine);
		}
		m_text.gameObject.SetActive(value: false);
		m_image.gameObject.SetActive(value: false);
		m_choicesContainer.gameObject.SetActive(value: false);
		m_isActive = false;
		DOTween.Kill(m_background);
		m_gameMenuState.ClearInMenu(GameMenuState.GameMenu.Examinable);
		m_gameMenuState.ClearInMenu(GameMenuState.GameMenu.ExaminableWithImage);
		m_background.DOFade(0f, m_backgroundAnimTime).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			m_background.gameObject.SetActive(value: false);
		});
		GameInputManager.GameInputActions.Game.Proceed.performed -= ProgressInputPressed;
		GameInputManager.GameInputActions.Game.Proceed.performed -= AcceptChoiceInput;
		GameInputManager.GameInputActions.UI.Cancel.performed -= CancelChoiceInput;
		GameInputManager.RemoveBackInputHandler(this);
	}

	private void ProgressInputPressed(InputAction.CallbackContext input)
	{
		ProgressInput();
	}

	private void ProgressInput()
	{
		if (!m_doneAnimating)
		{
			m_text.maxVisibleCharacters = m_text.text.Length;
		}
		else if (m_examinableAnchor.Item.HasMoreEntries(m_entryIndex))
		{
			m_entryIndex++;
			ShowEntry(m_examinableAnchor.Item, m_entryIndex);
		}
		else if (m_examinableAnchor.Item.HasChoice)
		{
			ShowChoices();
		}
		else
		{
			m_doneExaminationEventChannel.Raise(value: true);
		}
	}

	public void AcceptChoice()
	{
		m_doneExaminationEventChannel.Raise(value: true);
	}

	public void CancelChoice()
	{
		m_doneExaminationEventChannel.Raise(value: false);
	}

	private void ShowEntry(InteractableExaminableCommand examinable, int index)
	{
		m_text.gameObject.SetActive(value: true);
		m_text.text = examinable.GetText(index);
		Sprite sprite = examinable.GetSprite(index);
		if (sprite != null)
		{
			m_gameMenuState.SetInMenu(GameMenuState.GameMenu.ExaminableWithImage);
		}
		else
		{
			m_gameMenuState.ClearInMenu(GameMenuState.GameMenu.ExaminableWithImage);
		}
		m_image.sprite = sprite;
		m_image.gameObject.SetActive(sprite != null);
		m_morePrompt.SetActive(value: false);
		AnimateTextOut();
	}

	private void AnimateTextOut()
	{
		if (m_textRevealCoroutine != null)
		{
			StopCoroutine(m_textRevealCoroutine);
		}
		m_doneAnimating = false;
		m_textRevealCoroutine = StartCoroutine(DescriptionTextRevealCoroutine());
	}

	private IEnumerator DescriptionTextRevealCoroutine()
	{
		m_text.maxVisibleCharacters = 0;
		while (m_text.maxVisibleCharacters < m_text.text.Length)
		{
			m_text.maxVisibleCharacters++;
			yield return new WaitForSecondsRealtime(m_textRevealAnimTime);
		}
		m_doneAnimating = true;
		if (m_examinableAnchor.Item.HasMoreEntries(m_entryIndex))
		{
			m_morePrompt.SetActive(value: true);
		}
		else if (m_examinableAnchor.Item.HasChoice)
		{
			ShowChoices();
		}
	}

	private void ShowChoices()
	{
		if (!m_isShowingChoices)
		{
			m_isShowingChoices = true;
			StartCoroutine(ShowChoicesDelay());
		}
	}

	private IEnumerator ShowChoicesDelay()
	{
		yield return new WaitForSecondsRealtime(0.2f);
		GameInputManager.GameInputActions.Game.Proceed.performed -= ProgressInputPressed;
		if (m_examinableAnchor.Item != null)
		{
			m_choicesContainer.gameObject.SetActive(value: true);
			m_confirmText.text = m_examinableAnchor.Item.ConfirmText;
			m_cancelText.text = m_examinableAnchor.Item.CancelText;
			GameInputManager.GameInputActions.Game.Proceed.performed += AcceptChoiceInput;
			GameInputManager.GameInputActions.UI.Cancel.performed += CancelChoiceInput;
		}
	}

	private void AcceptChoiceInput(InputAction.CallbackContext obj)
	{
		AcceptChoice();
	}

	private void CancelChoiceInput(InputAction.CallbackContext obj)
	{
		CancelChoice();
	}

	public bool OnInputBack()
	{
		ProgressInput();
		return false;
	}

	public bool TryBackInput()
	{
		if (m_isActive)
		{
			ProgressInput();
			return true;
		}
		return false;
	}
}
