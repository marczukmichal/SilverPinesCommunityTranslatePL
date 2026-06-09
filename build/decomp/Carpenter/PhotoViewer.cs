using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PhotoViewer : MonoBehaviour, IInputBackHandler, ICursorOverrides
{
	[SerializeField]
	private PhotoViewerPhotoInstance[] m_viewerInstances;

	[SerializeField]
	private UserMapData m_userMapData;

	[Header("Delete")]
	[SerializeField]
	private GameObject m_deleteContainer;

	[SerializeField]
	private Slider m_slider;

	[SerializeField]
	private float m_deleteHoldTime = 1f;

	private int m_viewingIndex;

	private int m_photoCount;

	private float m_lastInputMoveTime;

	private float m_deleteTimer;

	public UnityAction<PhotoData> OnFocusOnPhoto;

	public UnityAction<PhotoData> OnDeletePhoto;

	private bool m_deleteInput;

	public bool IsActive => base.gameObject.activeSelf;

	public ICursorOverrides.CursorOverrideOption ShouldShowCursor
	{
		get
		{
			if (GlobalReferences.Instance.InputState.InputMode == InputState.Mode.Gamepad)
			{
				return ICursorOverrides.CursorOverrideOption.ForceOff;
			}
			return ICursorOverrides.CursorOverrideOption.Unchanged;
		}
	}

	private void Update()
	{
		if (IsActive)
		{
			if (m_deleteInput)
			{
				m_deleteTimer += Time.deltaTime;
				m_slider.value = Mathf.Clamp01(m_deleteTimer / m_deleteHoldTime);
			}
			else
			{
				m_deleteTimer = 0f;
			}
			m_deleteContainer.SetActive(m_deleteTimer > 0f);
			if (m_deleteTimer >= m_deleteHoldTime)
			{
				m_deleteTimer = 0f;
				m_deleteInput = false;
				DeletePhoto();
			}
		}
	}

	public void Show()
	{
		m_deleteInput = false;
		m_deleteTimer = 0f;
		m_deleteContainer.SetActive(value: false);
		base.gameObject.SetActive(value: true);
		GameInputManager.PushBackInputHandler(this);
		GameInputManager.GameInputActions.UI.RightClick.performed += RightClickInput;
		GameInputManager.GameInputActions.UI.Navigate.performed += NavigateInput;
		GameInputManager.GameInputActions.UI.Submit.performed += FocusInput;
		GameInputManager.GameInputActions.UI.MapDeletePhoto.performed += DeleteInputStart;
		GameInputManager.GameInputActions.UI.MapDeletePhoto.canceled += DeleteInputCancel;
		GlobalReferences.Instance.Anchors.Input.CursorOverridesAnchor.Set(this);
		m_photoCount = m_userMapData.Photos.Count;
		if (m_viewingIndex < 0 || m_viewingIndex >= m_photoCount)
		{
			m_viewingIndex = 0;
		}
		PopulatePhotos();
	}

	public void ShowPhoto(PhotoData photo)
	{
		m_viewingIndex = m_userMapData.Photos.IndexOf(photo);
		OnFocusOnPhoto?.Invoke(photo);
		Show();
	}

	private void PopulatePhotos()
	{
		m_photoCount = m_userMapData.Photos.Count;
		for (int i = 0; i < m_viewerInstances.Length; i++)
		{
			int index = m_viewingIndex + (i - 2);
			PhotoData photoForIndex = GetPhotoForIndex(index);
			m_viewerInstances[i].SetPhoto(photoForIndex);
		}
	}

	private PhotoData GetPhotoForIndex(int index)
	{
		List<PhotoData> photos = m_userMapData.Photos;
		if (index >= 0 && index < photos.Count)
		{
			return photos[index];
		}
		return null;
	}

	private void NavigateInput(InputAction.CallbackContext input)
	{
		if (Time.unscaledTime - m_lastInputMoveTime < 0.1f)
		{
			return;
		}
		bool flag = false;
		Vector2 vector = input.ReadValue<Vector2>();
		if (vector.x > 0.5f)
		{
			m_lastInputMoveTime = Time.unscaledTime;
			if (m_viewingIndex + 1 < m_photoCount)
			{
				m_viewingIndex++;
				flag = true;
			}
		}
		else if (vector.x < -0.5f)
		{
			m_lastInputMoveTime = Time.unscaledTime;
			if (m_viewingIndex > 0)
			{
				m_viewingIndex--;
				flag = true;
			}
		}
		if (flag)
		{
			PopulatePhotos();
			FocusOnCurrent();
		}
	}

	public void JumpToOffset(int offset)
	{
		m_viewingIndex += offset;
		m_viewingIndex = Mathf.Clamp(m_viewingIndex, 0, m_userMapData.Photos.Count - 1);
		PopulatePhotos();
		FocusOnCurrent();
	}

	public void FocusOnCurrent()
	{
		if (m_viewingIndex >= 0 && m_viewingIndex < m_photoCount)
		{
			OnFocusOnPhoto?.Invoke(m_userMapData.Photos[m_viewingIndex]);
		}
	}

	private void FocusInput(InputAction.CallbackContext input)
	{
		if (input.performed)
		{
			OnFocusOnPhoto?.Invoke(m_userMapData.Photos[m_viewingIndex]);
		}
	}

	private void RightClickInput(InputAction.CallbackContext obj)
	{
		Close();
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
		GlobalReferences.Instance.Anchors.Input.CursorOverridesAnchor.Set(null);
		GameInputManager.RemoveBackInputHandler(this);
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.UI.RightClick.performed -= RightClickInput;
			GameInputManager.GameInputActions.UI.Navigate.performed -= NavigateInput;
			GameInputManager.GameInputActions.UI.Submit.performed -= FocusInput;
			GameInputManager.GameInputActions.UI.MapDeletePhoto.performed -= DeleteInputStart;
			GameInputManager.GameInputActions.UI.MapDeletePhoto.canceled -= DeleteInputCancel;
		}
	}

	private void DeleteInputStart(InputAction.CallbackContext context)
	{
		m_deleteInput = true;
		m_deleteTimer = 0f;
	}

	private void DeleteInputCancel(InputAction.CallbackContext context)
	{
		m_deleteInput = false;
		m_deleteTimer = 0f;
	}

	private void DeletePhoto()
	{
		OnDeletePhoto?.Invoke(m_userMapData.Photos[m_viewingIndex]);
		m_photoCount = m_userMapData.Photos.Count;
		if (m_photoCount == 0)
		{
			Close();
			return;
		}
		if (m_viewingIndex >= m_photoCount)
		{
			m_viewingIndex = m_photoCount - 1;
		}
		PopulatePhotos();
		FocusOnCurrent();
	}

	public bool OnInputBack()
	{
		Close();
		return false;
	}

	public void Toggle()
	{
		if (IsActive)
		{
			Close();
		}
		else
		{
			Show();
		}
	}
}
