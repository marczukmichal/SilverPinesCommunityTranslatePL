using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public abstract class MenuDialogPopup : MonoBehaviour
{
	[SerializeField]
	private float m_inputDelayTime = 1f;

	[SerializeField]
	private Button m_cancelButton;

	private float m_enableInputTime;

	private void RegisterInputs()
	{
		GameInputManager.GameInputActions.UI.Submit.performed += InputConfirm;
		GameInputManager.GameInputActions.UI.Click.performed += InputClick;
	}

	private void UnregisterInputs()
	{
		GameInputManager.GameInputActions.UI.Submit.performed -= InputConfirm;
		GameInputManager.GameInputActions.UI.Click.performed -= InputClick;
	}

	public abstract void OnConfirm();

	private void InputClick(InputAction.CallbackContext context)
	{
		bool flag = false;
		if (m_cancelButton != null)
		{
			PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
			pointerEventData.position = Mouse.current.position.ReadValue();
			List<RaycastResult> list = new List<RaycastResult>();
			EventSystem.current.RaycastAll(pointerEventData, list);
			for (int i = 0; i < list.Count; i++)
			{
				Button componentInParent = list[i].gameObject.GetComponentInParent<Button>();
				if (componentInParent != null && componentInParent == m_cancelButton)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			Close();
		}
		else
		{
			Confirm();
		}
	}

	private void InputConfirm(InputAction.CallbackContext context)
	{
		if (!(Time.unscaledTime < m_enableInputTime + m_inputDelayTime))
		{
			Confirm();
		}
	}

	public virtual void Confirm()
	{
		OnConfirm();
		Close();
	}

	public virtual void Show()
	{
		m_enableInputTime = Time.unscaledTime;
		base.gameObject.SetActive(value: true);
		RegisterInputs();
	}

	public virtual void Close()
	{
		base.gameObject.SetActive(value: false);
		UnregisterInputs();
	}

	public bool IsShowing()
	{
		return base.gameObject.activeSelf;
	}
}
