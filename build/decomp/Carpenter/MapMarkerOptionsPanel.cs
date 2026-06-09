using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class MapMarkerOptionsPanel : MonoBehaviour, IInputBackHandler
{
	[SerializeField]
	private MapMarkerOptionsButton m_mapMarkerOptionPrefab;

	[SerializeField]
	private MapVisualsSettings m_mapIconSettings;

	[SerializeField]
	private Transform m_containerTransform;

	private float m_lastInputMoveTime;

	private bool m_populated;

	private List<MapMarkerOptionsButton> m_markerOptionImages;

	private MapUserMarkerIcon m_markerIcon;

	public UnityAction<int> OnSelectedSpriteIndex;

	public UnityAction OnClosedAction;

	private float m_showTime;

	public bool IsActive => base.gameObject.activeSelf;

	public MapUserMarkerIcon ActiveMarkerIcon => m_markerIcon;

	private void Awake()
	{
		Populate();
	}

	private void OnEnable()
	{
		m_showTime = Time.realtimeSinceStartup;
	}

	private void Populate()
	{
		if (!m_populated)
		{
			m_markerOptionImages = new List<MapMarkerOptionsButton>();
			for (int i = 0; i < m_mapIconSettings.GetUserMarkerSpriteCount(); i++)
			{
				MapMarkerOptionsButton mapMarkerOptionsButton = UnityEngine.Object.Instantiate(m_mapMarkerOptionPrefab, m_containerTransform);
				mapMarkerOptionsButton.SetIndex(i, m_mapIconSettings.GetSpriteForMarkerIndex(i));
				m_markerOptionImages.Add(mapMarkerOptionsButton);
				mapMarkerOptionsButton.OnButtonClicked = (UnityAction<int>)Delegate.Combine(mapMarkerOptionsButton.OnButtonClicked, new UnityAction<int>(SelectMarkerSpriteIndex));
			}
			m_populated = true;
		}
	}

	private void SelectMarkerSpriteIndex(int spriteIndex)
	{
		m_markerIcon.SetIconIndex(spriteIndex);
		HighlightMarkerSpriteByIndex(spriteIndex);
		OnSelectedSpriteIndex?.Invoke(spriteIndex);
	}

	private void HighlightMarkerSpriteByIndex(int spriteIndex)
	{
		for (int i = 0; i < m_markerOptionImages.Count; i++)
		{
			m_markerOptionImages[i].SetHighlight(spriteIndex == i);
		}
	}

	public void ShowMarkerOptions(MapUserMarkerIcon markerIcon)
	{
		Populate();
		base.gameObject.SetActive(value: true);
		m_markerIcon = markerIcon;
		HighlightMarkerSpriteByIndex(markerIcon.MarkerData.IconSpriteIndex);
		GameInputManager.PushBackInputHandler(this);
		GameInputManager.GameInputActions.UI.RightClick.performed += CloseInput;
		GameInputManager.GameInputActions.UI.SubmitGamepadOnly.performed += CloseInput;
		GameInputManager.GameInputActions.UI.Navigate.performed += NavigateInput;
		PositionOnMarker();
		StartCoroutine(GameUtils.TempBlockInputActionIfPressed(GameInputManager.GameInputActions.UI.SubmitGamepadOnly));
	}

	private void Update()
	{
		if (Mouse.current.leftButton.wasPressedThisFrame && Time.realtimeSinceStartup > m_showTime + 0.1f)
		{
			PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
			pointerEventData.position = Mouse.current.position.ReadValue();
			List<RaycastResult> list = new List<RaycastResult>();
			EventSystem.current.RaycastAll(pointerEventData, list);
			bool flag = false;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].gameObject.GetComponentInParent<MapMarkerOptionsPanel>() == this)
				{
					flag = true;
					break;
				}
				if (list[i].gameObject.GetComponentInParent<MapUserMarkerIcon>() == m_markerIcon)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				Close();
			}
		}
		PositionOnMarker();
	}

	private void PositionOnMarker()
	{
		if (m_markerIcon != null)
		{
			base.transform.position = m_markerIcon.transform.position;
		}
	}

	private void NavigateInput(InputAction.CallbackContext input)
	{
		if (Time.unscaledTime - m_lastInputMoveTime < 0.1f)
		{
			return;
		}
		Vector2 vector = input.ReadValue<Vector2>();
		if (vector.x > 0.5f)
		{
			m_lastInputMoveTime = Time.unscaledTime;
			int num = m_markerIcon.MarkerData.IconSpriteIndex + 1;
			if (num >= m_mapIconSettings.GetUserMarkerSpriteCount())
			{
				num = 0;
			}
			SelectMarkerSpriteIndex(num);
		}
		else if (vector.x < -0.5f)
		{
			m_lastInputMoveTime = Time.unscaledTime;
			int num2 = m_markerIcon.MarkerData.IconSpriteIndex - 1;
			if (num2 < 0)
			{
				num2 = m_mapIconSettings.GetUserMarkerSpriteCount() - 1;
			}
			SelectMarkerSpriteIndex(num2);
		}
	}

	private void CloseInput(InputAction.CallbackContext input)
	{
		if (input.performed)
		{
			Close();
		}
	}

	public void Close()
	{
		if (base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: false);
			GameInputManager.RemoveBackInputHandler(this);
			OnClosedAction?.Invoke();
			if (GameInputManager.GameInputActions != null)
			{
				GameInputManager.GameInputActions.UI.RightClick.performed -= CloseInput;
				GameInputManager.GameInputActions.UI.Navigate.performed -= NavigateInput;
				GameInputManager.GameInputActions.UI.SubmitGamepadOnly.performed -= CloseInput;
			}
		}
	}

	public bool OnInputBack()
	{
		Close();
		return false;
	}
}
