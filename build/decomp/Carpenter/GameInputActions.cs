using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class GameInputActions : IInputActionCollection2, IInputActionCollection, IEnumerable<InputAction>, IEnumerable, IDisposable
{
	public struct PlayerActions
	{
		private GameInputActions m_Wrapper;

		public InputAction Move => m_Wrapper.m_Player_Move;

		public InputAction AimDirection => m_Wrapper.m_Player_AimDirection;

		public InputAction Fire => m_Wrapper.m_Player_Fire;

		public InputAction Sprint => m_Wrapper.m_Player_Sprint;

		public InputAction Crouch => m_Wrapper.m_Player_Crouch;

		public InputAction Jump => m_Wrapper.m_Player_Jump;

		public InputAction Dodge => m_Wrapper.m_Player_Dodge;

		public InputAction AimMode => m_Wrapper.m_Player_AimMode;

		public InputAction Interact => m_Wrapper.m_Player_Interact;

		public InputAction Reload => m_Wrapper.m_Player_Reload;

		public InputAction QuickMap => m_Wrapper.m_Player_QuickMap;

		public InputAction Stomp => m_Wrapper.m_Player_Stomp;

		public InputAction ShortcutFlashlight => m_Wrapper.m_Player_ShortcutFlashlight;

		public InputAction PlayerCancel => m_Wrapper.m_Player_PlayerCancel;

		public InputAction ShortcutCamera => m_Wrapper.m_Player_ShortcutCamera;

		public InputAction QuickItemRight => m_Wrapper.m_Player_QuickItemRight;

		public InputAction QuickItemUse => m_Wrapper.m_Player_QuickItemUse;

		public InputAction QuickItemLeft => m_Wrapper.m_Player_QuickItemLeft;

		public InputAction CycleWeapon => m_Wrapper.m_Player_CycleWeapon;

		public bool enabled => Get().enabled;

		public PlayerActions(GameInputActions wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_Player;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(PlayerActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(IPlayerActions instance)
		{
			if (instance != null && !m_Wrapper.m_PlayerActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_PlayerActionsCallbackInterfaces.Add(instance);
				Move.started += instance.OnMove;
				Move.performed += instance.OnMove;
				Move.canceled += instance.OnMove;
				AimDirection.started += instance.OnAimDirection;
				AimDirection.performed += instance.OnAimDirection;
				AimDirection.canceled += instance.OnAimDirection;
				Fire.started += instance.OnFire;
				Fire.performed += instance.OnFire;
				Fire.canceled += instance.OnFire;
				Sprint.started += instance.OnSprint;
				Sprint.performed += instance.OnSprint;
				Sprint.canceled += instance.OnSprint;
				Crouch.started += instance.OnCrouch;
				Crouch.performed += instance.OnCrouch;
				Crouch.canceled += instance.OnCrouch;
				Jump.started += instance.OnJump;
				Jump.performed += instance.OnJump;
				Jump.canceled += instance.OnJump;
				Dodge.started += instance.OnDodge;
				Dodge.performed += instance.OnDodge;
				Dodge.canceled += instance.OnDodge;
				AimMode.started += instance.OnAimMode;
				AimMode.performed += instance.OnAimMode;
				AimMode.canceled += instance.OnAimMode;
				Interact.started += instance.OnInteract;
				Interact.performed += instance.OnInteract;
				Interact.canceled += instance.OnInteract;
				Reload.started += instance.OnReload;
				Reload.performed += instance.OnReload;
				Reload.canceled += instance.OnReload;
				QuickMap.started += instance.OnQuickMap;
				QuickMap.performed += instance.OnQuickMap;
				QuickMap.canceled += instance.OnQuickMap;
				Stomp.started += instance.OnStomp;
				Stomp.performed += instance.OnStomp;
				Stomp.canceled += instance.OnStomp;
				ShortcutFlashlight.started += instance.OnShortcutFlashlight;
				ShortcutFlashlight.performed += instance.OnShortcutFlashlight;
				ShortcutFlashlight.canceled += instance.OnShortcutFlashlight;
				PlayerCancel.started += instance.OnPlayerCancel;
				PlayerCancel.performed += instance.OnPlayerCancel;
				PlayerCancel.canceled += instance.OnPlayerCancel;
				ShortcutCamera.started += instance.OnShortcutCamera;
				ShortcutCamera.performed += instance.OnShortcutCamera;
				ShortcutCamera.canceled += instance.OnShortcutCamera;
				QuickItemRight.started += instance.OnQuickItemRight;
				QuickItemRight.performed += instance.OnQuickItemRight;
				QuickItemRight.canceled += instance.OnQuickItemRight;
				QuickItemUse.started += instance.OnQuickItemUse;
				QuickItemUse.performed += instance.OnQuickItemUse;
				QuickItemUse.canceled += instance.OnQuickItemUse;
				QuickItemLeft.started += instance.OnQuickItemLeft;
				QuickItemLeft.performed += instance.OnQuickItemLeft;
				QuickItemLeft.canceled += instance.OnQuickItemLeft;
				CycleWeapon.started += instance.OnCycleWeapon;
				CycleWeapon.performed += instance.OnCycleWeapon;
				CycleWeapon.canceled += instance.OnCycleWeapon;
			}
		}

		private void UnregisterCallbacks(IPlayerActions instance)
		{
			Move.started -= instance.OnMove;
			Move.performed -= instance.OnMove;
			Move.canceled -= instance.OnMove;
			AimDirection.started -= instance.OnAimDirection;
			AimDirection.performed -= instance.OnAimDirection;
			AimDirection.canceled -= instance.OnAimDirection;
			Fire.started -= instance.OnFire;
			Fire.performed -= instance.OnFire;
			Fire.canceled -= instance.OnFire;
			Sprint.started -= instance.OnSprint;
			Sprint.performed -= instance.OnSprint;
			Sprint.canceled -= instance.OnSprint;
			Crouch.started -= instance.OnCrouch;
			Crouch.performed -= instance.OnCrouch;
			Crouch.canceled -= instance.OnCrouch;
			Jump.started -= instance.OnJump;
			Jump.performed -= instance.OnJump;
			Jump.canceled -= instance.OnJump;
			Dodge.started -= instance.OnDodge;
			Dodge.performed -= instance.OnDodge;
			Dodge.canceled -= instance.OnDodge;
			AimMode.started -= instance.OnAimMode;
			AimMode.performed -= instance.OnAimMode;
			AimMode.canceled -= instance.OnAimMode;
			Interact.started -= instance.OnInteract;
			Interact.performed -= instance.OnInteract;
			Interact.canceled -= instance.OnInteract;
			Reload.started -= instance.OnReload;
			Reload.performed -= instance.OnReload;
			Reload.canceled -= instance.OnReload;
			QuickMap.started -= instance.OnQuickMap;
			QuickMap.performed -= instance.OnQuickMap;
			QuickMap.canceled -= instance.OnQuickMap;
			Stomp.started -= instance.OnStomp;
			Stomp.performed -= instance.OnStomp;
			Stomp.canceled -= instance.OnStomp;
			ShortcutFlashlight.started -= instance.OnShortcutFlashlight;
			ShortcutFlashlight.performed -= instance.OnShortcutFlashlight;
			ShortcutFlashlight.canceled -= instance.OnShortcutFlashlight;
			PlayerCancel.started -= instance.OnPlayerCancel;
			PlayerCancel.performed -= instance.OnPlayerCancel;
			PlayerCancel.canceled -= instance.OnPlayerCancel;
			ShortcutCamera.started -= instance.OnShortcutCamera;
			ShortcutCamera.performed -= instance.OnShortcutCamera;
			ShortcutCamera.canceled -= instance.OnShortcutCamera;
			QuickItemRight.started -= instance.OnQuickItemRight;
			QuickItemRight.performed -= instance.OnQuickItemRight;
			QuickItemRight.canceled -= instance.OnQuickItemRight;
			QuickItemUse.started -= instance.OnQuickItemUse;
			QuickItemUse.performed -= instance.OnQuickItemUse;
			QuickItemUse.canceled -= instance.OnQuickItemUse;
			QuickItemLeft.started -= instance.OnQuickItemLeft;
			QuickItemLeft.performed -= instance.OnQuickItemLeft;
			QuickItemLeft.canceled -= instance.OnQuickItemLeft;
			CycleWeapon.started -= instance.OnCycleWeapon;
			CycleWeapon.performed -= instance.OnCycleWeapon;
			CycleWeapon.canceled -= instance.OnCycleWeapon;
		}

		public void RemoveCallbacks(IPlayerActions instance)
		{
			if (m_Wrapper.m_PlayerActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(IPlayerActions instance)
		{
			foreach (IPlayerActions playerActionsCallbackInterface in m_Wrapper.m_PlayerActionsCallbackInterfaces)
			{
				UnregisterCallbacks(playerActionsCallbackInterface);
			}
			m_Wrapper.m_PlayerActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public struct MenuTogglesActions
	{
		private GameInputActions m_Wrapper;

		public InputAction SystemMenu => m_Wrapper.m_MenuToggles_SystemMenu;

		public InputAction Map => m_Wrapper.m_MenuToggles_Map;

		public InputAction Inventory => m_Wrapper.m_MenuToggles_Inventory;

		public InputAction Artifacts => m_Wrapper.m_MenuToggles_Artifacts;

		public InputAction Notes => m_Wrapper.m_MenuToggles_Notes;

		public InputAction ItemWheel => m_Wrapper.m_MenuToggles_ItemWheel;

		public InputAction QuickItemPanel => m_Wrapper.m_MenuToggles_QuickItemPanel;

		public bool enabled => Get().enabled;

		public MenuTogglesActions(GameInputActions wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_MenuToggles;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(MenuTogglesActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(IMenuTogglesActions instance)
		{
			if (instance != null && !m_Wrapper.m_MenuTogglesActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_MenuTogglesActionsCallbackInterfaces.Add(instance);
				SystemMenu.started += instance.OnSystemMenu;
				SystemMenu.performed += instance.OnSystemMenu;
				SystemMenu.canceled += instance.OnSystemMenu;
				Map.started += instance.OnMap;
				Map.performed += instance.OnMap;
				Map.canceled += instance.OnMap;
				Inventory.started += instance.OnInventory;
				Inventory.performed += instance.OnInventory;
				Inventory.canceled += instance.OnInventory;
				Artifacts.started += instance.OnArtifacts;
				Artifacts.performed += instance.OnArtifacts;
				Artifacts.canceled += instance.OnArtifacts;
				Notes.started += instance.OnNotes;
				Notes.performed += instance.OnNotes;
				Notes.canceled += instance.OnNotes;
				ItemWheel.started += instance.OnItemWheel;
				ItemWheel.performed += instance.OnItemWheel;
				ItemWheel.canceled += instance.OnItemWheel;
				QuickItemPanel.started += instance.OnQuickItemPanel;
				QuickItemPanel.performed += instance.OnQuickItemPanel;
				QuickItemPanel.canceled += instance.OnQuickItemPanel;
			}
		}

		private void UnregisterCallbacks(IMenuTogglesActions instance)
		{
			SystemMenu.started -= instance.OnSystemMenu;
			SystemMenu.performed -= instance.OnSystemMenu;
			SystemMenu.canceled -= instance.OnSystemMenu;
			Map.started -= instance.OnMap;
			Map.performed -= instance.OnMap;
			Map.canceled -= instance.OnMap;
			Inventory.started -= instance.OnInventory;
			Inventory.performed -= instance.OnInventory;
			Inventory.canceled -= instance.OnInventory;
			Artifacts.started -= instance.OnArtifacts;
			Artifacts.performed -= instance.OnArtifacts;
			Artifacts.canceled -= instance.OnArtifacts;
			Notes.started -= instance.OnNotes;
			Notes.performed -= instance.OnNotes;
			Notes.canceled -= instance.OnNotes;
			ItemWheel.started -= instance.OnItemWheel;
			ItemWheel.performed -= instance.OnItemWheel;
			ItemWheel.canceled -= instance.OnItemWheel;
			QuickItemPanel.started -= instance.OnQuickItemPanel;
			QuickItemPanel.performed -= instance.OnQuickItemPanel;
			QuickItemPanel.canceled -= instance.OnQuickItemPanel;
		}

		public void RemoveCallbacks(IMenuTogglesActions instance)
		{
			if (m_Wrapper.m_MenuTogglesActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(IMenuTogglesActions instance)
		{
			foreach (IMenuTogglesActions menuTogglesActionsCallbackInterface in m_Wrapper.m_MenuTogglesActionsCallbackInterfaces)
			{
				UnregisterCallbacks(menuTogglesActionsCallbackInterface);
			}
			m_Wrapper.m_MenuTogglesActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public struct GameActions
	{
		private GameInputActions m_Wrapper;

		public InputAction Proceed => m_Wrapper.m_Game_Proceed;

		public InputAction Skip => m_Wrapper.m_Game_Skip;

		public InputAction Close => m_Wrapper.m_Game_Close;

		public bool enabled => Get().enabled;

		public GameActions(GameInputActions wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_Game;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(GameActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(IGameActions instance)
		{
			if (instance != null && !m_Wrapper.m_GameActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_GameActionsCallbackInterfaces.Add(instance);
				Proceed.started += instance.OnProceed;
				Proceed.performed += instance.OnProceed;
				Proceed.canceled += instance.OnProceed;
				Skip.started += instance.OnSkip;
				Skip.performed += instance.OnSkip;
				Skip.canceled += instance.OnSkip;
				Close.started += instance.OnClose;
				Close.performed += instance.OnClose;
				Close.canceled += instance.OnClose;
			}
		}

		private void UnregisterCallbacks(IGameActions instance)
		{
			Proceed.started -= instance.OnProceed;
			Proceed.performed -= instance.OnProceed;
			Proceed.canceled -= instance.OnProceed;
			Skip.started -= instance.OnSkip;
			Skip.performed -= instance.OnSkip;
			Skip.canceled -= instance.OnSkip;
			Close.started -= instance.OnClose;
			Close.performed -= instance.OnClose;
			Close.canceled -= instance.OnClose;
		}

		public void RemoveCallbacks(IGameActions instance)
		{
			if (m_Wrapper.m_GameActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(IGameActions instance)
		{
			foreach (IGameActions gameActionsCallbackInterface in m_Wrapper.m_GameActionsCallbackInterfaces)
			{
				UnregisterCallbacks(gameActionsCallbackInterface);
			}
			m_Wrapper.m_GameActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public struct PhotoActions
	{
		private GameInputActions m_Wrapper;

		public InputAction CameraMove => m_Wrapper.m_Photo_CameraMove;

		public InputAction CameraZoom => m_Wrapper.m_Photo_CameraZoom;

		public bool enabled => Get().enabled;

		public PhotoActions(GameInputActions wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_Photo;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(PhotoActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(IPhotoActions instance)
		{
			if (instance != null && !m_Wrapper.m_PhotoActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_PhotoActionsCallbackInterfaces.Add(instance);
				CameraMove.started += instance.OnCameraMove;
				CameraMove.performed += instance.OnCameraMove;
				CameraMove.canceled += instance.OnCameraMove;
				CameraZoom.started += instance.OnCameraZoom;
				CameraZoom.performed += instance.OnCameraZoom;
				CameraZoom.canceled += instance.OnCameraZoom;
			}
		}

		private void UnregisterCallbacks(IPhotoActions instance)
		{
			CameraMove.started -= instance.OnCameraMove;
			CameraMove.performed -= instance.OnCameraMove;
			CameraMove.canceled -= instance.OnCameraMove;
			CameraZoom.started -= instance.OnCameraZoom;
			CameraZoom.performed -= instance.OnCameraZoom;
			CameraZoom.canceled -= instance.OnCameraZoom;
		}

		public void RemoveCallbacks(IPhotoActions instance)
		{
			if (m_Wrapper.m_PhotoActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(IPhotoActions instance)
		{
			foreach (IPhotoActions photoActionsCallbackInterface in m_Wrapper.m_PhotoActionsCallbackInterfaces)
			{
				UnregisterCallbacks(photoActionsCallbackInterface);
			}
			m_Wrapper.m_PhotoActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public struct UIActions
	{
		private GameInputActions m_Wrapper;

		public InputAction Navigate => m_Wrapper.m_UI_Navigate;

		public InputAction Submit => m_Wrapper.m_UI_Submit;

		public InputAction Cancel => m_Wrapper.m_UI_Cancel;

		public InputAction Point => m_Wrapper.m_UI_Point;

		public InputAction Click => m_Wrapper.m_UI_Click;

		public InputAction ScrollWheel => m_Wrapper.m_UI_ScrollWheel;

		public InputAction MiddleClick => m_Wrapper.m_UI_MiddleClick;

		public InputAction RightClick => m_Wrapper.m_UI_RightClick;

		public InputAction TrackedDevicePosition => m_Wrapper.m_UI_TrackedDevicePosition;

		public InputAction TrackedDeviceOrientation => m_Wrapper.m_UI_TrackedDeviceOrientation;

		public InputAction TabLeft => m_Wrapper.m_UI_TabLeft;

		public InputAction TabRight => m_Wrapper.m_UI_TabRight;

		public InputAction SecondaryTabLeft => m_Wrapper.m_UI_SecondaryTabLeft;

		public InputAction SecondaryTabRight => m_Wrapper.m_UI_SecondaryTabRight;

		public InputAction UseItem => m_Wrapper.m_UI_UseItem;

		public InputAction TakeItem => m_Wrapper.m_UI_TakeItem;

		public InputAction MapScroll => m_Wrapper.m_UI_MapScroll;

		public InputAction MapUpFloor => m_Wrapper.m_UI_MapUpFloor;

		public InputAction MapDownFloor => m_Wrapper.m_UI_MapDownFloor;

		public InputAction MapZoom => m_Wrapper.m_UI_MapZoom;

		public InputAction MapAddMarker => m_Wrapper.m_UI_MapAddMarker;

		public InputAction MapDeletePhoto => m_Wrapper.m_UI_MapDeletePhoto;

		public InputAction MapViewPhotos => m_Wrapper.m_UI_MapViewPhotos;

		public InputAction NextPage => m_Wrapper.m_UI_NextPage;

		public InputAction PreviousPage => m_Wrapper.m_UI_PreviousPage;

		public InputAction MoveSelected => m_Wrapper.m_UI_MoveSelected;

		public InputAction SecondaryMenuAction => m_Wrapper.m_UI_SecondaryMenuAction;

		public InputAction RotateSelected => m_Wrapper.m_UI_RotateSelected;

		public InputAction ToggleItemShortcut => m_Wrapper.m_UI_ToggleItemShortcut;

		public InputAction LocateItemOnMap => m_Wrapper.m_UI_LocateItemOnMap;

		public InputAction MapCycleSelection => m_Wrapper.m_UI_MapCycleSelection;

		public InputAction SubmitGamepadOnly => m_Wrapper.m_UI_SubmitGamepadOnly;

		public InputAction TakeMinigamePhoto => m_Wrapper.m_UI_TakeMinigamePhoto;

		public bool enabled => Get().enabled;

		public UIActions(GameInputActions wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_UI;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(UIActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(IUIActions instance)
		{
			if (instance != null && !m_Wrapper.m_UIActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_UIActionsCallbackInterfaces.Add(instance);
				Navigate.started += instance.OnNavigate;
				Navigate.performed += instance.OnNavigate;
				Navigate.canceled += instance.OnNavigate;
				Submit.started += instance.OnSubmit;
				Submit.performed += instance.OnSubmit;
				Submit.canceled += instance.OnSubmit;
				Cancel.started += instance.OnCancel;
				Cancel.performed += instance.OnCancel;
				Cancel.canceled += instance.OnCancel;
				Point.started += instance.OnPoint;
				Point.performed += instance.OnPoint;
				Point.canceled += instance.OnPoint;
				Click.started += instance.OnClick;
				Click.performed += instance.OnClick;
				Click.canceled += instance.OnClick;
				ScrollWheel.started += instance.OnScrollWheel;
				ScrollWheel.performed += instance.OnScrollWheel;
				ScrollWheel.canceled += instance.OnScrollWheel;
				MiddleClick.started += instance.OnMiddleClick;
				MiddleClick.performed += instance.OnMiddleClick;
				MiddleClick.canceled += instance.OnMiddleClick;
				RightClick.started += instance.OnRightClick;
				RightClick.performed += instance.OnRightClick;
				RightClick.canceled += instance.OnRightClick;
				TrackedDevicePosition.started += instance.OnTrackedDevicePosition;
				TrackedDevicePosition.performed += instance.OnTrackedDevicePosition;
				TrackedDevicePosition.canceled += instance.OnTrackedDevicePosition;
				TrackedDeviceOrientation.started += instance.OnTrackedDeviceOrientation;
				TrackedDeviceOrientation.performed += instance.OnTrackedDeviceOrientation;
				TrackedDeviceOrientation.canceled += instance.OnTrackedDeviceOrientation;
				TabLeft.started += instance.OnTabLeft;
				TabLeft.performed += instance.OnTabLeft;
				TabLeft.canceled += instance.OnTabLeft;
				TabRight.started += instance.OnTabRight;
				TabRight.performed += instance.OnTabRight;
				TabRight.canceled += instance.OnTabRight;
				SecondaryTabLeft.started += instance.OnSecondaryTabLeft;
				SecondaryTabLeft.performed += instance.OnSecondaryTabLeft;
				SecondaryTabLeft.canceled += instance.OnSecondaryTabLeft;
				SecondaryTabRight.started += instance.OnSecondaryTabRight;
				SecondaryTabRight.performed += instance.OnSecondaryTabRight;
				SecondaryTabRight.canceled += instance.OnSecondaryTabRight;
				UseItem.started += instance.OnUseItem;
				UseItem.performed += instance.OnUseItem;
				UseItem.canceled += instance.OnUseItem;
				TakeItem.started += instance.OnTakeItem;
				TakeItem.performed += instance.OnTakeItem;
				TakeItem.canceled += instance.OnTakeItem;
				MapScroll.started += instance.OnMapScroll;
				MapScroll.performed += instance.OnMapScroll;
				MapScroll.canceled += instance.OnMapScroll;
				MapUpFloor.started += instance.OnMapUpFloor;
				MapUpFloor.performed += instance.OnMapUpFloor;
				MapUpFloor.canceled += instance.OnMapUpFloor;
				MapDownFloor.started += instance.OnMapDownFloor;
				MapDownFloor.performed += instance.OnMapDownFloor;
				MapDownFloor.canceled += instance.OnMapDownFloor;
				MapZoom.started += instance.OnMapZoom;
				MapZoom.performed += instance.OnMapZoom;
				MapZoom.canceled += instance.OnMapZoom;
				MapAddMarker.started += instance.OnMapAddMarker;
				MapAddMarker.performed += instance.OnMapAddMarker;
				MapAddMarker.canceled += instance.OnMapAddMarker;
				MapDeletePhoto.started += instance.OnMapDeletePhoto;
				MapDeletePhoto.performed += instance.OnMapDeletePhoto;
				MapDeletePhoto.canceled += instance.OnMapDeletePhoto;
				MapViewPhotos.started += instance.OnMapViewPhotos;
				MapViewPhotos.performed += instance.OnMapViewPhotos;
				MapViewPhotos.canceled += instance.OnMapViewPhotos;
				NextPage.started += instance.OnNextPage;
				NextPage.performed += instance.OnNextPage;
				NextPage.canceled += instance.OnNextPage;
				PreviousPage.started += instance.OnPreviousPage;
				PreviousPage.performed += instance.OnPreviousPage;
				PreviousPage.canceled += instance.OnPreviousPage;
				MoveSelected.started += instance.OnMoveSelected;
				MoveSelected.performed += instance.OnMoveSelected;
				MoveSelected.canceled += instance.OnMoveSelected;
				SecondaryMenuAction.started += instance.OnSecondaryMenuAction;
				SecondaryMenuAction.performed += instance.OnSecondaryMenuAction;
				SecondaryMenuAction.canceled += instance.OnSecondaryMenuAction;
				RotateSelected.started += instance.OnRotateSelected;
				RotateSelected.performed += instance.OnRotateSelected;
				RotateSelected.canceled += instance.OnRotateSelected;
				ToggleItemShortcut.started += instance.OnToggleItemShortcut;
				ToggleItemShortcut.performed += instance.OnToggleItemShortcut;
				ToggleItemShortcut.canceled += instance.OnToggleItemShortcut;
				LocateItemOnMap.started += instance.OnLocateItemOnMap;
				LocateItemOnMap.performed += instance.OnLocateItemOnMap;
				LocateItemOnMap.canceled += instance.OnLocateItemOnMap;
				MapCycleSelection.started += instance.OnMapCycleSelection;
				MapCycleSelection.performed += instance.OnMapCycleSelection;
				MapCycleSelection.canceled += instance.OnMapCycleSelection;
				SubmitGamepadOnly.started += instance.OnSubmitGamepadOnly;
				SubmitGamepadOnly.performed += instance.OnSubmitGamepadOnly;
				SubmitGamepadOnly.canceled += instance.OnSubmitGamepadOnly;
				TakeMinigamePhoto.started += instance.OnTakeMinigamePhoto;
				TakeMinigamePhoto.performed += instance.OnTakeMinigamePhoto;
				TakeMinigamePhoto.canceled += instance.OnTakeMinigamePhoto;
			}
		}

		private void UnregisterCallbacks(IUIActions instance)
		{
			Navigate.started -= instance.OnNavigate;
			Navigate.performed -= instance.OnNavigate;
			Navigate.canceled -= instance.OnNavigate;
			Submit.started -= instance.OnSubmit;
			Submit.performed -= instance.OnSubmit;
			Submit.canceled -= instance.OnSubmit;
			Cancel.started -= instance.OnCancel;
			Cancel.performed -= instance.OnCancel;
			Cancel.canceled -= instance.OnCancel;
			Point.started -= instance.OnPoint;
			Point.performed -= instance.OnPoint;
			Point.canceled -= instance.OnPoint;
			Click.started -= instance.OnClick;
			Click.performed -= instance.OnClick;
			Click.canceled -= instance.OnClick;
			ScrollWheel.started -= instance.OnScrollWheel;
			ScrollWheel.performed -= instance.OnScrollWheel;
			ScrollWheel.canceled -= instance.OnScrollWheel;
			MiddleClick.started -= instance.OnMiddleClick;
			MiddleClick.performed -= instance.OnMiddleClick;
			MiddleClick.canceled -= instance.OnMiddleClick;
			RightClick.started -= instance.OnRightClick;
			RightClick.performed -= instance.OnRightClick;
			RightClick.canceled -= instance.OnRightClick;
			TrackedDevicePosition.started -= instance.OnTrackedDevicePosition;
			TrackedDevicePosition.performed -= instance.OnTrackedDevicePosition;
			TrackedDevicePosition.canceled -= instance.OnTrackedDevicePosition;
			TrackedDeviceOrientation.started -= instance.OnTrackedDeviceOrientation;
			TrackedDeviceOrientation.performed -= instance.OnTrackedDeviceOrientation;
			TrackedDeviceOrientation.canceled -= instance.OnTrackedDeviceOrientation;
			TabLeft.started -= instance.OnTabLeft;
			TabLeft.performed -= instance.OnTabLeft;
			TabLeft.canceled -= instance.OnTabLeft;
			TabRight.started -= instance.OnTabRight;
			TabRight.performed -= instance.OnTabRight;
			TabRight.canceled -= instance.OnTabRight;
			SecondaryTabLeft.started -= instance.OnSecondaryTabLeft;
			SecondaryTabLeft.performed -= instance.OnSecondaryTabLeft;
			SecondaryTabLeft.canceled -= instance.OnSecondaryTabLeft;
			SecondaryTabRight.started -= instance.OnSecondaryTabRight;
			SecondaryTabRight.performed -= instance.OnSecondaryTabRight;
			SecondaryTabRight.canceled -= instance.OnSecondaryTabRight;
			UseItem.started -= instance.OnUseItem;
			UseItem.performed -= instance.OnUseItem;
			UseItem.canceled -= instance.OnUseItem;
			TakeItem.started -= instance.OnTakeItem;
			TakeItem.performed -= instance.OnTakeItem;
			TakeItem.canceled -= instance.OnTakeItem;
			MapScroll.started -= instance.OnMapScroll;
			MapScroll.performed -= instance.OnMapScroll;
			MapScroll.canceled -= instance.OnMapScroll;
			MapUpFloor.started -= instance.OnMapUpFloor;
			MapUpFloor.performed -= instance.OnMapUpFloor;
			MapUpFloor.canceled -= instance.OnMapUpFloor;
			MapDownFloor.started -= instance.OnMapDownFloor;
			MapDownFloor.performed -= instance.OnMapDownFloor;
			MapDownFloor.canceled -= instance.OnMapDownFloor;
			MapZoom.started -= instance.OnMapZoom;
			MapZoom.performed -= instance.OnMapZoom;
			MapZoom.canceled -= instance.OnMapZoom;
			MapAddMarker.started -= instance.OnMapAddMarker;
			MapAddMarker.performed -= instance.OnMapAddMarker;
			MapAddMarker.canceled -= instance.OnMapAddMarker;
			MapDeletePhoto.started -= instance.OnMapDeletePhoto;
			MapDeletePhoto.performed -= instance.OnMapDeletePhoto;
			MapDeletePhoto.canceled -= instance.OnMapDeletePhoto;
			MapViewPhotos.started -= instance.OnMapViewPhotos;
			MapViewPhotos.performed -= instance.OnMapViewPhotos;
			MapViewPhotos.canceled -= instance.OnMapViewPhotos;
			NextPage.started -= instance.OnNextPage;
			NextPage.performed -= instance.OnNextPage;
			NextPage.canceled -= instance.OnNextPage;
			PreviousPage.started -= instance.OnPreviousPage;
			PreviousPage.performed -= instance.OnPreviousPage;
			PreviousPage.canceled -= instance.OnPreviousPage;
			MoveSelected.started -= instance.OnMoveSelected;
			MoveSelected.performed -= instance.OnMoveSelected;
			MoveSelected.canceled -= instance.OnMoveSelected;
			SecondaryMenuAction.started -= instance.OnSecondaryMenuAction;
			SecondaryMenuAction.performed -= instance.OnSecondaryMenuAction;
			SecondaryMenuAction.canceled -= instance.OnSecondaryMenuAction;
			RotateSelected.started -= instance.OnRotateSelected;
			RotateSelected.performed -= instance.OnRotateSelected;
			RotateSelected.canceled -= instance.OnRotateSelected;
			ToggleItemShortcut.started -= instance.OnToggleItemShortcut;
			ToggleItemShortcut.performed -= instance.OnToggleItemShortcut;
			ToggleItemShortcut.canceled -= instance.OnToggleItemShortcut;
			LocateItemOnMap.started -= instance.OnLocateItemOnMap;
			LocateItemOnMap.performed -= instance.OnLocateItemOnMap;
			LocateItemOnMap.canceled -= instance.OnLocateItemOnMap;
			MapCycleSelection.started -= instance.OnMapCycleSelection;
			MapCycleSelection.performed -= instance.OnMapCycleSelection;
			MapCycleSelection.canceled -= instance.OnMapCycleSelection;
			SubmitGamepadOnly.started -= instance.OnSubmitGamepadOnly;
			SubmitGamepadOnly.performed -= instance.OnSubmitGamepadOnly;
			SubmitGamepadOnly.canceled -= instance.OnSubmitGamepadOnly;
			TakeMinigamePhoto.started -= instance.OnTakeMinigamePhoto;
			TakeMinigamePhoto.performed -= instance.OnTakeMinigamePhoto;
			TakeMinigamePhoto.canceled -= instance.OnTakeMinigamePhoto;
		}

		public void RemoveCallbacks(IUIActions instance)
		{
			if (m_Wrapper.m_UIActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(IUIActions instance)
		{
			foreach (IUIActions uIActionsCallbackInterface in m_Wrapper.m_UIActionsCallbackInterfaces)
			{
				UnregisterCallbacks(uIActionsCallbackInterface);
			}
			m_Wrapper.m_UIActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public struct MinigameActions
	{
		private GameInputActions m_Wrapper;

		public InputAction HorizontalMove => m_Wrapper.m_Minigame_HorizontalMove;

		public InputAction VerticalMove => m_Wrapper.m_Minigame_VerticalMove;

		public InputAction RotateObjectHold => m_Wrapper.m_Minigame_RotateObjectHold;

		public InputAction Move2D => m_Wrapper.m_Minigame_Move2D;

		public bool enabled => Get().enabled;

		public MinigameActions(GameInputActions wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_Minigame;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(MinigameActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(IMinigameActions instance)
		{
			if (instance != null && !m_Wrapper.m_MinigameActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_MinigameActionsCallbackInterfaces.Add(instance);
				HorizontalMove.started += instance.OnHorizontalMove;
				HorizontalMove.performed += instance.OnHorizontalMove;
				HorizontalMove.canceled += instance.OnHorizontalMove;
				VerticalMove.started += instance.OnVerticalMove;
				VerticalMove.performed += instance.OnVerticalMove;
				VerticalMove.canceled += instance.OnVerticalMove;
				RotateObjectHold.started += instance.OnRotateObjectHold;
				RotateObjectHold.performed += instance.OnRotateObjectHold;
				RotateObjectHold.canceled += instance.OnRotateObjectHold;
				Move2D.started += instance.OnMove2D;
				Move2D.performed += instance.OnMove2D;
				Move2D.canceled += instance.OnMove2D;
			}
		}

		private void UnregisterCallbacks(IMinigameActions instance)
		{
			HorizontalMove.started -= instance.OnHorizontalMove;
			HorizontalMove.performed -= instance.OnHorizontalMove;
			HorizontalMove.canceled -= instance.OnHorizontalMove;
			VerticalMove.started -= instance.OnVerticalMove;
			VerticalMove.performed -= instance.OnVerticalMove;
			VerticalMove.canceled -= instance.OnVerticalMove;
			RotateObjectHold.started -= instance.OnRotateObjectHold;
			RotateObjectHold.performed -= instance.OnRotateObjectHold;
			RotateObjectHold.canceled -= instance.OnRotateObjectHold;
			Move2D.started -= instance.OnMove2D;
			Move2D.performed -= instance.OnMove2D;
			Move2D.canceled -= instance.OnMove2D;
		}

		public void RemoveCallbacks(IMinigameActions instance)
		{
			if (m_Wrapper.m_MinigameActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(IMinigameActions instance)
		{
			foreach (IMinigameActions minigameActionsCallbackInterface in m_Wrapper.m_MinigameActionsCallbackInterfaces)
			{
				UnregisterCallbacks(minigameActionsCallbackInterface);
			}
			m_Wrapper.m_MinigameActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public struct DebugActions
	{
		private GameInputActions m_Wrapper;

		public InputAction ToggleConsole => m_Wrapper.m_Debug_ToggleConsole;

		public InputAction CloseConsole => m_Wrapper.m_Debug_CloseConsole;

		public InputAction SubmitConsoleCommand => m_Wrapper.m_Debug_SubmitConsoleCommand;

		public bool enabled => Get().enabled;

		public DebugActions(GameInputActions wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_Debug;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(DebugActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(IDebugActions instance)
		{
			if (instance != null && !m_Wrapper.m_DebugActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_DebugActionsCallbackInterfaces.Add(instance);
				ToggleConsole.started += instance.OnToggleConsole;
				ToggleConsole.performed += instance.OnToggleConsole;
				ToggleConsole.canceled += instance.OnToggleConsole;
				CloseConsole.started += instance.OnCloseConsole;
				CloseConsole.performed += instance.OnCloseConsole;
				CloseConsole.canceled += instance.OnCloseConsole;
				SubmitConsoleCommand.started += instance.OnSubmitConsoleCommand;
				SubmitConsoleCommand.performed += instance.OnSubmitConsoleCommand;
				SubmitConsoleCommand.canceled += instance.OnSubmitConsoleCommand;
			}
		}

		private void UnregisterCallbacks(IDebugActions instance)
		{
			ToggleConsole.started -= instance.OnToggleConsole;
			ToggleConsole.performed -= instance.OnToggleConsole;
			ToggleConsole.canceled -= instance.OnToggleConsole;
			CloseConsole.started -= instance.OnCloseConsole;
			CloseConsole.performed -= instance.OnCloseConsole;
			CloseConsole.canceled -= instance.OnCloseConsole;
			SubmitConsoleCommand.started -= instance.OnSubmitConsoleCommand;
			SubmitConsoleCommand.performed -= instance.OnSubmitConsoleCommand;
			SubmitConsoleCommand.canceled -= instance.OnSubmitConsoleCommand;
		}

		public void RemoveCallbacks(IDebugActions instance)
		{
			if (m_Wrapper.m_DebugActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(IDebugActions instance)
		{
			foreach (IDebugActions debugActionsCallbackInterface in m_Wrapper.m_DebugActionsCallbackInterfaces)
			{
				UnregisterCallbacks(debugActionsCallbackInterface);
			}
			m_Wrapper.m_DebugActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public struct EngagementActions
	{
		private GameInputActions m_Wrapper;

		public InputAction Engagement => m_Wrapper.m_Engagement_Engagement;

		public bool enabled => Get().enabled;

		public EngagementActions(GameInputActions wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_Engagement;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(EngagementActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(IEngagementActions instance)
		{
			if (instance != null && !m_Wrapper.m_EngagementActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_EngagementActionsCallbackInterfaces.Add(instance);
				Engagement.started += instance.OnEngagement;
				Engagement.performed += instance.OnEngagement;
				Engagement.canceled += instance.OnEngagement;
			}
		}

		private void UnregisterCallbacks(IEngagementActions instance)
		{
			Engagement.started -= instance.OnEngagement;
			Engagement.performed -= instance.OnEngagement;
			Engagement.canceled -= instance.OnEngagement;
		}

		public void RemoveCallbacks(IEngagementActions instance)
		{
			if (m_Wrapper.m_EngagementActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(IEngagementActions instance)
		{
			foreach (IEngagementActions engagementActionsCallbackInterface in m_Wrapper.m_EngagementActionsCallbackInterfaces)
			{
				UnregisterCallbacks(engagementActionsCallbackInterface);
			}
			m_Wrapper.m_EngagementActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public interface IPlayerActions
	{
		void OnMove(InputAction.CallbackContext context);

		void OnAimDirection(InputAction.CallbackContext context);

		void OnFire(InputAction.CallbackContext context);

		void OnSprint(InputAction.CallbackContext context);

		void OnCrouch(InputAction.CallbackContext context);

		void OnJump(InputAction.CallbackContext context);

		void OnDodge(InputAction.CallbackContext context);

		void OnAimMode(InputAction.CallbackContext context);

		void OnInteract(InputAction.CallbackContext context);

		void OnReload(InputAction.CallbackContext context);

		void OnQuickMap(InputAction.CallbackContext context);

		void OnStomp(InputAction.CallbackContext context);

		void OnShortcutFlashlight(InputAction.CallbackContext context);

		void OnPlayerCancel(InputAction.CallbackContext context);

		void OnShortcutCamera(InputAction.CallbackContext context);

		void OnQuickItemRight(InputAction.CallbackContext context);

		void OnQuickItemUse(InputAction.CallbackContext context);

		void OnQuickItemLeft(InputAction.CallbackContext context);

		void OnCycleWeapon(InputAction.CallbackContext context);
	}

	public interface IMenuTogglesActions
	{
		void OnSystemMenu(InputAction.CallbackContext context);

		void OnMap(InputAction.CallbackContext context);

		void OnInventory(InputAction.CallbackContext context);

		void OnArtifacts(InputAction.CallbackContext context);

		void OnNotes(InputAction.CallbackContext context);

		void OnItemWheel(InputAction.CallbackContext context);

		void OnQuickItemPanel(InputAction.CallbackContext context);
	}

	public interface IGameActions
	{
		void OnProceed(InputAction.CallbackContext context);

		void OnSkip(InputAction.CallbackContext context);

		void OnClose(InputAction.CallbackContext context);
	}

	public interface IPhotoActions
	{
		void OnCameraMove(InputAction.CallbackContext context);

		void OnCameraZoom(InputAction.CallbackContext context);
	}

	public interface IUIActions
	{
		void OnNavigate(InputAction.CallbackContext context);

		void OnSubmit(InputAction.CallbackContext context);

		void OnCancel(InputAction.CallbackContext context);

		void OnPoint(InputAction.CallbackContext context);

		void OnClick(InputAction.CallbackContext context);

		void OnScrollWheel(InputAction.CallbackContext context);

		void OnMiddleClick(InputAction.CallbackContext context);

		void OnRightClick(InputAction.CallbackContext context);

		void OnTrackedDevicePosition(InputAction.CallbackContext context);

		void OnTrackedDeviceOrientation(InputAction.CallbackContext context);

		void OnTabLeft(InputAction.CallbackContext context);

		void OnTabRight(InputAction.CallbackContext context);

		void OnSecondaryTabLeft(InputAction.CallbackContext context);

		void OnSecondaryTabRight(InputAction.CallbackContext context);

		void OnUseItem(InputAction.CallbackContext context);

		void OnTakeItem(InputAction.CallbackContext context);

		void OnMapScroll(InputAction.CallbackContext context);

		void OnMapUpFloor(InputAction.CallbackContext context);

		void OnMapDownFloor(InputAction.CallbackContext context);

		void OnMapZoom(InputAction.CallbackContext context);

		void OnMapAddMarker(InputAction.CallbackContext context);

		void OnMapDeletePhoto(InputAction.CallbackContext context);

		void OnMapViewPhotos(InputAction.CallbackContext context);

		void OnNextPage(InputAction.CallbackContext context);

		void OnPreviousPage(InputAction.CallbackContext context);

		void OnMoveSelected(InputAction.CallbackContext context);

		void OnSecondaryMenuAction(InputAction.CallbackContext context);

		void OnRotateSelected(InputAction.CallbackContext context);

		void OnToggleItemShortcut(InputAction.CallbackContext context);

		void OnLocateItemOnMap(InputAction.CallbackContext context);

		void OnMapCycleSelection(InputAction.CallbackContext context);

		void OnSubmitGamepadOnly(InputAction.CallbackContext context);

		void OnTakeMinigamePhoto(InputAction.CallbackContext context);
	}

	public interface IMinigameActions
	{
		void OnHorizontalMove(InputAction.CallbackContext context);

		void OnVerticalMove(InputAction.CallbackContext context);

		void OnRotateObjectHold(InputAction.CallbackContext context);

		void OnMove2D(InputAction.CallbackContext context);
	}

	public interface IDebugActions
	{
		void OnToggleConsole(InputAction.CallbackContext context);

		void OnCloseConsole(InputAction.CallbackContext context);

		void OnSubmitConsoleCommand(InputAction.CallbackContext context);
	}

	public interface IEngagementActions
	{
		void OnEngagement(InputAction.CallbackContext context);
	}

	private readonly InputActionMap m_Player;

	private List<IPlayerActions> m_PlayerActionsCallbackInterfaces = new List<IPlayerActions>();

	private readonly InputAction m_Player_Move;

	private readonly InputAction m_Player_AimDirection;

	private readonly InputAction m_Player_Fire;

	private readonly InputAction m_Player_Sprint;

	private readonly InputAction m_Player_Crouch;

	private readonly InputAction m_Player_Jump;

	private readonly InputAction m_Player_Dodge;

	private readonly InputAction m_Player_AimMode;

	private readonly InputAction m_Player_Interact;

	private readonly InputAction m_Player_Reload;

	private readonly InputAction m_Player_QuickMap;

	private readonly InputAction m_Player_Stomp;

	private readonly InputAction m_Player_ShortcutFlashlight;

	private readonly InputAction m_Player_PlayerCancel;

	private readonly InputAction m_Player_ShortcutCamera;

	private readonly InputAction m_Player_QuickItemRight;

	private readonly InputAction m_Player_QuickItemUse;

	private readonly InputAction m_Player_QuickItemLeft;

	private readonly InputAction m_Player_CycleWeapon;

	private readonly InputActionMap m_MenuToggles;

	private List<IMenuTogglesActions> m_MenuTogglesActionsCallbackInterfaces = new List<IMenuTogglesActions>();

	private readonly InputAction m_MenuToggles_SystemMenu;

	private readonly InputAction m_MenuToggles_Map;

	private readonly InputAction m_MenuToggles_Inventory;

	private readonly InputAction m_MenuToggles_Artifacts;

	private readonly InputAction m_MenuToggles_Notes;

	private readonly InputAction m_MenuToggles_ItemWheel;

	private readonly InputAction m_MenuToggles_QuickItemPanel;

	private readonly InputActionMap m_Game;

	private List<IGameActions> m_GameActionsCallbackInterfaces = new List<IGameActions>();

	private readonly InputAction m_Game_Proceed;

	private readonly InputAction m_Game_Skip;

	private readonly InputAction m_Game_Close;

	private readonly InputActionMap m_Photo;

	private List<IPhotoActions> m_PhotoActionsCallbackInterfaces = new List<IPhotoActions>();

	private readonly InputAction m_Photo_CameraMove;

	private readonly InputAction m_Photo_CameraZoom;

	private readonly InputActionMap m_UI;

	private List<IUIActions> m_UIActionsCallbackInterfaces = new List<IUIActions>();

	private readonly InputAction m_UI_Navigate;

	private readonly InputAction m_UI_Submit;

	private readonly InputAction m_UI_Cancel;

	private readonly InputAction m_UI_Point;

	private readonly InputAction m_UI_Click;

	private readonly InputAction m_UI_ScrollWheel;

	private readonly InputAction m_UI_MiddleClick;

	private readonly InputAction m_UI_RightClick;

	private readonly InputAction m_UI_TrackedDevicePosition;

	private readonly InputAction m_UI_TrackedDeviceOrientation;

	private readonly InputAction m_UI_TabLeft;

	private readonly InputAction m_UI_TabRight;

	private readonly InputAction m_UI_SecondaryTabLeft;

	private readonly InputAction m_UI_SecondaryTabRight;

	private readonly InputAction m_UI_UseItem;

	private readonly InputAction m_UI_TakeItem;

	private readonly InputAction m_UI_MapScroll;

	private readonly InputAction m_UI_MapUpFloor;

	private readonly InputAction m_UI_MapDownFloor;

	private readonly InputAction m_UI_MapZoom;

	private readonly InputAction m_UI_MapAddMarker;

	private readonly InputAction m_UI_MapDeletePhoto;

	private readonly InputAction m_UI_MapViewPhotos;

	private readonly InputAction m_UI_NextPage;

	private readonly InputAction m_UI_PreviousPage;

	private readonly InputAction m_UI_MoveSelected;

	private readonly InputAction m_UI_SecondaryMenuAction;

	private readonly InputAction m_UI_RotateSelected;

	private readonly InputAction m_UI_ToggleItemShortcut;

	private readonly InputAction m_UI_LocateItemOnMap;

	private readonly InputAction m_UI_MapCycleSelection;

	private readonly InputAction m_UI_SubmitGamepadOnly;

	private readonly InputAction m_UI_TakeMinigamePhoto;

	private readonly InputActionMap m_Minigame;

	private List<IMinigameActions> m_MinigameActionsCallbackInterfaces = new List<IMinigameActions>();

	private readonly InputAction m_Minigame_HorizontalMove;

	private readonly InputAction m_Minigame_VerticalMove;

	private readonly InputAction m_Minigame_RotateObjectHold;

	private readonly InputAction m_Minigame_Move2D;

	private readonly InputActionMap m_Debug;

	private List<IDebugActions> m_DebugActionsCallbackInterfaces = new List<IDebugActions>();

	private readonly InputAction m_Debug_ToggleConsole;

	private readonly InputAction m_Debug_CloseConsole;

	private readonly InputAction m_Debug_SubmitConsoleCommand;

	private readonly InputActionMap m_Engagement;

	private List<IEngagementActions> m_EngagementActionsCallbackInterfaces = new List<IEngagementActions>();

	private readonly InputAction m_Engagement_Engagement;

	private int m_KeyboardMouseSchemeIndex = -1;

	private int m_GamepadSchemeIndex = -1;

	public InputActionAsset asset { get; }

	public InputBinding? bindingMask
	{
		get
		{
			return asset.bindingMask;
		}
		set
		{
			asset.bindingMask = value;
		}
	}

	public ReadOnlyArray<InputDevice>? devices
	{
		get
		{
			return asset.devices;
		}
		set
		{
			asset.devices = value;
		}
	}

	public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

	public IEnumerable<InputBinding> bindings => asset.bindings;

	public PlayerActions Player => new PlayerActions(this);

	public MenuTogglesActions MenuToggles => new MenuTogglesActions(this);

	public GameActions Game => new GameActions(this);

	public PhotoActions Photo => new PhotoActions(this);

	public UIActions UI => new UIActions(this);

	public MinigameActions Minigame => new MinigameActions(this);

	public DebugActions Debug => new DebugActions(this);

	public EngagementActions Engagement => new EngagementActions(this);

	public InputControlScheme KeyboardMouseScheme
	{
		get
		{
			if (m_KeyboardMouseSchemeIndex == -1)
			{
				m_KeyboardMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard&Mouse");
			}
			return asset.controlSchemes[m_KeyboardMouseSchemeIndex];
		}
	}

	public InputControlScheme GamepadScheme
	{
		get
		{
			if (m_GamepadSchemeIndex == -1)
			{
				m_GamepadSchemeIndex = asset.FindControlSchemeIndex("Gamepad");
			}
			return asset.controlSchemes[m_GamepadSchemeIndex];
		}
	}

	public GameInputActions()
	{
		asset = InputActionAsset.FromJson("{\r\n    \"version\": 1,\r\n    \"name\": \"GameInputActions\",\r\n    \"maps\": [\r\n        {\r\n            \"name\": \"Player\",\r\n            \"id\": \"a4ecfdfd-9133-4840-8d61-049a0dcbaa1b\",\r\n            \"actions\": [\r\n                {\r\n                    \"name\": \"Move\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"4ff7778c-dcae-4dee-a3ee-f2afb771557e\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"AimDirection\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"cca8a9a2-50c1-4e79-8833-d906e3a5bbf3\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"Fire\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"2505f4c9-5af8-4317-83ec-44243e993777\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Sprint\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"ca06282f-e767-4797-b45b-e15157face3f\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"Crouch\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"9c9f0a0b-9c1e-4829-bb7b-862eb11e3e7a\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"Jump\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"724189b0-13d5-48c5-8248-4cc3eaa3a89f\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Dodge\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"d1fb99d8-f7a3-40c5-afe8-7bff2e57ccb4\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"AimMode\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"8e635c5c-022e-425e-98d9-beafe35a11ba\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Interact\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"dbc359ee-c9c1-48ea-a056-7f4794c6f486\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Reload\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"f10bd295-8191-42a6-b0c2-0c0583f4357d\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"QuickMap\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"f5e863a2-b73d-4ba1-936a-a617926bfdd8\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Stomp\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"0f2db5c9-f6ca-44ba-a925-732107a61e4b\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"ShortcutFlashlight\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"74cac755-5215-4b07-8f60-ff7f3671a6c0\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"PlayerCancel\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"ed75dfb4-6276-4028-8fc0-e54d8ecb620c\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"ShortcutCamera\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"bae01fd2-2a88-48ea-941a-1fc527235c43\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"QuickItemRight\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"6a139d8b-af0b-4dbe-a813-f120523c1d1d\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"QuickItemUse\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"22861e6b-89db-4738-acd6-ed72899302ae\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"QuickItemLeft\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"a133a20c-81b6-460d-968f-707d3ee2fb8a\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"CycleWeapon\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"5528dcc8-2799-44e2-9344-e030dcda66b9\",\r\n                    \"expectedControlType\": \"Axis\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                }\r\n            ],\r\n            \"bindings\": [\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c1f7a91b-d0fd-4a62-997e-7fb9b69bf235\",\r\n                    \"path\": \"<Gamepad>/rightStick\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"StickDeadzone\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"AimDirection\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"2226bdc4-1b89-4415-901c-e9273a80e436\",\r\n                    \"path\": \"<Mouse>/delta\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"AimDirection\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"143bb1cd-cc10-4eca-a2f0-a3664166fe91\",\r\n                    \"path\": \"<Gamepad>/rightTrigger\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Fire\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"05f6913d-c316-48b2-a6bb-e225f14c7960\",\r\n                    \"path\": \"<Mouse>/leftButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Fire\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"7a32c68b-ccd4-405c-98fd-6b1823e483e1\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Fire\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"ce139025-0e7e-4ae1-9482-271d3e192b99\",\r\n                    \"path\": \"<Keyboard>/ctrl\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Crouch\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"3873f027-ea7e-417e-a455-21c129e2a5c6\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Crouch\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c9e37f9d-b0c2-49f3-9e33-bf8001711bf6\",\r\n                    \"path\": \"<Gamepad>/leftStickPress\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"Crouch\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"4503f9f0-90eb-44f7-8f8a-a70fbbec3df4\",\r\n                    \"path\": \"<Keyboard>/alt\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Jump\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"150c671c-8aea-4fd8-9d7d-c908fdc610e1\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Jump\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"7c7da51e-d052-4b63-b0e7-dc3b78031ac4\",\r\n                    \"path\": \"<Gamepad>/buttonNorth\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"Jump\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"622850cc-05bf-4416-a25a-afb711dee765\",\r\n                    \"path\": \"<Mouse>/rightButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"AimMode\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"edbf403a-4b59-4edb-9cf0-5712dce77118\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"AimMode\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"482410cd-1e8c-49c0-bc3a-c2677a331980\",\r\n                    \"path\": \"<Gamepad>/leftTrigger\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"AimMode\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"5290b353-0c7a-4307-afa6-37c52b41830f\",\r\n                    \"path\": \"<Gamepad>/{PrimaryAction}\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"Interact\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"6a23152d-a70c-4475-bd21-95f4548afc40\",\r\n                    \"path\": \"<Keyboard>/e\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Interact\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"41e6616f-3970-49d5-a3b0-b5509c4416f7\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Interact\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"95f8634f-a831-4de0-86f7-f36a2be65d98\",\r\n                    \"path\": \"<Keyboard>/r\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Reload\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"f4d97f3d-3f96-4d65-8b83-f6d253f1e025\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Reload\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"4fe04269-2ba9-42bf-9c5a-90c6cfffa686\",\r\n                    \"path\": \"<Gamepad>/buttonWest\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"Reload\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"100a14b2-b064-4da8-a1e1-d46a6df7125c\",\r\n                    \"path\": \"<Keyboard>/space\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Dodge\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"2c00df88-5324-4e53-be79-fe84cf40f970\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Dodge\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"7756f6aa-a8a5-42ec-b158-a58bf683593d\",\r\n                    \"path\": \"<Gamepad>/{Cancel}\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"Dodge\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"8412ee41-5d04-4900-84a6-19cbe412c953\",\r\n                    \"path\": \"<Keyboard>/leftShift\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Sprint\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"0c81ae94-97b2-4d94-a153-e8cca75e6843\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Sprint\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"d6de55bc-5da2-4d89-b277-5da8d008f31d\",\r\n                    \"path\": \"<Gamepad>/leftShoulder\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"Sprint\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"e3664bcb-22fe-4265-a961-0553d0f39bb9\",\r\n                    \"path\": \"<Gamepad>/rightShoulder\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"QuickMap\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"73920bbe-bffa-4dd5-8212-f4b7cc7fc15f\",\r\n                    \"path\": \"<Keyboard>/capsLock\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"QuickMap\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"2d48a34b-00f1-4827-8547-f4b01b5ee38b\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"QuickMap\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"002ce835-8ab3-42a3-8022-a4133f9135f4\",\r\n                    \"path\": \"<Gamepad>/buttonWest\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Stomp\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"d39238bb-550d-40bb-bdcb-74a5f32b4f01\",\r\n                    \"path\": \"<Keyboard>/g\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Stomp\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"8ab8bd30-9b0a-4ae0-8b31-63d7d6887054\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Stomp\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c90574e2-b6dc-4bdf-979c-5db6d6d59f67\",\r\n                    \"path\": \"<Keyboard>/f\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"ShortcutFlashlight\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"9141ce20-ca8a-433e-9daf-a976a0734cc6\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"ShortcutFlashlight\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"0ca7da9f-b58d-41ee-9c02-9eceb5a18214\",\r\n                    \"path\": \"<Gamepad>/rightStickPress\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"ShortcutFlashlight\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"1c850326-f735-4969-881d-2f7f526b2c1b\",\r\n                    \"path\": \"<Gamepad>/{Cancel}\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"PlayerCancel\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"cbddc460-cb72-4749-8b10-8a6709066d22\",\r\n                    \"path\": \"<Mouse>/rightButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"PlayerCancel\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"6e0b6e03-7a97-414a-9ed4-8d26cede40e8\",\r\n                    \"path\": \"<Keyboard>/escape\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"PlayerCancel\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c9967cfa-c7da-44c4-ada3-76a77430df02\",\r\n                    \"path\": \"<Keyboard>/c\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"ShortcutCamera\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"3d66b410-a313-4274-829b-04cfd658b6d0\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"ShortcutCamera\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"91aad6e1-7cec-412d-84d7-13893982fa7e\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"ShortcutCamera\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"44964000-ad62-4f0e-b882-953f60776b05\",\r\n                    \"path\": \"<Gamepad>/dpad/right\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"QuickItemRight\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"186939c7-d654-4bb6-b119-2484e5de0879\",\r\n                    \"path\": \"<Keyboard>/3\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"QuickItemRight\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"96260ca0-1cda-422f-876f-492cc2550d53\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"QuickItemRight\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"2bfebd7d-2926-43bf-97a6-ee0fb1816a7f\",\r\n                    \"path\": \"<Gamepad>/dpad/down\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"QuickItemUse\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"7498112c-d770-4669-8ed5-656f3dfbf17e\",\r\n                    \"path\": \"<Keyboard>/q\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"QuickItemUse\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"550781ff-2187-47ce-9062-3f838b7f9bd1\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"QuickItemUse\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"eb4939a8-b7c2-45fa-811d-b443a18e80fd\",\r\n                    \"path\": \"<Gamepad>/dpad/left\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"QuickItemLeft\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"4e9513e6-5214-4eb7-b702-1b01b36c7ac7\",\r\n                    \"path\": \"<Keyboard>/1\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"QuickItemLeft\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"2df30fa1-c4c6-4a85-97d7-deb736ef8849\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"QuickItemLeft\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"Gamepad Dpad Axis\",\r\n                    \"id\": \"64cf3011-3d2e-4d8e-8a26-c5c38e1325bc\",\r\n                    \"path\": \"1DAxis\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"CycleWeapon\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"positive\",\r\n                    \"id\": \"b9a3aabb-df34-4430-be32-64d32d6c95ac\",\r\n                    \"path\": \"<Gamepad>/dpad/up\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"CycleWeapon\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"negative\",\r\n                    \"id\": \"f71c5eb9-f572-4525-bff6-8790177397d7\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"CycleWeapon\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"Mouse Cycle Axis\",\r\n                    \"id\": \"e5a7d3aa-4ee5-4fd1-b101-67c3069e9b2a\",\r\n                    \"path\": \"1DAxis\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"CycleWeapon\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"negative\",\r\n                    \"id\": \"17ce6d1d-32b5-40da-a428-cf58444227a1\",\r\n                    \"path\": \"<Mouse>/scroll/down\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"CycleWeapon\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"negative\",\r\n                    \"id\": \"71587bc4-91bf-4037-9936-e912ef51e102\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"CycleWeapon\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"positive\",\r\n                    \"id\": \"23721260-aca4-478a-8030-a99bd586e9cf\",\r\n                    \"path\": \"<Mouse>/scroll/up\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"CycleWeapon\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"positive\",\r\n                    \"id\": \"c87465c5-d92e-4155-a1b8-9eb9e962ce2a\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"CycleWeapon\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"96130a92-030f-4270-a6f6-d083f1884583\",\r\n                    \"path\": \"<Gamepad>/leftStick\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"StickDeadzone\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"WASD\",\r\n                    \"id\": \"de45142a-9730-44f9-bdfd-dce21a9af16b\",\r\n                    \"path\": \"Dpad\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"112600d9-0586-48e3-897d-47c308052959\",\r\n                    \"path\": \"<Keyboard>/w\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"c892137e-1c2c-483f-ab32-3df19fa94e26\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"9cf09446-46ac-4922-8b24-890323fe3e23\",\r\n                    \"path\": \"<Keyboard>/s\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"eb60a088-dc1b-437a-ba79-ecb9ef860968\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"72559801-b56b-42ec-8dd3-823b0f4a3395\",\r\n                    \"path\": \"<Keyboard>/a\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"7b14cf0c-6055-4ff2-80ce-344a2c069665\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"dbe62f85-d4f7-4bda-9874-043e336d132f\",\r\n                    \"path\": \"<Keyboard>/d\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"df4ae7e9-00e1-43a8-9170-7a20450ad722\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"MenuToggles\",\r\n            \"id\": \"785e42fe-4c27-4707-abf3-f563655c9642\",\r\n            \"actions\": [\r\n                {\r\n                    \"name\": \"SystemMenu\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"39f46d78-730c-4752-a4fe-e400abab84dd\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Map\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"288f7555-69be-49a2-97cc-fa24c8ca3ea3\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Inventory\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"a92115b5-7094-484e-b524-9480699bb5ee\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Artifacts\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"6732d5ac-80b0-45da-8500-ab394924a024\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Notes\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"af249abd-2ee2-45f9-8a64-64c1f95b0810\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"ItemWheel\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"ac5a6efd-8209-40ae-9f52-2b0467b62c54\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"QuickItemPanel\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"409d0203-3eb9-4982-94b6-1d34c93fc7b8\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                }\r\n            ],\r\n            \"bindings\": [\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"a356a671-a3d9-482c-b576-5d559af4a3cd\",\r\n                    \"path\": \"<DualShockGamepad>/touchpadButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"SystemMenu\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"df8949d8-0692-4525-aef9-1452f2f367a6\",\r\n                    \"path\": \"<Gamepad>/select\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"SystemMenu\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"37e96c17-3937-4bc6-af27-7b975b1e222e\",\r\n                    \"path\": \"<Keyboard>/escape\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"SystemMenu\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"7e36af91-d931-476a-9387-e165b1a24e7b\",\r\n                    \"path\": \"<Keyboard>/m\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Map\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"6dfd1cc5-ae08-4961-9c81-213279e530b6\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Map\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c3d8993f-bac5-4b68-8692-d783fe187439\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Map\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"d97fe585-860d-4a4d-bf35-98c6fe3b7e75\",\r\n                    \"path\": \"<Keyboard>/tab\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Inventory\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"16c5c51c-73fa-47e4-9a63-7eb5e60f9055\",\r\n                    \"path\": \"<Keyboard>/i\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Inventory\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"ed44dcc5-42d3-4204-b763-b1992767dfb5\",\r\n                    \"path\": \"<Gamepad>/start\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Inventory\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"19218446-507a-44cf-ab96-8fe31270f358\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Artifacts\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"89632c18-54e9-4282-933d-01483734c3e6\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Artifacts\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"f7e74268-a06f-4bee-91eb-dfb258e74566\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Artifacts\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"7280bc2b-b0bd-47ab-bd08-36a1a6558a8a\",\r\n                    \"path\": \"<Keyboard>/n\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Notes\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"4f0cd20f-fa30-493f-b727-ec49f6c43ada\",\r\n                    \"path\": \"<Keyboard>/j\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Notes\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"e76ce352-7f0a-4225-bbae-438a867187dc\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Notes\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"80f757be-9bbe-41ee-8ade-27c2671c72cd\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"QuickItemPanel\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"2341e1f6-e5c6-4e57-aece-eeb6fa10d61e\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"QuickItemPanel\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"1d0beb23-89f4-41d0-a795-4c383539eb73\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"QuickItemPanel\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"Game\",\r\n            \"id\": \"b8475490-1ad9-4766-ae89-afc83a8fdcb9\",\r\n            \"actions\": [\r\n                {\r\n                    \"name\": \"Proceed\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"e8c7f0cc-25c4-4915-b540-11d3bcfc4111\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Skip\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"70581b22-2f28-4cdb-80de-f2c79538a565\",\r\n                    \"expectedControlType\": \"Digital\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"Close\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"20d6c334-0928-4cca-ab7b-4edb6edceba5\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                }\r\n            ],\r\n            \"bindings\": [\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"93874a73-4dda-4e55-a949-cd3fe75c93b0\",\r\n                    \"path\": \"<Mouse>/leftButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Proceed\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"9a556e54-a59e-4637-a408-9436a0552a1e\",\r\n                    \"path\": \"<Gamepad>/{Submit}\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"Proceed\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"f2193c06-c6c5-4c54-9d50-f80d38db9ca2\",\r\n                    \"path\": \"<Keyboard>/e\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Proceed\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"2514cb0d-2f63-4aa3-9651-47137803336b\",\r\n                    \"path\": \"<Keyboard>/space\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Proceed\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c8036fe5-ce72-4540-b3f7-f9160c77f938\",\r\n                    \"path\": \"<Keyboard>/f\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Skip\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"70a0d8a2-1044-42a9-96ac-96a518239d38\",\r\n                    \"path\": \"<Gamepad>/buttonNorth\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"Skip\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"2facbf69-5c71-4329-84b7-792cfa4fc15d\",\r\n                    \"path\": \"<Mouse>/rightButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Close\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"b9e77aa6-dafb-4aa3-9a46-bb570e9df481\",\r\n                    \"path\": \"<Keyboard>/escape\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Close\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"985c8c94-25ec-4659-b871-86b08f649f80\",\r\n                    \"path\": \"<Gamepad>/{Cancel}\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"Close\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"7a1fc28d-d018-4559-aa90-d019037048aa\",\r\n                    \"path\": \"*/{Cancel}\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Close\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"Photo\",\r\n            \"id\": \"9c0b8ad5-e599-4688-82f4-08fddc5f2e61\",\r\n            \"actions\": [\r\n                {\r\n                    \"name\": \"CameraMove\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"ecaa70be-8991-4d18-a968-b0372e8c87b4\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"CameraZoom\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"95e961e6-a40d-487f-a2ea-4f547fea06de\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                }\r\n            ],\r\n            \"bindings\": [\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"8f724add-e069-4b5e-8db9-4fb8331ad2f3\",\r\n                    \"path\": \"<Gamepad>/leftStick\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"StickDeadzone\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"CameraMove\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"WASD\",\r\n                    \"id\": \"34370755-7db0-4874-b2ea-d38da5d3c29b\",\r\n                    \"path\": \"Dpad\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"CameraMove\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"843ba92b-d628-40b1-b5a1-9cfe491c7ddb\",\r\n                    \"path\": \"<Keyboard>/w\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"CameraMove\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"15d6dafe-981a-4261-98a3-9158c756eb9c\",\r\n                    \"path\": \"<Keyboard>/upArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"CameraMove\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"a951b561-98e9-4165-b5bb-6b07d47b10e9\",\r\n                    \"path\": \"<Keyboard>/s\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"CameraMove\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"2a046948-c0be-45a5-913a-33b5ba9ab08c\",\r\n                    \"path\": \"<Keyboard>/downArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"CameraMove\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"f10eb601-87b7-450c-895f-2c5fb665aece\",\r\n                    \"path\": \"<Keyboard>/a\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"CameraMove\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"c248b1d7-e6d7-42a9-b394-e97ebc7c6864\",\r\n                    \"path\": \"<Keyboard>/leftArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"CameraMove\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"b373337b-3c68-4ca1-830a-42433994cff6\",\r\n                    \"path\": \"<Keyboard>/d\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"CameraMove\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"0229dadf-4c24-4089-a72f-5ac0d9fe13e1\",\r\n                    \"path\": \"<Keyboard>/rightArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"CameraMove\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"1D Axis\",\r\n                    \"id\": \"8be00151-6c11-40a7-9543-e0757cb03614\",\r\n                    \"path\": \"1DAxis\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"AxisDeadzone,Scale(factor=0.2)\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"CameraZoom\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"negative\",\r\n                    \"id\": \"66901783-f425-4319-9e58-96382e06bd8f\",\r\n                    \"path\": \"<Gamepad>/rightStick/down\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"CameraZoom\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"positive\",\r\n                    \"id\": \"f98b0148-720b-4b61-9783-f22d4679405f\",\r\n                    \"path\": \"<Gamepad>/rightStick/up\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"CameraZoom\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"9fd4522e-a60b-45a7-985d-52e3d1707909\",\r\n                    \"path\": \"<Mouse>/scroll/y\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Clamp(min=-1,max=1),Scale(factor=0.4)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"CameraZoom\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"UI\",\r\n            \"id\": \"7dcfe6c4-c77e-48e8-ba42-b79d2bc8b0ca\",\r\n            \"actions\": [\r\n                {\r\n                    \"name\": \"Navigate\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"7598a76d-021a-4598-a3c3-7b35912f706e\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"Submit\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"1cdbe408-cf4d-422c-845d-35d53c3843b5\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Cancel\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"d3c8f5ad-d375-4237-8893-c62d924f3ba9\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Point\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"2a63905e-cb19-4b5e-8805-be98fa5d04bc\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"Click\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"dcc577e5-68da-409f-8fc2-7b1bbeaf2158\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"ScrollWheel\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"a2358d87-83c5-4fe5-bc1d-7f5cf922702e\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"MiddleClick\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"b7360388-1199-4c2b-b2e3-fc243fce0b7c\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"RightClick\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"3d81b446-5b44-41bc-8a7f-4231bba37adb\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"TrackedDevicePosition\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"b4bd1a7d-adc8-481a-b17e-41e4624e4c08\",\r\n                    \"expectedControlType\": \"Vector3\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"TrackedDeviceOrientation\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"ccef0d22-affc-4db3-872f-c0a6697ab0f7\",\r\n                    \"expectedControlType\": \"Quaternion\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"TabLeft\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"6ef956d9-6af9-447b-b64f-53f5d447a6f7\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"TabRight\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"fbfdb3fd-68b4-47bc-80a8-d5dd93c398bd\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"SecondaryTabLeft\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"9334430b-227d-4bb1-8112-c1830542a52b\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"SecondaryTabRight\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"17ef3b2c-9c89-4910-bfbf-feb6b235c932\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"UseItem\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"8963d38c-eab7-43a2-8f44-796700305eb6\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"TakeItem\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"49b88189-6836-41c5-b81e-dc6200345b4d\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"MapScroll\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"af175bd1-5294-41b4-b86b-cec4dd746437\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"MapUpFloor\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"c1af5ae8-254c-4464-a4ea-6221aa9ff62b\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"MapDownFloor\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"ceee1e51-b0e8-457d-9ed2-569c5e0104df\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"MapZoom\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"e4913cbb-1bcc-4417-b283-97ff2d6b438a\",\r\n                    \"expectedControlType\": \"Axis\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"MapAddMarker\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"964c5574-41ca-4f82-b30b-edde0923342f\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"MapDeletePhoto\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"62116de0-2534-4d71-8505-4565ae34f799\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"MapViewPhotos\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"a8fa1e46-dfba-416c-9f76-3cd129b7e165\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"NextPage\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"30ce0e98-5405-49c4-9da9-21eebe5cdbbe\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"PreviousPage\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"46383722-f7de-4db3-9f92-886c85389642\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"MoveSelected\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"803f255d-48e5-424c-8447-4e4725afa83d\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"SecondaryMenuAction\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"97015ce7-f7bd-44f6-a84f-2b3780e30399\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"RotateSelected\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"048b3656-912e-4a03-889d-375a9aba7306\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"ToggleItemShortcut\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"5eed254e-58ec-42a2-a8e5-5f05fd850af9\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"LocateItemOnMap\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"3ba8b4c3-ba93-4030-8510-f7b42a91ff87\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"MapCycleSelection\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"c96566b7-7127-4bd4-b1c1-81a014fd42b5\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"SubmitGamepadOnly\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"fcf7c793-ec5f-4485-bc65-27f1008faaf5\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"Press(behavior=1)\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"TakeMinigamePhoto\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"0a61a6a7-b2b2-4379-9d9e-938ffd1a4fed\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                }\r\n            ],\r\n            \"bindings\": [\r\n                {\r\n                    \"name\": \"Gamepad\",\r\n                    \"id\": \"809f371f-c5e2-4e7a-83a1-d867598f40dd\",\r\n                    \"path\": \"2DVector\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"14a5d6e8-4aaf-4119-a9ef-34b8c2c548bf\",\r\n                    \"path\": \"<Gamepad>/leftStick/up\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"9144cbe6-05e1-4687-a6d7-24f99d23dd81\",\r\n                    \"path\": \"<Gamepad>/rightStick/up\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"2db08d65-c5fb-421b-983f-c71163608d67\",\r\n                    \"path\": \"<Gamepad>/leftStick/down\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"58748904-2ea9-4a80-8579-b500e6a76df8\",\r\n                    \"path\": \"<Gamepad>/rightStick/down\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"8ba04515-75aa-45de-966d-393d9bbd1c14\",\r\n                    \"path\": \"<Gamepad>/leftStick/left\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"712e721c-bdfb-4b23-a86c-a0d9fcfea921\",\r\n                    \"path\": \"<Gamepad>/rightStick/left\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"fcd248ae-a788-4676-a12e-f4d81205600b\",\r\n                    \"path\": \"<Gamepad>/leftStick/right\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"1f04d9bc-c50b-41a1-bfcc-afb75475ec20\",\r\n                    \"path\": \"<Gamepad>/rightStick/right\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"fb8277d4-c5cd-4663-9dc7-ee3f0b506d90\",\r\n                    \"path\": \"<Gamepad>/dpad\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"Keyboard\",\r\n                    \"id\": \"ff527021-f211-4c02-933e-5976594c46ed\",\r\n                    \"path\": \"2DVector\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"563fbfdd-0f09-408d-aa75-8642c4f08ef0\",\r\n                    \"path\": \"<Keyboard>/w\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"eb480147-c587-4a33-85ed-eb0ab9942c43\",\r\n                    \"path\": \"<Keyboard>/upArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"2bf42165-60bc-42ca-8072-8c13ab40239b\",\r\n                    \"path\": \"<Keyboard>/s\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"85d264ad-e0a0-4565-b7ff-1a37edde51ac\",\r\n                    \"path\": \"<Keyboard>/downArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"74214943-c580-44e4-98eb-ad7eebe17902\",\r\n                    \"path\": \"<Keyboard>/a\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"cea9b045-a000-445b-95b8-0c171af70a3b\",\r\n                    \"path\": \"<Keyboard>/leftArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"8607c725-d935-4808-84b1-8354e29bab63\",\r\n                    \"path\": \"<Keyboard>/d\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"4cda81dc-9edd-4e03-9d7c-a71a14345d0b\",\r\n                    \"path\": \"<Keyboard>/rightArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"03ab9812-bf25-434c-a63d-734479a25e5a\",\r\n                    \"path\": \"<Gamepad>/{Submit}\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"Submit\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"afea983d-53a5-402a-b7f8-c6b8eb8d276b\",\r\n                    \"path\": \"<Keyboard>/enter\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Submit\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"9e92bb26-7e3b-4ec4-b06b-3c8f8e498ddc\",\r\n                    \"path\": \"<Mouse>/{Submit}\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Submit\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"b3973fd4-85b5-4996-8a44-48adfa042206\",\r\n                    \"path\": \"<VirtualMouse>/leftButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Submit\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"3f37b8f9-4a12-4b54-90ee-ff869387f4a3\",\r\n                    \"path\": \"<Gamepad>/{Cancel}\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"Cancel\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"bed49c80-57e6-4e39-a632-a084335a7553\",\r\n                    \"path\": \"<Mouse>/rightButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Cancel\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"44a88755-6a6f-43ee-a1c2-2e5f1e353ef3\",\r\n                    \"path\": \"<Keyboard>/escape\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Cancel\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c52c8e0b-8179-41d3-b8a1-d149033bbe86\",\r\n                    \"path\": \"<Mouse>/position\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Point\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"e1394cbc-336e-44ce-9ea8-6007ed6193f7\",\r\n                    \"path\": \"<Pen>/position\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Point\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"4faf7dc9-b979-4210-aa8c-e808e1ef89f5\",\r\n                    \"path\": \"<Mouse>/leftButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Click\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"8d66d5ba-88d7-48e6-b1cd-198bbfef7ace\",\r\n                    \"path\": \"<Pen>/tip\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Click\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"38c99815-14ea-4617-8627-164d27641299\",\r\n                    \"path\": \"<Mouse>/scroll\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"ScrollWheel\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"24066f69-da47-44f3-a07e-0015fb02eb2e\",\r\n                    \"path\": \"<Mouse>/middleButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"MiddleClick\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"4c191405-5738-4d4b-a523-c6a301dbf754\",\r\n                    \"path\": \"<Mouse>/rightButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"RightClick\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"f8e67edc-03f9-4cac-96aa-ba583f1935c8\",\r\n                    \"path\": \"<Gamepad>/leftShoulder\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"TabLeft\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"f276115e-26ad-48d3-b3c2-bcc1751542b8\",\r\n                    \"path\": \"<Keyboard>/q\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"TabLeft\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"73ea6155-9941-46d1-b053-903650217b2f\",\r\n                    \"path\": \"<Gamepad>/rightShoulder\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"TabRight\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"25dd85f1-d682-43ce-9452-11a0f0b9c804\",\r\n                    \"path\": \"<Keyboard>/e\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"TabRight\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"8b615fb6-370d-42d0-af05-d510f92571ab\",\r\n                    \"path\": \"<Gamepad>/buttonNorth\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"UseItem\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"2018b379-52ec-414c-bfa5-dd7a295846b1\",\r\n                    \"path\": \"<Keyboard>/e\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"UseItem\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"e493f003-026c-4d43-81b0-0b63262324ae\",\r\n                    \"path\": \"<Gamepad>/leftStick\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"MapScroll\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"Joystick\",\r\n                    \"id\": \"ca80e56c-a13f-4dd3-b91b-31090ec267e7\",\r\n                    \"path\": \"2DVector\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"MapScroll\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"Keyboard\",\r\n                    \"id\": \"b2d3b49b-3667-40e2-be9d-ceb5b54f0abf\",\r\n                    \"path\": \"2DVector\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"MapScroll\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"16b1e71b-6b09-41f9-9546-7508675d4b48\",\r\n                    \"path\": \"<Keyboard>/w\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"MapScroll\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"8df97a39-43c2-4206-898e-0195f834405c\",\r\n                    \"path\": \"<Keyboard>/upArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"MapScroll\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"1b4ad959-5832-4aee-8cdc-ffb14238dc61\",\r\n                    \"path\": \"<Keyboard>/s\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"MapScroll\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"3c2828f6-2ed5-4f90-bb7c-dd524832c055\",\r\n                    \"path\": \"<Keyboard>/downArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"MapScroll\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"dceb7caf-2ea8-473a-ab47-4b985abd9267\",\r\n                    \"path\": \"<Keyboard>/a\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"MapScroll\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"99434717-253c-4db2-8cbc-3cb14b48a1d0\",\r\n                    \"path\": \"<Keyboard>/leftArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"MapScroll\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"a91eebab-74c3-47f9-9eba-819dcf8717fc\",\r\n                    \"path\": \"<Keyboard>/d\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"MapScroll\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"84cf8b46-d40f-4ebc-ba6f-472cea376bed\",\r\n                    \"path\": \"<Keyboard>/rightArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"MapScroll\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"a95f2973-1a54-4608-9446-7d80a1a6233c\",\r\n                    \"path\": \"<Gamepad>/rightTrigger\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"MapUpFloor\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"97be0dc3-ce8d-4f9f-8f84-718e5de1cf43\",\r\n                    \"path\": \"<Keyboard>/r\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"MapUpFloor\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"dba1defc-7cd2-4103-8177-a1d779578601\",\r\n                    \"path\": \"<Gamepad>/leftTrigger\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"MapDownFloor\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"0bbaaf5c-95b0-4b97-b8d3-126d7d44086b\",\r\n                    \"path\": \"<Keyboard>/f\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"MapDownFloor\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"1D Axis\",\r\n                    \"id\": \"48a7e59b-227a-4202-8512-047447e00c16\",\r\n                    \"path\": \"1DAxis\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"AxisDeadzone\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"MapZoom\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"negative\",\r\n                    \"id\": \"5fbdae1b-894a-4431-b2bb-3dad1ebfe66a\",\r\n                    \"path\": \"<Gamepad>/rightStick/down\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"MapZoom\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"positive\",\r\n                    \"id\": \"ba39b3aa-5290-4d31-aa0a-83261cfea405\",\r\n                    \"path\": \"<Gamepad>/rightStick/up\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"MapZoom\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"3d9bc2f8-b070-448b-a67d-3f00a12f4712\",\r\n                    \"path\": \"<Mouse>/scroll/y\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Clamp(min=-1,max=1)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"MapZoom\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"6c0c178c-79a3-41b6-8051-bb7798ba9538\",\r\n                    \"path\": \"<Gamepad>/dpad/right\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"NextPage\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"4ce9cc1b-9dea-465b-a8d2-7e94d0f533f9\",\r\n                    \"path\": \"<Gamepad>/leftStick/right\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"NextPage\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"4e5f160a-f889-4e8c-92a1-579ad955c1c7\",\r\n                    \"path\": \"<Keyboard>/d\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"NextPage\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"89e8b1f4-9993-46ce-b91f-b9dbbbec966a\",\r\n                    \"path\": \"<Gamepad>/dpad/left\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"PreviousPage\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"a34e0a67-47a4-4ac3-b3ac-536e53d99e95\",\r\n                    \"path\": \"<Gamepad>/leftStick/left\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"PreviousPage\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"4efbb7f0-c15d-4303-9f9a-f064a6de988a\",\r\n                    \"path\": \"<Keyboard>/a\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"PreviousPage\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"1e0cd3ac-d497-489d-9ab6-cb2debe19dec\",\r\n                    \"path\": \"<Gamepad>/buttonWest\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"MoveSelected\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"d7d316cf-81e8-422e-9819-460c7daa0743\",\r\n                    \"path\": \"<Keyboard>/space\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"MoveSelected\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"315f9395-ac1f-4047-b1d7-078ab136fed1\",\r\n                    \"path\": \"<Gamepad>/leftShoulder\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"SecondaryMenuAction\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"0c5de0ef-518f-4cd1-9249-8516ac5e1682\",\r\n                    \"path\": \"<Gamepad>/rightShoulder\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"SecondaryMenuAction\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"599a379b-6e6f-49b4-b305-1f5ecca586a0\",\r\n                    \"path\": \"<Keyboard>/t\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"SecondaryMenuAction\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"2c735f6f-1518-49c0-80f4-3f5fde6a8617\",\r\n                    \"path\": \"<Gamepad>/buttonNorth\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"RotateSelected\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"721b7c8d-682f-44bc-b7ba-fabccc66a9da\",\r\n                    \"path\": \"<Keyboard>/r\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"RotateSelected\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c1a8cdb7-9924-49bb-a08c-4e4d315f8750\",\r\n                    \"path\": \"<Gamepad>/leftTrigger\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"ToggleItemShortcut\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"9247475e-0dba-4e4d-b496-51dd13ec319e\",\r\n                    \"path\": \"<Keyboard>/1\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"ToggleItemShortcut\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"89702aa2-065d-4d65-a224-dec52df9f638\",\r\n                    \"path\": \"<Keyboard>/g\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"MapAddMarker\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"e145ad8a-4df2-46cd-8ec4-fc661da44b1e\",\r\n                    \"path\": \"<Gamepad>/buttonWest\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"MapAddMarker\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"ceaac4de-6418-4bb1-8ad7-1cefe805d850\",\r\n                    \"path\": \"<Keyboard>/x\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"MapDeletePhoto\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"a2dee690-175b-4a9b-bf7b-d2a82aa18829\",\r\n                    \"path\": \"<Gamepad>/buttonNorth\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"MapDeletePhoto\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"b9c20464-0dca-4843-b497-b7fa49cae26b\",\r\n                    \"path\": \"<Keyboard>/z\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"MapViewPhotos\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"cdca4fe0-3f2f-4504-8cad-2d173ae55384\",\r\n                    \"path\": \"<Gamepad>/buttonWest\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"MapViewPhotos\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"a61cae57-4726-41aa-aaf2-63998df95956\",\r\n                    \"path\": \"<Gamepad>/buttonWest\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"LocateItemOnMap\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"f3c0c25e-945b-4782-9e61-a39590c20541\",\r\n                    \"path\": \"<Keyboard>/x\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"LocateItemOnMap\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"8453e0f5-1f78-4704-8f1d-7fca50c12e28\",\r\n                    \"path\": \"<Gamepad>/buttonWest\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"MapCycleSelection\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"e84ab976-eb79-4cff-afd6-a1593834e6b5\",\r\n                    \"path\": \"<Mouse>/middleButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"MapCycleSelection\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"8c99aaa0-c83d-4446-8248-7067a57cd6ef\",\r\n                    \"path\": \"<Gamepad>/{Submit}\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"SubmitGamepadOnly\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"8859d0af-018f-456c-9586-ee93387f8c0d\",\r\n                    \"path\": \"<Mouse>/leftButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"TakeItem\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"9ce3c86d-3c3e-4852-9b03-2b659dbec146\",\r\n                    \"path\": \"<Gamepad>/{Submit}\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"TakeItem\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"9ada009a-cc65-4169-bbea-149b9eb420c4\",\r\n                    \"path\": \"<Keyboard>/space\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"TakeItem\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"3e8286d1-1c24-47a0-931b-642e437b58ee\",\r\n                    \"path\": \"<Gamepad>/leftTrigger\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"SecondaryTabLeft\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"74e8277c-ddec-4cf5-a147-9ffbb8d8afec\",\r\n                    \"path\": \"<Keyboard>/z\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"SecondaryTabLeft\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c9606501-ac86-4f46-b628-7faf2b3236d3\",\r\n                    \"path\": \"<Gamepad>/rightTrigger\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"SecondaryTabRight\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"a6dcad17-854b-42ed-8af7-f2cc7af70cb5\",\r\n                    \"path\": \"<Keyboard>/c\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"SecondaryTabRight\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"ec124011-fe30-4a50-b548-41742d112e4b\",\r\n                    \"path\": \"<Gamepad>/buttonNorth\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"TakeMinigamePhoto\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"cdf52a95-2430-49cf-8265-0c02c83d439c\",\r\n                    \"path\": \"<Keyboard>/c\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"TakeMinigamePhoto\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"Minigame\",\r\n            \"id\": \"14f3fa5e-a521-4d76-a193-c2d747292c4d\",\r\n            \"actions\": [\r\n                {\r\n                    \"name\": \"HorizontalMove\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"6dab0c13-c449-4ce1-9e43-0ed6648756cd\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"VerticalMove\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"f53eb6b8-3300-44d8-9046-6f2760e393d0\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"RotateObjectHold\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"ce9349f7-ddce-49a8-a4fb-25142a9fdffb\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Move2D\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"2d744a7f-0344-40fd-bfda-9a3ff3198dbf\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                }\r\n            ],\r\n            \"bindings\": [\r\n                {\r\n                    \"name\": \"1D Axis\",\r\n                    \"id\": \"313711c1-e750-4896-8d59-1272938b633b\",\r\n                    \"path\": \"1DAxis\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"HorizontalMove\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"negative\",\r\n                    \"id\": \"833ae149-eff0-44d0-ba5b-961470b0c163\",\r\n                    \"path\": \"<Gamepad>/leftStick/left\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"HorizontalMove\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"positive\",\r\n                    \"id\": \"f0c34c37-abae-425a-b34c-021b80ec0018\",\r\n                    \"path\": \"<Gamepad>/leftStick/right\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"HorizontalMove\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"1D Axis\",\r\n                    \"id\": \"c0aed204-33df-4659-ae0e-ce7197d6b459\",\r\n                    \"path\": \"1DAxis\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"HorizontalMove\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"negative\",\r\n                    \"id\": \"ef53f4ea-a745-42a2-87a6-0b1fd181b14e\",\r\n                    \"path\": \"<Mouse>/delta/left\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"HorizontalMove\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"positive\",\r\n                    \"id\": \"7e9d5d39-a32f-4748-83a1-202c5d47a623\",\r\n                    \"path\": \"<Mouse>/delta/right\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"HorizontalMove\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"1D Axis\",\r\n                    \"id\": \"feee52df-6df9-4bef-9e3d-2a13b6254fa3\",\r\n                    \"path\": \"1DAxis\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"HorizontalMove\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"negative\",\r\n                    \"id\": \"5a7e59eb-d188-4151-a5a8-e5d7f9669af5\",\r\n                    \"path\": \"<Keyboard>/a\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"HorizontalMove\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"positive\",\r\n                    \"id\": \"e5cf1a99-b42b-40fb-9e9e-b946b8ded252\",\r\n                    \"path\": \"<Keyboard>/d\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"HorizontalMove\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"1D Axis\",\r\n                    \"id\": \"7a605c2f-7f11-4bd4-8b46-c1ec9a3fbe72\",\r\n                    \"path\": \"1DAxis\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"HorizontalMove\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"negative\",\r\n                    \"id\": \"7806d55b-e35d-4d88-b46c-3b4667a66e1e\",\r\n                    \"path\": \"<Keyboard>/leftArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"HorizontalMove\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"positive\",\r\n                    \"id\": \"fd9af041-5b68-4690-8e5d-a90267e3a3d1\",\r\n                    \"path\": \"<Keyboard>/rightArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"HorizontalMove\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"3528e7d4-04a0-41ad-8c16-c7ba392b9908\",\r\n                    \"path\": \"<Gamepad>/leftShoulder\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"RotateObjectHold\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"168d224b-a751-44a3-91a4-6cd76b76c0a9\",\r\n                    \"path\": \"<Gamepad>/rightShoulder\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"RotateObjectHold\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"860c8451-aad4-4539-a633-0750a610725c\",\r\n                    \"path\": \"<Keyboard>/r\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"RotateObjectHold\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"1D Axis\",\r\n                    \"id\": \"997a2f9e-95c4-4222-8ce6-fceff655666d\",\r\n                    \"path\": \"1DAxis\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"VerticalMove\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"negative\",\r\n                    \"id\": \"fbf9858e-fba9-48b0-a1cb-cfe573017f29\",\r\n                    \"path\": \"<Gamepad>/leftStick/down\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"VerticalMove\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"positive\",\r\n                    \"id\": \"2226304c-a973-43a3-a202-03505ab349db\",\r\n                    \"path\": \"<Gamepad>/leftStick/down\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"VerticalMove\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"1D Axis\",\r\n                    \"id\": \"bf823a7e-e166-442e-a5c8-b3f0139cc6b5\",\r\n                    \"path\": \"1DAxis\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"VerticalMove\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"negative\",\r\n                    \"id\": \"2d1b7931-2f7f-4d1b-b542-74f257164f2f\",\r\n                    \"path\": \"<Mouse>/delta/down\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"VerticalMove\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"positive\",\r\n                    \"id\": \"c2371b26-a373-474f-af13-bca48a13bb6a\",\r\n                    \"path\": \"<Mouse>/delta/up\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"VerticalMove\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"1D Axis\",\r\n                    \"id\": \"98533017-7a03-4326-b1e8-28d23edeee45\",\r\n                    \"path\": \"1DAxis\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"VerticalMove\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"negative\",\r\n                    \"id\": \"a0ed9139-2161-4462-a0bc-a0e986607bc6\",\r\n                    \"path\": \"<Keyboard>/s\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"VerticalMove\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"positive\",\r\n                    \"id\": \"cdb93f56-d2e5-495e-ac0c-c60a841e057c\",\r\n                    \"path\": \"<Keyboard>/w\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"VerticalMove\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"1D Axis\",\r\n                    \"id\": \"adabbc46-3ef2-4c26-abb9-3d5d8f47c510\",\r\n                    \"path\": \"1DAxis\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"VerticalMove\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"negative\",\r\n                    \"id\": \"410500ae-f613-4519-9055-359381ac527b\",\r\n                    \"path\": \"<Keyboard>/downArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"VerticalMove\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"positive\",\r\n                    \"id\": \"041dced0-fd53-4461-84e9-5abe0c436a95\",\r\n                    \"path\": \"<Keyboard>/upArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"VerticalMove\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"ae493d9b-e15d-40bc-b447-230cff6f88f6\",\r\n                    \"path\": \"<Mouse>/delta\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move2D\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"Gamepad\",\r\n                    \"id\": \"866764ef-bda6-45a1-bb05-d8d63ea03728\",\r\n                    \"path\": \"2DVector\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Move2D\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"ee07a60f-2a2d-4062-850a-1bf4f686abd7\",\r\n                    \"path\": \"<Gamepad>/leftStick/up\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Move2D\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"34be0209-999e-4ea2-ad73-a69360a3d2c4\",\r\n                    \"path\": \"<Gamepad>/leftStick/down\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Move2D\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"8bf7856e-84bf-4e4b-bddf-0d471148b9af\",\r\n                    \"path\": \"<Gamepad>/leftStick/left\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Move2D\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"82a7ab5c-df72-4c55-a264-b20453e55bd7\",\r\n                    \"path\": \"<Gamepad>/leftStick/right\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Move2D\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"Debug\",\r\n            \"id\": \"1f0adf73-7a8c-44d3-99be-b214a90fdc77\",\r\n            \"actions\": [\r\n                {\r\n                    \"name\": \"ToggleConsole\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"b464127e-f4df-466e-bf45-8d9cce7e96e9\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"CloseConsole\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"2bb5a187-1a0e-4ce3-8a57-bb838848c281\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"SubmitConsoleCommand\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"f3d1a572-21a8-4ff0-8025-3dc40cac1e96\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                }\r\n            ],\r\n            \"bindings\": [\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"6affeb1c-7321-4291-bb59-1370576b3b80\",\r\n                    \"path\": \"<Keyboard>/backquote\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"ToggleConsole\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"8e9fbf91-2671-41dd-b59d-c0258f806fce\",\r\n                    \"path\": \"<Gamepad>/rightStickPress\",\r\n                    \"interactions\": \"MultiTap\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"ToggleConsole\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"065f9c4d-bd26-4585-ac92-8fdb36368cf2\",\r\n                    \"path\": \"<Keyboard>/escape\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"CloseConsole\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"b43017c5-6c56-419f-a5cf-aaae66752768\",\r\n                    \"path\": \"<Gamepad>/rightStickPress\",\r\n                    \"interactions\": \"MultiTap\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"CloseConsole\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"10753155-1769-40a5-af0c-1250b93743d9\",\r\n                    \"path\": \"<Gamepad>/buttonWest\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"SubmitConsoleCommand\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"Engagement\",\r\n            \"id\": \"7b557ac5-3114-4a77-bf14-7ca6bd7e1441\",\r\n            \"actions\": [\r\n                {\r\n                    \"name\": \"Engagement\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"db9f8101-df65-4fa2-a75f-b30ec997c61b\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                }\r\n            ],\r\n            \"bindings\": [\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c1a059b1-2eee-40e4-abff-05ebbb557125\",\r\n                    \"path\": \"<Keyboard>/anyKey\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Engagement\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"51ed98bc-c79f-4bbf-a6b8-8eecbdb3355a\",\r\n                    \"path\": \"<Mouse>/leftButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Engagement\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"7d7e2d2e-09ac-4eca-8840-513840e9b205\",\r\n                    \"path\": \"<Mouse>/rightButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Engagement\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"785616a2-a444-47dc-894e-e9a2e697b2cd\",\r\n                    \"path\": \"<Gamepad>/{Submit}\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Engagement\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"f2567146-5547-4379-8494-3b2cf4df4263\",\r\n                    \"path\": \"<Gamepad>/{Cancel}\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Engagement\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"6201330e-883d-4ff3-9317-67451ba02e73\",\r\n                    \"path\": \"<Gamepad>/buttonNorth\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Engagement\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"1bd7e499-fa4a-4842-a9e6-588b0dc80840\",\r\n                    \"path\": \"<Gamepad>/buttonWest\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Engagement\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"48daeb49-5156-409b-ab1c-aa49fc1efd74\",\r\n                    \"path\": \"<Gamepad>/dpad/up\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Engagement\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"1ff5479f-76ee-4835-93bd-58c2566fb553\",\r\n                    \"path\": \"<Gamepad>/dpad/down\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Engagement\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"b9612b7c-8afa-4a05-9427-94b4b535e35d\",\r\n                    \"path\": \"<Gamepad>/dpad/left\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Engagement\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"d5688d0a-c777-499e-a7b9-dbe9cf8ebd89\",\r\n                    \"path\": \"<Gamepad>/dpad/right\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Engagement\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"0a1bad3d-fc37-4707-b20e-580ec8c216f8\",\r\n                    \"path\": \"<Gamepad>/leftShoulder\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Engagement\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"d7a64e4b-7480-4b31-bf16-ec87a61e868c\",\r\n                    \"path\": \"<Gamepad>/leftTrigger\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Engagement\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"4744b8e5-c3aa-492a-b3cc-47c740434d6c\",\r\n                    \"path\": \"<Gamepad>/rightShoulder\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Engagement\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c3b2e301-29d5-440b-93f3-04b99ff1c608\",\r\n                    \"path\": \"<Gamepad>/rightTrigger\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Engagement\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c652a29e-49cb-45d8-88c7-d1573a3f1419\",\r\n                    \"path\": \"<Gamepad>/start\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Engagement\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c030c8f7-6bf7-475e-a331-be6e27d3b3a5\",\r\n                    \"path\": \"<Gamepad>/select\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Engagement\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                }\r\n            ]\r\n        }\r\n    ],\r\n    \"controlSchemes\": [\r\n        {\r\n            \"name\": \"Keyboard&Mouse\",\r\n            \"bindingGroup\": \"Keyboard&Mouse\",\r\n            \"devices\": [\r\n                {\r\n                    \"devicePath\": \"<Keyboard>\",\r\n                    \"isOptional\": false,\r\n                    \"isOR\": false\r\n                },\r\n                {\r\n                    \"devicePath\": \"<Mouse>\",\r\n                    \"isOptional\": false,\r\n                    \"isOR\": false\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"Gamepad\",\r\n            \"bindingGroup\": \"Gamepad\",\r\n            \"devices\": [\r\n                {\r\n                    \"devicePath\": \"<Gamepad>\",\r\n                    \"isOptional\": false,\r\n                    \"isOR\": false\r\n                }\r\n            ]\r\n        }\r\n    ]\r\n}");
		m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
		m_Player_Move = m_Player.FindAction("Move", throwIfNotFound: true);
		m_Player_AimDirection = m_Player.FindAction("AimDirection", throwIfNotFound: true);
		m_Player_Fire = m_Player.FindAction("Fire", throwIfNotFound: true);
		m_Player_Sprint = m_Player.FindAction("Sprint", throwIfNotFound: true);
		m_Player_Crouch = m_Player.FindAction("Crouch", throwIfNotFound: true);
		m_Player_Jump = m_Player.FindAction("Jump", throwIfNotFound: true);
		m_Player_Dodge = m_Player.FindAction("Dodge", throwIfNotFound: true);
		m_Player_AimMode = m_Player.FindAction("AimMode", throwIfNotFound: true);
		m_Player_Interact = m_Player.FindAction("Interact", throwIfNotFound: true);
		m_Player_Reload = m_Player.FindAction("Reload", throwIfNotFound: true);
		m_Player_QuickMap = m_Player.FindAction("QuickMap", throwIfNotFound: true);
		m_Player_Stomp = m_Player.FindAction("Stomp", throwIfNotFound: true);
		m_Player_ShortcutFlashlight = m_Player.FindAction("ShortcutFlashlight", throwIfNotFound: true);
		m_Player_PlayerCancel = m_Player.FindAction("PlayerCancel", throwIfNotFound: true);
		m_Player_ShortcutCamera = m_Player.FindAction("ShortcutCamera", throwIfNotFound: true);
		m_Player_QuickItemRight = m_Player.FindAction("QuickItemRight", throwIfNotFound: true);
		m_Player_QuickItemUse = m_Player.FindAction("QuickItemUse", throwIfNotFound: true);
		m_Player_QuickItemLeft = m_Player.FindAction("QuickItemLeft", throwIfNotFound: true);
		m_Player_CycleWeapon = m_Player.FindAction("CycleWeapon", throwIfNotFound: true);
		m_MenuToggles = asset.FindActionMap("MenuToggles", throwIfNotFound: true);
		m_MenuToggles_SystemMenu = m_MenuToggles.FindAction("SystemMenu", throwIfNotFound: true);
		m_MenuToggles_Map = m_MenuToggles.FindAction("Map", throwIfNotFound: true);
		m_MenuToggles_Inventory = m_MenuToggles.FindAction("Inventory", throwIfNotFound: true);
		m_MenuToggles_Artifacts = m_MenuToggles.FindAction("Artifacts", throwIfNotFound: true);
		m_MenuToggles_Notes = m_MenuToggles.FindAction("Notes", throwIfNotFound: true);
		m_MenuToggles_ItemWheel = m_MenuToggles.FindAction("ItemWheel", throwIfNotFound: true);
		m_MenuToggles_QuickItemPanel = m_MenuToggles.FindAction("QuickItemPanel", throwIfNotFound: true);
		m_Game = asset.FindActionMap("Game", throwIfNotFound: true);
		m_Game_Proceed = m_Game.FindAction("Proceed", throwIfNotFound: true);
		m_Game_Skip = m_Game.FindAction("Skip", throwIfNotFound: true);
		m_Game_Close = m_Game.FindAction("Close", throwIfNotFound: true);
		m_Photo = asset.FindActionMap("Photo", throwIfNotFound: true);
		m_Photo_CameraMove = m_Photo.FindAction("CameraMove", throwIfNotFound: true);
		m_Photo_CameraZoom = m_Photo.FindAction("CameraZoom", throwIfNotFound: true);
		m_UI = asset.FindActionMap("UI", throwIfNotFound: true);
		m_UI_Navigate = m_UI.FindAction("Navigate", throwIfNotFound: true);
		m_UI_Submit = m_UI.FindAction("Submit", throwIfNotFound: true);
		m_UI_Cancel = m_UI.FindAction("Cancel", throwIfNotFound: true);
		m_UI_Point = m_UI.FindAction("Point", throwIfNotFound: true);
		m_UI_Click = m_UI.FindAction("Click", throwIfNotFound: true);
		m_UI_ScrollWheel = m_UI.FindAction("ScrollWheel", throwIfNotFound: true);
		m_UI_MiddleClick = m_UI.FindAction("MiddleClick", throwIfNotFound: true);
		m_UI_RightClick = m_UI.FindAction("RightClick", throwIfNotFound: true);
		m_UI_TrackedDevicePosition = m_UI.FindAction("TrackedDevicePosition", throwIfNotFound: true);
		m_UI_TrackedDeviceOrientation = m_UI.FindAction("TrackedDeviceOrientation", throwIfNotFound: true);
		m_UI_TabLeft = m_UI.FindAction("TabLeft", throwIfNotFound: true);
		m_UI_TabRight = m_UI.FindAction("TabRight", throwIfNotFound: true);
		m_UI_SecondaryTabLeft = m_UI.FindAction("SecondaryTabLeft", throwIfNotFound: true);
		m_UI_SecondaryTabRight = m_UI.FindAction("SecondaryTabRight", throwIfNotFound: true);
		m_UI_UseItem = m_UI.FindAction("UseItem", throwIfNotFound: true);
		m_UI_TakeItem = m_UI.FindAction("TakeItem", throwIfNotFound: true);
		m_UI_MapScroll = m_UI.FindAction("MapScroll", throwIfNotFound: true);
		m_UI_MapUpFloor = m_UI.FindAction("MapUpFloor", throwIfNotFound: true);
		m_UI_MapDownFloor = m_UI.FindAction("MapDownFloor", throwIfNotFound: true);
		m_UI_MapZoom = m_UI.FindAction("MapZoom", throwIfNotFound: true);
		m_UI_MapAddMarker = m_UI.FindAction("MapAddMarker", throwIfNotFound: true);
		m_UI_MapDeletePhoto = m_UI.FindAction("MapDeletePhoto", throwIfNotFound: true);
		m_UI_MapViewPhotos = m_UI.FindAction("MapViewPhotos", throwIfNotFound: true);
		m_UI_NextPage = m_UI.FindAction("NextPage", throwIfNotFound: true);
		m_UI_PreviousPage = m_UI.FindAction("PreviousPage", throwIfNotFound: true);
		m_UI_MoveSelected = m_UI.FindAction("MoveSelected", throwIfNotFound: true);
		m_UI_SecondaryMenuAction = m_UI.FindAction("SecondaryMenuAction", throwIfNotFound: true);
		m_UI_RotateSelected = m_UI.FindAction("RotateSelected", throwIfNotFound: true);
		m_UI_ToggleItemShortcut = m_UI.FindAction("ToggleItemShortcut", throwIfNotFound: true);
		m_UI_LocateItemOnMap = m_UI.FindAction("LocateItemOnMap", throwIfNotFound: true);
		m_UI_MapCycleSelection = m_UI.FindAction("MapCycleSelection", throwIfNotFound: true);
		m_UI_SubmitGamepadOnly = m_UI.FindAction("SubmitGamepadOnly", throwIfNotFound: true);
		m_UI_TakeMinigamePhoto = m_UI.FindAction("TakeMinigamePhoto", throwIfNotFound: true);
		m_Minigame = asset.FindActionMap("Minigame", throwIfNotFound: true);
		m_Minigame_HorizontalMove = m_Minigame.FindAction("HorizontalMove", throwIfNotFound: true);
		m_Minigame_VerticalMove = m_Minigame.FindAction("VerticalMove", throwIfNotFound: true);
		m_Minigame_RotateObjectHold = m_Minigame.FindAction("RotateObjectHold", throwIfNotFound: true);
		m_Minigame_Move2D = m_Minigame.FindAction("Move2D", throwIfNotFound: true);
		m_Debug = asset.FindActionMap("Debug", throwIfNotFound: true);
		m_Debug_ToggleConsole = m_Debug.FindAction("ToggleConsole", throwIfNotFound: true);
		m_Debug_CloseConsole = m_Debug.FindAction("CloseConsole", throwIfNotFound: true);
		m_Debug_SubmitConsoleCommand = m_Debug.FindAction("SubmitConsoleCommand", throwIfNotFound: true);
		m_Engagement = asset.FindActionMap("Engagement", throwIfNotFound: true);
		m_Engagement_Engagement = m_Engagement.FindAction("Engagement", throwIfNotFound: true);
	}

	~GameInputActions()
	{
	}

	public void Dispose()
	{
		UnityEngine.Object.Destroy(asset);
	}

	public bool Contains(InputAction action)
	{
		return asset.Contains(action);
	}

	public IEnumerator<InputAction> GetEnumerator()
	{
		return asset.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Enable()
	{
		asset.Enable();
	}

	public void Disable()
	{
		asset.Disable();
	}

	public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
	{
		return asset.FindAction(actionNameOrId, throwIfNotFound);
	}

	public int FindBinding(InputBinding bindingMask, out InputAction action)
	{
		return asset.FindBinding(bindingMask, out action);
	}
}
