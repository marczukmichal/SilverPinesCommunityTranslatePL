using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Actions;

[ActionCategory("Inventory")]
public class TakePhotographAction : FsmStateAction
{
	private enum PhotoState
	{
		PhotoAim,
		FlashEnabled,
		PostPhoto
	}

	public TakingPhotoStateData m_stateData;

	public FsmEvent m_doneEvent;

	public AudioEvent m_takePhotoAudio;

	public AudioEvent m_noFilmPhotoAudio;

	public float m_flashOnTime = 0.1f;

	[RequiredField]
	public FsmGameObject m_cameraFlashGameObject;

	public ItemDefinition m_cameraItemDefinition;

	private ItemInstance m_cameraItemInstance;

	private PhotoState m_photoState;

	private float m_timer;

	private CharacterInventory m_inventory;

	private PhotoState CurrentPhotoState
	{
		get
		{
			return m_photoState;
		}
		set
		{
			if (m_photoState != value)
			{
				m_photoState = value;
				m_timer = 0f;
			}
		}
	}

	public override void Awake()
	{
		base.Awake();
	}

	public override void OnEnter()
	{
		m_stateData.Reset();
		m_inventory = base.Owner.GetComponent<CharacterInventory>();
		m_inventory.WeaponSwappingAllowed = false;
		ItemInstance itemOfType = m_inventory.Inventory.GetItemOfType(m_cameraItemDefinition);
		if (itemOfType != null)
		{
			m_cameraItemInstance = itemOfType;
		}
		else
		{
			Debug.LogError("Somehow managed to try to take photo without a camera in inventory?");
		}
		UpdatePhotoStateData();
		m_stateData.SetActive(active: true);
		m_photoState = PhotoState.PhotoAim;
		GameInputManager.GameInputActions.Photo.Enable();
		GameInputManager.GameInputActions.Player.Fire.performed += TryTakePhoto;
		GameInputManager.GameInputActions.Game.Close.performed += CancelPhoto;
	}

	private void TryTakePhoto(InputAction.CallbackContext obj)
	{
		if (GlobalReferences.Instance.UserMapData.GetAvailablePhotoCount() <= 0)
		{
			m_noFilmPhotoAudio.Play(base.Owner.transform.position);
			GlobalReferences.Instance.EventChannels.Photos.TooManyPhotosMemory.Raise();
		}
		else
		{
			m_takePhotoAudio.Play(base.Owner.transform.position);
			CurrentPhotoState = PhotoState.FlashEnabled;
			m_cameraFlashGameObject.Value.SetActive(value: true);
		}
	}

	private void CancelPhoto(InputAction.CallbackContext obj)
	{
		Done();
	}

	private void Done()
	{
		m_cameraFlashGameObject.Value.SetActive(value: false);
		base.Fsm.Event(m_doneEvent);
		Finish();
	}

	private void UpdatePhotoStateData()
	{
		float num = GameInputManager.GameInputActions.Photo.CameraZoom.ReadValue<float>();
		float num2 = (float)Screen.width * 0.5f;
		float num3 = (float)Screen.height * 0.5f;
		m_stateData.m_size += num * -5f * Time.deltaTime;
		m_stateData.m_size = Mathf.Clamp(m_stateData.m_size, 0.25f, 1f);
		m_stateData.m_zoomNormalized = Mathf.Lerp(4f, 1f, Mathf.InverseLerp(0.25f, 1f, m_stateData.m_size));
		float num4 = num2 - m_stateData.m_size * 0.5f * (float)Screen.height;
		float num5 = num3 - m_stateData.m_size * 0.5f * (float)Screen.height;
		m_stateData.m_offset.x = Mathf.Clamp(m_stateData.m_offset.x, 0f - num4, num4);
		m_stateData.m_offset.y = Mathf.Clamp(m_stateData.m_offset.y, 0f - num5, num5);
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		switch (m_photoState)
		{
		case PhotoState.PhotoAim:
		{
			Vector2 vector = GameInputManager.GameInputActions.Photo.CameraMove.ReadValue<Vector2>();
			if (GlobalReferences.Instance.InputState.InputMode == InputState.Mode.KeyboardMouse)
			{
				Vector2 value = Mouse.current.delta.value;
				if (value != Vector2.zero)
				{
					vector = value * 0.005f;
				}
			}
			else
			{
				vector *= 0.5f;
			}
			m_stateData.m_offset += vector * Screen.width * Time.deltaTime;
			UpdatePhotoStateData();
			break;
		}
		case PhotoState.FlashEnabled:
			GlobalReferences.Instance.EventChannels.Photos.TakePhoto.Raise();
			CurrentPhotoState = PhotoState.PostPhoto;
			break;
		case PhotoState.PostPhoto:
			m_timer += Time.deltaTime;
			if (m_timer > m_flashOnTime)
			{
				Done();
			}
			break;
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		m_stateData.SetActive(active: false);
		m_inventory.WeaponSwappingAllowed = true;
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.Photo.Disable();
			GameInputManager.GameInputActions.Player.Fire.performed -= TryTakePhoto;
			GameInputManager.GameInputActions.Game.Close.performed -= CancelPhoto;
		}
	}
}
