using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class MapIconActionsPanel : MonoBehaviour
{
	[SerializeField]
	private InventoryActionButton m_editButton;

	[SerializeField]
	private InventoryActionButton m_viewButton;

	[SerializeField]
	private InventoryActionButton m_deleteButton;

	[SerializeField]
	private InventoryActionButton m_cancelButton;

	public UnityAction OnEditClicked;

	public UnityAction OnViewClicked;

	public UnityAction OnDeleteClicked;

	public UnityAction OnCancelClicked;

	public UnityAction OnActionsPanelClosed;

	private Map3DIcon m_activeIcon;

	private float m_showTime;

	public Map3DIcon ActiveIcon => m_activeIcon;

	private void Start()
	{
		InventoryActionButton editButton = m_editButton;
		editButton.m_onClickEvent = (UnityAction)Delegate.Combine(editButton.m_onClickEvent, new UnityAction(OnEditClickedCallback));
		InventoryActionButton viewButton = m_viewButton;
		viewButton.m_onClickEvent = (UnityAction)Delegate.Combine(viewButton.m_onClickEvent, new UnityAction(OnViewClickedCallback));
		InventoryActionButton deleteButton = m_deleteButton;
		deleteButton.m_onClickEvent = (UnityAction)Delegate.Combine(deleteButton.m_onClickEvent, new UnityAction(OnDeleteClickedCallback));
		InventoryActionButton cancelButton = m_cancelButton;
		cancelButton.m_onClickEvent = (UnityAction)Delegate.Combine(cancelButton.m_onClickEvent, new UnityAction(OnCancelClickedCallback));
	}

	private void OnEditClickedCallback()
	{
		OnEditClicked?.Invoke();
	}

	private void OnViewClickedCallback()
	{
		OnViewClicked?.Invoke();
	}

	private void OnDeleteClickedCallback()
	{
		OnDeleteClicked?.Invoke();
	}

	private void OnCancelClickedCallback()
	{
		OnCancelClicked?.Invoke();
	}

	public void Show(Map3DIcon icon)
	{
		m_activeIcon = icon;
		m_showTime = Time.realtimeSinceStartup;
		base.gameObject.SetActive(value: true);
		m_editButton.ClearState();
		m_viewButton.ClearState();
		m_cancelButton.ClearState();
		m_deleteButton.ClearState();
		if (icon is MapUserMarkerIcon)
		{
			m_viewButton.gameObject.SetActive(value: false);
			m_editButton.gameObject.SetActive(value: true);
			SetSelectedButton(m_editButton);
		}
		else if (icon is MapPhoto)
		{
			m_viewButton.gameObject.SetActive(value: true);
			m_editButton.gameObject.SetActive(value: false);
			SetSelectedButton(m_viewButton);
		}
	}

	private void SetSelectedButton(InventoryActionButton button)
	{
		EventSystem.current.SetSelectedGameObject(button.gameObject);
		if (GlobalReferences.Instance.InputState.InputMode == InputState.Mode.Gamepad)
		{
			button.ForceSelected();
		}
	}

	public void Hide()
	{
		m_activeIcon = null;
		base.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (!Mouse.current.leftButton.wasPressedThisFrame || !(Time.realtimeSinceStartup > m_showTime + 0.1f))
		{
			return;
		}
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.position = Mouse.current.position.ReadValue();
		List<RaycastResult> list = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointerEventData, list);
		bool flag = false;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].gameObject.GetComponentInParent<MapIconActionsPanel>() == this)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			Hide();
			OnActionsPanelClosed?.Invoke();
		}
	}
}
