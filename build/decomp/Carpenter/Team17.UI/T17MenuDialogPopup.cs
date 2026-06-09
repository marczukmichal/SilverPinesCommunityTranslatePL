using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

namespace Team17.UI;

public class T17MenuDialogPopup : MenuDialogPopup
{
	public delegate void DialogEvent();

	private DialogEvent onConfirmEvent;

	private DialogEvent onCancelEvent;

	private GameObject previousSelectedObject;

	[SerializeField]
	private GameObject fallbackSelectedObject;

	[SerializeField]
	private TextMeshProUGUI m_titleTextObject;

	[SerializeField]
	private TextMeshProUGUI m_bodyTextObject;

	[Header("Button Prompts")]
	[SerializeField]
	private TextMeshProUGUI m_confirmPrompt;

	[SerializeField]
	private TextMeshProUGUI m_cancelPrompt;

	[SerializeField]
	private LocalizedString m_confirmText;

	[SerializeField]
	private LocalizedString m_cancelText;

	private readonly AwaitableCompletionSource _closeCompletionSource = new AwaitableCompletionSource();

	private readonly AwaitableCompletionSource<bool> _choiceCompletionSource = new AwaitableCompletionSource<bool>();

	private bool _choiceCompleted;

	private bool _isOpen;

	public Awaitable AwaitClose()
	{
		if (_isOpen)
		{
			return _closeCompletionSource.Awaitable;
		}
		AwaitableCompletionSource awaitableCompletionSource = new AwaitableCompletionSource();
		awaitableCompletionSource.SetResult();
		return awaitableCompletionSource.Awaitable;
	}

	public Awaitable<bool> ShowConfirmAndCancelDialogAsync(LocalizedString newTitleText, LocalizedString newBodyText)
	{
		ShowConfirmAndCancelDialog(newTitleText, newBodyText);
		return _choiceCompletionSource.Awaitable;
	}

	public Awaitable<bool> ShowCustomTwoOptionDialogAsync(LocalizedString newTitleText, LocalizedString newBodyText, LocalizedString newConfirmText, LocalizedString newCancelText = null)
	{
		ShowCustomTwoOptionDialog(newTitleText, newBodyText, newConfirmText, newCancelText);
		return _choiceCompletionSource.Awaitable;
	}

	public Awaitable<bool> ShowCustomTwoOptionDialogAsync(string newTitleText, string newBodyText, LocalizedString newConfirmText, LocalizedString newCancelText = null)
	{
		ShowCustomTwoOptionDialog(newTitleText, newBodyText, newConfirmText, newCancelText);
		return _choiceCompletionSource.Awaitable;
	}

	public Awaitable ShowCancelDialog(LocalizedString newTitleText, LocalizedString newBodyText, DialogEvent onCancelEventCallback = null)
	{
		ResetPrompts();
		m_confirmPrompt.transform.parent.gameObject.SetActive(value: false);
		onCancelEvent = onCancelEventCallback;
		UpdateDialogText(newTitleText, newBodyText);
		Show();
		return AwaitClose();
	}

	public void ShowConfirmAndCancelDialog(LocalizedString newTitleText, LocalizedString newBodyText, DialogEvent onConfirmEventCallback = null, DialogEvent onCancelEventCallback = null)
	{
		ResetPrompts();
		onConfirmEvent = onConfirmEventCallback;
		onCancelEvent = onCancelEventCallback;
		UpdateDialogText(newTitleText, newBodyText);
		Show();
	}

	public void ShowCustomTwoOptionDialog(LocalizedString newTitleText, LocalizedString newBodyText, LocalizedString newConfirmText, LocalizedString newCancelText = null, DialogEvent onConfirmEventCallback = null, DialogEvent onCancelEventCallback = null)
	{
		ResetPrompts();
		OverridePromptText(newConfirmText, newCancelText ?? m_cancelText);
		onConfirmEvent = onConfirmEventCallback;
		onCancelEvent = onCancelEventCallback;
		UpdateDialogText(newTitleText, newBodyText);
		Show();
	}

	public void ShowCustomTwoOptionDialog(string newTitleText, string newBodyText, LocalizedString newConfirmText, LocalizedString newCancelText = null, DialogEvent onConfirmEventCallback = null, DialogEvent onCancelEventCallback = null)
	{
		ResetPrompts();
		OverridePromptText(newConfirmText, newCancelText ?? m_cancelText);
		onConfirmEvent = onConfirmEventCallback;
		onCancelEvent = onCancelEventCallback;
		UpdateDialogText(newTitleText, newBodyText);
		Show();
	}

	public override void Show()
	{
		_isOpen = true;
		_choiceCompleted = false;
		_closeCompletionSource.Reset();
		_choiceCompletionSource.Reset();
		LockControllerMovement();
		RegisterBackInput();
		base.Show();
	}

	public override void Close()
	{
		if (_isOpen)
		{
			_isOpen = false;
			_closeCompletionSource.SetResult();
			_choiceCompletionSource.SetResult(in _choiceCompleted);
		}
		UnlockControllerMovement();
		UnregisterBackInput();
		base.Close();
		Object.Destroy(base.gameObject);
	}

	private void OnDestroy()
	{
		if (Services.TryGet<Team17DialogService>(out var service))
		{
			service.OnDialogClose();
		}
	}

	public override void OnConfirm()
	{
		_choiceCompleted = true;
		if (onConfirmEvent != null)
		{
			onConfirmEvent?.Invoke();
		}
	}

	private void LockControllerMovement()
	{
		previousSelectedObject = EventSystem.current.currentSelectedGameObject;
		EventSystem.current.SetSelectedGameObject(base.gameObject);
	}

	private void UnlockControllerMovement()
	{
		if (previousSelectedObject != null && previousSelectedObject.activeInHierarchy)
		{
			EventSystem.current.SetSelectedGameObject(previousSelectedObject);
		}
		else if (fallbackSelectedObject != null && fallbackSelectedObject.activeInHierarchy)
		{
			EventSystem.current.SetSelectedGameObject(fallbackSelectedObject);
		}
		previousSelectedObject = null;
	}

	private void UpdateDialogText(LocalizedString newTitleText, LocalizedString newBodyText)
	{
		m_titleTextObject.text = newTitleText.GetLocalizedString();
		m_bodyTextObject.text = newBodyText.GetLocalizedString();
	}

	private void UpdateDialogText(string newTitleText, string newBodyText)
	{
		m_titleTextObject.text = newTitleText;
		m_bodyTextObject.text = newBodyText;
	}

	private void ResetPrompts()
	{
		m_confirmPrompt.text = m_confirmText.GetLocalizedString();
		m_cancelPrompt.text = m_cancelText.GetLocalizedString();
		m_confirmPrompt.transform.parent.gameObject.SetActive(value: true);
		m_cancelPrompt.transform.parent.gameObject.SetActive(value: true);
	}

	private void OverridePromptText(LocalizedString confirmText, LocalizedString newCancelText)
	{
		m_confirmPrompt.text = confirmText.GetLocalizedString();
		m_cancelPrompt.text = newCancelText.GetLocalizedString();
	}

	private void RegisterBackInput()
	{
		GameInputManager.GameInputActions.UI.Cancel.performed += CancelPressed;
	}

	private void UnregisterBackInput()
	{
		GameInputManager.GameInputActions.UI.Cancel.performed -= CancelPressed;
	}

	private void CancelPressed(InputAction.CallbackContext context)
	{
		if (onCancelEvent != null)
		{
			onCancelEvent?.Invoke();
		}
		Close();
	}
}
